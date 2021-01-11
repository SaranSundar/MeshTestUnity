using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using Utils;
using Debug = UnityEngine.Debug;


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
    private Dictionary<int, bool> markedFaces;
    public static Stopwatch m_stopwatch = new Stopwatch();
    private Text time1Text;
    private Text time2Text;
    private Text time3Text;
    private Text fps;

    private Color32[] allowedTriangleFaceColors =
        {Color.black};

    void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        transform.position = new Vector3(0, 2, 0);

        // triangleFaces = MeshUtils.CreateUnitCircleMesh(mesh, allowedTriangleFaceColors, numOfTriangleFaces, size);
        triangleFaces = MeshUtils.CreateGridMesh(mesh, allowedTriangleFaceColors, 100, 100, 1f, 1f);
        vertices = mesh.vertices;
        colors = mesh.colors32;
        triangles = mesh.triangles;

        markedFaces = new Dictionary<int, bool>();

        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();

        time1Text = GameObject.Find("IntersectionsInnerTime").GetComponent<Text>();
        time2Text = GameObject.Find("FindIntersectingTIme").GetComponent<Text>();
        time3Text = GameObject.Find("UpdateTime").GetComponent<Text>();
        fps = GameObject.Find("FPS").GetComponent<Text>();
    }

    private void Update()

    {
        var startTime = DateTime.Now;
        FindIntersectingTriangleFaces();
        var elapsed = (DateTime.Now - startTime).Milliseconds;

        //Debug.Log("3 update: " + elapsed);
        time3Text.text = "3 update: " + elapsed + " ms";
        fps.text = "FPS: " + (1 / Time.deltaTime);
        //Debug.Log(1 / Time.deltaTime);
    }

    void FindIntersectingTriangleFaces()
    {
        var startTime1 = DateTime.Now;

        List<TriangleUtils.Triangle> playerTriangleFaces = fieldOfViewMesh.triangleFaces;
        List<TriangleUtils.Triangle> fogOfWarTriangleFaces = triangleFaces;

        ResetAllFogFacesToBlack();


        // for (int or = 0; or < numOfThreads; or++)
        // {
        //     for (int r = playerTriangleFaces.Count / numOfThreads * or; r < playerTriangleFaces.Count / numOfThreads * (or + 1); r++)
        //     {
        //         for (int c = 0; c < fogOfWarTriangleFaces.Count; c++)
        //         {
        //             Vector3 fieldOfViewMeshTransformPosition = fieldOfViewMesh.transform.position;
        //             Vector3 fogOfWarMeshTransformPosition = transform.position;
        //             StartCoroutine(ThreadCallingRoutine(playerTriangleFaces, fogOfWarTriangleFaces, r, c,
        //                 fieldOfViewMeshTransformPosition, fogOfWarMeshTransformPosition));
        //         }
        //     }
        // }

        //replace this with checking if a color is transparent so the face is marked
        markedFaces.Clear();

        float x = 0;
        float y = 0;

        int gridPaddingHeight = 10;
        int gridPaddingWidth = 10;

        int rowStart = 0;
        int rowEnd = rowStart + gridPaddingHeight;

        int colStart = 0;
        int colEnd = colStart + gridPaddingWidth;

        var startTime = DateTime.Now;

        for (int r = 0; r < playerTriangleFaces.Count; r++)
        {
            for (int c = 0; c < 100; c++)
            {
                if (!markedFaces.ContainsKey(c) || !markedFaces[c])
                {
                    Vector3 fieldOfViewMeshTransformPosition = fieldOfViewMesh.transform.position;
                    Vector3 fogOfWarMeshTransformPosition = transform.position;
                    TriangleUtils.Triangle playerFace =
                        applyPositionToTriangle(playerTriangleFaces[r], fieldOfViewMeshTransformPosition);
                    TriangleUtils.Triangle fogFace =
                        applyPositionToTriangle(fogOfWarTriangleFaces[c], fogOfWarMeshTransformPosition);

                    if (TriangleUtils.IsTriangleTriangleIntersecting(playerFace, fogFace))
                    {
                        markedFaces.Add(c, true);
                        ChangeTriangleFaceToTransparent(fogOfWarTriangleFaces[c]);
                    }
                }
            }
        }

        var elapsed = (DateTime.Now - startTime).Milliseconds;

        //Debug.Log("1 intersections: " + elapsed);
        time1Text.text = "1 intersections: " + elapsed + " ms";

        //KEEP TRACK IF A FACE IS MARKED

        // for (int r = 0; r < playerTriangleFaces.Count; r++)
        // {
        //     Vector3 fieldOfViewMeshTransformPosition = fieldOfViewMesh.transform.position;
        //     Vector3 fogOfWarMeshTransformPosition = transform.position;
        //     StartCoroutine(ThreadCallingRoutine(playerTriangleFaces, fogOfWarTriangleFaces, r,
        //         fieldOfViewMeshTransformPosition, fogOfWarMeshTransformPosition));
        // }

        mesh.colors32 = colors;

        var elapsed1 = (DateTime.Now - startTime1).Milliseconds;

        //Debug.Log("2 FindIntersectingTriangles: " + elapsed1);
        time2Text.text = "2 FindIntersectingTriangles: " + elapsed1 + " ms";
    }

    public IEnumerator ThreadCallingRoutine(List<TriangleUtils.Triangle> playerTriangleFaces,
        List<TriangleUtils.Triangle> fogOfWarTriangleFaces, int r, Vector3 fieldOfViewMeshTransformPosition,
        Vector3 fogOfWarMeshTransformPosition)
    {
        while (true)
        {
            for (int c = 0; c < fogOfWarTriangleFaces.Count; c++)
            {
                TriangleUtils.Triangle playerFace =
                    applyPositionToTriangle(playerTriangleFaces[r], fieldOfViewMeshTransformPosition);
                TriangleUtils.Triangle fogFace =
                    applyPositionToTriangle(fogOfWarTriangleFaces[c], fogOfWarMeshTransformPosition);

                if (TriangleUtils.IsTriangleTriangleIntersecting(playerFace, fogFace))
                {
                    ChangeTriangleFaceToTransparent(fogOfWarTriangleFaces[c]);
                }
            }


            yield return null;
        }
    }

    // StartCoroutine(WaitForAttackToComplete());
    //
    // public IEnumerator WaitForAttackToComplete()
    // {
    //     yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length +
    //                                     animator.GetCurrentAnimatorStateInfo(0).normalizedTime);
    //     AttackComplete();
    // }

    TriangleUtils.Triangle applyPositionToTriangle(TriangleUtils.Triangle original, Vector3 position)
    {
        // The z in the original is always 0, we need to switch positions z and y
        Vector3 p1 = original.p1 + position;
        Vector3 p2 = original.p2 + position;
        Vector3 p3 = original.p3 + position;
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