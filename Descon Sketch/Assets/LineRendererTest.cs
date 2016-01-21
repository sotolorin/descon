using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GUIText))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(LineRenderer))]
public partial class LineRendererTest : MonoBehaviour
{
    List<Vector3> linePoints = new List<Vector3>();
    List<Vector3> cuelinePoints = new List<Vector3>(); //For visual drag cue
    public float startWidth = 1.0f;
    public float endWidth = 1.0f;
    public float threshold = 0.001f;
    Camera thisCamera;
    int lineCount = 0;
    public Mesh mesh;
    public Mesh planemesh;
    public LineRenderer line; // Assign via inspector
    public MeshFilter meshf; // Assign via inspector
    public MeshRenderer meshr; // Assign via inspector
    public MeshCollider meshc; // Assign via inspector
    public GUIText textComponent; // Assign via inspector
    public Button button; // Assign via inspector
    bool isDrawing;
    private int numberOfPoints = 0;
    private Vector3 newPosition;
    private Vector3 closestVtx;
    private float _beamHeight;
    private string inputstring;
    internal static List<Vector3> originalVerts;
    internal static Vector3 originalRotation;
    private float maxLength;
    private static Vector3? clickPosition;
    private int dragthreshhold = 4;
    static int row = 3;
    static int col = 3;

    void Awake()
    {
        MeshCreator.Initialize();
        thisCamera = Camera.main;
        thisCamera.isOrthoGraphic = true;    
        thisCamera.orthographicSize = 25.0f;
        line = GetComponent<LineRenderer>();
        line.SetWidth(1f, 1f);
        line.SetColors(Color.green, Color.green);
        textComponent= GetComponent<GUIText>();
        textComponent.fontSize = 16;
        textComponent.color = Color.white;
        textComponent.fontStyle = FontStyle.Bold;
        textComponent.pixelOffset =new Vector2(650, 175);
        meshf = GetComponent<MeshFilter>();
        if (!meshf) meshf = gameObject.AddComponent<MeshFilter>();
        meshr = GetComponent<MeshRenderer>();
        meshr.material.SetColor("_Color", new Color(0f / 255f, 0f / 255f, 0f / 255f)); //start scene black
        meshc = GetComponent<MeshCollider>();
        linePoints.Clear();
        linePoints.Add(Vector3.zero);
        newPosition = Vector3.zero;
        closestVtx = Vector3.zero;
        originalVerts = new List<Vector3>();
        originalRotation = new Vector3(-30, -30, 15);
        maxLength = 100f;
        clickPosition = null;
        _beamHeight = 0.01f;
        button = GetComponent<Button>();
        button.GetComponent<Button>().onClick.AddListener(ResetAll);

        DrawPlane();
    }

    public void DrawPlane()
    {
        planemesh = CreateSinglePlateMesh(150, 150);
        planemesh.RecalculateNormals();
        planemesh.RecalculateBounds();
        meshf.mesh = planemesh;
        meshc.sharedMesh = planemesh;
        //thisCamera.orthographicSize = 3f * GetFieldOfView(planemesh.bounds.max, 0.01f);
    }

    //public void DrawGrid()
    //{
    //    //meshr.renderer.material.color = new Color(0.5f,1,1);
    //    meshr.renderer.material.color = Color.grey;
    //    var grids = CreateGridPlateMesh(5, 5);
    //    var combine = new CombineInstance[grids.ToArray().Length];
    //     int i = 0;
    //     while (i < grids.ToArray().Length)
    //     {
    //         meshf.sharedMesh = grids.ToArray()[i];
    //         combine[i].mesh = grids.ToArray()[i];
    //         combine[i].transform = meshf.transform.localToWorldMatrix;
    //         //meshFilters[i].gameObject.active = false;
    //         i++;
    //     }
    //     //GameObject combinedMesh = new GameObject("CombinedMesh");

    //     //combinedMesh.AddComponent("MeshFilter");
    //     //combinedMesh.AddComponent("MeshRenderer");
    //     //combinedMesh.GetComponent<MeshFilter>().sharedMesh = new Mesh();
    //     meshf.mesh.CombineMeshes(combine);
    //}

    public Vector3 NearestVertexTo(Vector3 point)
    {
        var minDistanceSqr = Mathf.Infinity;
        var nearestVertex = Vector3.zero;

        foreach (var vertex in mesh.vertices)
        {
            var diff = point - vertex;
            var distSqr = diff.sqrMagnitude;

            if (distSqr < minDistanceSqr)
            {
                minDistanceSqr = distSqr;
                nearestVertex = vertex;
            }
        }
        return transform.TransformPoint(nearestVertex);
    }



    private void Update()
    {
        if (clickPosition == null && Input.GetKey(KeyCode.Mouse0))
        {
            if (planemesh == null) return;
            var hit = new RaycastHit();
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                clickPosition = hit.point;
                meshr.material.SetColor("_Color", new Color(251f / 255f, 41f / 255f, 237f / 255f)); //bright magenta
                DrawMesh(_beamHeight);
            }
            return;
        }

        // text input updates
        if (Input.inputString.Length > 0)
        {
            foreach (var c in Input.inputString)
            {
                if (c == '\b')
                {
                    if (textComponent.text.Length != 0)
                    {
                        SetText(textComponent.text.Substring(0, textComponent.text.Length - 1));
                    }
                }
                else
                {
                    if (c == '\n' || c == '\r')
                    {
                        Debug.Log("User entered mesh height: " + textComponent.text);
                        var ht = float.Parse(textComponent.text, CultureInfo.InvariantCulture);
                        if (ht.Equals(0)) ht = 1f;
                        DrawMesh(ht);
                    }
                    else if (((c < '0' || c > '9') && c != '.') || (c == '.' && textComponent.text.Contains(".")))
                        continue;
                    else
                    {
                        var add = c.ToString();
                        SetText(add, true);
                    }
                }
            }
        }

