using System;
using System.Collections.Generic;
using System.Linq;
using Characters.CharacterBackgrounds;
using Cultures;
using GameScenarios;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Characters
{
    public class CharacterGenerator : MonoBehaviour
    {
        private readonly Dictionary<string, Culture> _cultures = new();
        private readonly Dictionary<string, CharacterBackground> _childhoodBackgrounds = new();
        private readonly Dictionary<string, CharacterBackground> _adulthoodBackgrounds = new();
        
        private List<NamingEntry> _maleNames = new();
        private List<NamingEntry> _femaleNames = new();
        private List<NamingEntry> _surnames = new();
        private List<NamingEntry> _nicknames = new() { new NamingEntry { NamingValue = "Ducky", AppearanceChance = 10 }, new NamingEntry { NamingValue = "The Golden", AppearanceChance = 10 } };
        
        private List<ScenarioModifier> _forcedCharacterElements = new();

        public bool InitializeGenerator(List<Culture> cultures, List<CharacterBackground> childhoodBackgrounds, List<CharacterBackground> adulthoodBackgrounds)
        {
            foreach (var culture in cultures)
            {
                _cultures.Add(culture.cultureName, culture);
            }

            foreach (var childhoodBackground in childhoodBackgrounds)
            {
                _childhoodBackgrounds.Add(childhoodBackground.BackgroundName, childhoodBackground);
            }

            foreach (var adulthoodBackground in adulthoodBackgrounds)
            {
                _adulthoodBackgrounds.Add(adulthoodBackground.BackgroundName, adulthoodBackground);
            }
            return true;
        }
        public Character GenerateCharacter()
        {
            var character = new Character
            {
                characterCulture = GetRandomCharacterCulture(),
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
                ChildhoodBackground = GetRandomCharacterBackground(_childhoodBackgrounds.Values.ToList()),
                AdulthoodBackground = GetRandomCharacterBackground(_adulthoodBackgrounds.Values.ToList())
            };
            LoadCultureNamings(character.characterCulture);
            
            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterName(_surnames);
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            
            return character;
        }

        public Character GenerateCharacter(string cultureName)
        {
            LoadCultureNamings(_cultures.GetValueOrDefault(cultureName));
            
            var character = new Character
            {
                characterCulture = _cultures.GetValueOrDefault(cultureName),
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
                ChildhoodBackground = GetRandomCharacterBackground(_childhoodBackgrounds.Values.ToList()),
                AdulthoodBackground = GetRandomCharacterBackground(_adulthoodBackgrounds.Values.ToList()),
            };
            
            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterName(_surnames);
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            
            return character;
        }
        public Character GenerateCharacter(string cultureName, string backgroundName)
        {
            LoadCultureNamings(_cultures.GetValueOrDefault(cultureName));

            var character = new Character
            {
                characterCulture = _cultures.GetValueOrDefault(cultureName),
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
            };

            if (_childhoodBackgrounds.TryGetValue(backgroundName, out var childhoodBackground))
            {
                character.ChildhoodBackground = childhoodBackground;
                character.AdulthoodBackground = GetRandomCharacterBackground(_adulthoodBackgrounds.Values.ToList());
            }
            else if(_adulthoodBackgrounds.TryGetValue(backgroundName, out var adulthoodBackground))
            {
                character.AdulthoodBackground = adulthoodBackground;
                character.ChildhoodBackground = GetRandomCharacterBackground(_childhoodBackgrounds.Values.ToList());
            }
            
            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterName(_surnames);
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            
            return character;
        }
        public Character GenerateCharacter(string cultureName, string childhoodBackgroundName, string adulthoodBackgroundName)
        {
            LoadCultureNamings(_cultures.GetValueOrDefault(cultureName));

            var character = new Character
            {
                characterCulture = _cultures.GetValueOrDefault(cultureName),
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
                ChildhoodBackground = _childhoodBackgrounds.GetValueOrDefault(childhoodBackgroundName),
                AdulthoodBackground = _adulthoodBackgrounds.GetValueOrDefault(adulthoodBackgroundName),
            };

            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterName(_surnames);
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            
            return character;
        }

        public Character GenerateCharacter(CharacterConstructor characterConstructor)
        {
            var character = new Character
            {
                characterID = characterConstructor.characterID,
                characterName = characterConstructor.characterName,
                characterSurname = characterConstructor.characterSurname,
                characterNickname = characterConstructor.characterNickname,
                characterAge = characterConstructor.characterAge,
                characterGender = characterConstructor.characterGender == "Male" ? CharacterGender.Male : CharacterGender.Female,
            };
            if(characterConstructor.characterCulture == null) character.characterCulture = GetRandomCharacterCulture();
            else if (_cultures.TryGetValue(characterConstructor.characterCulture, out var culture))
            {
                character.characterCulture = culture;
                LoadCultureNamings(culture);
            }
            if(characterConstructor.characterSurname == null) character.characterSurname = GetRandomCharacterName(_surnames);
            if(characterConstructor.childhoodBackground == null) character.ChildhoodBackground = GetRandomCharacterBackground(_childhoodBackgrounds.Values.ToList());
            if(characterConstructor.adulthoodBackground == null) character.AdulthoodBackground = GetRandomCharacterBackground(_adulthoodBackgrounds.Values.ToList());
            if(characterConstructor.characterAge == 0) character.characterAge = GetRandomCharacterAge();
            
            return character;
        }

        private void LoadCultureNamings(Culture culture)
        {
            _maleNames = culture.CultureNamesMale;
            _femaleNames = culture.CultureNamesFemale;
            _surnames = culture.CultureSurnames;
        }

        private Culture GetRandomCharacterCulture()
        {
            var cultureIndex = Random.Range(0, _cultures.Count);
            return _cultures.ElementAt(cultureIndex).Value;
        }

        private static CharacterBackground GetRandomCharacterBackground(IReadOnlyList<CharacterBackground> backgrounds)
        {
            return backgrounds[Random.Range(0, backgrounds.Count)];
        }
        private static int GetRandomCharacterAge()
        {
            return Random.Range(18, 80);
        }
        private static string GetRandomCharacterName(List<NamingEntry> names)
        {
            var totalChance = 0f;
            foreach (var namingEntry in names)
            {
                totalChance += namingEntry.AppearanceChance;
            }
            var randomValue = Random.Range(0f, totalChance);
            var currentChance = 0f;
            foreach (var namingEntry in names)
            {
                currentChance += namingEntry.AppearanceChance;
                if (randomValue <= currentChance)
                {
                    return namingEntry.NamingValue;
                }
            }
            return null;
        }
        private string GetRandomCharacterNickname(int characterAge)
        {
            var chanceForNickname = characterAge switch
            {
                <= 18 => 2.5f,
                <= 30 => 5,
                <= 50 => 10,
                > 50 => 12.5f
            };
            return chanceForNickname > Random.Range(0, 100) ? null : GetRandomCharacterName(_nicknames);
        }
        private static CharacterGender GetRandomCharacterGender() => Random.Range(0, 2) == 0 ? CharacterGender.Male : CharacterGender.Female;

        public Character GenerateCharacterWithModifiers(List<ScenarioModifier> scenarioModifiers)
        {
            _forcedCharacterElements = scenarioModifiers;
            var generatedCharacter = GenerateCharacter();
            ProcessCharacterModifiers(generatedCharacter);
            return generatedCharacter;
        }
        private void ProcessCharacterModifiers(Character generatedCharacter)
        {
            foreach (var scenarioModifier in _forcedCharacterElements)
            {
                VerifyCharacterModifiers(scenarioModifier, generatedCharacter);
            }
        }
        private void VerifyCharacterModifiers(ScenarioModifier modifier, Character generatedCharacter)
        {
            Debug.Log($"Modifier with type {modifier.ModiferType} and value {modifier.Value} has been applied to character {generatedCharacter.characterName}");
            switch (modifier.ModiferType)
            {
                case ScenarioModifiers.ChanceForCharacterBackground:
                    ApplyModifier(modifier.Value, ApplyBackgroundModifier(modifier.StringValue, generatedCharacter));
                    break;
                case ScenarioModifiers.ChanceForCharacterCulture:
                    ApplyModifier(modifier.Value, ApplyCultureModifier(modifier.StringValue, generatedCharacter));
                    break;
                case ScenarioModifiers.ChanceForCharacterTrait:
                    break;
            }
        }
        private void ApplyModifier(int modifierChance, Action modifierAction)
        {
            var isModifierToApply = modifierChance >= 100 || CheckModifierChance(modifierChance);
            
            if (isModifierToApply)
            {
                Debug.Log($"Modifier applied with chance {modifierChance}. Invoking action: {modifierAction.Method.Name}");
                modifierAction.Invoke();
            }
        }

        private Action ApplyBackgroundModifier(string backgroundName, Character generatedCharacter)
        {
            if (_childhoodBackgrounds.ContainsKey(backgroundName))
            {
                generatedCharacter.ChildhoodBackground = _childhoodBackgrounds.GetValueOrDefault(backgroundName);
                Debug.Log($"Forced Character Background: {generatedCharacter.ChildhoodBackground}");
            }
            else if (_adulthoodBackgrounds.ContainsKey(backgroundName))
            {
                generatedCharacter.AdulthoodBackground = _adulthoodBackgrounds.GetValueOrDefault(backgroundName);
                Debug.Log($"Forced Character Background: {generatedCharacter.AdulthoodBackground}");
            }
            return () => { };
        }
        private Action ApplyCultureModifier(string cultureName, Character generatedCharacter)
        {
            if (!_cultures.ContainsKey(cultureName)) return () => { };
            generatedCharacter.characterCulture = _cultures.GetValueOrDefault(cultureName);
            Debug.Log($"Forced Character Culture: {generatedCharacter.characterCulture}");
            return () => { };
        }
        private void ApplyTraitModifier(string traitName, Character generatedCharacter)
        {
            
        }

        private bool CheckModifierChance(int modifierChance)
        {
            var randomValue = Random.Range(0, 100);
            return randomValue <= modifierChance;
        }
    }
}