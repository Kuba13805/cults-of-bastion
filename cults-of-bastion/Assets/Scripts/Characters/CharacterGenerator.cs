using System.Collections.Generic;

namespace Characters
{
    public class CharacterGenerator
    {
        private List<string> _maleNames = new() {"Earl", "John", "Milton", "Isiah", "Oscar", "Charles"};
        private List<string> _femaleNames = new() {"Anne", "Johanne", "Kate", "Liv", "Mona", "Sarah"};
        private List<string> _surnames = new() { "Johnson", "Smith", "Williams", "Jones" };
        private List<string> _nicknames = new() { "Jr.", "Sr.", "Jnr.", "Hawk" };
        public  Character GenerateCharacter()
        {
            var character = new Character
            {
                characterGender = GetRandomCharacterGender(),
                characterAge = GetRandomCharacterAge(),
            };
            character.characterName = GetRandomCharacterName(character.characterGender == CharacterGender.Male ? _maleNames : _femaleNames);
            character.characterSurname = GetRandomCharacterSurname();
            character.characterNickname = GetRandomCharacterNickname(character.characterAge);
            return character;
        }
        private int GetRandomCharacterAge()
        {
            return UnityEngine.Random.Range(8, 80);
        }
        private string GetRandomCharacterName(IReadOnlyList<string> names)
        {
            return names[UnityEngine.Random.Range(0, names.Count)];
        }
        private string GetRandomCharacterSurname()
        {
            return _surnames[UnityEngine.Random.Range(0, _surnames.Count)];
        }
        private string GetRandomCharacterNickname(int characterAge)
        {
            var chanceForNickname = characterAge switch
            {
                <= 18 => 10,
                <= 30 => 15,
                <= 50 => 20,
                > 50 => 25
            };

            return chanceForNickname > UnityEngine.Random.Range(0, 100) ? null : _nicknames[UnityEngine.Random.Range(0, _nicknames.Count)];
        }
        private CharacterGender GetRandomCharacterGender()
        {
            return UnityEngine.Random.Range(0, 2) == 0 ? CharacterGender.Male : CharacterGender.Female;
        }
    }
}