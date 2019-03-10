using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator
{

    public static Mesh GenerateMesh(float scale, float vScale, DrawMode drawMode, CitySquare[,] cityTiles)
    {
        List<Vector2> uvs = new List<Vector2>();
        List<Color32> color32s = new List<Color32>();
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        float uUnit = 1f / cityTiles.GetLength(0);
        float vUnit = 1f / cityTiles.GetLength(1);
        for (int x = 0; x < cityTiles.GetLength(0); x++)
        {
            for (int y = 0; y < cityTiles.GetLength(1); y++)
            {
                CitySquare cityTile = cityTiles[x, y];
                Vector3[] tileVerts = cityTile.vertices;
                Vector2 uvTL = new Vector2(uUnit * x, vUnit * y);
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

    /*void CreateMesh(CitySquare citySquare)
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
    }*/
}
