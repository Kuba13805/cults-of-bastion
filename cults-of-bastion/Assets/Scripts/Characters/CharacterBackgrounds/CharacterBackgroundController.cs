using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Characters.CharacterBackgrounds
{
    public class CharacterBackgroundController : MonoBehaviour
    {
        private Dictionary<string, CharacterBackground> _childhoodBackgrounds;
        private Dictionary<string, CharacterBackground> _adulthoodBackgrounds;
        private BackgroundData _backgroundData;
        
        private IEnumerator LoadBackgrounds()
        {
            var backgroundConfig = Resources.Load<TextAsset>("DataToLoad/backgrounds");
            
            var parsedBackgroundConfigData = JsonUtility.FromJson<BackgroundData>(backgroundConfig.text);
            if (parsedBackgroundConfigData == null)
            {
                throw new Exception("Failed to parse background config data.");
            }

            _backgroundData = new BackgroundData()
            {
                ChildhoodBackgroundsConstructors = parsedBackgroundConfigData.ChildhoodBackgroundsConstructors,
                AdultBackgroundsConstructors = parsedBackgroundConfigData.AdultBackgroundsConstructors,
                BackgroundTypes = parsedBackgroundConfigData.BackgroundTypes
            };
            foreach (var characterBackground in _backgroundData.ChildhoodBackgroundsConstructors)
            {
                yield return StartCoroutine(CreateBackgroundFromConstructor(characterBackground, _childhoodBackgrounds));
            }
            foreach (var characterBackground in _backgroundData.AdultBackgroundsConstructors)
            {
                yield return StartCoroutine(CreateBackgroundFromConstructor(characterBackground, _adulthoodBackgrounds));
            }
        }

        private IEnumerator CreateBackgroundFromConstructor(CharacterBackgroundConstructor characterBackgroundConstructor, Dictionary<string, CharacterBackground> characterBackgrounds)
        {
            var newBackground = new CharacterBackground
            {
                BackgroundName = characterBackgroundConstructor.BackgroundName,
                BackgroundDescription = characterBackgroundConstructor.BackgroundDescription,
            };
            foreach (var backgroundType in _backgroundData.BackgroundTypes.Where(backgroundType => characterBackgroundConstructor.BackgroundTypeName == backgroundType.BackgroundTypeName))
            {
                newBackground.BackgroundType = backgroundType;
                break;
            }
            characterBackgrounds.Add(newBackground.BackgroundName, newBackground);
            yield return null;
        }
    }
    public class BackgroundData
    {
        public List<BackgroundType> BackgroundTypes;
        public List<CharacterBackgroundConstructor> ChildhoodBackgroundsConstructors;
        public List<CharacterBackgroundConstructor> AdultBackgroundsConstructors;
    }
}