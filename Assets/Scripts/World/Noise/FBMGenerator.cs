using UnityEngine;

namespace DwarfClone.World.Noise
{
    public static class FBMGenerator
    {
        public static float GetFBM(float x, float y, int octaves = 4, float persistence = 0.5f, float lacunarity = 2.0f, float scale = 0.02f, int seed = 1337)
        {
            float total = 0f;
            float frequency = scale;
            float amplitude = 1f;
            float maxValue = 0f;

            for (int i = 0; i < octaves; i++)
            {
                float sampleX = (x + seed + i * 113.17f) * frequency;
                float sampleY = (y + seed + i * 277.31f) * frequency;

                float perlin = Mathf.PerlinNoise(sampleX, sampleY);
                total += perlin * amplitude;

                maxValue += amplitude;
                amplitude *= persistence;
                frequency *= lacunarity;
            }

            return total / maxValue;
        }
    }
}
