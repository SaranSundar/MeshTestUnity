using System.Collections.Generic;
using UnityEngine;
using Utils;


[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class FogOfWarMesh : MonoBehaviour
{
    private Mesh mesh;
    private Vector3[] vertices;
    private Color32[] colors;
    private int[] triangles;
    private int numOfTriangleFaces = 6;
    private float size = 3.0f;
    private FieldOfViewMesh fieldOfViewMesh;
    private List<TriangleUtils.Triangle> triangleFaces;
    private Color32[] allowedTriangleFaceColors =
        {Color.black};

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        transform.position = new Vector3(0, 2, 0);
        
        triangleFaces = MeshUtils.CreateUnitCircleMesh(mesh, allowedTriangleFaceColors, numOfTriangleFaces, size);
        vertices = mesh.vertices;
        colors = mesh.colors32;
        triangles = mesh.triangles;

        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();
    }

    private void Update()
    {
        FindIntersectingTriangleFaces();
    }

    void FindIntersectingTriangleFaces()
    {
        List<TriangleUtils.Triangle> playerTriangleFaces = fieldOfViewMesh.triangleFaces;
        List<TriangleUtils.Triangle> fogOfWarTriangleFaces = triangleFaces;

        ResetAllFogFacesToBlack();

        for (int r = 0; r < playerTriangleFaces.Count; r++)
        {
            for (int c = 0; c < fogOfWarTriangleFaces.Count; c++)
            {
                TriangleUtils.Triangle playerFace =
                    applyPositionToTriangle(playerTriangleFaces[r], fieldOfViewMesh.transform.position);
                TriangleUtils.Triangle fogFace = applyPositionToTriangle(fogOfWarTriangleFaces[c], transform.position);

                if (TriangleUtils.IsTriangleTriangleIntersecting(playerFace, fogFace))
                {
                    ChangeTriangleFaceToTransparent(fogOfWarTriangleFaces[c]);
                }
            }
        }

        mesh.colors32 = colors;
    }

    TriangleUtils.Triangle applyPositionToTriangle(TriangleUtils.Triangle original, Vector3 position)
    {
        // The z in the original is always 0, we need to switch positions z and y
        Vector3 p1 = new Vector3(original.p1.x, original.p1.y, original.p1.z) + position;
        Vector3 p2 = new Vector3(original.p2.x, original.p2.y, original.p2.z) + position;
        Vector3 p3 = new Vector3(original.p3.x, original.p3.y, original.p3.z) + position;
        TriangleUtils.Triangle newTriangle = new TriangleUtils.Triangle(p1, p2, p3);
        return newTriangle;
    }

    void ResetAllFogFacesToBlack()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 255;
        }
    }

    void ChangeTriangleFaceToTransparent(TriangleUtils.Triangle fogFace)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            TriangleUtils.Triangle triangleFace = new TriangleUtils.Triangle(
                vertices[triangles[i + 0]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]);
            if (TriangleUtils.AreTrianglesEqual(triangleFace, fogFace))
            {
                colors[i].a = 150;
                colors[i + 1].a = 150;
                colors[i + 2].a = 150;
            }
        }
    }
}