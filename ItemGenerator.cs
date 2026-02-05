using CaliberSplitAmmoCases.Interfaces;
using CaliberSplitAmmoCases.Loaders;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;


namespace CaliberSplitAmmoCases
{
    internal class ItemGenerator(
        ISptLogger<CaliberSplitAmmoCases> logger,
        DatabaseService databaseService,
        ConfigLoader configLoader,
        ModDatabaseLoader modDatabaseLoader,
        CustomItemCreator customItemCreator
    )
    {
        private readonly Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();
        private bool SaveIDsDatabase = false;
        private CustomBarterConfig customBarterConfig = new();
        private readonly ConfigData modConfig = configLoader.Config;


        public void GenerateItems()
        {
            customBarterConfig = CreateCustomBarterConfig(modConfig, items);
            var allAmmo = LoadAmmo(modConfig, modDatabaseLoader);

            var itemCaseFilter = items["59fb042886f7746c5005a7b2"]?
                    .Properties?.Grids?.FirstOrDefault()?
                    .Properties?.Filters?.FirstOrDefault()?
                    .Filter;
            var thiccItemCaseFilter = items["5c0a840b86f7742ffa4f2482"]?
                    .Properties?.Grids?.FirstOrDefault()?
                    .Properties?.Filters?.FirstOrDefault()?
                    .Filter;

            foreach (var ammo in allAmmo)
            {
                var ammoType = ammo.Key;
                var ammoArray = ammo.Value;
                var knownAmmo = modDatabaseLoader.DbCaliberById.TryGetValue(ammoType, out CaliberInfo? value) ? value : new CaliberInfo { Name = ammoType, ShortName = ammoType };
                var newItem = new NewItemFromCloneDetails
                {
                    ItemTplToClone = "5aafbde786f774389d0cbc0f",
                    ParentId = "5795f317245977243854e041",
                    HandbookParentId = "5b5f6fa186f77409407a7eb7",
                    NewId = ResolveMongoId(modDatabaseLoader, $"CASEID{ammoType}"),
                    FleaPriceRoubles = Math.Floor(modConfig.HandbookPriceRoubles * 1.3),
                    HandbookPriceRoubles = modConfig.HandbookPriceRoubles,
                    OverrideProperties = new TemplateItemProperties
                    {
                        BackgroundColor = IsPluginLoaded() ? modConfig.BackgroundColorColorConverterAPI : modConfig.BackgroundColor,
                        Weight = 0,
                        Width = modConfig.Width,
                        Height = modConfig.Height
                    },
                    Locales = new Dictionary<string, LocaleDetails>
                    {
                        {
                            "en", new LocaleDetails
                            {
                                Name = $"<b>Custom Ammo Case for {knownAmmo.Name} ammo</b>",
                                ShortName = $"{knownAmmo.ShortName} CAC",
                                Description = $"<align=\"center\">Custom case that can store all your <b>{knownAmmo.Name}</b> ammunition!</align>"
                            }
                        }
                    }
                };
                if (modConfig.UseWholeCaseForCaliber_Mode)
                {
                    Grid wholeCaseGrid = new()
                    {
                        Id = ResolveMongoId(modDatabaseLoader, $"CASE{newItem.NewId}#AMMO:ALL#"),
                        Name = $"CASE:${newItem.NewId}#AMMO:ALL#",
                        Parent = newItem.NewId,
                        Prototype = "55d329c24bdc2d892f8b4567",
                        Properties = new()
                        {
                            CellsH = modConfig.WholeCaseHeight,
                            CellsV = modConfig.WholeCaseWidth,
                            Filters = [
                                new GridFilter {
                                    Filter = ammoArray
                                }
                            ],
                            IsSortingTable = false,
                            MaxCount = 0,
                            MaxWeight = 0,
                            MinCount = 0
                        }
                    };
                    newItem.OverrideProperties.Grids = [wholeCaseGrid];
                }
                else
                {
                    var grids = new List<Grid>();
                    foreach (var ammoId in ammoArray)
                    {
                        Grid columnCaseGrid = new()
                        {
                            Id = ResolveMongoId(modDatabaseLoader, $"CASE{newItem.NewId}#AMMO:{ammoId}#"),
                            Name = $"CASE:${newItem.NewId}#AMMO:{ammoId}#",
                            Parent = newItem.NewId,
                            Prototype = "55d329c24bdc2d892f8b4567",
                            Properties = new()
                            {
                                CellsH = 1,
                                CellsV = modConfig.CaseSlotsPerAmmo,
                                Filters = [
                                    new GridFilter {
                                        Filter = [ammoId]
                                    }
                                ],
                                IsSortingTable = false,
                                MaxCount = 0,
                                MaxWeight = 0,
                                MinCount = 0
                            }
                        };
                        grids.Add(columnCaseGrid);
                    }
                    newItem.OverrideProperties.Grids = grids;
                }
                var customItemConfig = new CustomItemConfig
                {
                    FleaBlacklisted = modConfig.FleaMarketBlacklisted
                };
                customItemCreator.AddItemToDatabase(newItem, customItemConfig, customBarterConfig);

                // Add case to filters of Item Case and THICC Item Case
                itemCaseFilter?.Add(newItem.NewId);
                thiccItemCaseFilter?.Add(newItem.NewId);
            }
            if (SaveIDsDatabase)
            {
                modDatabaseLoader.DbItemsIdsJsonSave();
            }
        }

        private Dictionary<string, HashSet<MongoId>> LoadAmmo(ConfigData config, ModDatabaseLoader modDatabaseLoader)
        {
            Dictionary<string, HashSet<MongoId>> ammo = [];

            foreach (TemplateItem item in items.Values)
            {
                if (item.Parent != "5485a8684bdc2da71d8b4567") continue;
                if (item?.Properties?.Caliber == null) continue;
                if (config.UseOnlyKnownCalibers && !modDatabaseLoader.DbCaliberById.ContainsKey(item.Properties.Caliber)) continue;
                if (config.RemoveBadCalibers && config.BadCalibers.Contains(item.Properties.Caliber)) continue;

                if (!ammo.TryGetValue(item.Properties.Caliber, out var list))
                {
                    list = [];
                    ammo[item.Properties.Caliber] = list;
                }
                list.Add(item.Id);
            }
            foreach (var caliber in ammo.Keys.ToList())
            {
                ammo[caliber] = [.. ammo[caliber].OrderBy(id => items.TryGetValue(id, out var item) ? item?.Properties?.PenetrationPower : 0)];
            }
            return ammo;
        }

        private string ResolveMongoId(ModDatabaseLoader modDatabaseLoader, string stringToMongoId)
        {
            if (!modDatabaseLoader.DbItemsIds.TryGetValue(stringToMongoId, out string? value))
            {
                SaveIDsDatabase = true;
                value = new MongoId();
                modDatabaseLoader.DbItemsIds.Add(stringToMongoId, value);
            }
            return value;
        }

        private static bool IsPluginLoaded()
        {
            const string pluginName = "rairai.colorconverterapi.dll";
            const string pluginsPath = "../BepInEx/plugins";

            try
            {
                if (!Directory.Exists(pluginsPath))
                    return false;

                var pluginList = Directory.GetFiles(pluginsPath)
                    .Select(System.IO.Path.GetFileName)
                    .Select(f => f.ToLowerInvariant());
                return pluginList.Contains(pluginName);
            }
            catch
            {
                return false;
            }
        }
        private CustomBarterConfig CreateCustomBarterConfig(ConfigData config, Dictionary<MongoId, TemplateItem> items)
        {
            if (config.CasesOnPeacekeeper)
            {
                return new CustomBarterConfig
                {
                    TraderId = Traders.PEACEKEEPER,
                    Price = config.USDPrice,
                    Barter = ItemTpl.MONEY_DOLLARS
                };
            }
            if (config.CasesOnRef)
            {
                return new CustomBarterConfig
                {
                    TraderId = Traders.REF,
                    Price = config.GpCoinPrice,
                    Barter = ItemTpl.MONEY_GP_COIN
                };
            }
            if (config.CasesOnSkier)
            {
                return new CustomBarterConfig
                {
                    TraderId = Traders.SKIER,
                    Price = config.EuroPrice,
                    Barter = ItemTpl.MONEY_EUROS
                };
            }
            if (config.CasesOnJaeger)
            {
                return new CustomBarterConfig
                {
                    TraderId = Traders.JAEGER,
                    Price = (int)Math.Floor(config.RoublesPriceMultiplier * config.HandbookPriceRoubles),
                    Barter = ItemTpl.MONEY_ROUBLES
                };
            }
            if (config.CasesOnPrapor)
            {
                if (MongoId.IsValidMongoId(config.BarterType) && items != null && items.TryGetValue(config.BarterType, out _))
                {
                    return new CustomBarterConfig
                    {
                        TraderId = Traders.PRAPOR,
                        Price = config.BarterPrice,
                        Barter = config.BarterType
                    };
                } else
                {
                    logger.LogWithColor($"[{GetType().Namespace}] MongoId for Prapor barter: {config.BarterType} do not exists! Cases are added to Peacekeeper instead!", LogTextColor.Red);
                }
            }
            return new CustomBarterConfig
            {
                TraderId = "PEACEKEEPER",
                LoyalLevel = 1,
                Price = config.USDPrice,
                Barter = "DOLLARS"
            };
        }
    }
}
