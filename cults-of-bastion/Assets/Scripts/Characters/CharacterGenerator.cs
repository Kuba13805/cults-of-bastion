using System.Collections.Generic;
using Cultures;

namespace Characters
{
    public class CharacterGenerator
    {
        private readonly List<NamingEntry> _maleNames;
        private readonly List<NamingEntry> _femaleNames;
        private readonly List<NamingEntry> _surnames;
        private readonly List<NamingEntry> _nicknames = new() { new NamingEntry { NamingValue = "Ducky", AppearanceChance = 10 }, new NamingEntry { NamingValue = "The Golden", AppearanceChance = 10 } };

        public CharacterGenerator(Culture culture)
        {
            _maleNames = culture.CultureNamesMale;
            _femaleNames = culture.CultureNamesFemale;
            _surnames = culture.CultureSurnames;
        }
        public  Character GenerateCharacter()
        {
            var character = new Character
            {
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
            };
            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterName(_surnames);
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            character.CharacterStats.Strength.Value = UnityEngine.Random.Range(10, 20);
            return character;
        }
        private static int GetRandomCharacterAge()
        {
            return UnityEngine.Random.Range(18, 80);
        }
        private string GetRandomCharacterName(List<NamingEntry> names)
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
        private static CharacterGender GetRandomCharacterGender()
        {
            return UnityEngine.Random.Range(0, 2) == 0 ? CharacterGender.Male : CharacterGender.Female;
        }
    }
}