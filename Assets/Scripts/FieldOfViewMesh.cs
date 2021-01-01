using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FieldOfViewMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    private int[] triangles;
    private int sides = 60;
    private float size = 2.0f;
    public List<Vector3[]> triangleFaces;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void CreateVerticeCoordinatesFromUnitCircle()
    {
        // 1 unit in local space is 1 meter in unity
        vertices = new Vector3[sides + 1];
        vertices[0] = new Vector3(0, 0, 0);
        for (int i = 1; i <= sides; i++)
        {
            // Points along a unit circle scaled up by size
            float x = size * Mathf.Cos(((Mathf.PI * 2) / sides) * i);
            float y = size * Mathf.Sin(((Mathf.PI * 2) / sides) * i);

            vertices[i] = new Vector3(x, y, 0);
        }
    }

    void Start()
    {
        transform.Rotate(-90, 0, 0);
        transform.position = new Vector3(0, 2, 0);
        CreateMesh();
    }

    void CreateMesh()
    {
        CreateVerticeCoordinatesFromUnitCircle();
        MakeMeshData();
        ColorTriangles();
    }
    
    void MakeMeshData()
    {
        triangles = new int[sides * 3];
        for (int t = 0; t < triangles.Length; t += 3)
        {
            triangles[t + 0] = 0; //set 0 to the center
            triangles[t + 1] = (t / 3) + 1; //first edge point
            //second edge point. If we're at the end this will loop back to 1
            triangles[t + 2] = ((t / 3) + 1) % (vertices.Length - 1) + 1;
        }
    }

    public void ColorTriangles()

    {
        Vector3[] verticesModified = new Vector3[triangles.Length];
        int[] trianglesModified = new int[triangles.Length];
        Color32 currentColor = new Color32();
        Color32[] colors = new Color32[triangles.Length];
        for (int i = 0; i < trianglesModified.Length; i++)
        {
            // Makes every vertex unique
            verticesModified[i] = vertices[triangles[i]];
            trianglesModified[i] = i;
            // Every third vertex randomly chooses new color
            if (i % 3 == 0)
            {
                currentColor = getColor();
            }

            colors[i] = currentColor;
        }

        // Applyes changes to mesh
        mesh.Clear();
        mesh.vertices = verticesModified;
        mesh.triangles = trianglesModified;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        CalculateTriangleFaces();
    }
    
    void CalculateTriangleFaces()
    {
        triangleFaces = new List<Vector3[]>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Vector3[] triangleFace = new Vector3[3];
            triangleFace[0] = vertices[triangles[i + 0]];
            triangleFace[1] = vertices[triangles[i + 1]];
            triangleFace[2] = vertices[triangles[i + 2]];
            triangleFaces.Add(triangleFace);
        }
    }

    Color32 getColor()
    {
        Color32[] color32 = new Color32[6];
        color32[0] = Color.red;
        color32[1] = Color.blue;
        color32[2] = Color.cyan;
        color32[3] = Color.green;
        color32[4] = Color.magenta;
        color32[5] = Color.yellow;
        int color = Random.Range(0, color32.Length);
        return color32[color];
    }
}