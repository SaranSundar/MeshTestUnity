using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

        // triangleFaces = MeshUtils.CreateUnitCircleMesh(mesh, allowedTriangleFaceColors, numOfTriangleFaces, size);
        triangleFaces = MeshUtils.CreateGridMesh(mesh, allowedTriangleFaceColors, 10, 10, 1f, 1f);
        vertices = mesh.vertices;
        colors = mesh.colors32;
        triangles = mesh.triangles;

        fieldOfViewMesh = GameObject.Find("FieldOfViewMesh").GetComponent<FieldOfViewMesh>();
    }

    private void Update()

    {
        //FindIntersectingTriangleFaces();
        Debug.Log(1 / Time.deltaTime);
    }

    void FindIntersectingTriangleFaces()
    {
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

        //KEEP TRACK IF A FACE IS MARKED

        for (int r = 0; r < playerTriangleFaces.Count; r++)
        {
            Vector3 fieldOfViewMeshTransformPosition = fieldOfViewMesh.transform.position;
            Vector3 fogOfWarMeshTransformPosition = transform.position;
            StartCoroutine(ThreadCallingRoutine(playerTriangleFaces, fogOfWarTriangleFaces, r,
                fieldOfViewMeshTransformPosition, fogOfWarMeshTransformPosition));
        }

        mesh.colors32 = colors;
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