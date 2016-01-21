using UnityEngine;

public class TestShape : MonoBehaviour {

	// Use this for initialization
	void Start () 
    {
        Vector3[] vert = new Vector3[]
        {
            //Side 1 of cube
            new Vector3 (-1, 1, 0),
            new Vector3 (1, 1, 0),
            new Vector3 (1, -1, 0),
            new Vector3 (-1, -1, 0),

            //Side 2 of cube
            new Vector3 (-1, 1, 1),
            new Vector3 (1, 1, 1),
            new Vector3 (1, -1, 1),
            new Vector3 (-1, -1, 1)

        };

        int[] tris = new int[]
        {
            0,1,2,
            0,3,2
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vert;
        mesh.triangles = tris;

        mesh.RecalculateNormals();

        if (!GetComponent<MeshFilter>())
            gameObject.AddComponent<MeshFilter>();

        if (!GetComponent<MeshRenderer>())
            gameObject.AddComponent<MeshRenderer>();

        gameObject.GetComponent<MeshFilter>().mesh = mesh;
	}
	
	
}
