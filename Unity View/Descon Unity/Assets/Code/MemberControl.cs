using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum ViewMask {LEFT = 1, TOP = 2, RIGHT = 4, FRONT = 8, D3 = 16}

/// <summary>
/// This class is attached to beam members and handles the selection/picking
/// </summary>
public class MemberControl : MonoBehaviour
{
    public GameObject connectionPrefab;
    public Color defaultColor;
    public Color selectedColor;

    private RaycastHit hit;
    public float highlightStrength = 0.1f;
    public bool isSelected = false;

    public string shapeName;
    public GameObject connectionMeshObject;
    public List<ConnectionControl> connectionObjects;
    public Dictionary<ViewMask, Drawable> drawables;
    public ViewMask viewMask;
    public Descon.Data.EMemberType memberType;
    public Descon.Data.EShapeType shapeType;
    private MeshFilter connectionFilter;
    public Dictionary<ViewMask, List<CustomLine>> customLines;
    public bool useCustomLines = false;
    public float customLineOffset = 0.001f;

    void Awake()
    {
        customLines = new Dictionary<ViewMask, List<CustomLine>>();

        customLines.Add(ViewMask.D3, new List<CustomLine>());
        customLines.Add(ViewMask.FRONT, new List<CustomLine>());
        customLines.Add(ViewMask.LEFT, new List<CustomLine>());
        customLines.Add(ViewMask.RIGHT, new List<CustomLine>());
        customLines.Add(ViewMask.TOP, new List<CustomLine>());

        connectionMeshObject = (GameObject)Instantiate(connectionPrefab);
        connectionMeshObject.transform.parent = this.transform;

        connectionFilter = connectionMeshObject.GetComponent<MeshFilter>();
        connectionFilter.sharedMesh = new Mesh();
        connectionObjects = new List<ConnectionControl>();

        drawables = new Dictionary<ViewMask, Drawable>();
        drawables.Add(ViewMask.D3, new Drawable());
        drawables.Add(ViewMask.FRONT, new Drawable());
        drawables.Add(ViewMask.LEFT, new Drawable());
        drawables.Add(ViewMask.RIGHT, new Drawable());
        drawables.Add(ViewMask.TOP, new Drawable());
    }

	// Use this for initialization
	void Start ()
    {
        renderer.material.color = defaultColor;
	}

    public Mesh GetConnectionMesh()
    {
        return connectionFilter.sharedMesh;
    }

    public MeshFilter GetConnectionFilter()
    {
        return connectionFilter;
    }

    public void AddConnection(Mesh mesh, Material material, Descon.Data.EMemberType memberType, Descon.Data.EMemberSubType subMemberType, ViewMask mask)
    {
        var obj = (GameObject)Instantiate(connectionPrefab);
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<Renderer>().material = material;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;

        var conCtrl = obj.GetComponent<ConnectionControl>();
        conCtrl.defaultColor = material.color;
        conCtrl.parentMemberType = memberType;
        conCtrl.subMemberType = subMemberType;
        conCtrl.viewMask = mask;

        obj.transform.parent = this.transform;

        connectionObjects.Add(conCtrl);
    }

    public void AddConnection(Mesh mesh, Material material, Descon.Data.EMemberType memberType, Descon.Data.EMemberSubType subMemberType, ViewMask mask, Dictionary<ViewMask, List<CustomLine>> customLines, float customLineOffset)
    {
        var obj = (GameObject)Instantiate(connectionPrefab);
        obj.GetComponent<MeshFilter>().sharedMesh = mesh;
        obj.GetComponent<Renderer>().material = material;
        obj.GetComponent<MeshCollider>().sharedMesh = mesh;

        var conCtrl = obj.GetComponent<ConnectionControl>();
        conCtrl.defaultColor = material.color;
        conCtrl.parentMemberType = memberType;
        conCtrl.subMemberType = subMemberType;
        conCtrl.viewMask = mask;
        conCtrl.useCustomLines = true;
        conCtrl.customLines.Clear();

        foreach (var item in customLines)
        {
            conCtrl.customLines[item.Key] = new List<CustomLine>(item.Value);
        }

        conCtrl.customLineOffset = customLineOffset;

        obj.transform.parent = this.transform;

        connectionObjects.Add(conCtrl);
    }

    public void DestroyConnectionMeshes()
    {
        foreach(var item in connectionObjects)
        {
            //item.DestroyDrawables();
            Destroy(item.gameObject);
        }

        connectionObjects.Clear();
    }

    void OnDestroy()
    {
        if (connectionMeshObject != null) Destroy(connectionMeshObject);
    }

    public void SetSelected(bool selected, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        isSelected = selected;

        if (isSelected)
        {
            renderer.material.color = selectedColor;

            if(sendMessage)
            {
                var click = Descon.Data.EClickType.Single;

                if(doubleClicked)
                    click = Descon.Data.EClickType.Double;
                else if(mouseButton == 1)
                {
                    click = Descon.Data.EClickType.Right;
                }

                MessageQueueTest.instance.SendUnityData(MessageQueueTest.GetClickString(memberType, Descon.Data.EMemberSubType.Main, click));
            }
        }
        else
        {
            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor;
        }
    }

    public void SetSelected(GameObject obj, bool sendMessage = false, int mouseButton = 0, bool doubleClicked = false)
    {
        SetSelected(obj == gameObject, sendMessage, mouseButton, doubleClicked);
    }

    public void SetHover(GameObject obj)
    {
        if (obj == gameObject)
        {
            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor + new Color(highlightStrength, highlightStrength, highlightStrength, 1);
        }
        else
        {
            if (isSelected)
            {
                renderer.material.color = selectedColor;
            }
            else
                renderer.material.color = defaultColor;
        }
    }
}
