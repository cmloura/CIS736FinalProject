using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public const int mapchunksize = 241;
    [Range(0,6)]
    public int levelOfDetail;
    public enum DrawMode {NoiseMap, ColorMap, Mesh}
    public DrawMode drawmode;
    public float noiseScale;
    
    public int octaves;
    [Range(0,1)]
    public float persistence;
    public float lacunarity;
    public float meshHeightMultiplier;
    public AnimationCurve meshheightcurve;

    public int seed;
    public Vector2 offset;

    public bool autoUpdate;
    public TerrainType[] regions;


    public void GenerateMap()
    {
        float[,] map = Noise.GenerateNoiseMap(mapchunksize, mapchunksize, seed, noiseScale, octaves, persistence, lacunarity, offset);
        Color[] colormap = new Color[mapchunksize * mapchunksize];

        for(int y = 0; y < mapchunksize; y++)
        {
            for(int x = 0; x < mapchunksize; x++)
            {
                float currentHeight = map[x,y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight <= regions[i].height)
                    {
                        colormap[y * mapchunksize + x] = regions[i].color;
                        break;
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch(drawmode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(map));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(colormap, mapchunksize, mapchunksize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(map, meshHeightMultiplier, meshheightcurve, levelOfDetail), TextureGenerator.TextureFromColorMap(colormap, mapchunksize, mapchunksize));
                break;
        }
    }

    void OnValidate()
    {
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
