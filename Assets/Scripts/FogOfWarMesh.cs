using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeshFilter))]
public class FogOfWarMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;
    int[] triangles;
    private int sides = 6;
    private float size = 3.0f;
    private float chaos = 0.0f;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    float calculateDistance(float size, float chaos)
    {
        return Random.Range(size - chaos, size + chaos);
    }

    void CreateVerticeCoordinates(int sides, float size)
    {
        vertices = new Vector3[sides + 1];
        colors = new Color[vertices.Length];
        vertices[0] = new Vector3(0, 0, 0);
        colors[0] = Color.black;
        triangles = new int[sides * 3];
        for (int i = 1; i <= sides; i++)
        {
            float x = calculateDistance(size, chaos) * Mathf.Cos(((Mathf.PI * 2) / sides) * i);
            float y = calculateDistance(size, chaos) * Mathf.Sin(((Mathf.PI * 2) / sides) * i);

            Vector3 verticePos = new Vector3(x, y, 0);
            vertices[i] = verticePos;
            colors[i] = Color.green;
        }
    }

    void Start()
    {
        transform.Rotate(-90, 0, 0);
        transform.position = new Vector3(0, 4, 0);
        CreateVerticeCoordinates(sides, size);
        MakeMeshData();
        CreateMesh();
    }


    void MakeMeshData()
    {
        for (int t = 0; t < triangles.Length; t += 3)
        {
            triangles[t + 0] = 0; //set 0 to the center
            triangles[t + 1] = (t / 3) + 1; //first edge point
            //second edge point. If we're at the end this will loop back to 1
            triangles[t + 2] = ((t / 3) + 1) % (vertices.Length - 1) + 1;
        }
    }

    Color32 getColor(int i)
    {
        Color32[] color32 = new Color32[27];
        color32[0] = Color.black;
        color32[1] = Color.black;
        color32[2] = Color.black;
        color32[3] = Color.black;
        color32[4] = Color.black;
        color32[5] = Color.black;
        String choice = "";
        if (i == 0)
        {
            choice = "black";
        }
        else if (i == 1)
        {
            choice = "blue";
        }
        else if (i == 2)
        {
            choice = "cyan";
        }
        else if (i == 3)
        {
            choice = "green";
        }
        else if (i == 4)
        {
            choice = "magenta";
        }
        else if (i == 5)
        {
            choice = "yellow";
        }

        Debug.Log("I is: " + i + " Color is: " + choice);
        return color32[i / 3];
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
                // currentColor = new Color(
                //     Random.Range (0.0f, 1.0f),
                //     Random.Range (0.0f, 1.0f),
                //     Random.Range (0.0f, 1.0f),
                //     1.0f
                // );
                currentColor = getColor(i);
                if (i == 3)
                {
                    currentColor.a = 150;
                }
            }

            colors[i] = currentColor;
        }

        // Applyes changes to mesh
        mesh.vertices = verticesModified;
        mesh.triangles = trianglesModified;
        mesh.colors32 = colors;
    }

    void CreateMesh()
    {
        // mesh.Clear();
        // mesh.vertices = vertices;
        // mesh.triangles = triangles;
        // mesh.colors = colors;

        mesh.Clear();
        ColorTriangles();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}