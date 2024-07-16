using System;
using System.Collections;
using System.Collections.Generic;
using Managers;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Serialization;

public class TimeManager : MonoBehaviour
{
    [SerializeField]private GameCalendar gameCalendar;

    public float currentTime;
    public int currentDay;
    public int currentMonth;
    public int currentYear;
    public float normalSpeed = 1f;
    public float highSpeed = 0.5f;
    
    private bool _paused;
    private Coroutine _timeCycleCoroutine;

    #region TimeCylcleEvents

    public static event Action<float> OnHourChanged;
    public static event Action OnDayChanged;

    #endregion
    private void Start()
    {
        SubscribeToEvents();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void SubscribeToEvents()
    {
        UIController.OnPauseGame += PauseTheGame;
        UIController.OnResumeGameWithNormalSpeed += ResumeTheGameWithNormalSpeed;
        UIController.OnResumeGameWithHighSpeed += ResumeTheGameWithHighSpeed;
    }

    private void UnsubscribeFromEvents()
    {
        UIController.OnPauseGame -= PauseTheGame;
        UIController.OnResumeGameWithNormalSpeed -= ResumeTheGameWithNormalSpeed;
        UIController.OnResumeGameWithHighSpeed -= ResumeTheGameWithHighSpeed;
    }

    private void PauseTheGame()
    {
        _paused = true;
        StopTimeCycle();
    }

    private void ResumeTheGameWithNormalSpeed()
    {
        _paused = false;
        StartTimeCycle(normalSpeed);
    }

    private void ResumeTheGameWithHighSpeed()
    {
        _paused = false;
        StartTimeCycle(highSpeed);
    }

    private void StartTimeCycle(float speed)
    {
        if (_timeCycleCoroutine != null)
        {
            StopCoroutine(_timeCycleCoroutine);
        }

        _timeCycleCoroutine = StartCoroutine(TimeCycleCoroutine(speed));
    }

    private void StopTimeCycle()
    {
        if (_timeCycleCoroutine != null)
        {
            StopCoroutine(_timeCycleCoroutine);
            _timeCycleCoroutine = null;
        }
    }
    
    private IEnumerator TimeCycleCoroutine(float speed)
    {
        while (!_paused)
        {
            yield return new WaitForSeconds(speed);
            
            currentTime += 1;
            OnHourChanged?.Invoke(currentTime);

            if (!(currentTime >= gameCalendar.hoursInDay)) continue;
            
            currentTime = 0;
            currentDay += 1;
            OnDayChanged?.Invoke();
            if (currentDay <= gameCalendar.daysInMonth) continue;
            
            currentDay = 1;
            currentMonth += 1;
            if (currentMonth <= gameCalendar.monthsInYear) continue;
            
            currentMonth = 1;
            currentYear += 1;
        }
    }

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
