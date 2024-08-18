using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Managers;
using UnityEngine;

namespace Cultures
{
    public class CultureController : MonoBehaviour
    {
        private Dictionary<string, Culture> _cultures = new();
        private CultureData _cultureData;
        
        public static event Action<Culture> OnReturnRequestedCulture;
        private void Awake()
        {
            StartCoroutine(LoadCultureData());
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            CharacterManager.OnRequestCultureAssignment += ReturnRequestedCulture;
            CharacterManager.OnRequestRandomCultureAssignment += ReturnRequestedRandomCulture;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            CharacterManager.OnRequestCultureAssignment -= ReturnRequestedCulture;
            CharacterManager.OnRequestRandomCultureAssignment -= ReturnRequestedRandomCulture;
        }

        private void ReturnRequestedCulture(string cultureName) => OnReturnRequestedCulture?.Invoke(_cultures[cultureName]);

        private void ReturnRequestedRandomCulture()
        {
            var cultureIndex = UnityEngine.Random.Range(0, _cultures.Count);
            OnReturnRequestedCulture?.Invoke(_cultures.Values.ElementAt(cultureIndex));;
        }

        private IEnumerator LoadCultureData()
        {
            var cultureDataConfig = Resources.Load<TextAsset>("DataToLoad/cultures");
            
            var parsedCultureData = JsonUtility.FromJson<CultureData>(cultureDataConfig.text);

            _cultureData = new CultureData
            {
                CultureConstructors = parsedCultureData.CultureConstructors
            };

            foreach (var constructor in _cultureData.CultureConstructors)
            {
                yield return StartCoroutine(CreateCultureFromConstructor(constructor));
            }
        }

        private IEnumerator CreateCultureFromConstructor(CultureConstructor cultureConstructor)
        {
            var newCulture = new Culture()
            {
                cultureName = cultureConstructor.cultureName,
                cultureDescription = cultureConstructor.cultureDescription
            };

            yield return StartCoroutine(LoadCultureNames(cultureConstructor.cultureNamesMale, result =>
            {
                if (result != null)
                    newCulture.CultureNamesMale = result;
            }));

            yield return StartCoroutine(LoadCultureNames(cultureConstructor.cultureNamesFemale, result =>
            {
                if (result != null)
                    newCulture.CultureNamesFemale = result;
            }));

            yield return StartCoroutine(LoadCultureNames(cultureConstructor.cultureSurnames, result =>
            {
                if (result != null)
                    newCulture.CultureSurnames = result;
            }));

            if (newCulture.CultureNamesMale != null && newCulture.CultureNamesFemale != null && newCulture.CultureSurnames != null)
            {
                _cultures.Add(newCulture.cultureName, newCulture);
                Debug.Log($"New culture created: {cultureConstructor.cultureName} with {newCulture.CultureNamesMale.Count} male names and {newCulture.CultureNamesFemale.Count} female names and {newCulture.CultureSurnames.Count} surnames.");
            }
            else
            {
                Debug.LogError($"Failed to create culture: {cultureConstructor.cultureName} due to invalid data.");
            }
        }

        private static IEnumerator LoadCultureNames(List<string> stringList, Action<List<NamingEntry>> callback)
        {
            var namingList = new List<NamingEntry>();
            foreach (var entry in stringList)
            {
                var parsedStringEntry = entry.Split(" ");
                if (parsedStringEntry.Length != 2 || !float.TryParse(parsedStringEntry[1], out var appearanceChance))
                {
                    Debug.LogError($"Invalid data format for entry: {entry}. Expected format: 'Name Chance'.");
                    callback(null);
                    yield break;
                }

                var newNamingEntry = new NamingEntry
                {
                    NamingValue = parsedStringEntry[0],
                    AppearanceChance = appearanceChance
                };
                namingList.Add(newNamingEntry);
            }

            callback(namingList);
            yield return null;
        }

        private class CultureData
        {
            public List<CultureConstructor> CultureConstructors = new();
        }
    }
}