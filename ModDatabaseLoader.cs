using CaliberSplitAmmoCases.Interfaces;
using System.Text.Json;
using System.Reflection;
using SPTarkov.Server.Core.Helpers;

namespace CaliberSplitAmmoCases
{
    public class ModDatabaseLoader
    {
        private readonly string _modFolder;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public Dictionary<string, CaliberInfo> DbCaliber { get; private set; }
        public Dictionary<string, string> DbItemsIds { get; private set; }
        public Dictionary<string, CaliberInfo> DbCaliberById { get; private set; }

        public ModDatabaseLoader(ModHelper modHelper)
        {
            _modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());

            DbItemsIds = LoadCombinedConfig<Dictionary<string, string>>(Path.Combine(_modFolder, "db", "Ids"));
            DbCaliber = LoadCombinedConfig<Dictionary<string, CaliberInfo>>(Path.Combine(_modFolder, "db", "Calibers"));
            DbCaliberById = DbCaliber.Values.ToDictionary(info => info.Id, info => info);
        }

        public void DbItemsIdsJsonSave()
        {
            string filePath = Path.Combine(_modFolder, "db", "Ids", "idDatabase.json");
            string json = JsonSerializer.Serialize(DbItemsIds, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            File.WriteAllText(filePath, json);
        }

        private T LoadCombinedConfig<T>(string subfolder, Action<T, T> mergeStrategy = null) where T : new()
        {
            var folderPath = Path.Combine(AppContext.BaseDirectory, subfolder);
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            var combinedConfig = new T();

            foreach (var file in Directory.GetFiles(folderPath, "*.json"))
            {
                var json = File.ReadAllText(file);
                var config = JsonSerializer.Deserialize<T>(json, JsonOptions);

                if (mergeStrategy != null)
                    mergeStrategy(combinedConfig, config);
                else
                    MergeDictionaries(combinedConfig, config);
            }

            return combinedConfig;
        }

        private void MergeDictionaries<T>(T target, T source)
        {
            var targetDict = target as System.Collections.IDictionary;
            var sourceDict = source as System.Collections.IDictionary;
            if (targetDict != null && sourceDict != null)
            {
                foreach (var key in sourceDict.Keys)
                {
                    targetDict[key] = sourceDict[key];
                }
            }
        }
    }
}
