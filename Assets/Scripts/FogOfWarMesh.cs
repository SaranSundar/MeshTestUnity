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
    private List<Triangle> triangleFaces;


    private void Awake()
    {
        mesh = GetComponent<MeshFilter>().mesh;
    }

    void Start()
    {
        transform.position = new Vector3(0, 0, -4);
        CreateMesh();
        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();
    }

    private void Update()
    {
        FindIntersectingTriangleFaces();
    }

    Triangle applyPositionToTriangle(Triangle original, Vector3 position)
    {
        // The z in the original is always 0, we need to switch positions z and y
        Vector3 p1 = new Vector3(original.p1.x, original.p1.z, original.p1.y) + position;
        Vector3 p2 = new Vector3(original.p2.x, original.p2.z, original.p2.y) + position;
        Vector3 p3 = new Vector3(original.p3.x, original.p3.z, original.p3.y) + position;
        p1.y = 0;
        p2.y = 0;
        p3.y = 0;
        Triangle newTriangle = new Triangle(p1, p2, p3);
        return newTriangle;
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

    void ResetAllFogFacesToBlack()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 255;
        }
    }

    void ChangeTriangleFaceToTransparent(Triangle fogFace)
    {
        //Debug.Log("Colliding");
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Triangle triangleFace = new Triangle(vertices[triangles[i + 0]], vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]);
            if (AreTrianglesEqual(triangleFace, fogFace))
            {
                Debug.Log(triangleFace.p1 + " " + triangleFace.p2 + " " + triangleFace.p3);
                Debug.Log("Colors are " + colors[i] + " " + colors[i + 1] + " " +
                          colors[i + 2]);
                colors[i].a = 150;
                colors[i + 1].a = 150;
                colors[i + 2].a = 150;
            }
        }
    }

    bool AreTrianglesEqual(Triangle t1, Triangle t2)
    {
        if (t1.p1.Equals(t2.p1) && t1.p2.Equals(t2.p2) && t1.p3.Equals(t2.p3))
        {
            return true;
        }

        return false;
    }

    void CalculateTriangleFaces()
    {
        triangleFaces = new List<Triangle>();
        for (int i = 0; i < triangles.Length; i += 3)
        {
            Triangle triangleFace = new Triangle(vertices[triangles[i + 0]], vertices[triangles[i + 1]],
                vertices[triangles[i + 2]]);
            triangleFaces.Add(triangleFace);
        }

        Debug.Log("there are " + triangleFaces.Count + " triangle faces");
    }

    void CreateVerticeCoordinatesFromUnitCircle()
    {
        // 1 unit in local space is 1 meter in unity
        vertices = new Vector3[sides + 1];
        vertices[0] = new Vector3(0, 0, 0);
        for (int i = 1; i <= sides; i++)
        {
            // Points along a unit circle scaled up by size
            float x = size * Mathf.Cos(Mathf.PI * 2 / sides * i);
            float y = size * Mathf.Sin(Mathf.PI * 2 / sides * i);

            vertices[i] = new Vector3(x, y, 0);
            //Debug.Log(vertices[i]);
            
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

    Color32 getColor(int i)
    {
        Color32[] color32 = new Color32[6];
        color32[0] = Color.black;
        color32[1] = Color.gray;
        color32[2] = Color.magenta;
        color32[3] = Color.blue;
        color32[4] = Color.red;
        color32[5] = Color.green;
        int color = i / 3;
        return color32[color];
    }

    public void ColorTriangles()
    {
        Vector3[] verticesModified = new Vector3[triangles.Length];
        int[] trianglesModified = new int[triangles.Length];
        Color32 currentColor = new Color32();
        colors = new Color32[triangles.Length];
        // Create map of triangle values to color values later
        for (int i = 0; i < trianglesModified.Length; i++)
        {
            // Makes every vertex unique
            verticesModified[i] = vertices[triangles[i]];
            trianglesModified[i] = i;
            // Every third vertex randomly chooses new color
            if (i % 3 == 0)
            {
                currentColor = getColor(i);
            }

            colors[i] = currentColor;
            Debug.Log("starting color is " + currentColor);
            Debug.Log(verticesModified[i]);
        }

        // Applyes changes to mesh
        vertices = verticesModified;
        triangles = trianglesModified;
        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors32 = colors;
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        CalculateTriangleFaces();
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
        bool isWithinTriangle = false;

        float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));

        float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
        float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
        float c = 1 - a - b;

        //The point is within the triangle if 0 <= a <= 1 and 0 <= b <= 1 and 0 <= c <= 1
        if (a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f)
        {
            isWithinTriangle = true;
        }

        return isWithinTriangle;
    }
}