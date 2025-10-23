using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Enums;

namespace CaliberSplitAmmoCases.Interfaces
{
    public class CustomBarterConfig
    {
        public MongoId TraderId { get; set; } = Traders.PEACEKEEPER;
        public int LoyalLevel { get; set; } = 1;
        public bool UnlimitedCount { get; set; } = true;
        public double StackObjectsCount { get; set; } = 5;
        public int Price { get; set; } = 5000;
        public MongoId Barter { get; set; } = ItemTpl.MONEY_DOLLARS;
    }
}
