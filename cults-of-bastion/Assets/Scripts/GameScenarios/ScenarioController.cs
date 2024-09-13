using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NewGame;
using UI.MainMenu.NewGameMenu;
using UnityEngine;

namespace GameScenarios
{
    public class ScenarioController : MonoBehaviour
    {
        #region Variables

        private ScenariosData _scenariosData;
        private readonly List<Scenario> _gameScenarios = new();
        private Scenario _currentScenario;

        #endregion

        #region Events

        public static event Action<List<Scenario>> OnPassScenarios;

        #endregion
        private void Awake()
        {
            LoadScenarios();
            StartCoroutine(GenerateScenarios());
        }

        private void Start()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            NewGameController.OnRequestGameScenarios += PassGameScenarios;
        }

        private void OnDestroy()
        {
            UnsubscribeFromEvents();
        }

        private void UnsubscribeFromEvents()
        {
            NewGameController.OnRequestGameScenarios -= PassGameScenarios;
        }

        #region ScenarioGeneration

        private void LoadScenarios()
        {
            var scenariosConfig = Resources.Load<TextAsset>("DataToLoad/scenarios");
            if (scenariosConfig == null) return;
            
            var parsedScenariosConfigData = JsonUtility.FromJson<ScenariosData>(scenariosConfig.text);
            if (parsedScenariosConfigData == null)
            {
                throw new Exception("Failed to parse scenarios config data.");
            }

            _scenariosData = new ScenariosData
            {
                ScenarioConstructors = parsedScenariosConfigData.ScenarioConstructors
            };
        }

        private IEnumerator GenerateScenarios()
        {
            foreach (var scenarioConstructor in _scenariosData.ScenarioConstructors)
            {
                var newScenario = new Scenario
                {
                    ScenarioName = scenarioConstructor.scenarioName,
                    ScenarioDescription = scenarioConstructor.scenarioDescription,
                    ScenarioStartingCharacterNumber = scenarioConstructor.scenarioStartingCharacterNumber
                };
                foreach (var newModifier in scenarioConstructor.scenarioModifiers.Select(CreateScenarioModifier))
                {
                    newScenario.ScenarioModifiers.Add(newModifier);
                }
                _gameScenarios.Add(newScenario);
                Debug.Log($"Scenario {newScenario.ScenarioName} created with {newScenario.ScenarioModifiers.Count} modifiers.");
            }
            yield return null;
        }

        #endregion

        #region ScenarioPassing

        private void PassGameScenarios() => OnPassScenarios?.Invoke(_gameScenarios);

        private void SetCurrentScenario(Scenario chosenScenario) => _currentScenario = chosenScenario;

        #endregion

        #region ModifierCreation
        private static ScenarioModifier CreateScenarioModifier(string modifierDefinition)
        {
            var definitions = modifierDefinition
                .Split(new[] { '=', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(def => def.Trim())
                .ToArray();
            
            if (definitions.Length < 2) 
            {
                Console.WriteLine($"Error: Invalid modifier definition '{modifierDefinition}'");
                return null;
            }

            var modifier = new ScenarioModifier();
            
            if (!Enum.TryParse<ScenarioModifiers>(definitions[0], out var scenarioModifier))
            {
                Console.WriteLine($"Error: Invalid scenario modifier type '{definitions[0]}'");
                return null;
            }

            modifier.ModiferType = scenarioModifier;

            try
            {
                modifier = scenarioModifier switch
                {
                    ScenarioModifiers.OrganizationExists => 
                        bool.TryParse(definitions[1], out var boolValue) 
                            ? CreateModifier(boolValue) 
                            : throw new ArgumentException($"Invalid boolean value for OrganizationExists: '{definitions[1]}'"),

                    ScenarioModifiers.TypeOfOrganization => 
                        CreateModifier(definitions[1]),

                    ScenarioModifiers.ChanceForCharacterBackground => 
                        CreateModifier(definitions[1], int.Parse(definitions[2]), int.Parse(definitions[3])),

                    ScenarioModifiers.StartingOwnedBuilding => 
                        CreateModifier(definitions[1], int.Parse(definitions[2])),

                    ScenarioModifiers.ChanceForCharacterCulture => 
                        CreateModifier(definitions[1], int.Parse(definitions[2]), int.Parse(definitions[3])),

                    ScenarioModifiers.ChanceForCharacterTrait => 
                        CreateModifier(definitions[1], int.Parse(definitions[2]), int.Parse(definitions[3])),

                    ScenarioModifiers.StartingResources => 
                        CreateModifier(definitions[1], int.Parse(definitions[2])),

                    ScenarioModifiers.StartingQuestline => 
                        CreateModifier(definitions[1]),

                    ScenarioModifiers.StartingEvent => 
                        CreateModifier(definitions[1]),

                    _ => throw new ArgumentOutOfRangeException($"Invalid scenario modifier type: {scenarioModifier}")
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error while processing modifier: {ex.Message}");
                return null;
            }

            Debug.Log($"Created modifier: {modifier.ModiferType} with value: {modifier.Value} and string value: {modifier.StringValue} and bool value: {modifier.BoolValue}");
            return modifier;
        }

        /// <summary>
        /// /Create boolean scenario modifier
        /// </summary>
        /// <param name="booleanValue"></param>
        /// <returns></returns>
        private static ScenarioModifier CreateModifier(bool booleanValue)
        {
            var modifier = new ScenarioModifier
            {
                BoolValue = booleanValue
            };
            return modifier;
        }
        /// <summary>
        /// Create string scenario modifier
        /// </summary>
        /// <param name="stringValue"></param>
        /// <returns></returns>
        private static ScenarioModifier CreateModifier(string stringValue)
        {
            var modifier = new ScenarioModifier
            {
                StringValue = stringValue
            };
            return modifier;
        }
        /// <summary>
        /// Create string and int scenario modifier.
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="intValue"></param>
        /// <returns></returns>
        private static ScenarioModifier CreateModifier(string stringValue, int intValue)
        {
            var modifier = new ScenarioModifier
            {
                Value = intValue,
                StringValue = stringValue
            };
            return modifier;
        }
        /// <summary>
        /// Create string, int and character number scenario modifier
        /// </summary>
        /// <param name="stringValue"></param>
        /// <param name="intValue"></param>
        /// <param name="characterNumber"></param>
        /// <returns></returns>
        private static ScenarioModifier CreateModifier(string stringValue, int intValue, int characterNumber)
        {
            var modifier = new ScenarioModifier
            {
                Value = intValue,
                StringValue = stringValue,
                NumberOfCharactersAffected = characterNumber
            };
            return modifier;
        }

        #endregion
    }

    public class ScenariosData
    {
        public List<ScenarioConstructor> ScenarioConstructors;
    }
}