using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    
    List<Vector3> vertices;
    List<int> triangles;
    Color32[] colorSet;
    List<Color32> color32s;
    List<Vector2> uvs;
    CitySquare[,] cityTiles;
    DrawMode drawMode;

    public Mesh GenerateMesh(CityPoint[,] cityPoints, float scale, float vScale, Color32 highColor, Color32 lowColor, int width, int height, DrawMode _drawMode)
    {
        drawMode = _drawMode;
        uvs = new List<Vector2>();
        cityTiles = new CitySquare[width, height];
        colorSet = new Color32[]{lowColor, highColor};
        MakeTiles(cityPoints, width, height, scale, vScale);
        color32s = new List<Color32>();
        vertices = new List<Vector3>();
        triangles = new List<int>();
        float uUnit = 1f / width;
        float vUnit = 1f / height;
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                CitySquare cityTile = cityTiles[x, y];
                Vector3[] tileVerts = cityTile.vertices;
                Vector2 uvTL = new Vector2(uUnit * x, vUnit * y);
                /*Vector2 uvTR = new Vector2(uUnit * x, vUnit * y);
                Vector2 uvBL = new Vector2(uUnit * x, vUnit * y);
                Vector2 uvBR = new Vector2(uUnit * x, vUnit * y);*/
                Vector3[] mVertices = new Vector3[] { tileVerts[0], tileVerts[1], tileVerts[2], tileVerts[1], tileVerts[3], tileVerts[2], };
                Vector2[] mUVs = new Vector2[] { uvTL, uvTL, uvTL, uvTL , uvTL, uvTL };
                int currVerts = vertices.Count;
                List<int> trianglesToAdd = new List<int>();
                List<Color32> colors = new List<Color32>(6);
                float alphaValue = cityTile.getRequestedValue(drawMode);
                byte alpha = (byte)(alphaValue * 255f);
                for (int g = 0; g < 6; g++)
                {
                    colors.Add(new Color32(0, 0, 0, alpha));
                    trianglesToAdd.Add(currVerts + g);
                }
                vertices.AddRange(mVertices);
                triangles.AddRange(trianglesToAdd);
                color32s.AddRange(colors.ToArray());
                uvs.AddRange(mUVs);
            }
        }


        Mesh mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.colors32 = color32s.ToArray();
        mesh.uv = uvs.ToArray();
        /*for (int x = 1; x < numMeshes; x++)
        {
            mesh.SetTriangles(triangles[x], x, true, (x * trianglesPerSubMesh));
        }*/
        mesh.RecalculateNormals();
        return mesh;
    }

    void CreateMesh(CitySquare citySquare)
    {
        Vector3[] mVertices = new Vector3[] { vertices[0], vertices[1], vertices[2], vertices[1], vertices[3], vertices[2], };
        int[] triangles = new int[] { 0, 1, 2, 3, 4, 5 };
        List<Color32> colors = new List<Color32>(6);
        float alphaValue = citySquare.getRequestedValue(drawMode);
        byte alpha = (byte)(alphaValue * 255f);
        for (int x = 0; x < 2; x++)
        {
            colors.Add(new Color32(0, 0, 0, alpha));
            colors.Add(new Color32(0, 0, 0, alpha));
            colors.Add(new Color32(0, 0, 0, alpha));
        }
        Mesh mesh = new Mesh();
        mesh.vertices = mVertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors.ToArray();
    }

    void MakeTiles(CityPoint[,] cityPoints, int width, int height, float scale, float vScale)
    {
        float halfWidth = width / 2f;
        float halfHeight = height / 2f;
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                //CitySquareMeshes[x, y] = Instantiate(CitySquareBase, transform.position + , Quaternion.identity);
                List<CityPoint> neighbors = new List<CityPoint>();
                for (int v = 0; v < 2; v++)
                {
                    for (int u = 0; u < 2; u++)
                    {
                        neighbors.Add(cityPoints[u + x, v + y]);
                    }
                }
                cityTiles[x, y] = new CitySquare();
                cityTiles[x, y].CreateData(
                    neighbors.ToArray(),
                    scale,
                    vScale,
                    drawMode,
                    new Vector3((x - halfWidth + .5f), 0, (halfHeight - y - .5f))
                );
            }
        }
    }
}
