using System;
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

    private Color32[] allowedTriangleFaceColors =
        {Color.red, Color.blue, Color.cyan, Color.green, Color.magenta, Color.yellow};

    private void Start()
    {
        mesh = GetComponent<MeshFilter>().mesh;
        transform.position = new Vector3(0, 1, 0);

        triangleFaces = MeshUtils.CreateUnitCircleMesh(mesh, allowedTriangleFaceColors, numOfTriangleFaces, size);
        vertices = mesh.vertices;
        colors = mesh.colors32;
        triangles = mesh.triangles;
    }

    private void Update()
    {
        Transform transform1;
        (transform1 = transform).position = transform.position + Vector3.back * (Input.GetAxisRaw("Vertical") * Time.deltaTime * 10);
        transform.position = transform1.position + Vector3.right * (Input.GetAxisRaw("Horizontal") * Time.deltaTime * 10);
        // if (Input.GetKey(KeyCode.UpArrow))
        // {
        //     var transformPosition = transform.position;
        //     transformPosition.z += 1;
        //     transform.position = transformPosition;
        // }
        // if (Input.GetKey(KeyCode.DownArrow))
        // {
        //     var transformPosition = transform.position;
        //     transformPosition.z -= 1;
        //     transform.position = transformPosition;
        // }
        // if (Input.GetKey(KeyCode.LeftArrow))
        // {
        //     var transformPosition = transform.position;
        //     transformPosition.x -= 1;
        //     transform.position = transformPosition;
        // }
        // if (Input.GetKey(KeyCode.RightArrow))
        // {
        //     var transformPosition = transform.position;
        //     transformPosition.x += 1;
        //     transform.position = transformPosition;
        // }
    }
}