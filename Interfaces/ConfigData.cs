namespace CaliberSplitAmmoCases.Interfaces
{
    public class ConfigData
    {
        // ### 1. Choose mode
        public bool UseWholeCaseForCaliber_Mode { get; set; } = true;
        public int WholeCaseWidth { get; set; } = 8;
        public int WholeCaseHeight { get; set; } = 8;

        public bool UseAmmoPerColumn_Mode { get; set; } = false;
        public int CaseSlotsPerAmmo { get; set; } = 12;

        // ### 2. Choose your own Trader!
        public bool CasesOnPeacekeeper { get; set; } = true;
        public int USDPrice { get; set; } = 1500;

        public bool CasesOnRef { get; set; } = false;
        public int GpCoinPrice { get; set; } = 25;

        public bool CasesOnSkier { get; set; } = false;
        public int EuroPrice { get; set; } = 1100;

        public bool CasesOnJaeger { get; set; } = false;
        public double RoublesPriceMultiplier { get; set; } = 1.25;

        public bool CasesOnPrapor { get; set; } = false;
        public string BarterType { get; set; } = "5aafbde786f774389d0cbc0f";
        public int BarterPrice { get; set; } = 1;

        // ### 3. Generation settings
        public bool UseOnlyKnownCalibers { get; set; } = false;
        public bool RemoveBadCalibers { get; set; } = true;
        public List<string> BadCalibers { get; set; } = new()
        {
            "Caliber40mmRU",
            "Caliber30x29",
            "Caliber20x1mm"
        };

        // ### 4. Cases configuration
        public string BackgroundColor { get; set; } = "red";
        public string BackgroundColorColorConverterAPI { get; set; } = "#cf404e";
        public int Width { get; set; } = 1;
        public int Height { get; set; } = 1;
        public bool FleaMarketBlacklisted { get; set; } = true;
        public int HandbookPriceRoubles { get; set; } = 200000;
    }
}
