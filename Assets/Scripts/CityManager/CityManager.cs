using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CityManager : ClickAccepter
{
    public HashSet<RealEstate> Properties;
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
    public GameObject Human;
    //public Terrain terrain;
    //private TerrainData terrainData;
    public Guid ID
    {
        get { return id; }
    }
    public void BuildRoad(Vector2 vector)
    {
        bool addRoad = Input.GetMouseButton(0);
        int xLoc = (int)(vector.x * (cityTiles.GetLength(0) + 1)), yLoc = (int)(vector.y * (cityTiles.GetLength(1) + 1));
        var roads = map[3];
        var road = roads[xLoc, yLoc];
        roads[xLoc, yLoc] = addRoad ? 1 : 0;
        cityTiles[xLoc, yLoc].HasRoad = addRoad;
        if (drawMode == DrawMode.RoadMap)
        {
            drawTexture();
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButton(0) || Input.GetMouseButton(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                ClickAccepter accepter = hit.collider.GetComponent<ClickAccepter>();
                if (accepter)
                {
                    accepter.Accept(hit.textureCoord);
                }
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
                CitySquare tile = cityTiles[x, y];
                if (tile.RealEstateValue > 1000 && tile.RealEstate == null && !tile.HasRoad)
                {
                    GameObject newRE = Instantiate(Home);
                    RealEstate newREVal = newRE.GetComponent<RealEstate>();
                    Properties.Add(newREVal);
                    newRE.transform.parent = this.transform;
                    tile.AddPropertyCentral(newREVal);
                    GameObject newHuman = Instantiate(Human);
                    Human human = newHuman.GetComponent<Human>();
                    human.init(this, newREVal, Factory.GetComponent<RealEstate>());
                    newREVal.addOccupant(human);
                }
                else if (tile.RealEstateValue < 1000 && tile.RealEstate != null)
                {
                    if(tile.RealEstate != Factory.GetComponent<RealEstate>())
                    {
                        tile.RemoveProperty();
                    }
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
                float propertyValue = cityTiles[x, y].CalculatePropertyValues(commuteDist, pollutionExp);
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
            drawTexture();
        }
    }
    private void init()
    {
        tick = 1000;
        if (Properties != null)
        {
            foreach (RealEstate g in Properties)
            {
                if (g != Factory.GetComponent<RealEstate>())
                    Destroy(g.gameObject);
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
        map = MapGenerator.GenerateMap(this);

        cityTiles = MapGenerator.CityTiles;
        propertyValues = new float[MapGenerator.width, MapGenerator.height];
        MapGenerator.CityTiles[MapGenerator.MaxLoc[0], MapGenerator.MaxLoc[1]].AddPropertyCentral(Factory.GetComponent<RealEstate>());
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
        drawTexture();
    }

    public override void Accept(Vector2 vec)
    {
        BuildRoad(vec);
    }

    public void drawTexture()
    {

        Texture2D tex = TextureGenerator.TextureFromHeightMap(map[(int)drawMode], colorSet);
        GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
    }
}
public class CitySquareDist
{
    public int distance;
    public CitySquare tile;
    public bool roadAccess;
    public int driveDistance;
    public CitySquareDist(int _distance, CitySquare _tile, bool _roadAccess)
    {
        distance = _distance;
        tile = _tile;
        roadAccess = _roadAccess;
        driveDistance = _distance;
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
