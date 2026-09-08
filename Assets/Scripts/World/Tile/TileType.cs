namespace DwarfClone.World.Tile
{
    public enum TileType
    {
        Air = 0,

        // Natural Ground & Soil
        Grass = 1,
        Dirt = 2,
        Sand = 3,
        Mud = 4,
        Water = 5,
        Bedrock = 6,

        // Natural Stones
        Granite = 10,
        Limestone = 11,
        Basalt = 12,
        Marble = 13,

        // Natural Ores & Minerals
        Coal_Ore = 20,
        Iron_Ore = 21,
        Copper_Ore = 22,
        Gold_Ore = 23,
        Silver_Ore = 24,

        // Built Structures - Walls
        Wall_Wood = 30,
        Wall_Stone = 31,
        Wall_Brick = 32,

        // Built Structures - Floors
        Floor_Wood = 40,
        Floor_Stone = 41,

        // Built Structures - Doors
        Door_Wood = 50,
        Door_Iron = 51,

        // Z-Level Vertical Stairs
        Stairs_Up = 60,
        Stairs_Down = 61,
        Stairs_Both = 62
    }
}
