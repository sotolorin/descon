using UnityEngine;
using System.Collections.Generic;

public class DrawTest : MonoBehaviour
{
    public float width = 1.0f;
    public Color color = Color.black;

    //public List<Transform> verts;

    void Start()
    {
        ////Create the mesh
        //MeshCreator.Reset();

        ////Make a simple cube
        //MeshCreator.AddQuad(new Vector3(-10, 10, -10), new Vector3(-10, 10, 10), new Vector3(10, 10, 10), new Vector3(10, 10, -10));
        //MeshCreator.AddQuad(new Vector3(-10, -10, -10), new Vector3(-10, -10, 10), new Vector3(-10, 10, 10), new Vector3(-10, 10, -10));
        //MeshCreator.AddQuad(new Vector3(10, 10, -10), new Vector3(10, 10, 10), new Vector3(10, -10, 10), new Vector3(10, -10, -10));
        //MeshCreator.AddQuad(new Vector3(10, -10, -10), new Vector3(10, -10, 10), new Vector3(-10, -10, 10), new Vector3(-10, -10, -10));
        //MeshCreator.AddQuad(new Vector3(-10, -10, 10), new Vector3(10, -10, 10), new Vector3(10, 10, 10), new Vector3(-10, 10, 10));
        //MeshCreator.AddQuad(new Vector3(-10, 10, -10), new Vector3(10, 10, -10), new Vector3(10, -10, -10), new Vector3(-10, -10, -10));

        ////Change the mesh of the object this is attached to
        //GetComponent<MeshFilter>().sharedMesh = MeshCreator.Create();

        //MeshCreator.Reset();

        GetComponent<MeshFilter>().sharedMesh = MeshCreator.CreateBoxMesh(new Vector3(5, 2, 10), new Vector3(6, 1, 10), new Vector3(0, 45, 0));
    }

    //Draw the lines
    //void OnGUI()
    //{
    //    var verts = GetComponent<MeshFilter>().sharedMesh.vertices;

    //    for (int i = 0; i < verts.Length; ++i)
    //    {
    //        verts[i] = transform.rotation * verts[i];
    //    }

    //    for(int i = 0; i < verts.Length / 6; ++i)
    //    {
    //        DrawEdge(Camera.main.WorldToScreenPoint(verts[i * 6 + 0]), Camera.main.WorldToScreenPoint(verts[i * 6 + 1]));
    //        DrawEdge(Camera.main.WorldToScreenPoint(verts[i * 6 + 1]), Camera.main.WorldToScreenPoint(verts[i * 6 + 2]));
    //        DrawEdge(Camera.main.WorldToScreenPoint(verts[i * 6 + 3]), Camera.main.WorldToScreenPoint(verts[i * 6 + 4]));
    //        DrawEdge(Camera.main.WorldToScreenPoint(verts[i * 6 + 4]), Camera.main.WorldToScreenPoint(verts[i * 6 + 0]));
    //    }
    //}

    //void DrawEdge(Vector3 pointA, Vector3 pointB)
    //{
    //    pointA.y = Screen.height - pointA.y;
    //    pointB.y = Screen.height - pointB.y;

    //    Drawing.DrawLine(pointA, pointB, color, width, false);
    //}
}
