using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public enum DrawMode {NoiseMap, ColorMap}
    public DrawMode drawmode;
    public int mapWidth;
    public int mapHeight;
    public float noiseScale;
    
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;
    public TerrainType[] regions;


    public void GenerateMap()
    {
        float[,] map = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octaves, persistence, lacunarity, offset);
        Color[] colormap = new Color[mapWidth * mapHeight];

        for(int y = 0; y < mapHeight; y++)
        {
            for(int x = 0; x < mapWidth; x++)
            {
                float currentHeight = map[x,y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colormap[y * mapWidth + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();
        if(drawmode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
        }
        else if(drawmode == DrawMode.ColorMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, mapWidth, mapHeight));
        }
    }

    void OnValidate()
    {
        if(mapWidth < 1)
        {
            mapWidth = 1;
        }

        if(mapHeight < 1)
        {
            mapHeight = 1;
        }

        if(lacunarity < 1)
        {
            lacunarity = 1;
        }

        if(octaves < 0)
        {
            octaves = 0;
        }
    }

    [System.Serializable]
    public struct TerrainType {
        public float height;
        public string name;
        public Color color;
    }
}
