using UnityEngine;
using System.Collections;

//The dimension callout works as follows: It has world points that the lines are draw between.
public class DimensionCallout : MonoBehaviour
{
    //      A ----------B
    //                  |
    //  P               |
    //  A               Text
    //  R               |
    //  T               |
    //      D ----------C

    [HideInInspector] public GameObject pointA;
    [HideInInspector] public GameObject pointB;
    [HideInInspector] public GameObject pointC;
    [HideInInspector] public GameObject pointD;
    public float spaceSize = 10.0f;
    public Camera viewCamera;
    private GameObject lineObject;
    private GameObject textObject;
    private Vector2 dimensionSize;
    private Vector3 dimensionDirection;

    [HideInInspector] public float aLength;
    [HideInInspector] public float dLength;

    //Stuff to make it draggable
    public bool isDraggable = true;
    //The drag component attached to the text object
    public DimensionDraggable draggable;
    public bool isConstrained = true;
    public RectTransform rectTransform;
    public bool isLeftView = false;
    public CameraViewportType viewType;
    public bool isVerticalDimension = false;
    public Descon.Data.EOffsetType offsetType;

    void Awake()
    {
        offsetType = Descon.Data.EOffsetType.Top;
        isVerticalDimension = true;

        pointA = new GameObject();
        pointA.hideFlags = HideFlags.HideAndDontSave;

        pointB = new GameObject();
        pointB.hideFlags = HideFlags.HideAndDontSave;

        pointC = new GameObject();
        pointC.hideFlags = HideFlags.HideAndDontSave;

        pointD = new GameObject();
        pointD.hideFlags = HideFlags.HideAndDontSave;

        lineObject = MeshCreator.CreateLineObject("DimensionLineObject");
        dimensionSize = Vector2.zero;
    }

    private bool isVisible;
    public bool Visible
    {
        set
        {
            isVisible = value;

            if (lineObject != null)
                lineObject.SetActive(isVisible);

            textObject.GetComponent<UnityEngine.UI.Text>().enabled = isVisible;
        }

        get
        {
            return isVisible;
        }
    }

    public void Init(Camera viewCamera, bool isLeftView)
    {
        this.viewCamera = viewCamera;
        textObject = MessageQueueTest.CreateTextObject(viewCamera.GetComponent<ViewportControl>().panel, "", Vector3.zero);
        draggable = textObject.AddComponent<DimensionDraggable>();
        rectTransform = textObject.GetComponent<RectTransform>();
        this.isLeftView = isLeftView;
    }

    public Color Color
    {
        get
        {
            return lineObject.renderer.material.color;
        }

        set
        {
            lineObject.renderer.material.color = value;
        }
    }

