using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LineTest : MonoBehaviour
{
    public float size = 0.00157f;
    public int numSegments = 10;
    private GameObject lineHolder;
    public List<Vector3> positions;
    public float t = 0.1f;
    public float t2 = 0.4f;
    public float segmentLength = 1f;
    public float spacing = 0.1f;
    public float length = 10.0f;

    private Mesh mesh;

	// Use this for initialization
	void Start ()
    {
        lineHolder = MeshCreator.CreateLineObject("Line Holder");

        mesh = lineHolder.GetComponent<MeshFilter>().sharedMesh;
        
	}
	
	// Update is called once per frame
	void Update ()
    {
        MeshCreator.Reset();
        MeshCreator.DrawBezierCurveWorldSpace(Camera.main, positions[0], positions[1], positions[2], positions[3], numSegments, size);

        var a = MeshCreator.GetBezierPoint(positions[0], positions[1], positions[2], positions[3], t);
        var b = MeshCreator.GetBezierPoint(positions[0], positions[1], positions[2], positions[3], 1 - t);

        MeshCreator.DrawEdgeWorldSpace(Camera.main, a, a + Vector3.down * length, size);
        MeshCreator.DrawEdgeWorldSpace(Camera.main, b, b + Vector3.down * length, size);

        a = MeshCreator.GetBezierPoint(positions[0], positions[1], positions[2], positions[3], t2);
        b = MeshCreator.GetBezierPoint(positions[0], positions[1], positions[2], positions[3], 1 - t2);

        MeshCreator.DrawDottedEdgeWorldSpace(Camera.main, a, a + Vector3.down * length, size, segmentLength, spacing);
        MeshCreator.DrawDottedEdgeWorldSpace(Camera.main, b, b + Vector3.down * length, size, segmentLength, spacing);

        //Draw the bottom spline
        MeshCreator.DrawBezierCurveWorldSpace(Camera.main, positions[0] + Vector3.down * length, positions[1] + Vector3.down * length, positions[2] + Vector3.down * length, positions[3] + Vector3.down * length, numSegments, size);

        MeshCreator.Create(mesh);
	} 
}
