using UnityEngine;
using System.Collections;

public class BoltTest : MonoBehaviour
{
    public MeshFilter filter;

	// Use this for initialization
	void Start ()
    {
        MeshCreator.Reset();
        MeshCreator.AddBolt(1, 0.625f);
        filter.sharedMesh = MeshCreator.Create();
	}
	
	// Update is called once per frame
	void Update ()
    {
	
	}
}
