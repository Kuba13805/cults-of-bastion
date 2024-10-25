using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Characters.CharacterBackgrounds;
using NaughtyAttributes;
using PlayerInteractions;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Managers
{
    public class FileManager : MonoBehaviour
    {
        public static FileManager Instance { get; private set; }

        private string _dataPath;
        private IDeserializer _yamlDeserializer;
        private ISerializer _yamlSerializer;

        private DateTime _lastCopyTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        private void OnValidate()
        {
            _dataPath = Path.Combine(Application.persistentDataPath, "GameData");
        }

        private void Start()
        {
            if (!Directory.Exists(_dataPath))
            {
                Directory.CreateDirectory(_dataPath);
            }
            _yamlDeserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            _yamlSerializer = new SerializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
        }
        public List<T> LoadFiles<T>(FileUsage fileUsage)
        {
            switch (fileUsage)
            {
                case FileUsage.SavedGame:
                    return LoadJsonFilesFromPath<T>("SavedGames");
                case FileUsage.NewGame:
                    return LoadJsonFilesFromPath<T>("NewGames");
                case FileUsage.Localization:
                    return LoadYamlFilesFromPath<T>("Localization");
                case FileUsage.Actions:
                    return LoadJsonFilesFromPath<T>("Actions");
                case FileUsage.Backgrounds:
                    return LoadJsonFilesFromPath<T>("Backgrounds");
                case FileUsage.Cultures:
                    return LoadJsonFilesFromPath<T>("Cultures");
                case FileUsage.LocationTypes:
                    return LoadJsonFilesFromPath<T>("Locations");
                case FileUsage.OrganizationTypes:
                    return LoadJsonFilesFromPath<T>("Organizations");
                case FileUsage.Scenarios:
                    return LoadJsonFilesFromPath<T>("Scenarios");
                default:
                    throw new ArgumentOutOfRangeException(nameof(fileUsage), fileUsage, null);
            }
        }

        private List<T> LoadYamlFilesFromPath<T>(string path)
        {
            var fullPath = Path.Combine(_dataPath, path);
            var files = LoadAllYamlFiles<T>(fullPath);
            Debug.Log($"Found {files.Count} files in {fullPath}");
            return files;
        }
        private List<T> LoadJsonFilesFromPath<T>(string path)
        {
            var fullPath = Path.Combine(_dataPath, path);
            var files = LoadAllJsonFiles<T>(fullPath);
            Debug.Log($"Found {files.Count} files in {fullPath}");
            return files;
        }

        private List<T> LoadAllJsonFiles<T>(string path)
        {
            return Directory.GetFiles(path, "*.json", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
                .Where(IsJsonFile)
                .Select(File.ReadAllText)
                .Select(JsonUtility.FromJson<T>)
                .ToList();
        }

        private List<T> LoadAllYamlFiles<T>(string path)
        {
            return Directory.GetFiles(path, "*.yaml", SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(path, "*.txt", SearchOption.AllDirectories))
                .Where(IsYamlFile)
                .Select(File.ReadAllText)
                .Select(yamlContent => _yamlDeserializer.Deserialize<T>(yamlContent))
                .ToList();
        }
        private static bool IsJsonFile(string filePath)
        {
            var content = File.ReadAllText(filePath).Trim();
            return content.StartsWith("{") || content.StartsWith("[");
        }

        private static bool IsYamlFile(string filePath)
        {
            var content = File.ReadAllText(filePath).Trim();
            return content.Contains(":") && !content.StartsWith("{") && !content.StartsWith("[");
        }


        #region CopyAndDeleteFiles_Editor

        [Button]
        public void CopyFilesToGameData()
        {
            var sourceFolder = "Assets/Resources/GameData";
            string destinationFolder = _dataPath;

            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogError($"Source folder does not exist: {sourceFolder}");
                return;
            }

            DirectoryInfo sourceDirectory = new DirectoryInfo(sourceFolder);
            FileInfo[] files = sourceDirectory.GetFiles("*.*", SearchOption.AllDirectories);

            foreach (FileInfo file in files)
            {
                string relativePath = file.FullName.Substring(sourceDirectory.FullName.Length + 1);
                string destinationPath = Path.Combine(destinationFolder, relativePath);

                string destinationDir = Path.GetDirectoryName(destinationPath);
                if (string.IsNullOrWhiteSpace(destinationDir))
                {
                    Debug.LogError($"Destination directory is invalid: {destinationDir}");
                    continue;
                }

                Directory.CreateDirectory(destinationDir);

                File.Copy(file.FullName, destinationPath, true);

                if (file.DirectoryName == sourceDirectory.FullName)
                {
                    string mainFileDestinationPath = Path.Combine(destinationFolder, file.Name);
                    File.Copy(file.FullName, mainFileDestinationPath, true);
                }
            }

            _lastCopyTime = DateTime.UtcNow;
            Debug.Log("Files copied successfully to GameData.");
        }

        [Button]
        public void DeleteOutdatedFiles()
        {
            if (!Directory.Exists(_dataPath))
            {
                Debug.LogError("Data path does not exist.");
                return;
            }

            var sourceFolder = "Assets/Resources/GameData";
            if (!Directory.Exists(sourceFolder))
            {
                Debug.LogError($"Source folder does not exist: {sourceFolder}");
                return;
            }

            var sourceFiles = Directory.GetFiles(sourceFolder, "*.*", SearchOption.AllDirectories)
                .Select(f => Path.GetRelativePath(sourceFolder, f))
                .ToHashSet();

            var filesInDataPath = Directory.GetFiles(_dataPath, "*.*", SearchOption.AllDirectories);

            foreach (var filePath in filesInDataPath)
            {
                string relativePath = Path.GetRelativePath(_dataPath, filePath);

                if (!sourceFiles.Contains(relativePath))
                {
                    File.Delete(filePath);
                    Debug.Log($"Deleted outdated file: {filePath}");
                }
            }

            DeleteEmptyDirectories(_dataPath);
            Debug.Log("Outdated files and empty directories deleted successfully.");
        }

        private void DeleteEmptyDirectories(string startLocation)
        {
            foreach (var directory in Directory.GetDirectories(startLocation))
            {
                DeleteEmptyDirectories(directory);

                if (!Directory.EnumerateFileSystemEntries(directory).Any())
                {
                    Directory.Delete(directory);
                    Debug.Log($"Deleted empty directory: {directory}");
                }
            }
        }

        #endregion
        public enum FileUsage
        {
            SavedGame,
            NewGame,
            Localization,
            Actions,
            Backgrounds,
            Cultures,
            LocationTypes,
            OrganizationTypes,
            Scenarios,
        }
    }
}
