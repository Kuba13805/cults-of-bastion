using System.Collections.Generic;
using System.Linq;
using Characters.CharacterBackgrounds;
using Cultures;
using UnityEngine;

namespace Characters
{
    public class CharacterGenerator : MonoBehaviour
    {
        private readonly Dictionary<string, Culture> _cultures = new();
        private readonly Dictionary<string, CharacterBackground> _childhoodBackgrounds = new();
        private readonly Dictionary<string, CharacterBackground> _adulthoodBackgrounds = new();
        
        private List<NamingEntry> _maleNames;
        private List<NamingEntry> _femaleNames;
        private List<NamingEntry> _surnames;
        private List<NamingEntry> _nicknames = new() { new NamingEntry { NamingValue = "Ducky", AppearanceChance = 10 }, new NamingEntry { NamingValue = "The Golden", AppearanceChance = 10 } };

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
                characterCulture = _cultures.GetValueOrDefault(characterConstructor.characterCulture),
                characterGender = characterConstructor.characterGender == "Male" ? CharacterGender.Male : CharacterGender.Female,
                ChildhoodBackground = _childhoodBackgrounds.GetValueOrDefault(characterConstructor.childhoodBackground),
                AdulthoodBackground = _adulthoodBackgrounds.GetValueOrDefault(characterConstructor.adulthoodBackground)
            };
            
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
            var cultureIndex = UnityEngine.Random.Range(0, _cultures.Count);
            return _cultures.ElementAt(cultureIndex).Value;
        }

        private static CharacterBackground GetRandomCharacterBackground(IReadOnlyList<CharacterBackground> backgrounds)
        {
            return backgrounds[UnityEngine.Random.Range(0, backgrounds.Count)];
        }
        private static int GetRandomCharacterAge()
        {
            return UnityEngine.Random.Range(18, 80);
        }
        private static string GetRandomCharacterName(List<NamingEntry> names)
        {
            var totalChance = 0f;
            foreach (var namingEntry in names)
            {
                totalChance += namingEntry.AppearanceChance;
            }
            var randomValue = UnityEngine.Random.Range(0f, totalChance);
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
            return chanceForNickname > UnityEngine.Random.Range(0, 100) ? null : GetRandomCharacterName(_nicknames);
        }
        private static CharacterGender GetRandomCharacterGender() => UnityEngine.Random.Range(0, 2) == 0 ? CharacterGender.Male : CharacterGender.Female;
    }
}