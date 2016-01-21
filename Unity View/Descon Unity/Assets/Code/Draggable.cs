using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public Camera cam;
    public bool isDragging = false;
    public Vector3 offset;              //Offset corresponds to the position of the member, the center point
    public Vector3 offset2;             //Used for additional leader arrows
    //[HideInInspector]
    public GameObject anchor;
    public GameObject arrowObject;
    public bool isWorldSpace;
    public bool useAddLeader;
    public float textVerticalOffset = 0.2f;
    public bool isWeld;
    public bool noArrow = false;
    public int weldType = 0;
    public Text labelText;
    public RectTransform rectTransform;
    public GameObject panel;
    public GameObject additionalTextObj;
    public Vector3 leftTextPosition;
    public Vector3 rightTextPosition;
    public ViewportControl viewControl;
    public CameraViewportType portType;
    [SerializeField]
    private ArrowAnchor arrowAnchorPrefab;

    [HideInInspector]
    public RectTransform arrowAnchor;
    private Vector3 draggingOffset;

    public Color LineColor
    {
        get
        {
            return arrowObject.renderer.material.color;
        }

        set
        {
            arrowObject.renderer.material.color = value;
        }
    }

    void Awake()
    {
        arrowObject = MeshCreator.CreateLineObject("Arrow Line");
        anchor = new GameObject("Anchor");

        if (arrowAnchorPrefab != null)
        {
            var arrowObj = (GameObject)Instantiate(arrowAnchorPrefab.gameObject);
            arrowAnchor = arrowObj.transform as RectTransform;
            arrowObj.GetComponent<ArrowAnchor>().draggable = this;
        }
    }

    public void InitCamera(Camera cam)
    {
        this.cam = cam;
    }

    void LateUpdate()
    {
        if (isDragging)
        {
            var vec = cam.ScreenToWorldPoint(Input.mousePosition);
            anchor.transform.position = vec;

            //viewControl.changed = true;
        }

        if (!isWeld)
        {
            //Determine pivot position
            var leftSide = GetViewSide(cam.GetComponent<ViewportControl>().portType);
            rectTransform.pivot = new Vector2(leftSide == true ? 1 : 0, 0);

            var pos = RectTransformUtility.WorldToScreenPoint(cam, anchor.transform.position) - new Vector2(cam.pixelRect.x, cam.pixelRect.y);
            rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        }

        if (arrowAnchor != null)
        {
            var pos = RectTransformUtility.WorldToScreenPoint(cam, offset) - new Vector2(cam.pixelRect.x, cam.pixelRect.y);
            arrowAnchor.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
        }

        //if(isDragging)
        //{
        //    if (!isWorldSpace)
        //    {
        //        transform.localPosition = Input.mousePosition -new Vector3(cam.pixelWidth / 2 + cam.pixelRect.x, cam.pixelHeight / 2 + cam.pixelRect.y, 0);
        //        anchor.transform.position = transform.position;
        //    }
        //    else
        //    {
        //        var vec = Input.mousePosition - new Vector3(cam.pixelWidth / 2 + cam.pixelRect.x, cam.pixelHeight / 2 + cam.pixelRect.y, 0);
        //        vec.x *= -1;

        //        transform.localPosition = vec;
        //        anchor.transform.position = transform.position;
        //        //transform.position = cam.ScreenToWorldPoint(Input.mousePosition) + cam.transform.forward * MeshCreator.textOffset;
        //        //anchor.transform.position = transform.position;
        //    }
        //}
    }

    public void SetDraggingOffset()
    {
        draggingOffset = cam.WorldToScreenPoint(anchor.transform.position) - Input.mousePosition;
    }

    public void SetOffsetPos(Vector3 mousePos, bool updateAnchor = false)
    {
        offset = cam.ScreenToWorldPoint(mousePos);

        //Anchor pos
        if(updateAnchor)
        {
            anchor.transform.position = cam.ScreenToWorldPoint(mousePos + draggingOffset);
        }

        if (!isWeld)
            UpdateArrowObject(cam, MessageQueueTest.GetLineSize(), portType);
        else
        {
            UpdateWeldObject(cam, MessageQueueTest.GetLineSize(), portType, weldType);
        }
    }

    public void UpdateAnchor()
    {
        var pos = RectTransformUtility.WorldToScreenPoint(cam, anchor.transform.position) - new Vector2(cam.pixelRect.x, cam.pixelRect.y);
        rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    void OnDestroy()
    {
        if(anchor != null)
        {
            Destroy(anchor);
        }

        if(arrowObject != null)
        {
            Destroy(arrowObject);
        }

        if(arrowAnchor != null)
        {
            Destroy(arrowAnchor.gameObject);
        }
    }

    public bool GetViewSide(CameraViewportType portType)
    {
        bool leftSide = false;

        if (portType == CameraViewportType.FRONT)
            leftSide = anchor.transform.position.x < 0 ? true : false;

        if (portType == CameraViewportType.LEFT)
            leftSide = anchor.transform.position.z > 0 ? true : false;

        if (portType == CameraViewportType.RIGHT)
            leftSide = anchor.transform.position.z < 0 ? true : false;

        if (portType == CameraViewportType.TOP)
            leftSide = anchor.transform.position.x < 0 ? true : false;

        return leftSide;
    }

    //TODO: This creates a lot of junk
    public void UpdateArrowObject(Camera cam, float thickness, CameraViewportType portType, bool isPrinting = false)
    {
        UpdateAnchor();
        LineColor = isPrinting ? Color.black : MessageQueueTest.GetDimensionColor();

        MeshCreator.Reset();

        bool leftSide = GetViewSide(portType);

        var strings = GetComponent<Text>().text.Split('\n');

        var textLength = 0.0f;

        if (labelText != null)
            textLength = LayoutUtility.GetPreferredWidth(labelText.rectTransform);
        else
        {
            Debug.Log("Label Text reference missing!");
        }

        if (!noArrow)
            MeshCreator.DrawArrowLineWorld(cam, offset, anchor.transform.position, thickness, textLength, leftSide, textVerticalOffset * strings.Length);
        else
            MeshCreator.DrawLineWorld(cam, offset, anchor.transform.position, thickness, textLength, leftSide, textVerticalOffset * strings.Length);

        if(useAddLeader)
        {
            MeshCreator.DrawArrowWorldSpace(cam, offset2, anchor.transform.position, thickness);
        }

        if (arrowObject != null)
        {
            MeshCreator.Create(arrowObject.GetComponent<MeshFilter>().sharedMesh);
        }
        else
            Debug.Log("Arrow is Null");
    }

    public void UpdateWeldObject(Camera cam, float thickness, CameraViewportType portType, int weldType = 0, bool isPrinting = false)
    {
        UpdateAnchor();
        LineColor = isPrinting ? Color.black : MessageQueueTest.GetWeldColor();

        MeshCreator.Reset();

        bool leftSide = false;

        if (portType == CameraViewportType.FRONT)
            leftSide = anchor.transform.position.x < 0 ? true : false;

        if (portType == CameraViewportType.LEFT)
            leftSide = anchor.transform.position.z > 0 ? true : false;

        if (portType == CameraViewportType.RIGHT)
            leftSide = anchor.transform.position.z < 0 ? true : false;

        if (portType == CameraViewportType.TOP)
            leftSide = anchor.transform.position.x < 0 ? true : false;

        var strings = GetComponent<Text>().text.Split('\n');

        var strlen = strings[strings.Length - 1].Length;

        MeshCreator.DrawWeldLineWorld(cam, offset, anchor.transform.position, thickness, strlen, leftSide, textVerticalOffset * strings.Length, weldType);

        if (useAddLeader)
        {
            MeshCreator.DrawWeldArrowLineWorld(cam, offset2, anchor.transform.position, thickness, strlen, leftSide, textVerticalOffset * strings.Length, weldType);
        }

        if(additionalTextObj != null)
        {
            if(leftSide)
            {
                (additionalTextObj.transform as RectTransform).anchoredPosition = leftTextPosition;
            }
            else
            {
                (additionalTextObj.transform as RectTransform).anchoredPosition = rightTextPosition;
            }
        }

        if (arrowObject != null)
        {
            MeshCreator.Create(arrowObject.GetComponent<MeshFilter>().sharedMesh);
        }
        else
            Debug.Log("Arrow is Null");
    }

    public void OnPointerDown(PointerEventData data)
    {
        if(data.button == PointerEventData.InputButton.Left)
            isDragging = true;
    }

    public void OnPointerUp(PointerEventData data)
    {
        if (data.button == PointerEventData.InputButton.Left)
            isDragging = false;
    }
}
