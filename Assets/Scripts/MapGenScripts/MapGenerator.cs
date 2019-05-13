using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DrawMode { Terain, Fertility, PropertyValue, HousingValue, ProductivityValue, RoadMap, RZone, IZone, CZone, Traffic, ShopValue };
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

    public Color32[] colorSet;

    List<float [,]> map;
    private CityPoint[,] cityPoints;
    public float scale;
    public float vScale;
    CitySquare[,] cityTiles;
    int[] maxLoc;
    public MeshCollider collider;
    public int[] MaxLoc {
        get { return maxLoc; }
    }
    public CitySquare[,] CityTiles
    {
        get { return cityTiles; }
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

    public List<float[,]> GenerateMap(CityManager city) {
        aWidth = width + 1;
        aHeight = height + 1;
        map = new List<float[,]>();
        for (int x = 0; x < 11; x++)
        {
            map.Add(new float[aWidth, aHeight]);
        }
        cityPoints = new CityPoint[aWidth, aHeight];
        cityTiles = new CitySquare[width, height];
        halfHeight = aHeight / 2f;
        halfWidth = aWidth / 2f;
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
        SmoothMap((int)DrawMode.Terain);
        SmoothMap((int)DrawMode.Fertility);
        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                cityPoints[x, y] = new CityPoint(map[0][x, y], map[0][x, y]);
            }
        }
        MakeTiles(cityPoints, width, height, scale, vScale, city);
        Mesh mesh = MeshGenerator.GenerateMesh(scale, vScale, drawMode, cityTiles);
        GetComponent<MeshFilter>().mesh = mesh;
        collider.sharedMesh = mesh;
        return map;
    }

    float[] SmoothMap(int mapIndex) {

        maxLoc = new int[2];
        float maxValue = -100;
        float minValue = 100;
        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                minValue = Mathf.Min(minValue, map[mapIndex][x, y]);
                if (map[mapIndex][x, y] > maxValue)
                {
                    maxValue = map[mapIndex][x, y];
                    maxLoc = new int[] { Mathf.Min(x, width - 1), Mathf.Min(y, height - 1) };
                }
            }
        }

        for (int x = 0; x < aWidth; x++)
        {
            for (int y = 0; y < aHeight; y++)
            {
                map[mapIndex][x, y] = Mathf.InverseLerp(minValue, maxValue, map[mapIndex][x, y]);
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
                map[mapIndex][x, y] += (Mathf.PerlinNoise((x - halfWidth) / newScale + xOffset, (y - halfHeight) / newScale + yOffset) - .5f) * 2 * persistance;
            }
        }
    }

    void MakeTiles(CityPoint[,] cityPoints, int width, int height, float scale, float vScale, CityManager city)
    {
        float halfWidth = width / 2f - .5f;
        float halfHeight = height / 2f - .5f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //CitySquareMeshes[x, y] = Instantiate(CitySquareBase, transform.position + , Quaternion.identity);
                List<CityPoint> neighbors = new List<CityPoint>();
                for (int v = 0; v < 2; v++)
                {
                    for (int u = 0; u < 2; u++)
                    {
                        neighbors.Add(cityPoints[u + x, v + y]);
                    }
                }
                cityTiles[x, y] = new CitySquare(
                    neighbors.ToArray(),
                    scale,
                    vScale,
                    drawMode,
                    new Vector3((x - halfWidth), 0, (halfHeight - y)),
                    city
                );
                if( x > 0)
                {
                    cityTiles[x - 1, y].addNeighbor(cityTiles[x, y]);
                }
                if (y > 0)
                {
                    cityTiles[x, y - 1].addNeighbor(cityTiles[x, y]);
                }
            }
        }
    }
}
