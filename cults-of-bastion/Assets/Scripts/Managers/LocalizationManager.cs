using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;
using UnityEngine.ResourceManagement.AsyncOperations;
using NaughtyAttributes;
using UnityEditor;
using UnityEditor.Localization;
using UnityEngine.AddressableAssets;

namespace Managers
{
    public class LocalizationManager : MonoBehaviour, ITableProvider
    {
        [SerializeField] private GameLanguage chosenGameLanguage;

        public static event Action<string> OnSendLocalizedString;
        private void Start()
        {
            TestLocalizedText.OnGetLocalizedString += LocalizeText;
        }

        private void OnDestroy()
        {
            TestLocalizedText.OnGetLocalizedString -= LocalizeText;
        }

        private void LocalizeText(string obj)
        {
            var localizedString = LocalizationSettings.StringDatabase.GetTableEntry("Test_table", obj);

            OnSendLocalizedString?.Invoke(localizedString.Entry.GetLocalizedString());
        }

        [Button]
        public void TestStart()
        {
            LoadLocalizationFiles();
        }

        [Button]
        public void TestClear()
        {
            StartCoroutine(ClearAllLocalizationTables());
        }

        private void LoadLocalizationFiles()
        {
            string languageFolder = chosenGameLanguage == GameLanguage.English ? "en" : "pl";
            string localizationFolderPath = Path.Combine("Localizations", languageFolder);

            Debug.Log($"Loading localization files from folder: {localizationFolderPath}");

            var localizationFiles = FileManager.Instance.LoadYamlFilesFromPath<Dictionary<string, Dictionary<string, string>>>(localizationFolderPath);

            foreach (var fileContent in localizationFiles)
            {
                StartCoroutine(LoadLocalizationToAsset(fileContent));
            }
        }

        private IEnumerator LoadLocalizationToAsset(Dictionary<string, Dictionary<string, string>> localizationData)
        {
            var patcher = new CustomTablePatcher();

            foreach (var tableNamespace in localizationData)
            {
                yield return StartCoroutine(AddStringTable(tableNamespace.Key, table =>
                {
                    foreach (var entry in tableNamespace.Value)
                    {
                        AddStringEntry(table, entry.Key, entry.Value);
                    }
                    
                    patcher.PostprocessTable(table);
                }));
            }
        }


        private IEnumerator AddStringTable(string tableName, Action<StringTable> onTableLoaded)
        {
            if (LocalizationSettings.StringDatabase == null)
            {
                Debug.LogError("LocalizationSettings.StringDatabase is not initialized.");
                yield break;
            }

            var existingTable = LocalizationSettings.StringDatabase.GetTable(tableName);
            if (existingTable != null)
            {
                onTableLoaded?.Invoke(existingTable);
                yield break;
            }

            var tableOperation = ProvideTableAsync<StringTable>(tableName, LocalizationSettings.SelectedLocale);
            yield return tableOperation;

            if (tableOperation.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log(tableOperation.Result.name);
                onTableLoaded?.Invoke(tableOperation.Result);
            }
            else
            {
                Debug.LogError($"Failed to load or create table '{tableName}' for locale {LocalizationSettings.SelectedLocale.LocaleName}");
            }

            Addressables.Release(tableOperation);
        }


        private void AddStringEntry(StringTable table, string key, string value)
        {
            if (table.GetEntry(key) == null)
            {
                table.AddEntry(key, value);
                if (IsSmartString(table.GetEntry(key).Value))
                {
                    table.GetEntry(key).IsSmart = true;
                }
                Debug.Log($"Added entry: Key='{key}', Value='{value}' to table '{table.name}'");
            }
            else
            {
                Debug.LogWarning($"Key '{key}' already exists in table '{table.name}', skipping...");
            }
        }

        private static bool IsSmartString(string value)
        {
            const string pattern = @"\{(\d+)(?::(plural|select):.*?(\|.*?)+)?\}";
            return Regex.IsMatch(value, pattern);
        }

        [Button]
        public void ChangeLanguage()
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[(int)chosenGameLanguage];
            LoadLocalizationFiles();
        }

        [Button]
        public void GetTestMessage()
        {
            var paramsString = new object[] { chosenGameLanguage.ToString(), 64 };
            var localizedString = LocalizationSettings.StringDatabase.GetLocalizedString("Test_table", "LanguageChange", paramsString);
            Debug.Log($"{localizedString}");
        }

        private static IEnumerator ClearAllLocalizationTables()
        {
            AsyncOperationHandle<IList<StringTable>> handle = LocalizationSettings.StringDatabase.GetAllTables();
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                foreach (StringTable table in handle.Result)
                {
                    foreach (var tableEntry in new List<TableEntry>(table.Values))
                    {
                        table.RemoveEntry(((StringTableEntry)tableEntry).KeyId);
                    }
                    Debug.Log($"Cleared all entries from table: {table.TableCollectionName}");
                    table.SharedData.Clear();
                }
            }
            else
            {
                Debug.LogError("Failed to load localization tables.");
            }
        }

        public void OnApplicationQuit()
        {
            StartCoroutine(ClearAllLocalizationTables());
        }

        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale) where TTable : LocalizationTable
        {
            var provider = new CustomTableProvider();
            return provider.ProvideTableAsync<TTable>(tableCollectionName, locale);
        }
    }

    public enum GameLanguage
    {
        English,
        Polish
    }

    [Serializable]
    public class CustomTableProvider : ITableProvider
    {
        public AsyncOperationHandle<TTable> ProvideTableAsync<TTable>(string tableCollectionName, Locale locale) where TTable : LocalizationTable
        {
            if (typeof(TTable) == typeof(StringTable))
            {
                var table = ScriptableObject.CreateInstance<StringTable>();
                table.name = $"{tableCollectionName}";
                table.SharedData = ScriptableObject.CreateInstance<SharedTableData>();
                table.SharedData.name = $"{tableCollectionName}SharedData";
                table.SharedData.TableCollectionName = tableCollectionName;
                table.LocaleIdentifier = locale.Identifier;
                

                table.AddEntry("MyEntry1", "My localized value 1");
                table.AddEntry("MyEntry2", "My localized value 2");

                Debug.Log($"Table with name '{table.name}' created for collection '{tableCollectionName}' and locale '{locale.Identifier} with shared data name '{table.SharedData.name}'");
                return Addressables.ResourceManager.CreateCompletedOperation(table as TTable, null);
            }

            Debug.LogWarning($"Table '{tableCollectionName}' not found for locale '{locale.Identifier}'");
            return default;
        }

    }

    [Serializable]
    public class CustomTablePatcher : ITablePostprocessor
    {
        public void PostprocessTable(LocalizationTable table)
        {
            if (table is StringTable stringTable)
            {
                stringTable.AddEntry("some new entry", "localized value");

                var entry = stringTable.GetEntry("some existing value");
                if (entry != null)
                {
                    entry.Value = "updated localized value";
                }
            }
        }
    }

    public static class AssignCustomTablePatcherExample
    {
        [MenuItem("Localization Samples/Assign Custom table postprocessor")]
        public static void AssignTablePostprocessor()
        {
            var provider = new CustomTablePatcher();
            var settings = LocalizationEditorSettings.ActiveLocalizationSettings;

            settings.GetStringDatabase().TablePostprocessor = provider;
            settings.GetAssetDatabase().TablePostprocessor = provider;

            EditorUtility.SetDirty(settings);
            Debug.Log("CustomTablePatcher has been assigned as the table postprocessor.");
        }
    }
}