        //Drag Updates
        if ( Input.GetKey(KeyCode.Mouse0)) //mouse down
        {
            //thisCamera.isOrthoGraphic = false;
            var mousePos = Vector3.zero;
            mousePos = Input.mousePosition;
            //if (closestVtx == Vector3.zero) closestVtx = NearestVertexTo(mousePos);
            numberOfPoints++;
            line.SetVertexCount(numberOfPoints);
            var worldPos = thisCamera.WorldToScreenPoint(mousePos);
            //line.SetPosition(numberOfPoints - 1, worldPos);
            linePoints.Add(new Vector3(worldPos.x, worldPos.y, worldPos.z));
            var endPt = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            line.SetPosition(0, endPt);
            cuelinePoints.Add(new Vector3(endPt.x, endPt.y, endPt.z));
            return;
        }
        else
        {
            numberOfPoints = 0;
            line.SetVertexCount(0);
        }
        if (linePoints.Count < dragthreshhold) return;
        //var quad = Math.Sign(closestVtx.y);
        var isDecrease = linePoints[0].y > linePoints[linePoints.Count - 1].y;
        var distance = linePoints.Max(pt => pt.y) - linePoints[0].y;
        //thisCamera.isOrthoGraphic = true;
        linePoints.Clear();
        cuelinePoints.Clear();
        closestVtx = Vector3.zero;
        var lowerLeft = new Vector3(-renderer.bounds.size.x/2, -renderer.bounds.size.y/2, 0f);
        var upperRight = new Vector3(renderer.bounds.size.x/2, renderer.bounds.size.y/2, 0f);
        lowerLeft = thisCamera.WorldToScreenPoint(lowerLeft);
        upperRight = thisCamera.WorldToScreenPoint(upperRight);

        var horzPixelsPerWorldUnit = upperRight.x - lowerLeft.x;
        distance = distance / horzPixelsPerWorldUnit; //distance/(horzPixelsPerWorldUnit/.85);
        var newVertices = new List<Vector3>();
        var newOriginalVertices = new List<Vector3>();
        if (originalVerts == null || !originalVerts.Any() || clickPosition == null) return;
        var maxY = Math.Round(originalVerts.Select(vt => vt.y).Max(), 3);
        for (var i = 0; i < originalVerts.Count; i++)
        {
            var vtc = originalVerts[i];
            var originalrot = Quaternion.Euler(originalRotation);
            var ver = vtc;
            var y = distance;
            if (!isDecrease && Math.Round(ver.y, 3).Equals(maxY))
            {
                ver.y += y;
                vtc.y += y;
            }
            else if (isDecrease && Math.Round(ver.y, 3).Equals(maxY))
            {
                ver.y -= y;
                vtc.y -= y;
            }
            newOriginalVertices.Add(vtc);
            ver = originalrot * ver;
            newVertices.Add(ver);
        }
        originalVerts = newOriginalVertices;
        mesh.vertices = newVertices.ToArray();
        _beamHeight = ((isDecrease?(-1):1) *(distance)) + Math.Abs(originalVerts.OrderByDescending(vt => vt.y).First().y);  //_beamHeight = Math.Abs(newVertices.OrderByDescending(vt => vt.y).First().y);
        SetText(_beamHeight.ToString());
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        meshf.sharedMesh = mesh;
        meshc.sharedMesh = meshf.mesh;
    }

    private void SetText(string txt, bool addIt = false)
    {     
        if (addIt)
        {
            var newTxt = textComponent.text += txt;
            if (float.Parse(newTxt, CultureInfo.InvariantCulture) > maxLength)
            {
                textComponent.text = textComponent.text.Substring(0, textComponent.text.Length - 1);
                return;
            }
            textComponent.text = newTxt;
        }
        else textComponent.text = txt; //delete text

        if (textComponent.text.Length == 0 || textComponent.text.EndsWith(".")) return;
        try
        {
            _beamHeight = float.Parse(textComponent.text, CultureInfo.InvariantCulture);
        }
        catch (Exception)
        {
            _beamHeight = 0;
        }
        if (_beamHeight.Equals(0)) _beamHeight = 1f;
    }

    void DrawMesh(float length)
    {
        thisCamera.isOrthoGraphic = true;
        mesh = CreateWideFlangeMesh(length);
        meshf.sharedMesh = mesh;
        meshc.sharedMesh = meshf.mesh;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        thisCamera.orthographicSize = _beamHeight.Equals(0) || _beamHeight < 35 ?25f: 3f * GetFieldOfView(mesh.bounds.max, _beamHeight);
    }

    float GetFieldOfView(Vector3 objectPosition, float objectHeight)
    {
        var diff = objectPosition - Camera.main.transform.position;
        var distance = Vector3.Dot(diff, Camera.main.transform.forward);
        var angle = Mathf.Atan((objectHeight * .5f) / distance);
        return angle * 2f * Mathf.Rad2Deg;
    }

    public Rect BoundsToScreenRect(Bounds bounds)
    {
        var origin = Camera.main.WorldToScreenPoint(new Vector3(bounds.min.x, bounds.max.y, bounds.center.z));
        var extent = Camera.main.WorldToScreenPoint(new Vector3(bounds.max.x, bounds.min.y, 0f));

        return new Rect(origin.x, Screen.height - origin.y, extent.x - origin.x, origin.y - extent.y);
    }

    void ResetAll()
    {
        Awake();
    }

    public void ClearAll()
    {
        foreach (var o in FindObjectsOfType<GameObject>())
        {
            Destroy(o);
        }
        linePoints.Clear();
        MeshCreator.Reset();
    }
}