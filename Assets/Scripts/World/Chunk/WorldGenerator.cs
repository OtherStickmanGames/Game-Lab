using System;
using UnityEngine;
using DwarfClone.Core;
using DwarfClone.World.Tile;
using DwarfClone.World.Noise;
using DwarfClone.World.Flora;

namespace DwarfClone.World.Chunk
{
    public class WorldGenerator : MonoBehaviour
    {
        public static WorldGenerator Instance { get; private set; }

        [Header("Generation Seed")]
        [SerializeField] private int worldSeed = 42;
        [SerializeField] private bool randomizeSeedOnStart = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void GenerateWorld()
        {
            if (randomizeSeedOnStart)
            {
                worldSeed = UnityEngine.Random.Range(1000, 999999);
            }

            Debug.Log($"[WorldGenerator] Generating Procedural Multi-Biome World with Seed: {worldSeed}...");

            var grid = WorldGrid.Instance;
            grid.AllocateGrid();

            int width = Constants.WORLD_WIDTH;
            int height = Constants.WORLD_HEIGHT;

            // 1. Generate multi-layer terrain
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float elev = FBMGenerator.GetFBM(x, y, octaves: 4, persistence: 0.5f, lacunarity: 2.0f, scale: 0.015f, seed: worldSeed);
                    float moist = FBMGenerator.GetFBM(x, y, octaves: 3, persistence: 0.5f, lacunarity: 2.0f, scale: 0.02f, seed: worldSeed + 101);
                    float temp = FBMGenerator.GetFBM(x, y, octaves: 3, persistence: 0.5f, lacunarity: 2.0f, scale: 0.02f, seed: worldSeed + 202);

                    BiomeType biome = DetermineBiome(elev, moist, temp);

                    // Z = 0: Indestructible Bedrock
                    grid.SetTile(x, y, 0, TileType.Bedrock);

                    // Z = 1..2: Deep Subterranean Rock & Precious Ores
                    for (int z = 1; z <= 2; z++)
                    {
                        TileType stone = GetDeepStoneType(x, y, z);
                        TileType ore = GetOreVein(x, y, z, deep: true);
                        grid.SetTile(x, y, z, ore != TileType.Air ? ore : stone);
                    }

                    // Z = 3: Underground Limestone / Soil with Coal & Iron
                    for (int z = 3; z <= 3; z++)
                    {
                        float caveNoise = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.05f, seed: worldSeed + 303);
                        if (caveNoise > 0.65f)
                        {
                            grid.SetTile(x, y, z, TileType.Air); // Natural cave chamber
                        }
                        else
                        {
                            TileType ore = GetOreVein(x, y, z, deep: false);
                            grid.SetTile(x, y, z, ore != TileType.Air ? ore : TileType.Limestone);
                        }
                    }

                    // Z = 4: Surface Layer
                    TileType surfaceTile = GetSurfaceGround(biome, elev);
                    grid.SetTile(x, y, 4, surfaceTile);

                    // Z = 5..9: Above Ground (Air or High Mountain Cliffs)
                    for (int z = 5; z < Constants.Z_LEVELS; z++)
                    {
                        if (elev > 0.75f + (z - 5) * 0.08f)
                        {
                            // Mountain Peak
                            grid.SetTile(x, y, z, TileType.Granite);
                        }
                        else
                        {
                            grid.SetTile(x, y, z, TileType.Air);
                        }
                    }
                }
            }

            // 2. Populate Surface Flora & Resources on Z=4
            FloraManager.Instance?.PopulateFlora(worldSeed);

            grid.MarkReady();
            Debug.Log("[WorldGenerator] World Generation Complete!");
        }

        public BiomeType DetermineBiome(float elev, float moist, float temp)
        {
            if (elev > 0.7f) return BiomeType.Mountain;
            if (moist < 0.35f && temp > 0.5f) return BiomeType.Desert;
            if (moist > 0.65f && elev < 0.45f) return BiomeType.Swamp;
            if (temp < 0.4f) return BiomeType.Taiga;
            return BiomeType.Plains;
        }

        private TileType GetSurfaceGround(BiomeType biome, float elev)
        {
            if (elev < 0.28f) return TileType.Water; // River or Lake

            switch (biome)
            {
                case BiomeType.Desert:
                    return TileType.Sand;
                case BiomeType.Swamp:
                    return elev < 0.36f ? TileType.Water : (elev < 0.45f ? TileType.Mud : TileType.Grass);
                case BiomeType.Mountain:
                    return elev > 0.8f ? TileType.Granite : TileType.Dirt;
                case BiomeType.Taiga:
                case BiomeType.Plains:
                default:
                    return TileType.Grass;
            }
        }

        private TileType GetDeepStoneType(int x, int y, int z)
        {
            float n = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.04f, seed: worldSeed + z * 50);
            if (n < 0.35f) return TileType.Granite;
            if (n < 0.65f) return TileType.Basalt;
            return TileType.Marble;
        }

        private TileType GetOreVein(int x, int y, int z, bool deep)
        {
            float coalN = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.08f, seed: worldSeed + 10);
            if (coalN > 0.72f) return TileType.Coal_Ore;

            float ironN = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.08f, seed: worldSeed + 20);
            if (ironN > 0.74f) return TileType.Iron_Ore;

            float copperN = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.08f, seed: worldSeed + 30);
            if (copperN > 0.75f) return TileType.Copper_Ore;

            if (deep)
            {
                float goldN = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.09f, seed: worldSeed + 40);
                if (goldN > 0.78f) return TileType.Gold_Ore;

                float silverN = FBMGenerator.GetFBM(x, y, octaves: 2, scale: 0.09f, seed: worldSeed + 50);
                if (silverN > 0.78f) return TileType.Silver_Ore;
            }

            return TileType.Air; // No ore vein here
        }
    }
}
