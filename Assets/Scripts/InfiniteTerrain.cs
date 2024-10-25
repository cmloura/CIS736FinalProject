using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfiniteTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;
    public Transform viewer;

    public static Vector2 viewerPosition;
    int chunksize;
    int chunksVisibleInViewDistance;

    Dictionary<Vector2, TerrainChunk> terrainChunkDict = new();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new();

    void Start()
    {
        chunksize = MapGenerator.mapchunksize - 1;
        chunksVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunksize);
    }   

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
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
                    if(terrainChunkDict[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDict[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDict.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunksize, transform));
                }
            }
        }
    }

    public class TerrainChunk
    {
        Vector2 position;
        GameObject meshObject;
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 pv3 = new Vector3(position.x, 0, position.y);
    
            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = pv3;
            meshObject.transform.localScale = Vector3.one * size / 10f;
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = bounds.SqrDistance(viewerPosition);
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance * maxViewDistance;
            SetVisible(visible);
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
}
