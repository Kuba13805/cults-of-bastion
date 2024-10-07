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
        public static event Action<List<CharacterModification>> OnReturnCharacterModifiers; 
        private void Awake()
        {
            CharacterBackgroundController.OnRequestBackgroundEffectsCreation += ReturnCharacterModifications;
            CharacterManager.OnRequestCharacterModificationFromModifiers += ModifyCharacter;
        }

        private void OnDestroy()
        {
            CharacterBackgroundController.OnRequestBackgroundEffectsCreation -= ReturnCharacterModifications;
            CharacterManager.OnRequestCharacterModificationFromModifiers -= ModifyCharacter;
        }

        private static void ReturnCharacterModifications(List<string> characterModifiers)
        {
            var modifiers = CreateCharacterModification(characterModifiers);
            OnReturnCharacterModifiers?.Invoke(modifiers);
        }
        
       private static List<CharacterModification> CreateCharacterModification(List<string> characterModifiersDefinition)
        {
            var modifiers = new List<CharacterModification>();

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

                    var newModifier = new CharacterModification
                    {
                        ModificationType = (CharacterModifications)Enum.Parse(typeof(CharacterModifications), splitDefinition[0])
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

        
        private static void ModifyCharacter(Character characterToModify, List<string> characterModifierDefinitions, bool isReverse)
        {
            var characterModifiers = CreateCharacterModification(characterModifierDefinitions);
            
            foreach (var modifier in characterModifiers)
            {
                ApplyModification(characterToModify, modifier, isReverse);
            }
        }

        private static void ModifyCharacter(Character characterToModify, List<CharacterModification> characterModifiers, bool isReverse)
        {
            foreach (var modifier in characterModifiers)
            {
                ApplyModification(characterToModify, modifier, isReverse);
            }
        }

        private static void ApplyModification(Character characterToModify, CharacterModification modification, bool isReverse)
        {
            Debug.Log($"Applying modifier: {modification.ModificationType} with value {modification.Value} and string value {modification.StringValue}.");
            switch (modification.ModificationType)
            {
                case CharacterModifications.ModifyStat:
                    ModifyStat(characterToModify, modification, isReverse);
                    break;
                case CharacterModifications.GiveTrait:
                    break;
                case CharacterModifications.RemoveTrait:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static void ModifyStat(Character characterToModify, CharacterModification modification, bool isReverse)
        {
            var statsFields = characterToModify.CharacterStats.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);

            foreach (var field in statsFields)
            {
                if (!typeof(BaseStat).IsAssignableFrom(field.FieldType)) continue;

                var stat = (BaseStat)field.GetValue(characterToModify.CharacterStats);

                if (field.Name != modification.StringValue) continue;
                Debug.Log($"Stat name: {field.Name} and modification stat name: {modification.StringValue}.");
                Debug.Log("Stat before modification: " + stat.Value);
                if (!isReverse)
                {
                    stat.Value += modification.Value;
                }
                else
                {
                    stat.Value -= modification.Value;
                }
                Debug.Log($"Modified stat: {stat.Name} with value {stat.Value}");
                break;
            }
        }
    }
}