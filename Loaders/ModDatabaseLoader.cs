using CaliberSplitAmmoCases.Interfaces;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace CaliberSplitAmmoCases.Loaders
{
    public class ModDatabaseLoader
    {
        private readonly string modFolder;
        private readonly JsonUtil _jsonutil;
        private readonly ISptLogger<CaliberSplitAmmoCases> _logger;
        private readonly ModHelper _modHelper;

        public Dictionary<string, CaliberInfo> DbCaliber { get; private set; }
        public Dictionary<string, string> DbItemsIds { get; private set; }
        public Dictionary<string, CaliberInfo> DbCaliberById { get; private set; }

        public ModDatabaseLoader(ISptLogger<CaliberSplitAmmoCases> logger, ModHelper modHelper, JsonUtil jsonUtil)
        {
            modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            _jsonutil = jsonUtil;
            _logger = logger;
            _modHelper = modHelper;

            DbCaliber = LoadDbCaliber(Path.Combine(modFolder, "db", "Calibers"), jsonUtil, logger);
            DbItemsIds = LoadOrCreateJSON(Path.Combine(modFolder, "db", "Ids", "idDatabase.json"), jsonUtil, logger);
            DbCaliberById = DbCaliber.Values.ToDictionary(info => info.Id, info => info);
        }

        public void DbItemsIdsJsonSave()
        {
            string filePath = Path.Combine(modFolder, "db", "Ids", "idDatabase.json");
            string json = _jsonutil.Serialize(DbItemsIds);
            File.WriteAllText(filePath, json);
            _logger.LogWithColor($"[CaliberSplitAmmoCases] File db/Ids/idDatabase.json has been changed", LogTextColor.Green);
        }

        public Dictionary<string, CaliberInfo> LoadDbCaliber(string directoryPath, JsonUtil jsonUtil, ISptLogger<CaliberSplitAmmoCases> logger)
        {
            var combinedData = new Dictionary<string, CaliberInfo>(StringComparer.OrdinalIgnoreCase);

            if (!Directory.Exists(directoryPath))
            {
                logger.LogWithColor($"[CaliberSplitAmmoCases] Directory not found: {directoryPath}!", LogTextColor.Yellow);
                return combinedData;
            }

            var files = Directory.GetFiles(directoryPath, "*.json", SearchOption.TopDirectoryOnly);

            foreach (var file in files)
            {
                try
                {

                    var data = _modHelper.GetJsonDataFromFile<Dictionary<string, CaliberInfo>>(modFolder, file);

                    if (data == null)
                        continue;

                    foreach (var kvp in data)
                    {
                        combinedData[kvp.Key] = kvp.Value; // overwrite duplicates
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWithColor($"[CaliberSplitAmmoCases] Error reading {Path.GetFileName(file)}: {ex.Message}", LogTextColor.Red);
                }
            }

            return combinedData;
        }

        public static Dictionary<string, string> LoadOrCreateJSON(string filePath, JsonUtil jsonUtil, ISptLogger<CaliberSplitAmmoCases> logger)
        {
            string? directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                logger.LogWithColor($"[CaliberSplitAmmoCases] Directory not found: {directory}! Creating folder...", LogTextColor.Green);
            }

            Dictionary<string, string> data;

            if (!File.Exists(filePath))
            {
                data = [];
                File.WriteAllText(filePath, jsonUtil.Serialize(data));
                logger.LogWithColor($"[CaliberSplitAmmoCases] File in db/Ids/idDatabase.json not found. Creating file...", LogTextColor.Green);
            }
            else
            {
                string file = File.ReadAllText(filePath);
                data = jsonUtil.Deserialize<Dictionary<string, string>>(file) ?? new Dictionary<string, string>();
            }

            return data;
        }
    }
}