    void LateUpdate()
    {
        if(draggable.isDragging)
        {
            if (!isConstrained)
            {
                var pos = Input.mousePosition - new Vector3(viewCamera.pixelWidth / 2 + viewCamera.pixelRect.x, viewCamera.pixelHeight / 2 + viewCamera.pixelRect.y, 0);

                pos = new Vector3(Mathf.Round(pos.x), Mathf.Round(pos.y), Mathf.Round(pos.z));

                draggable.transform.localPosition = pos;
                draggable.Anchor.transform.position = draggable.transform.position;

                var height = Vector3.Distance(pointA.transform.position, pointD.transform.position)/2.0f;
                var dir = GetDirectionVector();

                pointB.transform.position = draggable.Anchor.transform.position + dir * height;
                pointC.transform.position = draggable.Anchor.transform.position - dir * height;
            }
            else
            {
                //Constrain the movement to the axis
                var input = new Vector3(MouseManager.GetMouseAxisX(), MouseManager.GetMouseAxisY(), 0);

                input = viewCamera.ScreenToWorldPoint(input) - viewCamera.ScreenToWorldPoint(Vector3.zero);

                if (input.sqrMagnitude > 0)
                {
                    var project = Vector3.Project(input, dimensionDirection) * 10;

                    pointB.transform.position += project;
                    pointC.transform.position += project;

                    UpdateAnchor();
                }
            }
        }
        else
        {
            //draggable.transform.localPosition = viewCamera.WorldToScreenPoint(draggable.anchor.transform.position) - new Vector3(viewCamera.pixelWidth / 2 + viewCamera.pixelRect.x, viewCamera.pixelHeight / 2 + viewCamera.pixelRect.y, 0);
            var pos = RectTransformUtility.WorldToScreenPoint(viewCamera, draggable.Anchor.transform.position) - new Vector2(viewCamera.pixelRect.x, viewCamera.pixelRect.y);
            //rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));

            var textLength = (isVerticalDimension ? rectTransform.rect.height : rectTransform.rect.width);
            var otherTextLength = (!isVerticalDimension ? rectTransform.rect.height : rectTransform.rect.width);
            var dimensionDirectionCross = (pointA.transform.position - pointB.transform.position).normalized;//Vector3.Cross(pointD.transform.position - pointA.transform.position, viewCamera.transform.forward).normalized;
            var dimensionDir = (pointA.transform.position - pointD.transform.position).normalized;

            //Calculate the distance between the two points, if this distance is too small, move the text outward
            var offset = Vector3.zero;
            var textSize = 15;

            var dist = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(viewCamera, pointA.transform.position), RectTransformUtility.WorldToScreenPoint(viewCamera, pointD.transform.position));
            if (dist < textLength + textSize)
            {
                switch (offsetType)
                {
                    case Descon.Data.EOffsetType.Top:
                        offset = -(dimensionDirectionCross * otherTextLength * 0.5f + dimensionDirectionCross * 5);
                        break;

                    case Descon.Data.EOffsetType.Right:
                        offset = dimensionDir * textLength / 2 + dimensionDir * dist / 2 + dimensionDir * 15;
                        break;

                    case Descon.Data.EOffsetType.Bottom:
                        offset = -(dimensionDirectionCross * otherTextLength * 0.5f + dimensionDirectionCross * 5);
                        break;

                    case Descon.Data.EOffsetType.Left:
                        offset = -(dimensionDir * textLength / 2 + dimensionDir * dist / 2 + dimensionDir * 15);
                        break;
                }
            }

            //Move the text over based on the offset behaviour
            rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x + (viewType == CameraViewportType.LEFT || viewType == CameraViewportType.RIGHT ? offset.z : offset.x)), Mathf.Round(pos.y + (viewType == CameraViewportType.TOP ? offset.z : offset.y)));
        }
    }

    public void UpdateAnchor()
    {
        draggable.Anchor.transform.position = (pointB.transform.position + pointC.transform.position) / 2.0f;
        var pos = RectTransformUtility.WorldToScreenPoint(viewCamera, draggable.Anchor.transform.position) - new Vector2(viewCamera.pixelRect.x, viewCamera.pixelRect.y);
        rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x), Mathf.Round(pos.y));
    }

    Vector3 GetDirectionVector()
    {
        return (pointA.transform.position - pointD.transform.position).normalized;
    }

    Vector3 GetScreenDirection()
    {
        return (viewCamera.WorldToScreenPoint(pointA.transform.position) - viewCamera.WorldToScreenPoint(pointD.transform.position)).normalized;
    }

    public string Text
    {
        set
        {
            textObject.GetComponent<UnityEngine.UI.Text>().text = value;
        }

        get
        {
            return textObject.GetComponent<UnityEngine.UI.Text>().text;
        }
    }

    public Color TextColor
    {
        set
        {
            textObject.GetComponent<UnityEngine.UI.Text>().color = value;
        }

        get
        {
            return textObject.GetComponent<UnityEngine.UI.Text>().color;
        }
    }

    public int TextSize
    {
        set
        {
            textObject.GetComponent<UnityEngine.UI.Text>().fontSize = value;
        }

        get
        {
            return textObject.GetComponent<UnityEngine.UI.Text>().fontSize;
        }
    }

    public FontStyle TextStyle
    {
        set
        {
            textObject.GetComponent<UnityEngine.UI.Text>().fontStyle = value;
        }

        get
        {
            return textObject.GetComponent<UnityEngine.UI.Text>().fontStyle;
        }
    }

    public void Clean()
    {
        draggable.Clean();

        if (pointA != null) Destroy(pointA);
        if (pointB != null) Destroy(pointB);
        if (pointC != null) Destroy(pointC);
        if (pointD != null) Destroy(pointD);
        if (lineObject != null) Destroy(lineObject);
        if (textObject != null) Destroy(textObject);
    }

    public void SetPoints(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        pointA.transform.position = a;
        pointB.transform.position = b;
        pointC.transform.position = c;
        pointD.transform.position = d;

        var screenDirection = GetScreenDirection();

        isVerticalDimension = Mathf.Abs(screenDirection.y) < 1.0f ? false : true;

        dimensionDirection = (b - a).normalized;

        //if (isLeftView)
        //{
        //    dimensionDirection.z *= -1;
        //    //dimensionDirection.x *= -1;
        //}

        //if(dimensionDirection.x > 0.0f)
        //{
        //    dimensionDirection.x *= -1;
        //}

        //if (dimensionDirection.y < 0.0f)
        //{
        //    dimensionDirection.y *= -1;
        //}

        //if (viewType == CameraViewportType.TOP)
        //{
        //    if (dimensionDirection.z > 0.0f)
        //    {
        //        dimensionDirection.z *= -1;
        //    }
        //}

        //Calc the dimension size
        dimensionSize.x = Vector3.Distance(a, d);

        //Move the text
        draggable.Anchor.transform.position = (b + c) / 2.0f;
    }

    public void Resize(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        Vector3 direction = (pointB.transform.position - pointA.transform.position).normalized;

        var armLength = Vector3.Distance(pointA.transform.position, pointB.transform.position);

        //Calculate positions b and c based on the arm length
        b = a + direction * armLength;
        c = d + direction * armLength;

        SetPoints(a, b, c, d);
    }

    public void CreateLines()
    {
        //Draw the lines
        MeshCreator.Reset();

        var textLength = (isVerticalDimension ? rectTransform.rect.height : rectTransform.rect.width);
        var otherTextLength = (!isVerticalDimension ? rectTransform.rect.height : rectTransform.rect.width);
        var lineSize = MessageQueueTest.GetLineSize();
        var dimensionDirectionCross = (pointA.transform.position - pointB.transform.position).normalized;//Vector3.Cross(pointD.transform.position - pointA.transform.position, viewCamera.transform.forward).normalized;
        var dimensionDir = (pointA.transform.position - pointD.transform.position).normalized;

        //Move the anchor
        draggable.Anchor.transform.position = (pointB.transform.position + pointC.transform.position) / 2.0f;

        var guiDirection = (viewCamera.WorldToScreenPoint(pointB.transform.position) - viewCamera.WorldToScreenPoint(pointC.transform.position)).normalized;

        //Move the text
        var pos = RectTransformUtility.WorldToScreenPoint(viewCamera, draggable.Anchor.transform.position) - new Vector2(viewCamera.pixelRect.x, viewCamera.pixelRect.y);

        //Calculate the distance between the two points, if this distance is too small, move the text outward
        var offset = Vector3.zero;
        var textSize = 15;

        var dist = Vector2.Distance(RectTransformUtility.WorldToScreenPoint(viewCamera, pointA.transform.position), RectTransformUtility.WorldToScreenPoint(viewCamera, pointD.transform.position));
        if (dist < textLength + textSize)
        {
            switch (offsetType)
            {
                case Descon.Data.EOffsetType.Top:
                    offset = -(dimensionDirectionCross * otherTextLength * 0.5f + dimensionDirectionCross * 5);
                    break;

                case Descon.Data.EOffsetType.Right:
                    offset = dimensionDir * textLength / 2 + dimensionDir * dist / 2 + dimensionDir * 15;
                    break;

                case Descon.Data.EOffsetType.Bottom:
                    offset = -(dimensionDirectionCross * otherTextLength * 0.5f + dimensionDirectionCross * 5);
                    break;

                case Descon.Data.EOffsetType.Left:
                    offset = -(dimensionDir * textLength / 2 + dimensionDir * dist / 2 + dimensionDir * 15);
                    break;
            }

            //Distance is too small, draw arrows on outside
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointB.transform.position), viewCamera.WorldToScreenPoint(pointB.transform.position) + guiDirection * 15, lineSize);
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointC.transform.position), viewCamera.WorldToScreenPoint(pointC.transform.position) - guiDirection * 15, lineSize);
        }
        else
        {
            var arrowLength = Mathf.Max((dist - textLength - textSize) / 2, 10);

            //Draw the arrows towards the center
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointB.transform.position), viewCamera.WorldToScreenPoint(pointB.transform.position) - guiDirection * arrowLength, lineSize);
            MeshCreator.DrawArrow(viewCamera, viewCamera.WorldToScreenPoint(pointC.transform.position), viewCamera.WorldToScreenPoint(pointC.transform.position) + guiDirection * arrowLength, lineSize);
        }

        //Move the text over based on the offset behaviour
        rectTransform.anchoredPosition = new Vector2(Mathf.Round(pos.x + (viewType == CameraViewportType.LEFT || viewType == CameraViewportType.RIGHT ? offset.z : offset.x)), Mathf.Round(pos.y + (viewType == CameraViewportType.TOP ? offset.z : offset.y)));

        var zOffset = 0;// MeshCreator.GetZPlaneOffset();

        //Draw the lines that extend up towards the text
        MeshCreator.DrawEdgeWorldSpace(viewCamera, pointA.transform.position, pointB.transform.position, lineSize, zOffset);
        MeshCreator.DrawEdgeWorldSpace(viewCamera, pointD.transform.position, pointC.transform.position, lineSize, zOffset);

        if (aLength != 0.0f)
        {
            var dirLength = (pointA.transform.position - pointB.transform.position).normalized * aLength;
            MeshCreator.DrawEdgeWorldSpace(viewCamera, pointA.transform.position + dirLength, pointB.transform.position, MessageQueueTest.GetLineSize());
        }

        if (dLength != 0.0f)
        {
            var dirLength = (pointD.transform.position - pointC.transform.position).normalized * dLength;
            MeshCreator.DrawEdgeWorldSpace(viewCamera, pointC.transform.position, pointD.transform.position + dirLength, MessageQueueTest.GetLineSize());
        }

        MeshCreator.Create(lineObject.GetComponent<MeshFilter>().sharedMesh);

        var tpos = textObject.transform.localPosition;
        tpos.z = MeshCreator.textOffset;
        textObject.transform.localPosition = tpos;
    }
}
