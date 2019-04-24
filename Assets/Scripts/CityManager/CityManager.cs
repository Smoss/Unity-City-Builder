﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum ClickMode { Road, Factory, Select, RZone, IZone, CZone }
public class CityManager : ClickAccepter
{
    public HashSet<RealEstate> Properties;
    public GameObject Factory;
    public GameObject Home;
    public MapGenerator MapGenerator;
    CitySquare[,] cityTiles;
    float[,] propertyValues;
    public DrawMode drawMode;
    Color32[] colorSet;
    List<float[,]> map;
    //float timeSince;
    public int tick;
    public int numberOfTicks;
    public int commuteDist;
    public float pollutionExp;
    private float maxREValue;
    private float minREValue;
    public GameObject Human;
    HashSet<Occupation> occupations;
    public ClickMode clickMode; 
    public List<Human> humans { get; private set; }
    public int population;
    public float costOfLiving;
    const float FactoryMultiplier = 1;
    const float HousingMultiplier = 1;
    const float HousingIncomeMultiplier = 5;
    public float EconomicOutput;
    public float EconomicSurplus;
    public float TaxRevenue;
    public EconomicUnit Economy;
    public EconomicUnit Government;
    private RealEstate SelectedRealEstate;
    public Text selectedBuildingType;
    public Text selectedBuildingValue;
    public Canvas SelectedItemInfo;
    public bool paused;
    //public Terrain terrain;
    //private TerrainData terrainData;
    public float taxValue;
    public void Build(Vector2 vector, bool addHuh)
    {
        int xLoc = (int)(vector.x * (cityTiles.GetLength(0) + 1)), yLoc = (int)(vector.y * (cityTiles.GetLength(1) + 1));
        var roads = map[(int)DrawMode.RoadMap];
        var road = roads[xLoc, yLoc];
        var tile = cityTiles[xLoc, yLoc];
        switch (this.clickMode)
        {
            case ClickMode.RZone:
                map[(int)DrawMode.RZone][xLoc, yLoc] = 1;
                tile.ZonedFor.Add(Zoning.RZone);
                break;
            case ClickMode.IZone:
                map[(int)DrawMode.IZone][xLoc, yLoc] = 1;
                tile.ZonedFor.Add(Zoning.IZone);
                break;
            case ClickMode.CZone:
                map[(int)DrawMode.CZone][xLoc, yLoc] = 1;
                tile.ZonedFor.Add(Zoning.CZone);
                break;
            case ClickMode.Factory:
                roads[xLoc, yLoc] = 0;
                if (addHuh)
                {
                    BuildProperty(this.Factory, tile, this.Government);
                }
                else
                {
                    DemolishProperty(tile);
                }
                break;
            case ClickMode.Road:
                roads[xLoc, yLoc] = addHuh ? 1 : 0;
                cityTiles[xLoc, yLoc].HasRoad = addHuh;
                break;
            default:
                break;
        }
        drawTexture();
    }

    public void SetRZoning()
    {
        this.clickMode = ClickMode.RZone;
    }

    public void SetIZoning()
    {
        this.clickMode = ClickMode.IZone;
    }

    public void SetCZoning()
    {
        this.clickMode = ClickMode.CZone;
    }

    // Start is called before the first frame update
    void Start()
    {
        GenerateMap();
    }

