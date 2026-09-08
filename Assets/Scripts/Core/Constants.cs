namespace DwarfClone.Core
{
    public static class Constants
    {
        // World grid and chunk sizes
        public const int TILE_SIZE = 64;           // 64x64 pixel sprites
        public const float TILE_WORLD_SIZE = 1.0f; // 1 Unity unit per tile
        public const int CHUNK_SIZE = 16;          // 16x16 tiles per chunk
        public const int WORLD_WIDTH = 128;        // 128x128 map
        public const int WORLD_HEIGHT = 128;
        public const int Z_LEVELS = 10;            // 10 vertical Z-levels (0..2 deep caves, 3 underground, 4 surface, 5..9 upper levels)
        public const int SURFACE_Z_LEVEL = 4;     // Surface elevation
        public const int CHUNKS_X = WORLD_WIDTH / CHUNK_SIZE;   // 8
        public const int CHUNKS_Y = WORLD_HEIGHT / CHUNK_SIZE; // 8

        // Inventory & Stacking
        public const int MAX_STACK_SIZE = 99;
        public const int DEFAULT_INVENTORY_SLOTS = 24;

        // Time simulation
        public const float BASE_TICK_RATE = 1.0f; // 1 tick per second at 1x speed
        public const float MINUTES_PER_TICK = 5f;
        public const int HOURS_PER_DAY = 24;
        public const int DAYS_PER_MONTH = 30;

        // Character Needs & Stats
        public const float MAX_NEED_VALUE = 100f;
        public const float HUNGER_DECREASE_PER_SEC = 0.25f;
        public const float STARVATION_DAMAGE_PER_SEC = 1.5f;
        public const float NATURAL_REGEN_PER_SEC = 0.2f;
        public const float INCAPACITATED_BLEEDOUT_TIME = 90f;

        // Pathfinding
        public const float COST_STRAIGHT = 1.0f;
        public const float COST_DIAGONAL = 1.4142f;
        public const float COST_STAIRS = 2.0f;
        public const int MAX_PATH_ITERATIONS = 4000;

        // Layer and Sorting Orders
        public const int SORTING_ORDER_TERRAIN_BELOW = -10;
        public const int SORTING_ORDER_TERRAIN = 0;
        public const int SORTING_ORDER_ZONES = 3;
        public const int SORTING_ORDER_DESIGNATIONS = 5;
        public const int SORTING_ORDER_ITEMS = 10;
        public const int SORTING_ORDER_BUILDINGS = 15;
        public const int SORTING_ORDER_ENTITIES = 20;
        public const int SORTING_ORDER_EFFECTS = 30;
        public const int SORTING_ORDER_OVERLAY = 40;

        // Resource Paths
        public const string SPRITES_PATH = "Sprites/";
        public const string TILES_SPRITES_PATH = "Sprites/Tiles/";
        public const string FLORA_SPRITES_PATH = "Sprites/Flora/";
        public const string CHARACTERS_SPRITES_PATH = "Sprites/Characters/";
        public const string ANIMALS_SPRITES_PATH = "Sprites/Animals/";
        public const string WORKBENCHES_SPRITES_PATH = "Sprites/Workbenches/";
        public const string ITEMS_SPRITES_PATH = "Sprites/Items/";
        public const string UI_SPRITES_PATH = "Sprites/UI/";
    }
}
