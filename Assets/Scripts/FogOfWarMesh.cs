using System.Collections.Generic;
using UnityEngine;

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
    private List<Triangle> triangleFaces;

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        transform.position = new Vector3(0, 2, 0);
        CreateMesh();
        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();
    }

    void CreateMesh()
    {
        // Allowed colors
        Color32[] color32 = {Color.red, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow};

        // Generate three vertices per triangle face, one color per vertex
        colors = new Color32[sides * 3];
        vertices = new Vector3[sides * 3];
        triangles = new int[sides * 3];
        for (int i = 0; i < sides; i++)
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
        for (int i = 0; i < vertices.Length; i += 3)
        {
            triangleFaces.Add(new FogOfWarMesh.Triangle(
                vertices[i], vertices[i + 1], vertices[i + 2]));
        }
    }

    private void Update()
    {
        FindIntersectingTriangleFaces();
    }

    void FindIntersectingTriangleFaces()
    {
        List<Triangle> playerTriangleFaces = fieldOfViewMesh.triangleFaces;
        List<Triangle> fogOfWarTriangleFaces = triangleFaces;

        ResetAllFogFacesToBlack();

        for (int r = 0; r < playerTriangleFaces.Count; r++)
        {
            for (int c = 0; c < fogOfWarTriangleFaces.Count; c++)
            {
                Triangle playerFace =
                    applyPositionToTriangle(playerTriangleFaces[r], fieldOfViewMesh.transform.position);
                Triangle fogFace = applyPositionToTriangle(fogOfWarTriangleFaces[c], transform.position);

                if (IsTriangleTriangleIntersecting(playerFace, fogFace))
                {
                    ChangeTriangleFaceToTransparent(fogOfWarTriangleFaces[c]);
                }
            }
        }

        mesh.colors32 = colors;
    }

    Triangle applyPositionToTriangle(Triangle original, Vector3 position)
    {
        // The z in the original is always 0, we need to switch positions z and y
        Vector3 p1 = new Vector3(original.p1.x, original.p1.y, original.p1.z) + position;
        Vector3 p2 = new Vector3(original.p2.x, original.p2.y, original.p2.z) + position;
        Vector3 p3 = new Vector3(original.p3.x, original.p3.y, original.p3.z) + position;
        Triangle newTriangle = new Triangle(p1, p2, p3);
        return newTriangle;
    }

    void ResetAllFogFacesToBlack()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 255;
        }
    }

    void ChangeTriangleFaceToTransparent(Triangle fogFace)
    {
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Triangle triangleFace = new Triangle(
                vertices[triangles[i + 0]],
                vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]);
            if (AreTrianglesEqual(triangleFace, fogFace))
            {
                colors[i].a = 150;
                colors[i + 1].a = 150;
                colors[i + 2].a = 150;
            }
        }
    }

    bool AreTrianglesEqual(Triangle t1, Triangle t2)
    {
        return t1.p1.Equals(t2.p1) && t1.p2.Equals(t2.p2) && t1.p3.Equals(t2.p3);
    }

    public struct Triangle
    {
        //Corners of the triangle
        public Vector3 p1, p2, p3;

        //The 3 line segments that make up this triangle
        public LineSegment[] lineSegments;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            this.p1 = p1;
            this.p2 = p2;
            this.p3 = p3;

            lineSegments = new LineSegment[3];

            lineSegments[0] = new LineSegment(p1, p2);
            lineSegments[1] = new LineSegment(p2, p3);
            lineSegments[2] = new LineSegment(p3, p1);
        }
    }

    public struct LineSegment
    {
        //Start/end coordinates
        public Vector3 p1, p2;

        public LineSegment(Vector3 p1, Vector3 p2)
        {
            this.p1 = p1;
            this.p2 = p2;
        }
    }

    //The triangle-triangle intersection in 2D algorithm
    bool IsTriangleTriangleIntersecting(Triangle triangle1, Triangle triangle2)
    {
        bool isIntersecting = false;

        //Step 1. AABB intersection
        if (IsIntersectingAABB(triangle1, triangle2))
        {
            //Step 2. Line segment - triangle intersection
            if (AreAnyLineSegmentsIntersecting(triangle1, triangle2))
            {
                isIntersecting = true;
            }
            //Step 3. Point in triangle intersection - if one of the triangles is inside the other
            else if (AreCornersIntersecting(triangle1, triangle2))
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    bool IsIntersectingAABB(Triangle t1, Triangle t2)
    {
        //Find the size of the bounding box

        //Triangle 1
        float t1_minX = Mathf.Min(t1.p1.x, Mathf.Min(t1.p2.x, t1.p3.x));
        float t1_maxX = Mathf.Max(t1.p1.x, Mathf.Max(t1.p2.x, t1.p3.x));
        float t1_minZ = Mathf.Min(t1.p1.z, Mathf.Min(t1.p2.z, t1.p3.z));
        float t1_maxZ = Mathf.Max(t1.p1.z, Mathf.Max(t1.p2.z, t1.p3.z));

        //Triangle 2
        float t2_minX = Mathf.Min(t2.p1.x, Mathf.Min(t2.p2.x, t2.p3.x));
        float t2_maxX = Mathf.Max(t2.p1.x, Mathf.Max(t2.p2.x, t2.p3.x));
        float t2_minZ = Mathf.Min(t2.p1.z, Mathf.Min(t2.p2.z, t2.p3.z));
        float t2_maxZ = Mathf.Max(t2.p1.z, Mathf.Max(t2.p2.z, t2.p3.z));


        //Are the rectangles intersecting?
        //If the min of one box in one dimension is greater than the max of another box then the boxes are not intersecting
        //They have to intersect in 2 dimensions. We have to test if box 1 is to the left or box 2 and vice versa
        bool isIntersecting = true;

        //X axis
        if (t1_minX > t2_maxX)
        {
            isIntersecting = false;
        }
        else if (t2_minX > t1_maxX)
        {
            isIntersecting = false;
        }
        //Z axis
        else if (t1_minZ > t2_maxZ)
        {
            isIntersecting = false;
        }
        else if (t2_minZ > t1_maxZ)
        {
            isIntersecting = false;
        }

        return isIntersecting;
    }

    //Check if any of the edges that make up one of the triangles is intersecting with any of
//the edges of the other triangle
    bool AreAnyLineSegmentsIntersecting(Triangle t1, Triangle t2)
    {
        bool isIntersecting = false;

        //Loop through all edges
        for (int i = 0; i < t1.lineSegments.Length; i++)
        {
            for (int j = 0; j < t2.lineSegments.Length; j++)
            {
                //The start/end coordinates of the current line segments
                Vector3 t1_p1 = t1.lineSegments[i].p1;
                Vector3 t1_p2 = t1.lineSegments[i].p2;
                Vector3 t2_p1 = t2.lineSegments[j].p1;
                Vector3 t2_p2 = t2.lineSegments[j].p2;

                //Are they intersecting?
                if (AreLineSegmentsIntersecting(t1_p1, t1_p2, t2_p1, t2_p2))
                {
                    isIntersecting = true;

                    //To stop the outer for loop
                    i = int.MaxValue - 1;

                    break;
                }
            }
        }

        return isIntersecting;
    }

    //Check if 2 line segments are intersecting in 2d space
//http://thirdpartyninjas.com/blog/2008/10/07/line-segment-intersection/
//p1 and p2 belong to line 1, p3 and p4 belong to line 2
    bool AreLineSegmentsIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
    {
        bool isIntersecting = false;

        float denominator = (p4.z - p3.z) * (p2.x - p1.x) - (p4.x - p3.x) * (p2.z - p1.z);

        //Make sure the denominator is != 0, if 0 the lines are parallel
        if (denominator != 0)
        {
            float u_a = ((p4.x - p3.x) * (p1.z - p3.z) - (p4.z - p3.z) * (p1.x - p3.x)) / denominator;
            float u_b = ((p2.x - p1.x) * (p1.z - p3.z) - (p2.z - p1.z) * (p1.x - p3.x)) / denominator;

            //Is intersecting if u_a and u_b are between 0 and 1
            if (u_a >= 0 && u_a <= 1 && u_b >= 0 && u_b <= 1)
            {
                isIntersecting = true;
            }
        }

        return isIntersecting;
    }

    bool AreCornersIntersecting(Triangle t1, Triangle t2)
    {
        bool isIntersecting = false;

        //We only have to test one corner from each triangle
        //Triangle 1 in triangle 2
        if (IsPointInTriangle(t1.p1, t2.p1, t2.p2, t2.p3))
        {
            isIntersecting = true;
        }
        //Triangle 2 in triangle 1
        else if (IsPointInTriangle(t2.p1, t1.p1, t1.p2, t1.p3))
        {
            isIntersecting = true;
        }

        return isIntersecting;
    }

    //Is a point p inside a triangle p1-p2-p3?
//From http://totologic.blogspot.se/2014/01/accurate-point-in-triangle-test.html
    bool IsPointInTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));
        float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
        float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
        float c = 1 - a - b;

        // The point is within the triangle if...
        return a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f;
    }
}