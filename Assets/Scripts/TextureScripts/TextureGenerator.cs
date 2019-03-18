using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TextureGenerator
{
    public static Texture2D TextureFromColorMap(Color32[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels32(colorMap);
        texture.Apply();
        return texture;
    }

    public static Texture2D TextureFromHeightMap(float[,] intensityMap, Color32[] color32s)
    {
        int width = intensityMap.GetLength(0);
        int height = intensityMap.GetLength(1);

        Color32[] colorMap = new Color32[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float mVal = intensityMap[x, y];
                colorMap[y * width + x] = Color32.Lerp(color32s[1], color32s[0], intensityMap[x, y]);
            }
        }

        return TextureFromColorMap(colorMap, width, height);
    }
    public static Texture2D TextureFromColor(Color color)
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.filterMode = FilterMode.Point;
        texture.wrapMode = TextureWrapMode.Clamp;
        texture.SetPixels(new Color[] { color });
        texture.Apply();
        return texture;
    }
}
