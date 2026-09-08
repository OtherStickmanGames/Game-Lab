namespace DwarfClone.Crafting
{
    public enum CraftingStationType
    {
        CarpenterBench = 1,
        Sawmill = 2,
        StonecutterTable = 3,
        Smelter = 4,
        BlacksmithAnvil = 5,
        WeaponsmithBench = 6,
        ArmorsmithForge = 7,
        TanningRack = 8,
        LeatherworkerBench = 9,
        Loom = 10,
        TailorBench = 11,
        CookingStove = 12,
        Brewery = 13,
        Mill = 14,
        ButcherTable = 15,
        JewelerBench = 16,
        ApothecaryTable = 17,
        MasonryBench = 18,
        Kiln = 19,
        GlassmakerFurnace = 20,
        MechanicBench = 21,
        SiegeWorkshop = 22,
        ResearchBench = 23,
        ToolmakerBench = 24,
        FisheryStation = 25
    }

    public enum CraftingMode
    {
        SingleOrder,       // Execute N times then remove
        ContinuousOnFlow   // Keep crafting on flow ("Поставить на поток" - Kenshi style)
    }
}
