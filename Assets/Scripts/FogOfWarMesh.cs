using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FogOfWarMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Color32[] colors;
    private int[] triangles;
    private int sides = 6;
    private float size = 3.0f;
    private FieldOfViewMesh fieldOfViewMesh;
    private List<Vector3[]> triangleFaces;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        transform.Rotate(-90, 0, 0);
        transform.position = new Vector3(0, 4, 0);
        CreateMesh();
        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();
    }

    private void Update()
    {
        //Debug.Log(fieldOfViewMesh.transform.position);
    }

    void FindIntersectingTriangleFaces()
    {
        List<Vector3[]> playerTriangleFaces = fieldOfViewMesh.triangleFaces;
        List<Vector3[]> fogOfWarTriangleFaces = triangleFaces;
        
        
        
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

        //Debug.Log("there are " + triangleFaces.Count + " triangle faces");
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
        colors = new Color32[triangles.Length];
        for (int i = 0; i < trianglesModified.Length; i++)
        {
            // Makes every vertex unique
            verticesModified[i] = vertices[triangles[i]];
            trianglesModified[i] = i;
            // Every third vertex randomly chooses new color
            if (i % 3 == 0)
            {
                currentColor = Color.black;
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
}