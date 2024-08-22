using System;
using System.Collections.Generic;
using System.Reflection;
using Characters.CharacterBackgrounds;
using Managers;
using UnityEngine;

namespace Characters
{
    public class CharacterModificationController : MonoBehaviour
    {
        public static event Action<List<CharacterModifier>> OnReturnCharacterModifiers; 
        private void Awake()
        {
            CharacterBackgroundController.OnRequestBackgroundEffectsCreation += ReturnCharacterModifiers;
            CharacterManager.OnRequestCharacterModificationFromModifiers += ModifyCharacter;
        }

        private void OnDestroy()
        {
            CharacterBackgroundController.OnRequestBackgroundEffectsCreation -= ReturnCharacterModifiers;
            CharacterManager.OnRequestCharacterModificationFromModifiers -= ModifyCharacter;
        }

        private static void ReturnCharacterModifiers(List<string> characterModifiers)
        {
            var modifiers = CreateCharacterModifier(characterModifiers);
            OnReturnCharacterModifiers?.Invoke(modifiers);
        }
        
       private static List<CharacterModifier> CreateCharacterModifier(List<string> characterModifiersDefinition)
        {
            var modifiers = new List<CharacterModifier>();

            foreach (var definition in characterModifiersDefinition)
            {
                try
                {
                    var splitDefinition = definition.Split(new[] { ' ', '=' }, StringSplitOptions.RemoveEmptyEntries);

                    if (splitDefinition.Length < 2)
                    {
                        Debug.LogError($"Invalid format in definition: {definition}");
                        continue;
                    }

                    var newModifier = new CharacterModifier
                    {
                        ModifierType = (CharacterModifiers)Enum.Parse(typeof(CharacterModifiers), splitDefinition[0])
                    };

                    if (int.TryParse(splitDefinition[1], out var value))
                    {
                        newModifier.Value = value;
                    }
                    else
                    {
                        newModifier.StringValue = splitDefinition[1];

                        if (splitDefinition.Length > 2 && int.TryParse(splitDefinition[2], out var intValue))
                        {
                            newModifier.Value = intValue;
                        }
                        else
                        {
                            newModifier.Value = 0;
                            Debug.LogWarning($"Value not provided or invalid in definition: {definition}, defaulting to 0.");
                        }
                    }

                    modifiers.Add(newModifier);
                    Debug.Log($"New modifier created: {newModifier.ModifierType} with value {newModifier.Value} and string value {newModifier.StringValue}");
                }
                catch (FormatException ex)
                {
                    Debug.LogError($"FormatException encountered while processing definition: {definition}. Error: {ex.Message}");
                }
                catch (IndexOutOfRangeException ex)
                {
                    Debug.LogError($"IndexOutOfRangeException encountered while processing definition: {definition}. Error: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Unexpected exception encountered while processing definition: {definition}. Error: {ex.Message}");
                }
            }

            return modifiers;
        }

        
        private static void ModifyCharacter(Character characterToModify, List<string> characterModifierDefinitions)
        {
            var characterModifiers = CreateCharacterModifier(characterModifierDefinitions);
            
            foreach (var modifier in characterModifiers)
            {
                ApplyModifier(characterToModify, modifier);
            }
        }

        private static void ModifyCharacter(Character characterToModify, List<CharacterModifier> characterModifiers)
        {
            foreach (var modifier in characterModifiers)
            {
                ApplyModifier(characterToModify, modifier);
            }
        }

        private static void ApplyModifier(Character characterToModify, CharacterModifier modifier)
        {
            Debug.Log($"Applying modifier: {modifier.ModifierType} with value {modifier.Value} and string value {modifier.StringValue}.");
            switch (modifier.ModifierType)
            {
                case CharacterModifiers.ModifyStat:
                    ModifyStat(characterToModify, modifier);
                    break;
                case CharacterModifiers.GiveTrait:
                    break;
                case CharacterModifiers.RemoveTrait:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ModifyStat(Character characterToModify, CharacterModifier modifier)
        {
            var statsFields = characterToModify.CharacterStats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in statsFields)
            {
                if (!typeof(BaseStat).IsAssignableFrom(field.FieldType)) continue;

                var stat = (BaseStat)field.GetValue(characterToModify.CharacterStats);

                if (field.Name != modifier.StringValue) continue;
                Debug.Log($"Stat name: {field.Name} and modification stat name: {modifier.StringValue}.");
                Debug.Log("Stat before modification: " + stat.Value);
                stat.Value += modifier.Value;
                Debug.Log($"Modified stat: {stat.Name} with value {stat.Value}");
                break;
            }
        }
    }
}