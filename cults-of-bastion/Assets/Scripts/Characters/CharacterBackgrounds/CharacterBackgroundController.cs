using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using NewGame;
using UnityEngine;

namespace Characters.CharacterBackgrounds
{
    public class CharacterBackgroundController : MonoBehaviour
    {
        #region Variables

        private readonly Dictionary<string, CharacterBackground> _childhoodBackgrounds = new();
        private readonly Dictionary<string, CharacterBackground> _adulthoodBackgrounds = new();
        private BackgroundData _backgroundData;

        #endregion

        #region Events

        public static event Action<(List<CharacterBackground>, List<CharacterBackground>)> OnReturnBackgrounds;
        public static event Action<List<string>> OnRequestBackgroundEffectsCreation;
        public static event Action OnCharacterBackgroundControllerInitialized;

        #endregion
        private void Awake()
        {
            SubscribeToEvents();
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            CharacterManager.OnRequestCharacterGeneratorData += ReturnBackgrounds;
            NewGameController.OnRequestGameData += ReturnBackgrounds;
            GameManager.OnStartDataLoading += StartBackgroundLoading;
        }

        private void UnsubscribeFromEvents()
        {
            CharacterManager.OnRequestCharacterGeneratorData -= ReturnBackgrounds;
            NewGameController.OnRequestGameData -= ReturnBackgrounds;
            GameManager.OnStartDataLoading -= StartBackgroundLoading;
        }

        private void ReturnBackgrounds() => OnReturnBackgrounds?.Invoke((_childhoodBackgrounds.Values.ToList(), _adulthoodBackgrounds.Values.ToList()));
        
        #region BackgroundsCreation

        private void StartBackgroundLoading()
        {
            StartCoroutine(LoadBackgrounds());
        }

        private IEnumerator LoadBackgrounds()
        {
            var backgroundConfig = Resources.Load<TextAsset>("DataToLoad/backgrounds");
            if (backgroundConfig == null)
            {
                throw new Exception("Background config file could not be loaded. Ensure the file exists in the Resources/DataToLoad/ directory.");
            }
    
            var parsedBackgroundConfigData = JsonUtility.FromJson<BackgroundData>(backgroundConfig.text);
            if (parsedBackgroundConfigData == null)
            {
                throw new Exception("Failed to parse background config data.");
            }

            _backgroundData = new BackgroundData
            {
                ChildhoodBackgroundsConstructors = parsedBackgroundConfigData.ChildhoodBackgroundsConstructors,
                AdulthoodBackgroundsConstructors = parsedBackgroundConfigData.AdulthoodBackgroundsConstructors,
                BackgroundTypes = parsedBackgroundConfigData.BackgroundTypes
            };

            if (_backgroundData.ChildhoodBackgroundsConstructors == null)
            {
                throw new Exception("Childhood backgrounds constructors data is null.");
            }
            foreach (var characterBackground in _backgroundData.ChildhoodBackgroundsConstructors)
            {
                yield return StartCoroutine(CreateBackgroundFromConstructor(characterBackground, _childhoodBackgrounds));
            }

            if (_backgroundData.AdulthoodBackgroundsConstructors == null)
            {
                throw new Exception("Adulthood backgrounds constructors data is null.");
            }
            foreach (var characterBackground in _backgroundData.AdulthoodBackgroundsConstructors)
            {
                yield return StartCoroutine(CreateBackgroundFromConstructor(characterBackground, _adulthoodBackgrounds));
            }
            
            OnCharacterBackgroundControllerInitialized?.Invoke();
            GameManager.OnStartDataLoading -= StartBackgroundLoading;
        }


        private IEnumerator CreateBackgroundFromConstructor(CharacterBackgroundConstructor characterBackgroundConstructor, IDictionary<string, CharacterBackground> characterBackgrounds)
        {
            var newBackground = new CharacterBackground
            {
                BackgroundName = characterBackgroundConstructor.backgroundName,
                BackgroundDescription = characterBackgroundConstructor.backgroundDescription,
                AllowedCulturesForBackground = characterBackgroundConstructor.allowedCulturesForBackground,
                DisallowedCulturesForBackground = characterBackgroundConstructor.disallowedCulturesForBackground
            };
            foreach (var backgroundType in _backgroundData.BackgroundTypes.Where(backgroundType => characterBackgroundConstructor.backgroundTypeName == backgroundType.backgroundTypeName))
            {
                newBackground.BackgroundType = backgroundType;
                break;
            }
            
            yield return StartCoroutine(AssignBackgroundModifiers(newBackground, characterBackgroundConstructor.backgroundEffects));
            characterBackgrounds.Add(newBackground.BackgroundName, newBackground);
            yield return null;
        }

        private static IEnumerator AssignBackgroundModifiers(CharacterBackground background, List<string> backgroundEffectsFromConstructor)
        {
            Action<List<CharacterModification>> assignModifiers = modifierList =>
            {
                background.BackgroundModifiers = modifierList;
            };
            
            CharacterModificationController.OnReturnCharacterModifiers += assignModifiers;
            OnRequestBackgroundEffectsCreation?.Invoke(backgroundEffectsFromConstructor);
            yield return new WaitUntil(() => background.BackgroundModifiers != null);
            CharacterModificationController.OnReturnCharacterModifiers -= assignModifiers;
        }

        #endregion
    }
    public class BackgroundData
    {
        public List<BackgroundType> BackgroundTypes = new();
        public List<CharacterBackgroundConstructor> ChildhoodBackgroundsConstructors = new();
        public List<CharacterBackgroundConstructor> AdulthoodBackgroundsConstructors = new();
    }
}