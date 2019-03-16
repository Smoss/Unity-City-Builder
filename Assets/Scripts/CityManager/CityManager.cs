using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CityManager : MonoBehaviour
{
    HashSet<GameObject> Properties;
    public GameObject Factory;
    public GameObject Home;
    public MapGenerator MapGenerator;
    CitySquare[,] cityTiles;
    float[,] propertyValues;
    readonly Guid id;
    public DrawMode drawMode;
    Color32[] colorSet;
    float[,,] map;
    //float timeSince;
    public int tick;
    public int commuteDist;
    public float pollutionExp;
    public Guid ID
    {
        get { return id; }
    }
    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        //Change this to use ticks
        tick++;
        if(tick > 1000)
        {
            CalculatePropertyValues();
            ScheduleBuilds();
            tick = 0;
        }
    }
    private void OnValidate()
    {
        if (commuteDist <= 0)
        {
            commuteDist = 1;
        }
    }

    private void ScheduleBuilds()
    {
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                if(cityTiles[x, y].RealEstateValue > 1000 && cityTiles[x, y].RealEstate == null)
                {
                    GameObject newRE = Instantiate(Home);
                    Properties.Add(newRE);
                    newRE.transform.parent = this.transform;
                    cityTiles[x, y].AddPropertyCentral(newRE);
                }
            }
        }
    }

    private void CalculatePropertyValues()
    {
        float minValue = float.MaxValue;
        float maxValue = float.MinValue;
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                CitySquare localSquare = cityTiles[x, y];
                HashSet<CitySquareDist> nearbyTiles = NearbyTiles(commuteDist, localSquare);
                float propertyValue = -Mathf.Pow(localSquare.Pollution, pollutionExp);
                foreach (CitySquareDist tile in nearbyTiles)
                {
                    CitySquare square = tile.tile;
                    propertyValue += square.AvgProductivity * 10 / (2 * tile.distance) - Mathf.Pow(square.Pollution / (tile.distance + 1), pollutionExp) + 1;
                }
                localSquare.RealEstateValue = propertyValue;
                propertyValues[x, y] = propertyValue;
                maxValue = Mathf.Max(propertyValue, maxValue);
                minValue = Mathf.Min(propertyValue, minValue);
            }
        }
        if(drawMode == DrawMode.PropertyValue)
        {
            for (int x = 0; x < cityTiles.GetLength(0); x++)
            {
                for (int y = 0; y < cityTiles.GetLength(1); y++)
                {
                    map[2, x, y] = Mathf.InverseLerp(minValue, maxValue, propertyValues[x, y]);
                }
            }
            Texture2D tex = TextureGenerator.TextureFromHeightMap(map, (int)drawMode, colorSet);
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
        }
    }

    private HashSet<CitySquareDist> NearbyTiles(int nearbyDist, CitySquare originalTile)
    {
        HashSet<CitySquareDist> nearbyTiles = new HashSet<CitySquareDist>();
        HashSet<CitySquare> tilesToSearch = new HashSet<CitySquare>(originalTile.Neighbors);
        for (int dist = 1; dist <= nearbyDist; dist++)
        {
            HashSet<CitySquare> nextTilesToSearch = new HashSet<CitySquare>();
            foreach (CitySquare tile in tilesToSearch)
            {
                CitySquareDist neighborTile = new CitySquareDist(dist, tile);
                if(!nearbyTiles.Contains(neighborTile) && neighborTile.tile != originalTile)
                {
                    nearbyTiles.Add(neighborTile);
                    nextTilesToSearch.UnionWith(tile.Neighbors);
                }
            }
            tilesToSearch = nextTilesToSearch;
        }
        return nearbyTiles;
    }
    private void init()
    {
        tick = 1000;
        if (Properties != null)
        {
            foreach (GameObject g in Properties)
            {
                if (g != Factory)
                    Destroy(g);
            }
        }
        Properties = new HashSet<GameObject>();
        Properties.Add(Factory);
    }
    public void GenerateMap()
    {
        init();
        colorSet = new Color32[2];
        colorSet[0] = Color.green;
        colorSet[1] = Color.red;
        MapGenerator.drawMode = drawMode;
        MapGenerator.colorSet = colorSet;
        map = MapGenerator.GenerateMap();

        cityTiles = MapGenerator.CityTiles;
        propertyValues = new float[MapGenerator.width, MapGenerator.height];
        MapGenerator.CityTiles[MapGenerator.MaxLoc[0], MapGenerator.MaxLoc[1]].AddPropertyCentral(Factory);
        Texture2D tex = TextureGenerator.TextureFromHeightMap(map, (int)drawMode, colorSet);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
    }
}
public class CitySquareDist
{
    public int distance;
    public CitySquare tile;
    public CitySquareDist(int _distance, CitySquare _tile)
    {
        distance = _distance;
        tile = _tile;
    }

    public override bool Equals(System.Object obj)
    {
        if(obj is CitySquareDist)
        {
            return tile == ((CitySquareDist)obj).tile;
        }
        return false;
    }

    public override int GetHashCode()
    {
        return -1987462863 + EqualityComparer<CitySquare>.Default.GetHashCode(tile);
    }
}
