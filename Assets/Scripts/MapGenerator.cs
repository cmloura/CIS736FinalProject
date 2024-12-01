using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class MapGenerator : MonoBehaviour
{
    public const int mapchunksize = 239;
    [Range(0,6)]
    public int editorPreviewLOD;
    public enum DrawMode {NoiseMap, Mesh, FalloffMap}
    public DrawMode drawmode;
    public TerrainData terraindata;
    public NoiseData noisedata;
    public TextureData texturedata;
    public Material terrainmaterial;
    public WaterGenerator watergenerator;
    public TreeGenerator treegenerator;
    public InfiniteTerrain.TerrainChunk terrainchunk;

    public bool autoUpdate;
    float[,] falloffMap;
    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new(); 
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new();
    
    public void DrawMapInEditor()
    {
        MapData mapdata = GenerateMapData(Vector2.zero);
        MapDisplay display = FindObjectOfType<MapDisplay>();

        switch(drawmode)
        {
            case DrawMode.NoiseMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapdata.heightMap));
                break;
            case DrawMode.Mesh:
                display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, terraindata.meshHeightMultiplier, terraindata.meshheightcurve, editorPreviewLOD));
                break;
            case DrawMode.FalloffMap:
                display.DrawTexture(TextureGenerator.TextureFromHeightMap(FalloffGenerator.GenerateFalloffMap(mapchunksize)));
                break;
        }

        if (treegenerator != null)
        {
            treegenerator.GenerateTrees(mapdata.heightMap);
        }

        if (watergenerator != null)
        {
            watergenerator.GenerateWater(mapdata.heightMap, terraindata);
        }

    }
    public MapData GenerateMapData(Vector2 center)
    {
        float[,] map = Noise.GenerateNoiseMap(mapchunksize+2, mapchunksize+2, noisedata.seed, noisedata.noiseScale, noisedata.octaves, noisedata.persistence, noisedata.lacunarity, center + noisedata.offset, noisedata.normalisemode);

        if(terraindata.useFalloff)
        {
            if(falloffMap == null)
            {
                falloffMap = FalloffGenerator.GenerateFalloffMap(mapchunksize + 2);
            }
        }        
        for(int y = 0; y < mapchunksize+2; y++)
        {
            for(int x = 0; x < mapchunksize+2; x++)
            {
                if(terraindata.useFalloff)
                {
                    map[x,y] = Mathf.Clamp01(map[x,y] - falloffMap[x,y]);
                }
            }
        }
        texturedata.UpdateMeshHeights(terrainmaterial, terraindata.minHeight, terraindata.maxHeight);
        return new MapData(map);
    }

    void OnValuesUpdated()
    {
        if(!Application.isPlaying)
        {
            DrawMapInEditor();
        }
    }

    void OnTextureValuesUpdated()
    {
        texturedata.ApplyToMaterial(terrainmaterial);
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
        MeshData md = MeshGenerator.GenerateTerrainMesh(mapdata.heightMap, terraindata.meshHeightMultiplier, terraindata.meshheightcurve, lod);
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

    void OnValidate()
    {
        if(terraindata != null)
        {
            terraindata.OnValuesUpdated -= OnValuesUpdated;
            terraindata.OnValuesUpdated += OnValuesUpdated;
        }

        if(noisedata != null)
        {
            noisedata.OnValuesUpdated -= OnValuesUpdated;
            noisedata.OnValuesUpdated += OnValuesUpdated;
        }

        if(texturedata != null)
        {
            texturedata.OnValuesUpdated -= OnValuesUpdated;
            texturedata.OnValuesUpdated += OnValuesUpdated;
        }
    }

    public struct MapData {
        public readonly float[,] heightMap;

        public MapData(float[,] heightmap)
        {
            this.heightMap = heightmap;
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
