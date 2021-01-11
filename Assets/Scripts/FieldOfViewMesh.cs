using System.Collections.Generic;
using UnityEngine;
using Utils;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FieldOfViewMesh : MonoBehaviour
{
    private Color32[] colors;
    private Mesh mesh;
    private readonly int numOfTriangleFaces = 6;
    private readonly float size = 2.0f;
    public List<TriangleUtils.Triangle> triangleFaces;
    private int[] triangles;
    private Vector3[] vertices;

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        transform.position = new Vector3(0, 1, 0);
        CreateMesh();
    }

    private void CreateMesh()
    {
        // Allowed colors
        Color32[] allowedTriangleFaceColors =
            {Color.red, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow};

        // Generate three vertices per triangle face, one color per vertex
        colors = new Color32[numOfTriangleFaces * 3];
        vertices = new Vector3[numOfTriangleFaces * 3];
        triangles = new int[numOfTriangleFaces * 3];
        triangleFaces = new List<TriangleUtils.Triangle>();

        for (var i = 0; i < numOfTriangleFaces; i++)
        {
            // Create triangle points
            Color32 triangleFaceColor = allowedTriangleFaceColors[i % allowedTriangleFaceColors.Length];
            int index1 = 3 * i;
            int index2 = index1 + 1;
            int index3 = index2 + 1;

            // Set all triangle points to use same color
            colors[index1] = triangleFaceColor;
            colors[index2] = triangleFaceColor;
            colors[index3] = triangleFaceColor;

            // Set all triangle points to be the index of the vertices
            triangles[index1] = index1;
            triangles[index2] = index2;
            triangles[index3] = index3;

            // Center point
            vertices[index1] = Vector3.zero;

            // Along current angle in unit circle
            vertices[index2] = new Vector3(
                Mathf.Cos(2 * Mathf.PI / numOfTriangleFaces * i),
                0,
                Mathf.Sin(2 * Mathf.PI / numOfTriangleFaces * i)) * size;

            // Along next angle in unit circle
            vertices[index3] = new Vector3(
                Mathf.Cos(2 * Mathf.PI / numOfTriangleFaces * (i + 1)),
                0,
                Mathf.Sin(2 * Mathf.PI / numOfTriangleFaces * (i + 1))) * size;

            triangleFaces.Add(new TriangleUtils.Triangle(
                vertices[index1], vertices[index2], vertices[index3]));
        }

        // Apply calculations to the mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
    }
}