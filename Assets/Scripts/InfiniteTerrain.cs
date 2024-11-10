using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    const float scale = 2f;
    const float viewermovethresholdforchunkupdate = 25f;
    const float squareviewermovethresholdforchunkupdate = viewermovethresholdforchunkupdate * viewermovethresholdforchunkupdate;
    
    public static float maxViewDistance;
    public LODInfo[] detailLevels;
    public Transform viewer;
    static MapGenerator mapgenerator;
    public Material mapmaterial;
    Vector2 viewerpositionold;
    public static Vector2 viewerPosition;

    int chunksize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    void Start()
    {
        mapgenerator = FindObjectOfType<MapGenerator>();
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreshold;
        chunksize = MapGenerator.mapchunksize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunksize);
        UpdateVisibleChunks();

    }   

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;
        if((viewerpositionold-viewerPosition).sqrMagnitude > squareviewermovethresholdforchunkupdate)
        {
            viewerpositionold = viewerPosition;
            UpdateVisibleChunks();
        }
        
    }

    void UpdateVisibleChunks()
    {
        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunksize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunksize);

        for(int yoffset = -chunksVisibleInViewDistance; yoffset <= chunksVisibleInViewDistance; yoffset++)
        {
            for(int xoffset = -chunksVisibleInViewDistance; xoffset <= chunksVisibleInViewDistance; xoffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xoffset, currentChunkCoordY + yoffset);

                if(terrainChunkDict.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDict[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, chunksize, detailLevels, transform, mapmaterial);
                    terrainChunkDict.Add(viewedChunkCoord, newChunk);
                    newChunk.UpdateTerrainChunk();
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 position;
        GameObject meshObject;
        Bounds bounds;
        MeshRenderer meshrenderer;
        MeshFilter meshfilter;
        MeshCollider meshcollider;
        LODInfo[] detaillevels;
        LODMesh[] lodmeshes;
        MapGenerator.MapData mapdata;
        bool mapdatareceived;
        int prevlodindex = -1;

        public TerrainChunk(Vector2 coord, int size, LODInfo[] detaillevels, Transform parent, Material material)
        {
            this.detaillevels = detaillevels;
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 pv3 = new Vector3(position.x, 0, position.y);
    
            meshObject = new GameObject("Terrain Chunk");
            meshrenderer = meshObject.AddComponent<MeshRenderer>();
            meshfilter = meshObject.AddComponent<MeshFilter>();
            meshcollider = meshObject.AddComponent<MeshCollider>();
            meshrenderer.material = material;

            meshObject.transform.position = pv3 * scale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale;
            SetVisible(false);
            lodmeshes = new LODMesh[detaillevels.Length];

            for(int i = 0; i < detaillevels.Length; i++)
            {
                lodmeshes[i] = new LODMesh(this.detaillevels[i].lod, UpdateTerrainChunk);
            }

            mapgenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapGenerator.MapData mapdata)
        {
            this.mapdata = mapdata;
            mapdatareceived = true;
            Texture2D texture = TextureGenerator.TextureFromColorMap(mapdata.colorMap, MapGenerator.mapchunksize, MapGenerator.mapchunksize);
            meshrenderer.material.mainTexture = texture;
            UpdateTerrainChunk();
        }

        void OnMeshDataReceived(MeshData meshdata)
        {
            meshfilter.mesh = meshdata.CreateMesh();
        }

        public void UpdateTerrainChunk()
        {
            if(mapdatareceived)
            {
                float viewerDistanceFromNearestEdge = bounds.SqrDistance(viewerPosition);
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance * maxViewDistance;
                if(visible)
                {
                    int lodindex = 0;
                    for(int i = 0; i < detaillevels.Length - 1; i++)
                    {
                        if(viewerDistanceFromNearestEdge > detaillevels[i].visibleDistanceThreshold)
                        {
                            lodindex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }
                    if(lodindex != prevlodindex)
                    {
                        LODMesh lodmesh = lodmeshes[lodindex];
                        if(lodmesh.hasMesh)
                        {
                            prevlodindex = lodindex;
                            meshfilter.mesh = lodmesh.mesh;
                            meshcollider.sharedMesh = lodmesh.mesh;
                        }
                        else if(!lodmesh.hasRequestedMesh)
                        {
                            lodmesh.RequestMesh(mapdata);
                        }
                    }
                    terrainChunksVisibleLastUpdate.Add(this);
                }  
                SetVisible(visible);   
            }                 
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh 
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        int lod;
        System.Action updatecallback;

        public LODMesh(int lod, System.Action updatecallback)
        {
            this.lod = lod;
            this.updatecallback = updatecallback;
        }

        void OnMeshDataReceived(MeshData meshdata)
        {
            mesh = meshdata.CreateMesh();
            hasMesh = true;
            updatecallback();
        }

        public void RequestMesh(MapGenerator.MapData mapdata)
        {
            hasRequestedMesh = true;
            mapgenerator.RequestMeshData(mapdata, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public float visibleDistanceThreshold;
    }
}
