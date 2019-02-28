using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{

    public SquareGrid squareGrid;
    List<Vector3> vertices;
    List<int>[] triangles;
    Color32[] colorSet;
    List<Color32> color32s;
    List<Vector2> uvs;

    public Mesh GenerateMesh(float[,,] map, float squareSize, Color32 highColor, Color32 lowColor, float maxValue, float minValue, int drawMode)
    {
        uvs = new List<Vector2>();
        colorSet = new Color32[]{lowColor, highColor};
        squareGrid = new SquareGrid(map, squareSize, colorSet, maxValue, minValue, drawMode);
        color32s = new List<Color32>();
        int trianglesPerSubMesh = 65532;
        int numMeshes = map.GetLength(1) * map.GetLength(2) * 6 / trianglesPerSubMesh + 1;
        vertices = new List<Vector3>();
        triangles = new List<int>[numMeshes];
        for(int x = 0; x < numMeshes; x++)
        {
            triangles[x] = new List<int>();
        }
        int meshTrack = 0;
        int meshNum = 0;
        for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
        {
            for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
            {
                TriangulateSquare(squareGrid.squares[x, y], triangles[meshTrack]);
                meshNum += 6;
                if(meshNum >= trianglesPerSubMesh)
                {
                    meshNum = 0;
                    meshTrack++;
                }
            }
        }


        Mesh mesh = new Mesh();
        //GetComponent<MeshFilter>().mesh = mesh;
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles[0].ToArray();
        mesh.colors32 = color32s.ToArray();
        mesh.uv = uvs.ToArray();
        /*for (int x = 1; x < numMeshes; x++)
        {
            mesh.SetTriangles(triangles[x], x, true, (x * trianglesPerSubMesh));
        }*/
        mesh.RecalculateNormals();
        return mesh;
    }


    void TriangulateSquare(Square square, List<int> _triangles)
    {
        /*switch (square.configuration)
        {
            case 0:
                break;

            // 1 points:
            case 1:
                MeshFromPoints(square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 2:
                MeshFromPoints(square.centreRight, square.bottomRight, square.centreBottom);
                break;
            case 4:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight);
                break;
            case 8:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreLeft);
                break;

            // 2 points:
            case 3:
                MeshFromPoints(square.centreRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 6:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.centreBottom);
                break;
            case 9:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreBottom, square.bottomLeft);
                break;
            case 12:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreLeft);
                break;
            case 5:
                MeshFromPoints(square.centreTop, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft, square.centreLeft);
                break;
            case 10:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 3 point:
            case 7:
                MeshFromPoints(square.centreTop, square.topRight, square.bottomRight, square.bottomLeft, square.centreLeft);
                break;
            case 11:
                MeshFromPoints(square.topLeft, square.centreTop, square.centreRight, square.bottomRight, square.bottomLeft);
                break;
            case 13:
                MeshFromPoints(square.topLeft, square.topRight, square.centreRight, square.centreBottom, square.bottomLeft);
                break;
            case 14:
                MeshFromPoints(square.topLeft, square.topRight, square.bottomRight, square.centreBottom, square.centreLeft);
                break;

            // 4 point:
            case 15:
                
                break;
        }*/
        MeshFromPoints(_triangles, square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);
    }

    void MeshFromPoints(List<int> _triangles, params Node[] points)
    {
        if (points.Length >= 3)
            CreateTriangle(points[0], points[1], points[2], _triangles);
        if (points.Length >= 4)
            CreateTriangle(points[0], points[2], points[3], _triangles);
    }

    /*void AssignVertices(Node[] points)
    {
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i].vertexIndex == -1)
            {
                points[i].vertexIndex = vertices.Count;
                vertices.Add(points[i].position);
            }
        }
    }*/

    void CreateTriangle(Node a, Node b, Node c, List<int> _triangles)
    {
        _triangles.Add(vertices.Count);
        vertices.Add(a.position);
        color32s.Add(a.color);
        uvs.Add(a.uv);
        _triangles.Add(vertices.Count);
        vertices.Add(b.position);
        color32s.Add(b.color);
        uvs.Add(b.uv);
        _triangles.Add(vertices.Count);
        vertices.Add(c.position);
        color32s.Add(c.color);
        uvs.Add(c.uv);
    }

    void OnDrawGizmos()
    {
        /*if (squareGrid != null)
        {
            for (int x = 0; x < squareGrid.squares.GetLength(0); x++)
            {
                for (int y = 0; y < squareGrid.squares.GetLength(1); y++)
                {

                    Gizmos.color = (squareGrid.squares[x, y].topLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topLeft.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].topRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].topRight.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomRight.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomRight.position, Vector3.one * .4f);

                    Gizmos.color = (squareGrid.squares[x, y].bottomLeft.active) ? Color.black : Color.white;
                    Gizmos.DrawCube(squareGrid.squares[x, y].bottomLeft.position, Vector3.one * .4f);


                    Gizmos.color = Color.grey;
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreTop.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreRight.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreBottom.position, Vector3.one * .15f);
                    Gizmos.DrawCube(squareGrid.squares[x, y].centreLeft.position, Vector3.one * .15f);

                }
            }
        }*/
    }

    public class SquareGrid
    {
        public Square[,] squares;

        public SquareGrid(float[,,] map, float squareSize, Color32[] colorSet, float maxValue, float minValue, int mapIndex)
        {
            int nodeCountX = map.GetLength(1);
            int nodeCountY = map.GetLength(2);
            float mapWidth = nodeCountX * squareSize;
            float mapHeight = nodeCountY * squareSize;

            ControlNode[,] controlNodes = new ControlNode[nodeCountX, nodeCountY];

            for (int x = 0; x < nodeCountX; x++)
            {
                for (int y = 0; y < nodeCountY; y++)
                {
                    Vector3 pos = new Vector3(-mapWidth / 2 + x * squareSize + squareSize / 2, map[0, x, y], -mapHeight / 2 + y * squareSize + squareSize / 2);
                    float uOff = x / (float)nodeCountX;
                    float vOff = y / (float)nodeCountY;
                    Vector2 uv = new Vector2(uOff, vOff);
                    byte alpha = (byte) (Mathf.InverseLerp(minValue, maxValue, map[mapIndex, x, y]) * 255f);
                    controlNodes[x, y] = new ControlNode(pos, map[0, x, y], squareSize, new Color32(0,0,0, (alpha)), uv, 1f / nodeCountX / 2f, 1f / nodeCountY / 2f);
                }
            }

            squares = new Square[nodeCountX - 1, nodeCountY - 1];
            for (int x = 0; x < nodeCountX - 1; x++)
            {
                for (int y = 0; y < nodeCountY - 1; y++)
                {
                    squares[x, y] = new Square(controlNodes[x, y + 1], controlNodes[x + 1, y + 1], controlNodes[x + 1, y], controlNodes[x, y]);
                }
            }

        }
    }

    public class Square
    {

        public ControlNode topLeft, topRight, bottomRight, bottomLeft;
        public Node centreTop, centreRight, centreBottom, centreLeft;
        //public int configuration;

        public Square(ControlNode _topLeft, ControlNode _topRight, ControlNode _bottomRight, ControlNode _bottomLeft)
        {
            topLeft = _topLeft;
            topRight = _topRight;
            bottomRight = _bottomRight;
            bottomLeft = _bottomLeft;

            centreTop = topLeft.right;
            centreRight = bottomRight.above;
            centreBottom = bottomLeft.right;
            centreLeft = bottomLeft.above;
        }
    }

    public class Node
    {
        public Vector3 position;
        public int vertexIndex = -1;
        public Color32 color;
        public Vector2 uv;

        public Node(Vector3 _pos, Color32 _color, Vector2 _uv)
        {
            color = _color;
            position = _pos;
            uv = _uv;
        }
    }

    public class ControlNode : Node
    {
        public float height;
        public Node above, right;

        public ControlNode(Vector3 _pos, float _height, float squareSize, Color32 _color, Vector2 _uv, float uOff, float vOff): base(_pos, _color, _uv)
        {
            height = _height;
            float unit = squareSize / 2f;
            above = new Node(position + Vector3.forward * unit, _color, uv + Vector2.up * vOff);
            right = new Node(position + Vector3.right * unit, _color, uv + Vector2.right * uOff);
        }
    }
}
