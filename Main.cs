using CaliberSplitAmmoCases.Loaders;
using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Helpers;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;
using SPTarkov.Server.Core.Utils;

namespace CaliberSplitAmmoCases;

[Injectable(TypePriority = OnLoadOrder.PostDBModLoader + 97323)]
public class CaliberSplitAmmoCases(
    ISptLogger<CaliberSplitAmmoCases> logger,
    ModHelper modHelper,
    JsonUtil jsonUtil,
    ConfigServer configServer,
    CustomItemService customItemService,
    DatabaseService databaseService
) : IOnLoad
{
    public Task OnLoad()
    {
        ConfigLoader configLoader = new(logger, modHelper);
        ModDatabaseLoader modDatabaseLoader = new(logger, modHelper, jsonUtil);
        CustomItemCreator customItemCreator = new(logger, configServer, customItemService, databaseService);
        ItemGenerator itemGenerator = new(
            logger,
            databaseService,
            configLoader,
            modDatabaseLoader,
            customItemCreator
        );

        itemGenerator.GenerateItems();

        logger.LogWithColor($"[{GetType().Namespace}] Mod finished loading. Created {customItemCreator.itemsLoaded} custom ammo cases!", LogTextColor.Green);

        return Task.CompletedTask;
    }
}