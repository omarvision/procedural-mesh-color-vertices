using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshColors03 : MonoBehaviour
{
    public Material material = null;
    public Vector3 GridSize = new Vector3(3, 3, 3);
    public float Zoom = 1.0f;
    public float SurfaceLevel = 0.5f;

    private GridPoint[,,] p = null;
    private List<Vector3> vertices = new List<Vector3>();
    private List<int> triangles = new List<int>();
    private List<Color> colors = new List<Color>();
    private GridCell cell = new GridCell();
    private bool bGridReady = false;
    private bool bRequestUpdate = false;

    private void Start()
    {
        InitGrid();
        BuildMesh();
    }
    private void Update()
    {
        if (bRequestUpdate == true)
        {
            InitGrid();
            BuildMesh();
            bRequestUpdate = false;
        }
    }
    private void InitGrid()
    {
        p = new GridPoint[(int)GridSize.x + 1, (int)GridSize.y + 1, (int)GridSize.z + 1];

        for (int z = 0; z <= GridSize.z; z++)
        {
            for (int y = 0; y <= GridSize.y; y++)
            {
                for (int x = 0; x <= GridSize.x; x++)
                {
                    float nx = Zoom * (x / GridSize.x);
                    float ny = Zoom * (y / GridSize.y);
                    float nz = Zoom * (z / GridSize.z);
                    p[x, y, z] = new GridPoint();
                    p[x, y, z].Position = new Vector3(x, y, z);
                    p[x, y, z].Value = Mathf.PerlinNoise(nx, nz) * ny;
                }
            }
        }

        bGridReady = true;
    }
    private void BuildMesh()
    {
        if (bGridReady == false)
            return;

        /*  vertex 8 (0-7)
              E4-------------F5         7654-3210
              |               |         HGFE-DCBA
              |               |
        H7-------------G6     |
        |     |         |     |
        |     |         |     |
        |     A0--------|----B1  
        |               |
        |               |
        D3-------------C2               */

        vertices.Clear();
        triangles.Clear();
        colors.Clear();

        for (int z = 0; z < GridSize.z; z++)
        {
            for (int y = 0; y < GridSize.y; y++)
            {
                for (int x = 0; x < GridSize.x; x++)
                {
                    cell.p[0] = p[x, y, z + 1];         //A0
                    cell.p[1] = p[x + 1, y, z + 1];     //B1
                    cell.p[2] = p[x + 1, y, z];         //C2
                    cell.p[3] = p[x, y, z];             //D3
                    cell.p[4] = p[x, y + 1, z + 1];     //E4
                    cell.p[5] = p[x + 1, y + 1, z + 1]; //F5
                    cell.p[6] = p[x + 1, y + 1, z];     //G6
                    cell.p[7] = p[x, y + 1, z];         //H7
                    MarchingCube.IsoFaces(ref cell, SurfaceLevel);
                    BuildMeshCellData(ref cell);
                }
            }
        }

        Mesh m = GetMesh();
        SetMesh(ref m);

        ColorByNormals(ref m);
        SetMesh(ref m);
    }
    private void BuildMeshCellData(ref GridCell cell)
    {
        for (int i = 0; i < cell.numtriangles; i++)
        {
            vertices.Add(cell.triangle[i].p[0]);
            vertices.Add(cell.triangle[i].p[1]);
            vertices.Add(cell.triangle[i].p[2]);

            triangles.Add(vertices.Count - 1);
            triangles.Add(vertices.Count - 2);
            triangles.Add(vertices.Count - 3);

            colors.Add(new Color(1,0,0));
            colors.Add(new Color(0,1,0));
            colors.Add(new Color(0,0,1));

            //colors.Add(new Color(cell.triangle[i].p[0].y / GridSize.y, 0.8f - (cell.triangle[i].p[0].y / GridSize.y), cell.triangle[i].p[0].z / GridSize.z));
            //colors.Add(new Color(cell.triangle[i].p[1].y / GridSize.y, 0.8f - (cell.triangle[i].p[1].y / GridSize.y), cell.triangle[i].p[1].z / GridSize.z));
            //colors.Add(new Color(cell.triangle[i].p[2].y / GridSize.y, 0.8f - (cell.triangle[i].p[2].y / GridSize.y), cell.triangle[i].p[2].z / GridSize.z));
        }
    }
    private Mesh GetMesh()
    {
        Mesh m = null;

        if (Application.isEditor == true)
        {
            MeshFilter mf = this.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = this.gameObject.AddComponent<MeshFilter>();
                mf.sharedMesh = new Mesh();
            }
            m = mf.sharedMesh;
        }
        else
        {
            MeshFilter mf = this.GetComponent<MeshFilter>();
            if (mf == null)
            {
                mf = this.gameObject.AddComponent<MeshFilter>();
                mf.mesh = new Mesh();
            }
            m = mf.mesh;
        }

        MeshRenderer mr = this.GetComponent<MeshRenderer>();
        if (mr == null)
            mr = this.gameObject.AddComponent<MeshRenderer>();
        mr.material = material;

        return m;
    }
    private void SetMesh(ref Mesh m)
    {
        m.Clear();

        m.vertices = vertices.ToArray();
        m.triangles = triangles.ToArray();
        m.colors = colors.ToArray();

        m.RecalculateBounds();
        m.RecalculateNormals();
        m.RecalculateTangents();
    }
    private void ColorByNormals(ref Mesh m)
    {
        //normals are at vertices (not triangles) so the count will be the same as the vertices.

        List<Vector3> normals = new List<Vector3>();
        m.GetNormals(normals);
        m.GetColors(colors);

        float? min = null;
        float? max = null;
        for (int i = 0; i < normals.Count; i++)
        {
            float up = Vector3.Dot(Vector3.up, normals[i]);
            if (min == null || min.Value > up)
                min = up;
            if (max == null || max.Value < up)
                max = up;            
        }

        float range = max.Value - min.Value;
        for (int i = 0; i < normals.Count; i++)
        {
            float up = Vector3.Dot(Vector3.up, normals[i]);
            colors[i] = new Color(1 - up / range, 0.0f, up / range);
        }
    }
    public void RequestUpdate()
    {
        if (bRequestUpdate == false)
            bRequestUpdate = true;
    }
}
