using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawMode { Terain, Fertility };

public class MapGenerator : MonoBehaviour
{

    public DrawMode drawMode;
    public int width;
    public int height;
    private int aWidth;
    private int aHeight;
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

    Color32[] colorSet;

    float[,,] map;
    private CityPoint[,] cityPoints;
    public GameObject CitySquareBase;
    public float scale;
    public float verticalScale;
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
        if (scale < 1)
        {
            scale = .0001f;
        }
    }

    public void GenerateMap() {
        aWidth = width + 1;
        aHeight = height + 1;
        map = new float[2, aWidth, aHeight];
        cityPoints = new CityPoint[aWidth, aHeight];
        halfHeight = aHeight / 2f;
        halfWidth = aWidth / 2f;
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
        if (drawMode != DrawMode.Terain)
        {
            SmoothMap((int)DrawMode.Terain);
        }
        SmoothMap((int)drawMode);
        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                cityPoints[x, y] = new CityPoint(map[0, x, y], map[1, x, y]);
            }
        }
        MeshGenerator meshGen = GetComponent<MeshGenerator>();
        Mesh mesh = meshGen.GenerateMesh(cityPoints, scale, verticalScale, colorSet[0], colorSet[1], width, height, drawMode);
        GetComponent<MeshFilter>().mesh = mesh;
        Texture2D tex = TextureGenerator.TextureFromHeightMap(map, (int)drawMode);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
    }

    float[] SmoothMap(int mapIndex) {


        float maxValue = -100;
        float minValue = 100;
        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                minValue = Mathf.Min(minValue, map[mapIndex, x, y]);
                maxValue = Mathf.Max(maxValue, map[mapIndex, x, y]);
            }
        }

        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                map[mapIndex, x, y] = Mathf.InverseLerp(minValue, maxValue, map[mapIndex, x, y]);
            }
        }
        return new float[] { minValue, maxValue };
    }

    void BuildMapAtIndex(int mapIndex, float newScale, float persistance, System.Random pseudoRandom, Vector2 offset)
    {
        float xOffset = (float)pseudoRandom.NextDouble() * 200000 - 100000 + offset.x;
        float yOffset = (float)pseudoRandom.NextDouble() * 200000 - 100000 + offset.y;
        for (int x = 0; x < aWidth; x++)
        {
            for(int y=0; y < aHeight; y++)
            {
                map[mapIndex, x, y] += (Mathf.PerlinNoise((x - halfWidth) / newScale + xOffset, (y - halfHeight) / newScale + yOffset) - .5f) * 2 * persistance;
            }
        }
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
