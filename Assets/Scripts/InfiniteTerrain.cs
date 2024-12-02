using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class InfiniteTerrain : MonoBehaviour
{
    const float viewermovethresholdforchunkupdate = 25f;
    const float squareviewermovethresholdforchunkupdate = viewermovethresholdforchunkupdate * viewermovethresholdforchunkupdate;
    
    public static float maxViewDistance;
    public LODInfo[] detailLevels;
    public Transform viewer;
    public static MapGenerator mapgenerator;
    public Material mapmaterial;
    Vector2 viewerpositionold;
    public static Vector2 viewerPosition;
    public GameObject treePrefab;
    public int minTreesPerChunk = 5;
    public int maxTreesPerChunk = 10;

    int chunksize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    public static List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

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
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / mapgenerator.terraindata.uniformscale;
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
        public Vector2 position;
        GameObject meshObject;
        Bounds bounds;
        MeshRenderer meshrenderer;
        MeshFilter meshfilter;
        MeshCollider meshcollider;
        InfiniteTerrain.LODInfo[] detaillevels;
        InfiniteTerrain.LODMesh[] lodmeshes;
        InfiniteTerrain.LODMesh collisionLODmesh;
        MapGenerator.MapData mapdata;
        bool mapdatareceived;
        int prevlodindex = -1;
        private List<GameObject> spawnedTrees = new();
        InfiniteTerrain infiniteterrain = new InfiniteTerrain();

        public TerrainChunk(Vector2 coord, int size, InfiniteTerrain.LODInfo[] detaillevels, Transform parent, Material material)
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

            meshObject.transform.position = pv3 * InfiniteTerrain.mapgenerator.terraindata.uniformscale;
            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * InfiniteTerrain.mapgenerator.terraindata.uniformscale;
            SetVisible(false);
            lodmeshes = new InfiniteTerrain.LODMesh[detaillevels.Length];

            for(int i = 0; i < detaillevels.Length; i++)
            {
                lodmeshes[i] = new InfiniteTerrain.LODMesh(this.detaillevels[i].lod, UpdateTerrainChunk);
                if(detaillevels[i].useforcollider)
                {
                    collisionLODmesh = lodmeshes[i];
                }
            }

            InfiniteTerrain.mapgenerator.RequestMapData(position, OnMapDataReceived);
        }

        void SpawnTrees()
        {
            InfiniteTerrain terrainGenerator = InfiniteTerrain.mapgenerator.GetComponent<InfiniteTerrain>();
            if (terrainGenerator == null || terrainGenerator.treePrefab == null) return;

            foreach (GameObject tree in spawnedTrees.ToArray())
            {
                if (tree != null)
                    UnityEngine.Object.Destroy(tree);
            }
            spawnedTrees.Clear();

            int treeCount = Random.Range(terrainGenerator.minTreesPerChunk, terrainGenerator.maxTreesPerChunk + 1);

            for (int i = 0; i < treeCount; i++)
            {
                Vector2 randomPos2D = new Vector2(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y)
                );

                float height = SampleTerrainHeight(randomPos2D);

                Vector3 worldPosition = new Vector3(
                    randomPos2D.x * InfiniteTerrain.mapgenerator.terraindata.uniformscale, 
                    height * InfiniteTerrain.mapgenerator.terraindata.uniformscale, 
                    randomPos2D.y * InfiniteTerrain.mapgenerator.terraindata.uniformscale
                );

                if (Physics.Raycast(worldPosition + Vector3.up * 100, Vector3.down, out RaycastHit hit, Mathf.Infinity))
                {
                    if (hit.point.y < height * InfiniteTerrain.mapgenerator.terraindata.uniformscale)
                        continue;

                    GameObject newTree = UnityEngine.Object.Instantiate(
                        terrainGenerator.treePrefab, 
                        hit.point, 
                        Quaternion.Euler(0, Random.Range(0, 360), 0)
                    );

                    newTree.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * newTree.transform.rotation;

                    newTree.transform.SetParent(meshObject.transform, true);

                    float randomScale = Random.Range(0.8f, 1.2f);
                    newTree.transform.localScale = Vector3.one * randomScale;

                    spawnedTrees.Add(newTree);
                }
            }
        }
    
        float SampleTerrainHeight(Vector2 positionXZ)
        {
            // Normalize position relative to chunk
            Vector2 normalizedPos = new Vector2(
                Mathf.InverseLerp(bounds.min.x, bounds.max.x, positionXZ.x),
                Mathf.InverseLerp(bounds.min.y, bounds.max.y, positionXZ.y)
            );

            // Check if heightMap is valid
            if (mapdata.heightMap != null && 
                mapdata.heightMap.GetLength(0) > 0 && 
                mapdata.heightMap.GetLength(1) > 0)
            {
                int heightMapWidth = mapdata.heightMap.GetLength(0);
                int heightMapHeight = mapdata.heightMap.GetLength(1);

                // Convert normalized position to height map coordinates
                int x = Mathf.FloorToInt(normalizedPos.x * (heightMapWidth - 1));
                int y = Mathf.FloorToInt(normalizedPos.y * (heightMapHeight - 1));

                // Clamp to prevent out of bounds
                x = Mathf.Clamp(x, 0, heightMapWidth - 1);
                y = Mathf.Clamp(y, 0, heightMapHeight - 1);

                // Return the height from the height map
                return mapdata.heightMap[x, y];
            }

            // Fallback if no height map is available
            return 0f;
        }

        void OnMapDataReceived(MapGenerator.MapData mapdata)
        {
            this.mapdata = mapdata;
            mapdatareceived = true;
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
                float viewerDistanceFromNearestEdge = bounds.SqrDistance(InfiniteTerrain.viewerPosition);
                bool visible = viewerDistanceFromNearestEdge <= InfiniteTerrain.maxViewDistance * InfiniteTerrain.maxViewDistance;
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
                        InfiniteTerrain.LODMesh lodmesh = lodmeshes[lodindex];
                        if(lodmesh.hasMesh)
                        {
                            prevlodindex = lodindex;
                            meshfilter.mesh = lodmesh.mesh;
                        }
                        else if(!lodmesh.hasRequestedMesh)
                        {
                            lodmesh.RequestMesh(mapdata);
                        }
                    }
                    if(lodindex == 0)
                    {
                        if(collisionLODmesh.hasMesh)
                        {
                            meshcollider.sharedMesh = collisionLODmesh.mesh;
                        }
                        else if(!collisionLODmesh.hasRequestedMesh)
                        {
                            collisionLODmesh.RequestMesh(mapdata);
                        }
                    }
                    InfiniteTerrain.terrainChunksVisibleLastUpdate.Add(this);
                    if(spawnedTrees.Count == 0)
                    {
                        SpawnTrees();
                    }
                }
                else
                {
                    foreach (GameObject tree in spawnedTrees)
                    {
                        if (tree != null)
                            UnityEngine.Object.Destroy(tree);
                    }
                    spawnedTrees.Clear();
            
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

    public class LODMesh 
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
        public bool useforcollider;
    }
}
