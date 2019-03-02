using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Direction       { NW, N, NE, W, C, E, SW, S, SE };
public enum MirrorDirection { SE, S, SW, E, C, W, NE, N, NW };

public class CitySquareMeshGenerator : MonoBehaviour
{
    static Direction[] clockwise = new Direction[] {
        Direction.N,
        Direction.NE,
        Direction.E,
        Direction.SE,
        Direction.S,
        Direction.SW,
        Direction.W,
        Direction.NW,
    };
    public Vector3[] vertices;
    CitySquare data;
    Dictionary<Direction, CitySquare> neighbors;
    float scale;
    float vScale;
    DrawMode drawMode;
    Texture2D tex;
    public void CreateData(
        CitySquare[] _neighbors, 
        float _scale, 
        float _vScale,
        DrawMode _drawMode
    )
    {
        drawMode = _drawMode;
        data = _neighbors[(int)Direction.C];
        vScale = _vScale;
        vertices = new Vector3[9];
        neighbors = new Dictionary<Direction, CitySquare>();
        scale = _scale;
        for(int x = 0; x < 9; x++)
        {
            neighbors.Add((Direction)x, _neighbors[x]);
            //vertices[x] = new Vector3((x%3 - 1) * unit, Mathf.Lerp(neighbors.h))
        }
        this.CreateMesh();
    }
    void CreateMesh()
    {
        float unit = scale / 2;
        for (int x = 0; x < 9; x++)
        {
            vertices[x] = new Vector3(
                (x / 3 - 1) * unit,
                (neighbors[(Direction)x].Height + data.Height) / 2f * scale * vScale,
                (x % 3 - 1) * unit
            );
        }
        List<Vector3> mVertices = new List<Vector3>(24);
        List<int> triangles = new List<int>(24);
        List<Color32> colors = new List<Color32>(24);
        float alphaValue = data.getRequestedValue(drawMode);
        byte alpha = (byte)(alphaValue * 255f);
        for (int x = 0; x < clockwise.Length; x++)
        {
            int firstP = (int)clockwise[x];
            int secondP = (int)clockwise[(x + 1) % 8];
            triangles.Add(mVertices.Count);
            mVertices.Add(vertices[firstP]);
            colors.Add(new Color32(0, 0, 0, alpha));
            triangles.Add(mVertices.Count);
            mVertices.Add(vertices[secondP]);
            colors.Add(new Color32(0, 0, 0, alpha));
            triangles.Add(mVertices.Count);
            mVertices.Add(vertices[(int)Direction.C]);
            colors.Add(new Color32(0, 0, 0, alpha));
        }
        Mesh mesh = new Mesh();
        mesh.vertices = mVertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors32 = colors.ToArray();
        GetComponent<MeshFilter>().mesh = mesh;
    }
    void UpdateFromNeighbors(Direction direction, CitySquare newNeighbor)
    {
        neighbors[direction] = newNeighbor;
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
