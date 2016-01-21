using UnityEngine;
using System.Collections;

//Attached to a gameObject, this script holds the member mesh data
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
public class ShapeControl : MonoBehaviour
{
    public Color Color
    {
        set
        {
            meshRenderer.material.color = value;
        }

        get
        {
            return meshRenderer.material.color;
        }
    }

    public Mesh Mesh
    {
        set
        {
            meshFilter.mesh = value;
            meshCollider.sharedMesh = value;
        }

        get
        {
            return meshFilter.mesh;
        }
    }

    /// <summary>
    /// Sets/Gets the world positon of the mesh
    /// </summary>
    public Vector3 Position
    {
        set
        {
            meshTransform.position = value;
        }

        get
        {
            return meshTransform.position;
        }
    }

    //Hide these variables from code, but show them in the inspector with serializeField
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private Transform meshTransform; //Cache the transform object
    public ViewMask viewMask;       //Used to diabled the mesh from being draw in certain views

    private void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
    }
}
