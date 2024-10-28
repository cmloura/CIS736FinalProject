using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Removed [ExtensionOfNativeClass]
public class MeshGenerator 
{
    public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightmultiplier, AnimationCurve ac, int levelOfDetail)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);
        float topleftx = (width - 1) / -2f;
        float topleftz = (height - 1) / 2f;

        int meshsimplificationincrement = (levelOfDetail == 0) ? 1 : (levelOfDetail * 2);
        int verticesPerLine = ((width - 1) / meshsimplificationincrement) + 1;

        MeshData meshdata = new MeshData(verticesPerLine, verticesPerLine);
        int vertexindex = 0;

        for (int y = 0; y < height; y+= meshsimplificationincrement)
        {
            for (int x = 0; x < width; x+= meshsimplificationincrement)
            {
                lock(ac){
                meshdata.vertices[vertexindex] = new Vector3(topleftx + x, ac.Evaluate(heightmap[x, y]) * heightmultiplier, topleftz - y);
                }
                meshdata.uvs[vertexindex] = new Vector2((x / (float)width), (y / (float)height));

                if ((x < width - 1) && (y < height - 1))
                {
                    meshdata.AddTriangle(vertexindex, vertexindex + verticesPerLine + 1, vertexindex + verticesPerLine);
                    meshdata.AddTriangle(vertexindex + verticesPerLine + 1, vertexindex, vertexindex + 1);
                }
                vertexindex++;
            }
        }
        return meshdata;
    }
}

public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    int triangleindex;
    public Vector2[] uvs;

    public MeshData(int meshwidth, int meshheight)
    {
        uvs = new Vector2[meshwidth * meshheight];
        vertices = new Vector3[meshwidth * meshheight];
        triangles = new int[(meshwidth - 1) * (meshheight - 1) * 6];
    }

    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleindex] = a;
        triangles[triangleindex + 1] = b;
        triangles[triangleindex + 2] = c;
        triangleindex += 3;   
    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        return mesh;
    }
}