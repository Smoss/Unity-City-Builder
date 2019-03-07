using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,,] intensityMap, int mapIndex, Color32[] color32s)
    {
        int width = intensityMap.GetLength(1);
        int height = intensityMap.GetLength(2);

        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float mVal = intensityMap[mapIndex, x, y];
                colorMap[y * width + x] = Color.Lerp(color32s[1], color32s[0], intensityMap[mapIndex,x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
}
