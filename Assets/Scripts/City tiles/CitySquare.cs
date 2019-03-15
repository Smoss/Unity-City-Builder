using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction       { NW, NE, SW, SE };
public enum MirrorDirection { SE, SW, NE, NW };
public class CitySquare
{
    static Direction[] clockwise = new Direction[] {
        Direction.NE,
        Direction.SE,
        Direction.SW,
        Direction.NW,
    };
    static Vector3[] clockwiseVs = new Vector3[] {
        new Vector3(-.5f, 0, .5f),
        new Vector3(.5f, 0, .5f),
        new Vector3(-.5f, 0, -.5f),
        new Vector3(.5f, 0, -.5f),
    };
    public Vector3[] vertices;
    Dictionary<Direction, CityPoint> corners;
    float scale;
    float vScale;
    DrawMode drawMode;
    Texture2D tex;
    public int xPos;
    public int zPos;
    float height;
    float fertility;
    Vector3 offset;
    float realEstateValue;
    HashSet<CitySquare> neighbors;
    RealEstate realEstate;
    //float productivity;
    public float Productivity {
        get { return realEstate != null ? realEstate.Productivity : 0; }
    }
    public float AvgProductivity
    {
        get { return realEstate != null ? realEstate.AvgProductivity : 0; }
    }
    public float RealEstateValue {
        get { return realEstateValue; }
        set { realEstateValue = value; }
    }
    public RealEstate RealEstate
    {
        get { return realEstate; }
    }
    public HashSet<CitySquare> Neighbors
    {
        get { return neighbors; }
    }
    public float Height
    {
        get { return height; }
    }
    public float Fertility
    {
        get { return fertility; }
    }
    public void addNeighbor(CitySquare _neighbor)
    {
        _neighbor.Neighbors.Add(this);
        this.neighbors.Add(_neighbor);
    }
    public CitySquare(
        CityPoint[] _corners,
        float _scale,
        float _vScale,
        DrawMode _drawMode,
        Vector3 _offset
    )
    {
        neighbors = new HashSet<CitySquare>();
        offset = _offset;
        drawMode = _drawMode;
        vScale = _vScale;
        vertices = new Vector3[4];
        corners = new Dictionary<Direction, CityPoint>();
        scale = _scale;
        for (int x = 0; x < 4; x++)
        {
            CityPoint corner = _corners[x];
            corners.Add((Direction)x, corner);
            fertility += corner.Fertility;
            height += corner.Height;
            //vertices[x] = new Vector3((x%3 - 1) * unit, Mathf.Lerp(neighbors.h))
        }
        height /= 4;
        fertility /= 4;
        height *= vScale;
        for (int x = 0; x < 4; x++)
        {
            Vector3 newOffset = clockwiseVs[x] + offset;
            vertices[x] = new Vector3(
                0,
                (corners[(Direction)x].Height) * scale,
                0
            ) + (clockwiseVs[x] + offset) * scale;
        }
        realEstateValue = 0f;
    }
    public float getRequestedValue(DrawMode drawMode)
    {
        switch (drawMode)
        {
            case DrawMode.Fertility:
                return this.Fertility;
            default:
                return this.Height;
        }
    }
    void UpdateFromNeighbors(Direction direction, CityPoint newNeighbor)
    {
        corners[direction] = newNeighbor;
    }

    public void AddProperty(GameObject ReProperty)//, Space relativeTo)
    {

    }
    public void AddPropertyCentral(GameObject ReProperty)
    {
        RealEstate _realEstate = ReProperty.GetComponent<RealEstate>();
        if (realEstate != null || _realEstate == null)
        {
            return;
        }
        realEstate = _realEstate;
        realEstate.price = RealEstateValue;
        ReProperty.transform.position = offset + new Vector3(0, (height) + .5f);
    }
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnDestroy()
    {
        
    }
}
