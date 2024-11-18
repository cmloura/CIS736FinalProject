using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class TextureData : UpdateableData
{
    public Color[] basecolors;
    [Range(0,1)]
    public float[] basestartheights;

    float savedminheight;
    float savedmaxheight;
    public void ApplyToMaterial(Material material)
    {
        //UpdateMeshHeights(material, savedminheight, savedmaxheight);
    }

    public void UpdateMeshHeights(Material material, float minheight, float maxheight)
    {

    }
}
