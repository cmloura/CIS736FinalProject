using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Noise
{
    public enum NormalizeMode {Local, Global};
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, int seed, float scale, int octaves, float persistence, float lacunarity, Vector2 offset, NormalizeMode normalizemode)
    {
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];
        float[,] noiseMap = new float[mapWidth, mapHeight];
        float maxpossibleheight = 0;
        float amplitude = 1;
        float frequency = 1;

        for(int x = 0; x < octaves; x++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;

            octaveOffsets[x] = new Vector2(offsetX, offsetY);
            maxpossibleheight += amplitude;
            amplitude *= persistence;
        }

        if (scale <= 0)
        {
            scale = 0.0001f;
        }
        float maxLocalNoiseHeight = float.MinValue;
        float minLocalNoiseHeight = float.MaxValue;

        float halfwidth = mapWidth / 2f;
        float halfheight = mapHeight / 2f;

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;

                for (int k = 0; k < octaves; k++)
                {
                    float samplex = (j - halfwidth) / scale * frequency + octaveOffsets[k].x * frequency;
                    float sampley = (i - halfheight) / scale * frequency - octaveOffsets[k].y * frequency;

                    float perlinValue = Mathf.PerlinNoise(samplex, sampley) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }
                if (noiseHeight > maxLocalNoiseHeight)
                {
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight < minLocalNoiseHeight)
                {
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[j, i] = noiseHeight;
            }
        }

        for (int i = 0; i < mapHeight; i++)
        {
            for (int j = 0; j < mapWidth; j++)
            {
                if(normalizemode == NormalizeMode.Local)
                {
                    noiseMap[j, i] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[j, i]);
                }
                else
                {
                    float normalizedheight = (noiseMap[j,i] + 1) / (2f * maxpossibleheight / 2f);
                    noiseMap[j,i] = Mathf.Clamp(normalizedheight, 0, int.MaxValue);
                }
            }
        }
        return noiseMap;
    }
}
