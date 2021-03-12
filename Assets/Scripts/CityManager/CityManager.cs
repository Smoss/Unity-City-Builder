using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public enum ClickMode { Road, Factory, Select, RZone, IZone, CZone }
public class CityManager : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    static HashSet<ClickMode> ZoningModesC = new HashSet<ClickMode>(new ClickMode[] { ClickMode.CZone, ClickMode.IZone, ClickMode.RZone });
    static HashSet<DrawMode> ZoningModesD = new HashSet<DrawMode>(new DrawMode[] { DrawMode.CZone, DrawMode.IZone, DrawMode.RZone });
    public HashSet<RealEstate> Properties;
    public GameObject Factory;
    public GameObject Home;
    public MapGenerator MapGenerator;
    CitySquare[,] cityTiles;
    float[,] propertyValues;
    private DrawMode selectedDrawMode;
    public DrawMode SelectedDrawMode
    {
        get
        {
            return selectedDrawMode;
        }
        set
        {
            selectedDrawMode = value;
            if (SelectedDrawModeName != null)
            {
                SelectedDrawModeName.text = Enum.GetName(typeof(DrawMode), selectedDrawMode);
            }
            this.drawTexture();
        }
    }
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
    private ClickMode selectedClickMode;
    public ClickMode SelectedClickMode
    {
        get
        {
            return selectedClickMode;
        }
        set
        {
            selectedClickMode = value;
            painting = false;
            if (SelectedClickModeName != null)
            {
                SelectedClickModeName.text = Enum.GetName(typeof(ClickMode), selectedClickMode);
            }
        }
    }
    public Dropdown dropdown;
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
    /*public Text selectedBuildingType;
    public Text selectedBuildingValue;*/
    public Text SelectedClickModeName;
    public Text SelectedDrawModeName;
    public Canvas SelectedItemInfo;
    public bool paused;
    //public Terrain terrain;
    //private TerrainData terrainData;
    public Single taxValue;
    private bool painting;
    private PointerEventData painterEvent;
    private PointerEventData.InputButton InitialButton;
    private Vector3 InitialVector;
    public Text taxText;
    public float trafficLimit;
    public void Build(Vector2 vector, bool addHuh, float assignValue = 1f)
    {
        int xLoc = (int)(vector.x), yLoc = (int)(vector.y);
        var roads = map[(int)DrawMode.RoadMap];
        var road = roads[xLoc, yLoc];
        var tile = cityTiles[xLoc, yLoc];
        switch (this.SelectedClickMode)
        {
            case ClickMode.RZone:
                map[(int)DrawMode.RZone][xLoc, yLoc] = addHuh ? 1 : 0;
                tile.ZonedFor.Add(Zoning.RZone);
                break;
            case ClickMode.IZone:
                map[(int)DrawMode.IZone][xLoc, yLoc] = addHuh ? 1 : 0;
                tile.ZonedFor.Add(Zoning.IZone);
                break;
            case ClickMode.CZone:
                map[(int)DrawMode.CZone][xLoc, yLoc] = addHuh ? 1 : 0;
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

    public void Zone(Vector2 vector, float assignValue = 1f)
    {
        int xLoc = (int)(vector.x), yLoc = (int)(vector.y);
        var tile = cityTiles[xLoc, yLoc];
        map[(int)SelectedClickMode + 3][xLoc, yLoc] = assignValue;
        if (assignValue == 1f)
        {
            tile.ZonedFor.Add((Zoning)SelectedClickMode);
        }
        else if (assignValue == 0f)
        {
            tile.ZonedFor.Remove((Zoning)SelectedClickMode);
        }
    }

    public void SetRZoning()
    {
        this.SelectedClickMode = ClickMode.RZone;
    }

    public void SetIZoning()
    {
        this.SelectedClickMode = ClickMode.IZone;
    }

    public void SetCZoning()
    {
        this.SelectedClickMode = ClickMode.CZone;
    }

    public void SetDrawMode(Int32 drawMode)
    {
        SelectedDrawMode = (DrawMode)drawMode;
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
            this.SelectedClickMode = ClickMode.Factory;
        }
        if(Input.GetKeyDown(KeyCode.R))
        {
            this.SelectedClickMode = ClickMode.Road;
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            this.SelectedClickMode = ClickMode.Select;
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            this.selectedDrawMode = DrawMode.RoadMap;
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            this.selectedDrawMode = DrawMode.RZone;
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            this.selectedDrawMode = DrawMode.IZone;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            this.selectedDrawMode = DrawMode.CZone;
        }
        dropdown.value = (int)this.selectedDrawMode;
        updateUI();
        resetTextureSquares();
        var tempPainting = painting;
        painting = painting && painterEvent != null && ClickMode.Factory != SelectedClickMode && continuePainting(painterEvent);
        if (tempPainting && !painting)
        {
            drawTexture();
        }
        if (painting)
        {
            if (ZoningModesC.Contains(selectedClickMode))
            {
                highlightSquare(.5f);
            }
            else
            {
                Build(convertToCitySquare(painterEvent.pointerCurrentRaycast.worldPosition), painterEvent.button == PointerEventData.InputButton.Left);
            }
        }
        //Change this to use ticks
        //Allows simulation to be paused
        if (!this.paused)
        {
            tick++;
        }
        // Only update the game state every numberOfTicks ticks
        // This will speed up calculation time
        if(tick > numberOfTicks)
        {
            CalculatePropertyValues();
            ScheduleBuilds();
            SetColors();
            CalculateEconomy();
            tick = 0;
        }
    }

    private void resetTextureSquares()
    {
        if(ZoningModesD.Contains(SelectedDrawMode) && ((int)selectedClickMode + 3) == (int)selectedDrawMode && painting)
        {
            var drawMap = map[(int)selectedClickMode + 3];
            for (int x = 0; x < cityTiles.GetLength(0); x++)
            {
                for(int y = 0; y < cityTiles.GetLength(1); y++)
                {
                    drawMap[x, y] = cityTiles[x, y].ZonedFor.Contains((Zoning)selectedClickMode) ? 1 : 0;
                }
            }
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
    // This currently doesn't do anything
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
        if (taxValue > 1)
        {
            taxValue = 1;
        }
        if (taxValue < 0)
        {
            taxValue = 0;
        }
        if (trafficLimit == 0)
        {
            trafficLimit = .1f;
        }
    }

    private void SetColors()
    {
        foreach(RealEstate estate in Properties)
        {
            //estate.SetTexture(minREValue, maxREValue);
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
                // Find available unoccupied real estate slots and add them to the options for a given occupation
                foreach (var commute in occ.Location.CitySquare.Commutes)
                {
                    var dest = commute.Squares[commute.Squares.Count - 1];
                    if (dest.RealEstate != null && dest.RealEstate.OpenUnits && dest.RealEstate.Price < occ.Income * HousingIncomeMultiplier)
                    {
                        options.Add(commute);
                    }
                }
                // If there's available housing for jobs, fill it with a worker
                if(options.Count >= 1)
                {
                    float best = float.MaxValue;
                    int index = 0;
                    for (int x = 0; x < options.Count; x++)
                    {
                        var commute = options[x];
                        var dest = commute.Squares[commute.Squares.Count - 1];
                        // Shorter commutes with lower REValues are better(assumed to be renters and not owners for now)
                        var reValue = dest.RealEstateValue + commute.Length * 100;
                        if (reValue < best)
                        {
                            index = x;
                            best = reValue;
                        }
                    }
                    // Applicant pool is currently assumed to be unlimited so a worker will be found immediately
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
                List<GameObject> Buildables = new List<GameObject>();
                CitySquare tile = cityTiles[x, y];
                if (
                    tile.HousingValue > Home.GetComponent<RealEstate>().maxOccupants * costOfLiving * HousingMultiplier &&
                    tile.RealEstate == null && 
                    !tile.HasRoad && 
                    tile.HousingValue > tile.RealEstateValue &&
                    tile.ZonedFor.Contains(Zoning.RZone)
                )
                {
                    Buildables.Add(Home);
                }
                if (tile.ProductivityValue > 20000 * FactoryMultiplier &&
                    tile.RealEstate == null &&
                    !tile.HasRoad &&
                    tile.ProductivityValue > tile.RealEstateValue &&
                    tile.ZonedFor.Contains(Zoning.IZone)
                )
                {
                    Buildables.Add(Factory);
                }
                // If the land is valuable enough there's a chance something will get built
                if(Buildables.Count > 0 && UnityEngine.Random.value > .8f)
                {
                    BuildProperty(Buildables[(int) (UnityEngine.Random.value * Buildables.Count)], tile, this.Economy);
                }
            }
        }
    }

    private void BuildProperty(GameObject newConstruction, CitySquare tile, EconomicUnit owner)
    {
        DemolishProperty(tile);
        if (tile.RealEstate == null)
        {
            Vector2 index = offsetToIndex(tile.Offset);
            GameObject newRE = Instantiate(newConstruction);
            RealEstate newREVal = newRE.GetComponent<RealEstate>();
            int mapIndex = (int)newREVal.zone + 3;
            map[mapIndex][(int)index.x, (int)index.y] = 1;
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
        float minTraffic = float.MaxValue;
        float maxTraffic = 0;
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                map[(int)DrawMode.Traffic][x, y] = cityTiles[x, y].Traffic / trafficLimit;
                float propertyValue = cityTiles[x, y].CalculatePropertyValues(commuteDist, pollutionExp);
                switch (SelectedDrawMode)
                {
                    case DrawMode.ShopValue:
                        propertyValue = cityTiles[x, y].ShopValue;
                        propertyValues[x, y] = propertyValue;
                        maxREValue = 100000;
                        minREValue = 0;
                        break;
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
        if (SelectedDrawMode == DrawMode.PropertyValue || SelectedDrawMode == DrawMode.HousingValue || SelectedDrawMode == DrawMode.ProductivityValue || SelectedDrawMode == DrawMode.ShopValue)
        {
            for (int x = 0; x < cityTiles.GetLength(0); x++)
            {
                for (int y = 0; y < cityTiles.GetLength(1); y++)
                {
                    map[(int)SelectedDrawMode][x, y] = Mathf.InverseLerp(minREValue, maxREValue, propertyValues[x, y]);
                }
            }
        }
        drawTexture();
    }
    private void init()
    {
        SelectedClickMode = selectedClickMode;
        SelectedDrawMode = selectedDrawMode;
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
        colorSet = new Color32[2];
        init();
        //terrainData = terrain.terrainData;
        MapGenerator.drawMode = SelectedDrawMode;
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

    public void drawTexture()
    {
        switch(this.selectedDrawMode)
        {
            case DrawMode.RoadMap:
                colorSet[0] = Color.black;
                colorSet[1] = Color.red;
                break;
            case DrawMode.RZone:
                colorSet[0] = Color.green;
                colorSet[1] = Color.red;
                break;
            case DrawMode.CZone:
                colorSet[0] = Color.blue;
                colorSet[1] = Color.red;
                break;
            case DrawMode.IZone:
                colorSet[0] = Color.yellow;
                colorSet[1] = Color.red;
                break;
            default:
                colorSet[0] = Color.green;
                colorSet[1] = Color.red;
                break;
        }
        if (map != null)
        {
            Texture2D tex = TextureGenerator.TextureFromHeightMap(map[(int)SelectedDrawMode], colorSet);
            GetComponent<MeshRenderer>().sharedMaterial.mainTexture = tex;
        }
    }

    private Vector2 offsetToIndex(Vector3 coord)
    {
        var tempVector = new Vector2(coord.x, coord.z) + (this.MapGenerator.scale * new Vector2(cityTiles.GetLength(0) / 2f, cityTiles.GetLength(1) / 2f));
        return new Vector2((float)Math.Floor(tempVector.x), (float)(cityTiles.GetLength(1) - Math.Ceiling(tempVector.y)));
    }

    private Vector2 convertToCitySquare(Vector3 worldPosition)
    {
        Vector3 worldCoord = worldPosition - this.transform.position;
        return offsetToIndex(worldCoord);
    }

    private bool continuePainting(PointerEventData eventData)
    {
        return !(
            SelectedClickMode == ClickMode.Select ||
            eventData.pointerCurrentRaycast.gameObject != this.gameObject ||
            eventData.button == PointerEventData.InputButton.Middle
        );
    }

    // Controls highlighting the square a given color while zoning
    private void highlightSquare(float assignValue = 1f, Vector3? initialPosition = null)
    {
        var startSquare = convertToCitySquare(initialPosition ?? painterEvent.pointerPressRaycast.worldPosition);
        var endSquare = convertToCitySquare(painterEvent.pointerCurrentRaycast.worldPosition);
        var beginCoord = Vector2.Min(startSquare, endSquare);
        var endCoord = Vector2.Max(startSquare, endSquare);
        for (int x = (int)beginCoord.x; x <= endCoord.x; x++)
        {
            for (int y = (int)beginCoord.y; y <= endCoord.y; y++)
            {
                Zone(new Vector2(x, y), assignValue);
            }
        }
        drawTexture();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (ZoningModesC.Contains(selectedClickMode) && !painting)
        {
            InitialVector = eventData.pointerCurrentRaycast.worldPosition;
            InitialButton = eventData.button;
            painting = true;
            painterEvent = eventData;
        }
        else if (painting && ZoningModesC.Contains(selectedClickMode))
        {
            resetTextureSquares();
            painting = false;
            drawTexture();
        }
        else
        {
            painting = continuePainting(eventData) && (!painting || ((painterEvent.button) != eventData.button));
            if (!painting)
            {
                return;
            }
            painterEvent = eventData;
            Build(convertToCitySquare(eventData.pointerPressRaycast.worldPosition), eventData.button == PointerEventData.InputButton.Left);
        }
    }
    public void ChangeTaxes(Single _taxes)
    {
        taxValue = _taxes / 100;
        if (taxText != null)
        {
            taxText.text = "Tax Rate: " + _taxes.ToString() + "%";
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (painting && ZoningModesC.Contains(selectedClickMode) && painterEvent != null && eventData.button == painterEvent.button)
        {
            highlightSquare(painterEvent.button == PointerEventData.InputButton.Left ? 1 : 0, InitialVector);
            painting = false;
            resetTextureSquares();
            return;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        resetTextureSquares();
        painting = false;
        drawTexture();
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