    // Update is called once per frame
    void Update()
    {
        population = humans.Count;
        if(Input.GetKeyDown(KeyCode.P))
        {
            this.paused = !this.paused;
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
            this.clickMode = ClickMode.Factory;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            this.clickMode = ClickMode.Road;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.clickMode = ClickMode.Select;
        }
        var mouseDown = (Input.GetMouseButton(0) || Input.GetMouseButton(1));
        if (mouseDown)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit = new RaycastHit();
            if (Physics.Raycast(ray, out hit))
            {
                ClickAccepter accepter = hit.collider.GetComponent<ClickAccepter>();
                if (accepter)
                {
                    accepter.Accept(hit.textureCoord, clickMode, Input.GetMouseButton(0));
                }
                
            }
            else if (clickMode == ClickMode.Select)
            {
                setSelectedBuilding(null);
            }
        }
        updateUI();
        //Change this to use ticks
        if (!this.paused)
        {
            tick++;
        }
        if(tick > numberOfTicks)
        {
            CalculatePropertyValues();
            ScheduleBuilds();
            SetColors();
            CalculateEconomy();
            tick = 0;
        }
    }

    private void updateUI()
    {
        string buildingType = "";
        string price = "";
        if (SelectedRealEstate != null)
        {
            switch (SelectedRealEstate.type)
            {
                case PropertyType.Home:
                    buildingType = "Home";
                    break;
                default:
                    buildingType = "Factory";
                    break;
            }
            price = SelectedRealEstate.Price.ToString();
        }
        /*this.selectedBuildingType.text = buildingType;
        this.selectedBuildingValue.text = price;*/
    }

    private void minimizeUIs()
    {
        foreach (RealEstate re in Properties)
        {
            re.minimizeUI();
        }
    }
    
    public void setSelectedBuilding(RealEstate property)
    {
        this.SelectedRealEstate = property;
    }

    private void CalculateEconomy()
    {
        this.EconomicOutput = Economy.Income;
        this.EconomicSurplus = EconomicOutput - Economy.Expenses;
        this.TaxRevenue = Government.Income;
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
        occupations = new HashSet<Occupation>();
        foreach(var prop in Properties)
        {
            occupations.UnionWith(prop.OccupationsList);
            prop.CalculateTaxes();
        }
        foreach(var occ in occupations)
        {
            if (occ.Employee == null)
            {
                var options = new List<Route>();
                foreach (var commute in occ.Location.CitySquare.Commutes)
                {
                    var dest = commute.Squares[commute.Squares.Count - 1];
                    if (dest.RealEstate != null && dest.RealEstate.OpenUnits && dest.RealEstate.Price < occ.Income * HousingIncomeMultiplier)
                    {
                        options.Add(commute);
                    }
                }
                if(options.Count > 1)
                {
                    float best = float.MaxValue;
                    int index = 0;
                    for (int x = 0; x < options.Count; x++)
                    {
                        var commute = options[x];
                        var dest = commute.Squares[commute.Squares.Count - 1];
                        var reValue = dest.RealEstateValue + commute.Length * 100;
                        if (reValue < best)
                        {
                            index = x;
                            best = reValue;
                        }
                    }
                    var optimalHome = options[index].Squares[options[index].Squares.Count - 1];
                    GameObject newHuman = Instantiate(Human);
                    Human human = newHuman.GetComponent<Human>();
                    humans.Add(human);
                    human.init(this, optimalHome.RealEstate, occ.Requirements, this.Economy);
                    occ.interview(human);
                    optimalHome.RealEstate.addOccupant(human);
                }

            }
        }
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                CitySquare tile = cityTiles[x, y];
                if (
                    tile.HousingValue > Home.GetComponent<RealEstate>().maxOccupants * costOfLiving * HousingMultiplier &&
                    tile.RealEstate == null && 
                    !tile.HasRoad && 
                    tile.HousingValue > tile.RealEstateValue
                )
                {
                    BuildProperty(Home, tile, this.Economy);
                }
                else if (tile.ProductivityValue > 200000 * FactoryMultiplier &&
                    tile.RealEstate == null &&
                    !tile.HasRoad &&
                    tile.ProductivityValue > tile.RealEstateValue
                )
                {
                    BuildProperty(Factory, tile, this.Economy);
                }
            }
        }
    }

    private void BuildProperty(GameObject newConstruction, CitySquare tile, EconomicUnit owner)
    {
        DemolishProperty(tile);
        if (tile.RealEstate == null)
        {
            GameObject newRE = Instantiate(newConstruction);
            RealEstate newREVal = newRE.GetComponent<RealEstate>();
            Properties.Add(newREVal);
            newRE.transform.parent = this.transform;
            tile.AddPropertyCentral(newREVal, owner);
        }
    }

    private void DemolishProperty(CitySquare tile)
    {
        tile.RemoveProperty();
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
                switch (drawMode)
                {
                    case DrawMode.HousingValue:
                        propertyValue = cityTiles[x, y].HousingValue;
                        propertyValues[x, y] = propertyValue;
                        maxREValue = 240000;
                        minREValue = 0;
                        break;
                    case DrawMode.ProductivityValue:
                        propertyValue = cityTiles[x, y].ProductivityValue;
                        propertyValues[x, y] = propertyValue;
                        maxREValue = 360000;
                        minREValue = 0;
                        break;
                    default:
                        propertyValues[x, y] = propertyValue;
                        maxREValue = Mathf.Max(propertyValue, maxREValue);
                        minREValue = Mathf.Min(propertyValue, minREValue);
                        break;
                }
            }
        }
        if(drawMode == DrawMode.PropertyValue || drawMode == DrawMode.HousingValue || drawMode == DrawMode.ProductivityValue)
        {
            for (int x = 0; x < cityTiles.GetLength(0); x++)
            {
                for (int y = 0; y < cityTiles.GetLength(1); y++)
                {
                    map[(int)drawMode][x, y] = Mathf.InverseLerp(minREValue, maxREValue, propertyValues[x, y]);
                }
            }
            drawTexture();
        }
    }
    private void init()
    {
        humans = new List<Human>();
        Economy = new EconomicUnit(null);
        Government = new EconomicUnit(Economy);
        //RealEstate reTest = Factory.GetComponent<RealEstate>();
        tick = numberOfTicks;
        occupations = new HashSet<Occupation>();
        foreach (var occ in occupations)
        {
            occ.Employee = null;
        }
        if (Properties != null)
        {
            foreach (RealEstate g in Properties)
            {
                //if (g.gameObject != Factory)
                    //Destroy(g.gameObject);
            }
        }
        Properties = new HashSet<RealEstate>();
        //Properties.Add(Factory.GetComponent<RealEstate>());
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
        //MapGenerator.CityTiles[MapGenerator.MaxLoc[0], MapGenerator.MaxLoc[1]].AddPropertyCentral(Factory.GetComponent<RealEstate>());
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

    public override void Accept(Vector2 vec, ClickMode mode, bool isLeftMouse)
    {
        Build(vec, isLeftMouse);
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
