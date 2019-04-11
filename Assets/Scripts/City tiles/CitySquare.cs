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
    public float HousingValue { get; private set; }
    public float ProductivityValue { get; private set; }
    public float Housing
    {
        get { return RealEstate != null ? RealEstate.Housing : 0; }
    }
    public float Productivity {
        get { return RealEstate != null ? RealEstate.Productivity : 0; }
    }
    public float AvgProductivity
    {
        get { return RealEstate != null ? RealEstate.AvgProductivity : 0; }
    }
    public float EightyProductivity
    {
        get { return RealEstate != null ? RealEstate.EightyProductivity : 0; }
    }
    public float OccupationCount
    {
        get { return RealEstate != null ? RealEstate.OccupationsList.Count : 0; }
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
    public List<Route> Commutes { get; private set; }
    Guid guid;
    public float CalculatePropertyValues(int commuteDist, float pollutionExp)
    {
        Routes = new Dictionary<CitySquare, Route>();
        float propertyValue = 0;
        this.HousingValue = 0;
        this.ProductivityValue = 0;
        if (!this.HasRoad)
        {
            HashSet<CitySquareDist> nearbyTiles = this.NearbyTiles(commuteDist);
            propertyValue = -Mathf.Pow(this.Pollution, pollutionExp);
            int productiveCount = 0;
            foreach (CitySquareDist tile in nearbyTiles)
            {
                CitySquare square = tile.tile;
                float multiplier = 5 / tile.driveDistance;
                //float avgProductivityAdd = tile.roadAccess ? (square.AvgProductivity * multiplier) : 0;
                // This is complicated but boils down to the 80th percentile times number of works times .1 divided by distance
                float productivityAdd = tile.roadAccess ? (square.EightyProductivity * square.OccupationCount * multiplier * .02f) : 0;
                float housingAdd = tile.roadAccess && !tile.tile.hasRoad ? (square.Housing * multiplier) : 0;
                productiveCount += tile.roadAccess ? 1 : 0;
                propertyValue += productivityAdd - Mathf.Pow(square.Pollution / (tile.distance + 1), pollutionExp) + 1;
                float accessValue = tile.roadAccess ? 200 / tile.driveDistance : 0;
                this.HousingValue += productivityAdd + accessValue;
                this.HousingValue -= housingAdd;
                this.ProductivityValue += housingAdd + accessValue * 2;
            }
            propertyValue /= Mathf.Sqrt(productiveCount);
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
                            nearbyTiles.Add(neighborTile, new CitySquareDist(dist, neighborTile, hasRoadAccess));
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
        Commutes = new List<Route>(Routes.Values);
        Commutes.Sort();
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
        HousingValue = 0;
        ProductivityValue = 0;
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

public class Route : IComparable
{
    public List<CitySquare> Squares { get; private set; }
    public HashSet<CitySquare> UsedSquares { get; private set; }
    public int Length { get { return Squares.Count; } }
    public Route(CitySquare square)
    {
        Squares = new List<CitySquare>();
        Squares.Add(square);
        UsedSquares = new HashSet<CitySquare>(Squares);
    }
    public Route(List<CitySquare> _squares, CitySquare _square)
    {
        Squares = new List<CitySquare>(_squares);
        Squares.Add(_square);
        UsedSquares = new HashSet<CitySquare>(Squares);
    }

    public int CompareTo(object obj)
    {
        if (obj == null) return 1;

        Route otherRoute = obj as Route;
        if (otherRoute != null)
            return this.Length > otherRoute.Length ? 1 : -1;
        else
            throw new ArgumentException("Object is not a Route");
    }
}