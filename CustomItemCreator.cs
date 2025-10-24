using CaliberSplitAmmoCases.Interfaces;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Logging;
using SPTarkov.Server.Core.Models.Spt.Config;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Servers;
using SPTarkov.Server.Core.Services;
using SPTarkov.Server.Core.Services.Mod;

namespace CaliberSplitAmmoCases
{
    
    public class CustomItemCreator(ISptLogger<CaliberSplitAmmoCases> logger, ConfigServer configServer, CustomItemService customItemService, DatabaseService databaseService)
    {
        private readonly Globals globals = databaseService.GetGlobals();
        private readonly Dictionary<MongoId, TemplateItem> items = databaseService.GetItems();
        private readonly Dictionary<MongoId, Trader> traders = databaseService.GetTraders();
        public int itemsLoaded = 0;

        public void AddItemToDatabase(NewItemFromCloneDetails item, CustomItemConfig itemConfig, CustomBarterConfig barterConfig)
        {
            
            customItemService.CreateItemFromClone(item);

            if (item.NewId != null && itemConfig.AirdropBlacklisted)
            {
                var airdropConfig = configServer.GetConfig<AirdropConfig>();
                foreach (var airdrop in airdropConfig.Loot)
                {
                    airdropConfig.Loot[airdrop.Key].ItemBlacklist.Add(item.NewId);
                }
            }
            if (item.NewId != null && itemConfig.FenceBlacklisted)
            {
                TraderConfig traderConfig = configServer.GetConfig<TraderConfig>();
                traderConfig.Fence.Blacklist.Add(item.NewId);
            }
            if (item.NewId != null && itemConfig.FleaBlacklisted)
            {
                var fleaConfig = configServer.GetConfig<RagfairConfig>();
                fleaConfig.Dynamic.Blacklist.Custom.Add(item.NewId);
            }
            if (item.NewId != null && itemConfig.AddToInventorySlots.Length > 0)
            {
                AddItemToInventorySlots(item.NewId, itemConfig);
            }
            if (item.NewId != null && itemConfig.MasteryName != "")
            {
                AddItemToMasteries(item.NewId, itemConfig);
            }
            if (item.NewId != null && barterConfig.LoyalLevel != 0)
            {
                AddItemToTrader(item.NewId, barterConfig);
            }
            itemsLoaded++;
        }
        private void AddItemToInventorySlots(string itemId, CustomItemConfig itemConfig)
        {
            TemplateItem defaultInventory = items["55d7217a4bdc2d86028b456d"];
            IEnumerable<Slot>? defaultInventorySlots = defaultInventory?.Properties?.Slots;
            if (defaultInventorySlots != null && defaultInventorySlots.Any())
            {
                foreach (var slot in defaultInventorySlots)
                {
                    if (itemConfig.AddToInventorySlots.Contains(slot.Name) && slot.Properties != null)
                    {
                        var filters = slot.Properties.Filters;
                        if (filters != null)
                        {
                            foreach (var filter in filters)
                            {
                                if (filter != null && filter.Filter != null && !filter.Filter.Contains(itemId))
                                {
                                    filter.Filter.Add(itemId);
                                }
                            }
                        }
                        
                    }
                }
            }
        }
        private void AddItemToMasteries(string itemId, CustomItemConfig itemConfig)
        {
            var mastering = globals.Configuration.Mastering;
            var existingMastery = mastering.FirstOrDefault(existing => existing.Name == itemConfig.MasteryName);
            if (existingMastery != null)
            {
                existingMastery.Templates = existingMastery.Templates.Append(itemId);
            }
            else
            {
                logger.LogWithColor($"[{GetType().Namespace}] MasteryName '{itemConfig.MasteryName}' is incorrect!", LogTextColor.Red);
            }
        }
        private void AddItemToTrader(string itemId, CustomBarterConfig barterConfig)
        {
            MongoId traderKey = barterConfig.TraderId;
            if (!MongoId.IsValidMongoId(traderKey) || !traders.TryGetValue(traderKey, out _))
            {
                logger.LogWithColor($"[{GetType().Namespace}] Trader name / Trader ID '{traderKey}' is incorrect!", LogTextColor.Red);
                return;
            }
            var trader = traders[traderKey];

            MongoId barter = barterConfig.Barter;
            if (!MongoId.IsValidMongoId(barter) || !items.TryGetValue(barter, out _))
            {
                logger.LogWithColor($"[{GetType().Namespace}] Barter item of id '{barter}' is incorrect!", LogTextColor.Red);
                return;
            }

            var newItem = new Item
            {
                Id = itemId,
                Template = itemId,
                ParentId = "hideout",
                SlotId = "hideout",
                Upd = new Upd
                {
                    UnlimitedCount = barterConfig.UnlimitedCount,
                    StackObjectsCount = barterConfig.StackObjectsCount
                }
            };
            var assort = trader.Assort.Items;
            assort?.Add(newItem);

            var newBarterScheme = new BarterScheme
            {
                Count = barterConfig.Price,
                Template = barter
            };
            var assortBarterScheme = trader.Assort.BarterScheme;

            if (!assortBarterScheme.ContainsKey(itemId))
            {
                assortBarterScheme[itemId] = [];
                assortBarterScheme[itemId].Add([newBarterScheme]);
            }

            trader.Assort.LoyalLevelItems[itemId] = barterConfig.LoyalLevel;
        }
    }
}
