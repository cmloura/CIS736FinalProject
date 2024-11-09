using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public const int mapchunksize = 241;
    [Range(0,6)]
    public int editorPreviewLOD;
    public enum DrawMode {NoiseMap, ColorMap, Mesh, FalloffMap}
    public DrawMode drawmode;
    public Noise.NormalizeMode normalisemode;
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
    public bool useFalloff;
    public TerrainType[] regions;
    float[,] falloffMap;
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new(); 
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    
    void Awake()
    {
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapchunksize);
    }
    public void DrawMapInEditor()
    {
        MapData mapdata = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch(drawmode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.heightMap));
                break;
            case DrawMode.ColorMap:
                display.DrawTexture(TextureGenerator.TextureFromColorMap(mapdata.colorMap, mapchunksize, mapchunksize));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, meshHeightMultiplier, meshheightcurve, editorPreviewLOD), TextureGenerator.TextureFromColorMap(mapdata.colorMap, mapchunksize, mapchunksize));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapchunksize)));
                break;
        }
    }
    MapData GenerateMapData(Vector2 center)
    {
        float[,] map = Noise.GenerateNoiseMap(mapchunksize, mapchunksize, seed, noiseScale, octaves, persistence, lacunarity, center + offset, normalisemode);
        Color[] colormap = new Color[mapchunksize * mapchunksize];

        for(int y = 0; y < mapchunksize; y++)
        {
            for(int x = 0; x < mapchunksize; x++)
            {
                if(useFalloff)
                {
                    map[x,y] = Mathf.Clamp01(map[x,y] - falloffMap[x,y]);
                }
                float currentHeight = map[x,y];
                for(int i = 0; i < regions.Length; i++)
                {
                    if(currentHeight >= regions[i].height)
                    {
                        colormap[y * mapchunksize + x] = regions[i].color;
                    }
                    else
                    {
                        break;
                    }
                }
            }
        }

        return new MapData(map, colormap);
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
        falloffMap = FalloffGenerator.GenerateFalloffMap(mapchunksize);
    }

    public void RequestMeshData(MapData mapdata, int lod, Action<MeshData> callback)
    {
        ThreadStart threadstart = delegate{
            MeshDataThread(mapdata, lod, callback);
        };

        new Thread(threadstart).Start();
    }

    void MeshDataThread(MapData mapdata, int lod, Action<MeshData> callback)
    {
        MeshData md = MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, meshHeightMultiplier, meshheightcurve, lod);
        lock(meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, md));
        }
    }
    
    public void RequestMapData(Vector2 center, Action<MapData> callback)
    {
        ThreadStart threadstart = delegate{
            MapDataThread(center, callback);
        };

        new Thread(threadstart).Start();
    }

    void MapDataThread(Vector2 center, Action<MapData> callback)
    {
        MapData mapdata = GenerateMapData(center);
        lock(mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapdata));
        }

    }

    void Update()
    {
        if(mapDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if(meshDataThreadInfoQueue.Count > 0)
        {
            for(int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    [System.Serializable]
    public struct TerrainType {
        public float height;
        public string name;
        public Color color;
    }

    public struct MapData {
        public readonly float[,] heightMap;
        public readonly Color[] colorMap;

        public MapData(float[,] heightmap, Color[] colormap)
        {
            this.heightMap = heightmap;
            this.colorMap = colormap;
        }
    }

    struct MapThreadInfo<T> {
        public readonly  Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
}
