using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colormap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colormap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] heightmap)
    {
        int width = heightmap.GetLength(0);
        int height = heightmap.GetLength(1);

        Color[] colorMap = new Color[width*height];
        
        for(int i = 0; i < height; i++)
        {
            for(int j = 0; j < width; j++)
            {
                colorMap[i*width + j] = Color.Lerp(Color.black, Color.white, heightmap[j,i]);
            }
        }
        return TextureFromColorMap(colorMap, width, height);
    }
}
