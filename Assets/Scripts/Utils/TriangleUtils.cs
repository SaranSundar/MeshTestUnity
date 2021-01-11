using UnityEngine;

namespace Utils
{
    public static class TriangleUtils
    {
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

        public static bool AreTrianglesEqual(Triangle t1, Triangle t2)
        {
            return t1.p1.Equals(t2.p1) && t1.p2.Equals(t2.p2) && t1.p3.Equals(t2.p3);
        }

        //The triangle-triangle intersection in 2D algorithm
        public static bool IsTriangleTriangleIntersecting(Triangle triangle1, Triangle triangle2)
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

        public static bool IsIntersectingAABB(Triangle t1, Triangle t2)
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
        public static bool AreAnyLineSegmentsIntersecting(Triangle t1, Triangle t2)
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
        public static bool AreLineSegmentsIntersecting(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
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

        public static bool AreCornersIntersecting(Triangle t1, Triangle t2)
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
        public static bool IsPointInTriangle(Vector3 p, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            float denominator = ((p2.z - p3.z) * (p1.x - p3.x) + (p3.x - p2.x) * (p1.z - p3.z));
            float a = ((p2.z - p3.z) * (p.x - p3.x) + (p3.x - p2.x) * (p.z - p3.z)) / denominator;
            float b = ((p3.z - p1.z) * (p.x - p3.x) + (p1.x - p3.x) * (p.z - p3.z)) / denominator;
            float c = 1 - a - b;

            // The point is within the triangle if...
            return a >= 0f && a <= 1f && b >= 0f && b <= 1f && c >= 0f && c <= 1f;
        }
    }
}