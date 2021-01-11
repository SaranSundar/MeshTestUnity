using System.Collections.Generic;
using UnityEngine;

namespace Utils
{
    public class MeshUtils
    {
        public static List<TriangleUtils.Triangle> CreateUnitCircleMesh(Mesh mesh, Color32[] allowedTriangleFaceColors, int numOfTriangleFaces, float size)
        {
            // Generate three vertices per triangle face, one color per vertex
            Color32[] colors = new Color32[numOfTriangleFaces * 3];
            Vector3[] vertices = new Vector3[numOfTriangleFaces * 3];
            int[] triangles = new int[numOfTriangleFaces * 3];
            List<TriangleUtils.Triangle> triangleFaces = new List<TriangleUtils.Triangle>();

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
            return triangleFaces;
        }
    }
}