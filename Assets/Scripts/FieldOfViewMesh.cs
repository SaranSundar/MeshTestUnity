using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FieldOfViewMesh : MonoBehaviour
{
    private Color32[] colors;
    private Mesh mesh;
    private readonly int sides = 6;
    private readonly float size = 2.0f;
    public List<FogOfWarMesh.Triangle> triangleFaces;
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
        Color32[] color32 = {Color.red, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow};

        // Generate three vertices per triangle face, one color per vertex
        colors = new Color32[sides * 3];
        vertices = new Vector3[sides * 3];
        triangles = new int[sides * 3];
        for (var i = 0; i < sides; i++)
        {
            // Create triangle points, set all points to the same color

            // Center point
            colors[3 * i] = color32[i % color32.Length];
            vertices[3 * i] = Vector3.zero;
            triangles[3 * i] = 3 * i;

            // Along current angle in unit circle
            colors[3 * i + 1] = color32[i % color32.Length];
            vertices[3 * i + 1] = new Vector3(
                Mathf.Cos(2 * Mathf.PI / sides * i),
                0,
                Mathf.Sin(2 * Mathf.PI / sides * i)) * size;
            triangles[3 * i + 1] = 3 * i + 1;

            // Along next angle in unit circle
            colors[3 * i + 2] = color32[i % color32.Length];
            vertices[3 * i + 2] = new Vector3(
                Mathf.Cos(2 * Mathf.PI / sides * (i + 1)),
                0,
                Mathf.Sin(2 * Mathf.PI / sides * (i + 1))) * size;
            triangles[3 * i + 2] = 3 * i + 2;
        }

        // Apply calculations to the mesh
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();

        // Create "triangle" array
        triangleFaces = new List<FogOfWarMesh.Triangle>();
        for (var i = 0; i < vertices.Length; i += 3)
            triangleFaces.Add(new FogOfWarMesh.Triangle(
                vertices[i], vertices[i + 1], vertices[i + 2]));
    }
}