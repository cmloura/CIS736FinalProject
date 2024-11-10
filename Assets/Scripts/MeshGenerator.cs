using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator 
{
    public static MeshData GenerateTerrainMesh(float[,] heightmap, float heightmultiplier, AnimationCurve ac, int levelOfDetail)
    {
        int meshsimplificationincrement = (levelOfDetail == 0) ? 1 : (levelOfDetail * 2);
        int borderedsize = heightmap.GetLength(0);
        int meshsize = borderedsize - 2*meshsimplificationincrement;
        int meshsizeunsimplified = borderedsize - 2;

        float topleftx = (meshsizeunsimplified - 1) / -2f;
        float topleftz = (meshsizeunsimplified - 1) / 2f;

        int verticesPerLine = ((meshsize - 1) / meshsimplificationincrement) + 1;

        MeshData meshdata = new MeshData(verticesPerLine);
        int[,] vertexindicesmap = new int[borderedsize, borderedsize];
        int meshvertexindex = 0;
        int bordervertexindex = -1;

        for (int y = 0; y < borderedsize; y+= meshsimplificationincrement)
        {
            for (int x = 0; x < borderedsize; x+= meshsimplificationincrement)
            {
                bool isbordervertex = y ==0 || y == borderedsize - 1 || x == 0 || x == borderedsize - 1;
                if(isbordervertex)
                {
                    vertexindicesmap[x,y] = bordervertexindex;
                    bordervertexindex--;
                }
                else
                {
                    vertexindicesmap[x,y] = meshvertexindex;
                    meshvertexindex++;
                }
            }
        }

        for (int y = 0; y < borderedsize; y+= meshsimplificationincrement)
        {
            for (int x = 0; x < borderedsize; x+= meshsimplificationincrement)
            {
                int vertexindex = vertexindicesmap[x,y];
                Vector2 percent = new Vector2(((x - meshsimplificationincrement) / (float)meshsize), ((y - meshsimplificationincrement) / (float)meshsize));
                float height;
                lock(ac)
                {
                    height = ac.Evaluate(heightmap[x, y]) * heightmultiplier;
                }
                Vector3 vertexposition = new Vector3(topleftx + percent.x * meshsizeunsimplified, height, topleftz - percent.y * meshsizeunsimplified);               

                meshdata.AddVertex(vertexposition, percent, vertexindex);
                if ((x < borderedsize - 1) && (y < borderedsize - 1))
                {
                    int a = vertexindicesmap[x,y];
                    int b = vertexindicesmap[x + meshsimplificationincrement,y];
                    int c = vertexindicesmap[x,y + meshsimplificationincrement];
                    int d = vertexindicesmap[x + meshsimplificationincrement,y + meshsimplificationincrement];
                    meshdata.AddTriangle(a,d,c);
                    meshdata.AddTriangle(d,a,b);
                }
                vertexindex++;
            }
        }
        return meshdata;
    }
}

public class MeshData
{
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uvs;
    Vector3[] bordervertices;
    int[] bordertriangles;
    int triangleindex;
    int bordertriangleindex;

    public MeshData(int verticesperline)
    {
        uvs = new Vector2[verticesperline * verticesperline];
        vertices = new Vector3[verticesperline * verticesperline];
        triangles = new int[(verticesperline - 1) * (verticesperline - 1) * 6];
    
        bordervertices = new Vector3[verticesperline * 4 + 4];
        bordertriangles = new int[24*verticesperline];
    }

    public void AddVertex(Vector3 vertexposition, Vector2 uv, int vertexindex)
    {
        if(vertexindex < 0)
        {
            bordervertices[-vertexindex-1] = vertexposition;
        }
        else
        {
            vertices[vertexindex] = vertexposition;
            uvs[vertexindex] = uv;
        }
    }
    public void AddTriangle(int a, int b, int c)
    {
        if(a < 0 || b < 0 || c < 0)
        {
            bordertriangles[bordertriangleindex] = a;
            bordertriangles[bordertriangleindex + 1] = b;
            bordertriangles[bordertriangleindex + 2] = c;
            bordertriangleindex += 3;  
        }
        else
        {
            triangles[triangleindex] = a;
            triangles[triangleindex + 1] = b;
            triangles[triangleindex + 2] = c;
            triangleindex += 3;  
        }
         
    }

    Vector3[] CalculateNormals()
    {
        Vector3[] vertexnormals = new Vector3[vertices.Length];
        int trianglecount = triangles.Length / 3;
        for(int i = 0; i < trianglecount; i++)
        {
            int normaltriangleindex = i * 3;
            int vertexindexA = triangles[normaltriangleindex];
            int vertexindexB = triangles[normaltriangleindex+1];
            int vertexindexC = triangles[normaltriangleindex+2];
        
            Vector3 trianglenormal = SurfaceNormalFromIndices(vertexindexA, vertexindexB, vertexindexC);
            vertexnormals[vertexindexA] += trianglenormal;
            vertexnormals[vertexindexB] += trianglenormal;
            vertexnormals[vertexindexC] += trianglenormal;
        }

        int bordertrianglecount = bordertriangles.Length / 3;
        for(int i = 0; i < bordertrianglecount; i++)
        {
            int normaltriangleindex = i * 3;
            int vertexindexA = bordertriangles[normaltriangleindex];
            int vertexindexB = bordertriangles[normaltriangleindex+1];
            int vertexindexC = bordertriangles[normaltriangleindex+2];
        
            Vector3 trianglenormal = SurfaceNormalFromIndices(vertexindexA, vertexindexB, vertexindexC);
            if(vertexindexA >= 0)
            {
                vertexnormals[vertexindexA] += trianglenormal;
            }
            if(vertexindexB >= 0)
            {
                vertexnormals[vertexindexB] += trianglenormal;
            }
            if(vertexindexC >= 0)
            {
                vertexnormals[vertexindexC] += trianglenormal;
            }
        }

        for(int i = 0; i < vertexnormals.Length; i++)
        {
            vertexnormals[i].Normalize();
        }
        return vertexnormals;
    }

    Vector3 SurfaceNormalFromIndices(int ia, int ib, int ic)
    {
        Vector3 pointA = (ia < 0)? bordervertices[-ia-1] : vertices[ia];
        Vector3 pointB = (ib < 0)? bordervertices[-ib-1] : vertices[ib];
        Vector3 pointC = (ic < 0)? bordervertices[-ic-1] : vertices[ic];

        Vector3 sideAB = pointB - pointA;
        Vector3 sideAC = pointC - pointA;
        return Vector3.Cross(sideAB, sideAC).normalized;

    }

    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.normals = CalculateNormals();
        return mesh;
    }
}