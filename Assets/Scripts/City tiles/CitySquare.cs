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
        new Vector3(-1, 0, 1),
        new Vector3(1, 0, 1),
        new Vector3(-1, 0, -1),
        new Vector3(1, 0, -1),
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
    public float Height
    {
        get { return height; }
    }
    public float Fertility
    {
        get { return fertility; }
    }
    public void CreateData(
        CityPoint[] _corners,
        float _scale,
        float _vScale,
        DrawMode _drawMode,
        Vector3 _offset
    )
    {
        offset = _offset;
        drawMode = _drawMode;
        vScale = _vScale;
        vertices = new Vector3[4];
        corners = new Dictionary<Direction, CityPoint>();
        scale = _scale;
        for(int x = 0; x < 4; x++)
        {
            CityPoint corner = _corners[x];
            corners.Add((Direction)x, corner);
            fertility += corner.Fertility;
            height += corner.Height;
            //vertices[x] = new Vector3((x%3 - 1) * unit, Mathf.Lerp(neighbors.h))
        }
        height /= 4;
        fertility /= 4;
        this.CreateMesh();
        float unit = scale / 2;
        for (int x = 0; x < 4; x++)
        {
            vertices[x] = new Vector3(
                0,
                (corners[(Direction)x].Height) * scale * vScale,
                0
            ) + (clockwiseVs[x] + offset) * unit;
        }
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
    void CreateMesh()
    {
        Vector3[] mVertices = new Vector3[]{ vertices[0], vertices[1], vertices[2], vertices[1], vertices[3], vertices[2],};
        int[] triangles = new int[] { 0, 1, 2, 3, 4, 5};
        List<Color32> colors = new List<Color32>(6);
        float alphaValue = getRequestedValue(drawMode);
        byte alpha = (byte)(alphaValue * 255f);
        for (int x = 0; x < 2; x ++)
        {
            colors.Add(new Color32(0, 0, 0, alpha));
            colors.Add(new Color32(0, 0, 0, alpha));
            colors.Add(new Color32(0, 0, 0, alpha));
        }
        Mesh mesh = new Mesh();
        mesh.vertices = mVertices;
        mesh.triangles = triangles  ;
        mesh.colors32 = colors.ToArray();
    }
    void UpdateFromNeighbors(Direction direction, CityPoint newNeighbor)
    {
        corners[direction] = newNeighbor;
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
