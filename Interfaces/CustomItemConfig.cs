namespace CaliberSplitAmmoCases.Interfaces
{
    public class CustomItemConfig
    {
        public bool AirdropBlacklisted { get; set; } = false;
        public bool FenceBlacklisted { get; set; } = false;
        public bool FleaBlacklisted { get; set; } = false;
        public string[] AddToInventorySlots { get; set; } = [];
        public string MasteryName { get; set; } = "";
        public string MasteryId { get; set; } = "";
    }
}
