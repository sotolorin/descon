using UnityEngine;
using System.Collections;

public class CalloutTest : MonoBehaviour
{
    public DimensionCallout callout;
    public Vector3 pointA;
    public Vector3 pointB;
    public Vector3 pointC;
    public Vector3 pointD;

	// Use this for initialization
	void Start ()
    {
        //Put a sapce infront to balance the inches symbol
        callout.Text = " 5.5\"";
        callout.SetPoints(pointA, pointB, pointC, pointD);
	}
}
