using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{

    public enum DrawMode { Terain, Fertility};
    public DrawMode drawMode;
    public int width;
    public int height;
    private float halfWidth;
    private float halfHeight;
    public float heightScale;
    [Range(0.01f, 1)]
    public float heightPersistence;
    public float heightLacunarity;
    public Vector2 heightOffset;
    public float fertilityScale;
    [Range(0.01f, 1)]
    public float fertilityPersistence;
    public float fertilityLacunarity;
    public Vector2 fertilityOffset;

    public string seed;
    public bool useRandomSeed;
    public bool autoUpdate;
    
    public int heightNoisePasses;
    public int fertilityNoisePasses;

    private float maxValue;
    private float minValue;

    Color32[] colorSet;

    float[,,] map;

    // Start is called before the first frame update
    void Start() {
        GenerateMap();
    }

    // Update is called once per frame
    void Update() {
    }

    private void OnValidate()
    {
        if (heightScale <= 0)
        {
            heightScale = 0.0001f;
        }
        if (fertilityScale <= 0)
        {
            fertilityScale = 0.0001f;
        }
        if (heightNoisePasses < 1)
        {
            heightNoisePasses = 1;
        }
        if (fertilityNoisePasses < 1)
        {
            fertilityNoisePasses = 1;
        }
        if (heightLacunarity < 1)
        {
            heightLacunarity = 1;
        }
        if (fertilityLacunarity < 1)
        {
            fertilityLacunarity = 1;
        }
    }

    public void GenerateMap() {
        map = new float[2, width, height];
        halfHeight = height / 2f;
        halfWidth = width / 2f;
        colorSet = new Color32[2];
        colorSet[0] = Color.green;
        colorSet[1] = Color.red;
        //RandomFillMap();
        if (useRandomSeed)
        {
            seed = Time.time.ToString();
        }
        
        System.Random pseudoRandom = new System.Random(seed.GetHashCode());
        for (int i = 0; i < heightNoisePasses; i++)
        {
            BuildMapAtIndex(0, heightScale * Mathf.Pow(heightLacunarity, (float) i), Mathf.Pow(heightPersistence, i), pseudoRandom, heightOffset);
        }
        for (int i = 0; i < fertilityNoisePasses; i++)
        {
            BuildMapAtIndex(1, fertilityScale * Mathf.Pow(fertilityLacunarity, (float)i), Mathf.Pow(fertilityPersistence, i), pseudoRandom, fertilityOffset);
        }
        minValue = 100;
        maxValue = -100;
        SmoothMap((int)drawMode);
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        Mesh mesh = meshGen.GenerateMesh(map, 1, colorSet[1], colorSet[0], maxValue, minValue, (int)drawMode);
        GetComponent<MeshFilter>().mesh = mesh;
        Texture2D tex = TextureGenerator.TextureFromHeightMap(map, (int)drawMode);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
    }

    void SmoothMap(int mapIndex) {

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                minValue = Mathf.Min(minValue, map[mapIndex, x, y]);
                maxValue = Mathf.Max(maxValue, map[mapIndex, x, y]);
            }
        }
    }

    void BuildMapAtIndex(int mapIndex, float newScale, float persistance, System.Random pseudoRandom, Vector2 offset)
    {
        float xOffset = (float)pseudoRandom.NextDouble() * 200000 - 100000 + offset.x;
        float yOffset = (float)pseudoRandom.NextDouble() * 200000 - 100000 + offset.y;
        for (int x = 0; x < width; x++)
        {
            for(int y=0; y < height; y++)
            {
                map[mapIndex, x, y] += (Mathf.PerlinNoise((x - halfWidth) / newScale + xOffset, (y - halfHeight) / newScale + yOffset) - .5f) * 2 * persistance;
            }
        }
    }
    
    int GetSurroundingWallCount(int gridX, int gridY)
    {
        int wallCount = 0;
        for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX++)
        {
            for(int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY++)
            {
                if(neighbourX >= 0 && neighbourX < width && neighbourY >= 0 && neighbourY < width)
                {
                    if (neighbourX != gridX || neighbourY != gridY)
                    {
                    }
                }
                else
                {
                    wallCount++;
                }
            }
        }
        return wallCount;
    }

    /*private void OnDrawGizmos()
    {
        if (map != null)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Gizmos.color = (map[x, y] == 1) ? Color.black : Color.white;
                    Vector3 pos = new Vector3(-width / 2 + x + .5f, 0, -height / 2 + y + .5f);
                    Gizmos.DrawCube(pos, Vector3.one);
                }
            }
        }
    }*/
}
