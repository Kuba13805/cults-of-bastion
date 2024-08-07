using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using PlayerResources;
using UI;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Managers
{
    public class TimeManager : MonoBehaviour
    {
        [SerializeField] private GameCalendar gameCalendar;

        public float currentTime;
        public int currentDay;
        public int currentMonth;
        public int currentYear;
        public float normalSpeed = 1f; // Real world seconds per game hour
        public float highSpeed = 0.5f; // Real world seconds per game hour

        private bool _paused;
        private JobHandle _timeCycleJobHandle;
        private TimeCycleJob _timeCycleJob;
        private NativeArray<float> _jobCurrentTime;
        private NativeArray<int> _jobCurrentDay;
        private NativeArray<int> _jobCurrentMonth;
        private NativeArray<int> _jobCurrentYear;
        private NativeArray<int> _jobHoursInDay;
        private NativeArray<int> _jobDaysInMonth;
        private NativeArray<int> _jobMonthsInYear;
        private float _currentSpeed;
        private Thread _timeThread;

        private readonly object _lock = new();
        private SynchronizationContext _mainThreadContext;

        private int _passedDays;

        #region TimeCycleEvents

        public static event Action<float> OnHourChanged;
        public static event Action<int, int, int> OnDayChanged;
        public static event Action OnWeekCycle;

        #endregion

        private void Start()
        {
            _mainThreadContext = SynchronizationContext.Current;

            CreateNewTimeJob();
            SubscribeToEvents();
            
            OnDayChanged?.Invoke(currentDay, currentMonth, currentYear);
            OnHourChanged?.Invoke(currentTime);
        }

        private void OnDestroy()
        {
            DisposeTimeJob();
            UnsubscribeFromEvents();
        }
        
        private void SubscribeToEvents()
        {
            UIController.OnPauseGame += PauseTheGame;
            UIController.OnResumeGameWithNormalSpeed += ResumeTheGameWithNormalSpeed;
            UIController.OnResumeGameWithHighSpeed += ResumeTheGameWithHighSpeed;
            InputManager.Instance.PlayerInputControls.CityViewActions.PauzeGame.performed += PauseTheGame;
            InputManager.Instance.PlayerInputControls.CityViewActions.ResumeGameNormalSpeed.performed +=
                ResumeTheGameWithNormalSpeed;
            InputManager.Instance.PlayerInputControls.CityViewActions.ResumeGameHighSpeed.performed +=
                ResumeTheGameWithHighSpeed;
        }

        private void UnsubscribeFromEvents()
        {
            UIController.OnPauseGame -= PauseTheGame;
            UIController.OnResumeGameWithNormalSpeed -= ResumeTheGameWithNormalSpeed;
            UIController.OnResumeGameWithHighSpeed -= ResumeTheGameWithHighSpeed;
            InputManager.Instance.PlayerInputControls.CityViewActions.PauzeGame.performed -= PauseTheGame;
            InputManager.Instance.PlayerInputControls.CityViewActions.ResumeGameNormalSpeed.performed -=
                ResumeTheGameWithNormalSpeed;
            InputManager.Instance.PlayerInputControls.CityViewActions.ResumeGameHighSpeed.performed -=
                ResumeTheGameWithHighSpeed;
        }
        
        #region TimeJobSystem

        private void CreateNewTimeJob()
        {
            _jobCurrentTime = new NativeArray<float>(1, Allocator.Persistent);
            _jobCurrentDay = new NativeArray<int>(1, Allocator.Persistent);
            _jobCurrentMonth = new NativeArray<int>(1, Allocator.Persistent);
            _jobCurrentYear = new NativeArray<int>(1, Allocator.Persistent);
            _jobHoursInDay = new NativeArray<int>(1, Allocator.Persistent);
            _jobDaysInMonth = new NativeArray<int>(1, Allocator.Persistent);
            _jobMonthsInYear = new NativeArray<int>(1, Allocator.Persistent);

            _jobHoursInDay[0] = gameCalendar.hoursInDay;
            _jobDaysInMonth[0] = gameCalendar.daysInMonth;
            _jobMonthsInYear[0] = gameCalendar.monthsInYear;
            
            _jobCurrentTime[0] = currentTime;
            _jobCurrentDay[0] = currentDay;
            _jobCurrentMonth[0] = currentMonth;
            _jobCurrentYear[0] = currentYear;
        }


        private void DisposeTimeJob()
        {
            _timeCycleJobHandle.Complete();
            if (_timeThread is { IsAlive: true }) _timeThread.Abort();

            _jobCurrentTime.Dispose();
            _jobCurrentDay.Dispose();
            _jobCurrentMonth.Dispose();
            _jobCurrentYear.Dispose();
            _jobHoursInDay.Dispose();
            _jobDaysInMonth.Dispose();
            _jobMonthsInYear.Dispose();
        }

        #endregion

        #region TimeCycleControls

        private void PauseTheGame()
        {
            _paused = true;
            _timeCycleJobHandle.Complete();
            if (_timeThread is { IsAlive: true })
            {
                _timeThread.Abort();
            }

        }

        private void ResumeTheGameWithNormalSpeed()
        {
            _paused = false;
            _currentSpeed = normalSpeed;
            StartTimeThread();
        }

        private void ResumeTheGameWithHighSpeed()
        {
            _paused = false;
            _currentSpeed = highSpeed;
            StartTimeThread();
        }

        #endregion

        #region TimeCycleControlsFromInputManager

        private void PauseTheGame(InputAction.CallbackContext obj)
        {
            PauseTheGame();
        }
        private void ResumeTheGameWithNormalSpeed(InputAction.CallbackContext obj)
        {
            ResumeTheGameWithNormalSpeed();
        }
        private void ResumeTheGameWithHighSpeed(InputAction.CallbackContext obj)
        {
            ResumeTheGameWithHighSpeed();
        }

        #endregion

        #region TimeCycleJobSystem

        private void StartTimeThread()
        {
            if (_timeThread is { IsAlive: true })
            {
                _timeThread.Abort();
            }
            _timeThread = new Thread(TimeThreadMethod);
            _timeThread.Start();
        }

        private void TimeThreadMethod()
        {
            while (!_paused)
            {
                Thread.Sleep((int)(_currentSpeed * 1000));
                StartTimeCycleWithJobSystem();
            }
        }

        private void StartTimeCycleWithJobSystem()
        {
            _timeCycleJobHandle.Complete();

            _timeCycleJob = new TimeCycleJob
            {
                CurrentTime = _jobCurrentTime,
                CurrentDay = _jobCurrentDay,
                CurrentMonth = _jobCurrentMonth,
                CurrentYear = _jobCurrentYear,
                HoursInDay = _jobHoursInDay,
                DaysInMonth = _jobDaysInMonth,
                MonthsInYear = _jobMonthsInYear
            };

            _timeCycleJobHandle = _timeCycleJob.Schedule();
            _timeCycleJobHandle.Complete();

            lock (_lock)
            {
                currentTime = _jobCurrentTime[0];
                currentDay = _jobCurrentDay[0];
                currentMonth = _jobCurrentMonth[0];
                currentYear = _jobCurrentYear[0];
            }

            _mainThreadContext.Post(_ => UpdateInGameTime(currentTime), null);
        }

        private void UpdateInGameTime(float time)
        {
            OnHourChanged?.Invoke(time);
            if (time == 0)
            {
                OnDayChanged?.Invoke(currentDay, currentMonth, currentYear);
                _passedDays += 1;
            }

            if (_passedDays != 7) return;
            OnWeekCycle?.Invoke();
            _passedDays = 0;
        }

        private struct TimeCycleJob : IJob
        {
            public NativeArray<float> CurrentTime;
            public NativeArray<int> CurrentDay;
            public NativeArray<int> CurrentMonth;
            public NativeArray<int> CurrentYear;
            public NativeArray<int> HoursInDay;
            public NativeArray<int> DaysInMonth;
            public NativeArray<int> MonthsInYear;

            public void Execute()
            {
                CurrentTime[0] += 1;

                if (!(CurrentTime[0] >= HoursInDay[0])) return;
                CurrentTime[0] = 0;
                CurrentDay[0] += 1;

                if (CurrentDay[0] <= DaysInMonth[0]) return;
                CurrentDay[0] = 1;
                CurrentMonth[0] += 1;

                if (CurrentMonth[0] <= MonthsInYear[0]) return;
                CurrentMonth[0] = 1;
                CurrentYear[0] += 1;
            }
        }

        #endregion

        [Serializable]
        private struct GameCalendar
        {
            public int hoursInDay;
            public int daysInMonth;
            public int monthsInYear;

            public Dictionary<int, string> Months => new()
            {
                { 1, "month_1" },
                { 2, "month_2" },
                { 3, "month_3" },
                { 4, "month_4" },
                { 5, "month_5" },
                { 6, "month_6" },
                { 7, "month_7" },
                { 8, "month_8" },
                { 9, "month_9" },
                { 10, "month_10" },
                { 11, "month_11" },
                { 12, "month_12" }
            };
        }
    }
}
