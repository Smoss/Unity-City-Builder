using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CityManager : MonoBehaviour
{
    HashSet<RealEstate> Properties;
    public GameObject Factory;
    public GameObject Home;
    public MapGenerator MapGenerator;
    CitySquare[,] cityTiles;
    float[,] propertyValues;
    readonly Guid id;
    public DrawMode drawMode;
    Color32[] colorSet;
    List<float[,]> map;
    //float timeSince;
    public int tick;
    public int commuteDist;
    public float pollutionExp;
    private float maxREValue;
    private float minREValue;
    //public Terrain terrain;
    //private TerrainData terrainData;
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
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Mouse Down hit: " + hit.collider.name);
            }
        }
        //Change this to use ticks
        tick++;
        if(tick > 1000)
        {
            CalculatePropertyValues();
            ScheduleBuilds();
            SetColors();
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

    private void SetColors()
    {
        foreach(RealEstate estate in Properties)
        {
            estate.SetTexture(minREValue, maxREValue);
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
                    RealEstate newREVal = newRE.GetComponent<RealEstate>();
                    Properties.Add(newREVal);
                    newRE.transform.parent = this.transform;
                    cityTiles[x, y].AddPropertyCentral(newREVal);
                }
            }
        }
    }

    private void CalculatePropertyValues()
    {
        minREValue = float.MaxValue;
        maxREValue = float.MinValue;
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
                maxREValue = Mathf.Max(propertyValue, maxREValue);
                minREValue = Mathf.Min(propertyValue, minREValue);
            }
        }
        if(drawMode == DrawMode.PropertyValue)
        {
            for (int x = 0; x < cityTiles.GetLength(0); x++)
            {
                for (int y = 0; y < cityTiles.GetLength(1); y++)
                {
                    map[2][x, y] = Mathf.InverseLerp(minREValue, maxREValue, propertyValues[x, y]);
                }
            }
            Texture2D tex = TextureGenerator.TextureFromHeightMap(map[(int)drawMode], colorSet);
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
            foreach (RealEstate g in Properties)
            {
                if (g != Factory.GetComponent<RealEstate>())
                    Destroy(g);
            }
        }
        Properties = new HashSet<RealEstate>();
        Properties.Add(Factory.GetComponent<RealEstate>());
    }
    public void GenerateMap()
    {
        init();
        //terrainData = terrain.terrainData;
        colorSet = new Color32[2];
        colorSet[0] = Color.green;
        colorSet[1] = Color.red;
        MapGenerator.drawMode = drawMode;
        MapGenerator.colorSet = colorSet;
        map = MapGenerator.GenerateMap();

        cityTiles = MapGenerator.CityTiles;
        propertyValues = new float[MapGenerator.width, MapGenerator.height];
        MapGenerator.CityTiles[MapGenerator.MaxLoc[0], MapGenerator.MaxLoc[1]].AddPropertyCentral(Factory.GetComponent<RealEstate>());
        Texture2D tex = TextureGenerator.TextureFromHeightMap(map[(int)drawMode], colorSet);
        int width = cityTiles.GetLength(0);
        int height = cityTiles.GetLength(1);
        float[,] tMap = new float[height, width];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tMap[y, x] = map[0][x, y];
            }
        }
        //terrainData.size = new Vector3(height, 1, width);
        //terrainData.SetHeights(0, 0, tMap);
        //RenderTexture = new RenderTexture()
        //terrainData.heightmapTexture = tex;
        //terrain.terrainData = terrainData;
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
