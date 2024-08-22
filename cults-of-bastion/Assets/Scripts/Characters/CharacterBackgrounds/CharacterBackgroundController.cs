using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.CharacterBackgrounds
{
    public class CharacterBackgroundController : MonoBehaviour
    {
        private Dictionary<string, CharacterBackground> _childhoodBackgrounds = new();
        private Dictionary<string, CharacterBackground> _adulthoodBackgrounds = new();
        private BackgroundData _backgroundData;

        public static event Action<List<string>> OnRequestBackgroundEffectsCreation; 
        private void Awake()
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
        }


        private IEnumerator CreateBackgroundFromConstructor(CharacterBackgroundConstructor characterBackgroundConstructor, Dictionary<string, CharacterBackground> characterBackgrounds)
        {
            var newBackground = new CharacterBackground
            {
                BackgroundName = characterBackgroundConstructor.backgroundName,
                BackgroundDescription = characterBackgroundConstructor.backgroundDescription,
            };
            foreach (var backgroundType in _backgroundData.BackgroundTypes.Where(backgroundType => characterBackgroundConstructor.backgroundTypeName == backgroundType.backgroundTypeName))
            {
                newBackground.BackgroundType = backgroundType;
                break;
            }
            
            yield return StartCoroutine(AssignBackgroundModifiers(newBackground, characterBackgroundConstructor.backgroundEffects));
            characterBackgrounds.Add(newBackground.BackgroundName, newBackground);
            Debug.Log($"New background created: {newBackground.BackgroundName} with type: {newBackground.BackgroundType.backgroundTypeName} and modifiers count: {newBackground.BackgroundModifiers.Count}");
            yield return null;
        }

        private static IEnumerator AssignBackgroundModifiers(CharacterBackground background, List<string> backgroundEffectsFromConstructor)
        {
            Action<List<CharacterModifier>> assignModifiers = modifierList =>
            {
                background.BackgroundModifiers = modifierList;
            };
            
            CharacterModificationController.OnReturnCharacterModifiers += assignModifiers;
            OnRequestBackgroundEffectsCreation?.Invoke(backgroundEffectsFromConstructor);
            yield return new WaitUntil(() => background.BackgroundModifiers != null);
            CharacterModificationController.OnReturnCharacterModifiers -= assignModifiers;
        }
    }
    public class BackgroundData
    {
        public List<BackgroundType> BackgroundTypes = new();
        public List<CharacterBackgroundConstructor> ChildhoodBackgroundsConstructors = new();
        public List<CharacterBackgroundConstructor> AdulthoodBackgroundsConstructors = new();
    }
}