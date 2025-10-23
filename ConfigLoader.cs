using CaliberSplitAmmoCases.Interfaces;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Utils;
using System.Reflection;

namespace CaliberSplitAmmoCases
{
    public class ConfigLoader(ISptLogger<CaliberSplitAmmoCases> logger, ModHelper modHelper, JsonUtil jsonUtil)
    {
        public ConfigData LoadConfig()
        {
            string modFolder = modHelper.GetAbsolutePathToModFolder(Assembly.GetExecutingAssembly());
            string configPath = Path.Combine(modFolder, "config", "config.jsonc");
            try
            {
                if (!File.Exists(configPath))
                {
                    logger.LogWithColor($"[{GetType().Namespace}] Config file not found at {configPath}. Please re-install the mod. Loading default config", LogTextColor.Red);
                    return new ConfigData();
                }
                var json = File.ReadAllText(configPath);
                var config = jsonUtil.Deserialize<ConfigData>(json);
                if (config == null)
                {
                    logger.LogWithColor($"[{GetType().Namespace}] Config file is null. How did you done this? Loading default config", LogTextColor.Red);
                    return new ConfigData();
                }
                return config;
            }
            catch (Exception ex)
            {
                logger.LogWithColor($"[{GetType().Namespace}] Failed to load config: {ex.Message}", LogTextColor.Red);
                return new ConfigData();
            }
        }
    }
}
