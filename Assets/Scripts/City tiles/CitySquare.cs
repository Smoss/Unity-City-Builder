using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

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
    Vector3 offset;
    float realEstateValue;
    //float productivity;
    CityManager city;
    bool hasRoad;
    public Vector3 Offset { get { return offset; }}
    public Dictionary<CitySquare, Route> Routes { get; private set; }
    public bool HasRoad {
        get { return hasRoad; }
        set {
            hasRoad = value;
            RemoveProperty();
        }
    }
    public float Productivity {
        get { return RealEstate != null ? RealEstate.Productivity : 0; }
    }
    public float AvgProductivity
    {
        get { return RealEstate != null ? RealEstate.AvgProductivity : 0; }
    }
    public float Pollution {
        get { return RealEstate != null ? RealEstate.pollution : 0; }
    }
    public float RealEstateValue {
        get { return realEstateValue; }
        set {
            realEstateValue = value;
            if (RealEstate != null)
            {
                RealEstate.price = value;
            }
        }
    }
    public RealEstate RealEstate { get; private set; }
    public HashSet<CitySquare> Neighbors { get; }
    public float Height { get; }
    public float Fertility { get; }
    Guid guid;
    public float CalculatePropertyValues(int commuteDist, float pollutionExp)
    {
        Routes = new Dictionary<CitySquare, Route>();
        float propertyValue = 0;
        if (!this.HasRoad)
        {
            HashSet<CitySquareDist> nearbyTiles = this.NearbyTiles(commuteDist);
            propertyValue = -Mathf.Pow(this.Pollution, pollutionExp);
            foreach (CitySquareDist tile in nearbyTiles)
            {
                CitySquare square = tile.tile;
                float productivityAdd = tile.roadAccess ? (square.AvgProductivity * 10 / (2 * tile.driveDistance)) : 0;
                propertyValue += productivityAdd - Mathf.Pow(square.Pollution / (tile.distance + 1), pollutionExp) + 1;
            }
        }
        this.RealEstateValue = propertyValue;
        return propertyValue;
    }

    private HashSet<CitySquareDist> NearbyTiles(int nearbyDist)
    {
        Dictionary<CitySquare, CitySquareDist> nearbyTiles = new Dictionary<CitySquare, CitySquareDist>();
        HashSet<CitySquare> tilesToSearch = new HashSet<CitySquare>();
        foreach (CitySquare tile in this.Neighbors)
        {
            nearbyTiles.Add(tile, new CitySquareDist(1, tile, true));
            if(tile != this)
            {
                tilesToSearch.Add(tile);
            }
            Routes.Add(tile, new Route(tile));
        }
        for (int dist = 2; dist <= nearbyDist && tilesToSearch.Count > 0; dist++)
        {
            HashSet<CitySquare> nextTilesToSearch = new HashSet<CitySquare>();
            foreach (CitySquare tile in tilesToSearch)
            {
                foreach (CitySquare neighborTile in tile.Neighbors)
                {
                    bool hasRoadAccess = nearbyTiles[tile].roadAccess && tile.HasRoad;
                    if (neighborTile != this)
                    {
                        if (!nearbyTiles.ContainsKey(neighborTile))
                        {
                            nearbyTiles.Add(neighborTile, new CitySquareDist(1, neighborTile, hasRoadAccess));
                            nextTilesToSearch.Add(neighborTile);
                        }
                        else if (hasRoadAccess && !nearbyTiles[neighborTile].roadAccess)
                        {
                            CitySquareDist neighbor = nearbyTiles[neighborTile];
                            neighbor.roadAccess = true;
                            neighbor.driveDistance = dist;
                            nextTilesToSearch.Add(neighbor.tile);
                        }
                        if(hasRoadAccess && !Routes.ContainsKey(neighborTile))
                        {
                            addRoute(tile, neighborTile);
                        }
                    }
                }
            }
            tilesToSearch = nextTilesToSearch;
        }
        return new HashSet<CitySquareDist>(nearbyTiles.Values);
    }

    private void addRoute(CitySquare tile, CitySquare newRouteTo)
    {
        Route route = new Route(Routes[tile].Squares, newRouteTo);
        Routes.Add(newRouteTo, route);
    }
    public void addNeighbor(CitySquare _neighbor)
    {
        _neighbor.Neighbors.Add(this);
        this.Neighbors.Add(_neighbor);
    }
    public CitySquare(
        CityPoint[] _corners,
        float _scale,
        float _vScale,
        DrawMode _drawMode,
        Vector3 _offset,
        CityManager _city
    )
    {
        guid = Guid.NewGuid();
        city = _city;
        hasRoad = false;
        Neighbors = new HashSet<CitySquare>();
        scale = _scale;
        offset = _offset * scale;
        drawMode = _drawMode;
        vScale = _vScale;
        vertices = new Vector3[4];
        corners = new Dictionary<Direction, CityPoint>();
        for (int x = 0; x < 4; x++)
        {
            CityPoint corner = _corners[x];
            corners.Add((Direction)x, corner);
            Fertility += corner.Fertility;
            Height += corner.Height;
            //vertices[x] = new Vector3((x%3 - 1) * unit, Mathf.Lerp(neighbors.h))
        }
        Height /= 4;
        Fertility /= 4;
        Height *= vScale;
        for (int x = 0; x < 4; x++)
        {
            vertices[x] = new Vector3(
                0,
                (corners[(Direction)x].Height * vScale),
                0
            ) + (clockwiseVs[x] * scale + offset);
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
    public void AddPropertyCentral(RealEstate ReProperty)
    {
        if (RealEstate != null)
        {
            return;
        }
        RealEstate = ReProperty;
        RealEstate.price = RealEstateValue;
        ReProperty.init(city, this);
        ReProperty.transform.localPosition = offset + new Vector3(0, (Height) + .5f);
    }

    public void RemoveProperty()
    {
        if(RealEstate != null)
        {
            city.Properties.Remove(RealEstate);
            foreach(Human human in RealEstate.Occupants)
            {
                GameObject.Destroy(human.gameObject);
            }
            GameObject.Destroy(RealEstate.gameObject);
            RealEstate = null;
        }
    }

    public override bool Equals(object obj)
    {
        var square = obj as CitySquare;
        return square != null &&
               EqualityComparer<Texture2D>.Default.Equals(tex, square.tex) &&
               xPos == square.xPos &&
               zPos == square.zPos &&
               Height == square.Height &&
               Fertility == square.Fertility &&
               offset.Equals(square.offset) &&
               realEstateValue == square.realEstateValue &&
               EqualityComparer<HashSet<CitySquare>>.Default.Equals(Neighbors, square.Neighbors) &&
               EqualityComparer<RealEstate>.Default.Equals(RealEstate, square.RealEstate);
    }

    public override int GetHashCode()
    {
        var hashCode = 976150762;
        hashCode = hashCode * -1521134295 + guid.GetHashCode();
        return hashCode;
    }
}

public class Route {
    public List<CitySquare> Squares { get; private set; }
    public HashSet<CitySquare> UsedSquares { get; private set; }
    public int Length { get { return Squares.Count; } }
    public Route(CitySquare square)
    {
        Squares = new List<CitySquare>();
        Squares.Add(square);
        UsedSquares = new HashSet<CitySquare>(Squares);
    }
    public Route(List<CitySquare> squares, CitySquare square)
    {
        Squares = new List<CitySquare>(squares);
        squares.Add(square);
        UsedSquares = new HashSet<CitySquare>(Squares);
    }
}