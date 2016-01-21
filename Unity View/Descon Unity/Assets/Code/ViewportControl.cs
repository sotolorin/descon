using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using Descon.Data;

public enum CameraViewportType{D3, TOP, LEFT, RIGHT, BOTTOM, FRONT}

/// <summary>
/// Class that controls the placement and resizing of camera views within Unity.
/// </summary>
public class ViewportControl : MonoBehaviour
{
    public Texture2D borderTexture;
    public float borderWidth = 2.0f;
    public Camera viewCamera;
    public CameraViewportType portType;
    public bool selected;

    public CameraControl cameraControl;
    public bool isEnabled = true;
    public GameObject panel;
    public Text textPrefab;
    public string textLayerName;
    public bool is3DView;

    //Text objects and dimensions
    public Dictionary<string, LabelTextObject> drawLabelDict = new Dictionary<string, LabelTextObject>();
    public Dictionary<string, DimensionCallout> drawDimensionDict = new Dictionary<string, DimensionCallout>();

    public GameObject labelPrefab;
    public bool changed = true;
    public GameObject additionalTextPrefab;
    public bool isPrinting = false;
    public bool printInColor = true;
    public BorderSizer border;

    void Update()
    {
        viewCamera.enabled = isEnabled;

        if(isEnabled)
        {
            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                if (viewCamera.pixelRect.Contains(Input.mousePosition))
                {
                    selected = true;
                }
                else
                {
                    selected = false;
                }
            }

            if (selected)
            {
                //Do manipulation
                if (cameraControl != null)
                    cameraControl.ControlUpdate();

                if(cameraControl.zoomChanged)
                {
                    changed = true;
                    cameraControl.zoomChanged = false;
                }
            }
        }
    }

    void LateUpdate()
    {
        foreach (var item in drawLabelDict)
        {
            item.Value.dragComponent.UpdateAnchor();

            if(item.Value.dragComponent.isDragging)
            {
                //Update the label object for that line
                if (!item.Value.dragComponent.isWeld)
                    item.Value.dragComponent.UpdateArrowObject(viewCamera, MessageQueueTest.GetLineSize(), portType, isPrinting);
                else
                {
                    item.Value.dragComponent.UpdateWeldObject(viewCamera, MessageQueueTest.GetLineSize(), portType, item.Value.dragComponent.weldType, isPrinting);
                }
            }
        }

        foreach (var item in drawDimensionDict)
        {
            if (item.Value.draggable.isDragging)
                item.Value.CreateLines();
        }
    }

    void OnPreCull()
    {
        if (changed)
        {
            cameraControl.ApplyZoom();
            UpdateLines();

            changed = false;
        }

        //Update colors
        if (isEnabled)
        {
            if (isPrinting)
            {
                border.BorderColor = Color.black;
                border.BorderWidth = 1;
            }
            else
            {
                border.BorderColor = selected == true ? MessageQueueTest.GetSelectedColor() : MessageQueueTest.GetBorderColor();
                border.BorderWidth = selected ? 2 : 1;
            }
        }
    }

    public void SetDrawLabelVisible(bool visible, bool isWeld = false)
    {
        foreach(var label in drawLabelDict)
        {
            if (label.Value.dragComponent.isWeld == isWeld)
                label.Value.Visible = visible;
        }
    }

    public void SetDrawDimensionVisible(bool visible)
    {
        foreach(var dim in drawDimensionDict)
        {
            dim.Value.Visible = visible;
        }
    }

    public void UpdateLabelsAndDimensions()
    {
        foreach (var item in drawLabelDict)
        {
            item.Value.dragComponent.UpdateAnchor();
        }

        foreach (var item in drawDimensionDict)
        {
            item.Value.UpdateAnchor();
            item.Value.CreateLines();
        }
    }

    public void UpdateTextColors()
    {
        var printBlack = !printInColor;
        if (!isPrinting) printBlack = false;

        foreach (var item in drawLabelDict)
        {
            item.Value.TextColor = printBlack ? Color.black : MessageQueueTest.instance.textColor;
        }

        foreach (var item in drawDimensionDict)
        {
            item.Value.TextColor = printBlack ? Color.black : MessageQueueTest.instance.textColor;
        }
    }

    public void SetSizeAndStyle(int textSize, FontStyle style)
    {
        foreach (var item in drawLabelDict)
        {
            item.Value.TextSize = textSize;
            item.Value.TextStyle = style;
        }

        foreach (var item in drawDimensionDict)
        {
            item.Value.TextSize = textSize;
            item.Value.TextStyle = style;
        }

        border.text.fontSize = isPrinting ? 16 : 14;
        border.text.fontStyle = style;
    }

    public void ZoomFit(bool reset = false)
    {
        if (cameraControl.hasMoved == false || reset)
        {
            //Change the camera's zoom based on the member shape
            float zoom = 0.0f;

            switch (portType)
            {
                case CameraViewportType.LEFT:
                case CameraViewportType.RIGHT:
                        zoom = (float)ConfigDrawMethods.GetPrimaryLength() + 14;
                        cameraControl.ResetPosition();
                    break;
                case CameraViewportType.FRONT:
                        zoom = (float)ConfigDrawMethods.GetPrimaryLength() + 14;
                        SetCameraCenter();

                    break;
                case CameraViewportType.TOP:
                        zoom = (float)ConfigDrawMethods.GetBeamLength() + 20;
                        SetCameraCenter();

                    break;
                case CameraViewportType.D3:
                        cameraControl.ResetPosition();
                        cameraControl.ResetRotation();
                    break;
            }

            cameraControl.SetZoom(zoom);
            cameraControl.hasMoved = false;
            changed = true;
        }
    }

    private void SetCameraCenter()
    {
        //Set the camera's position based on the active elements
        var offset = 0.0f;

        var rightComponent = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        var leftComponent = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

        if (leftComponent.IsActive ^ rightComponent.IsActive)
        {
            var beamLength = Mathf.Min(12, (float)MessageQueueTest.instance.prevPrimaryLength) * 2;
            var primaryWidth = (float)DrawingMethods.GetPrimaryWidth();

            if (rightComponent.IsActive)
            {
                offset = -primaryWidth + (primaryWidth * 2 + beamLength) / 2;
            }
            else
            {
                offset = primaryWidth - (primaryWidth * 2 + beamLength) / 2;
            }

            if (portType == CameraViewportType.FRONT)
                cameraControl.anchor.transform.position = new Vector3(offset, 0, -100);
            else if (portType == CameraViewportType.TOP)
            {
                cameraControl.anchor.transform.position = new Vector3(offset, 0, 0) + cameraControl.startPosition;
            }
        }
        else
        {
            cameraControl.ResetPosition();
        }
    }

    public void DestroyAllLabelsAndDimensions()
    {
        foreach (var item in drawLabelDict)
        {
            Destroy(item.Value.gameObject);
        }

        //Clear the labels
        drawLabelDict.Clear();

        foreach (var item in drawDimensionDict)
        {
            item.Value.Clean();
            Destroy(item.Value.gameObject);
        }

        //Clear the dimensions
        drawDimensionDict.Clear();
    }

    public void DestroyDrawLabelsAndDimensionsWithTags(string nameTags, string ignoreTags = "")
    {
        DestroyDrawDimensionsWithTags(nameTags, ignoreTags);
        DestroyDrawLabelsWithTags(nameTags, ignoreTags);
    }

    //Legacy function
    public void DestroyLabel(string type, string name)
    {
        DestroyDrawLabelsAndDimensionsWithTags(type + " " + name);
    }

    /// <summary>
    /// Destroys draw labels that contain the space delmimted tags in the key
    /// </summary>
    /// <param name="tags"></param>
    public void DestroyDrawLabelsWithTags(string nameTags, string ignoreTags = "")
    {
        var searchTags = nameTags.Split(' ');

        string[] searchIgnoreTags = null;

        if (ignoreTags.Length > 0)
        {
            searchIgnoreTags = ignoreTags.Split(' ');
        }

        var keysToRemove = new List<string>();

        foreach(var item in drawLabelDict)
        {
            bool ignore = false;

            if (ignoreTags.Length > 0)
            {
                foreach (var it in searchIgnoreTags)
                {
                    if (item.Key.Contains(it))
                    {
                        ignore = true;
                        break;
                    }
                }
            }

            if (!ignore)
            {
                bool wasFound = true;
                foreach (var t in searchTags)
                {
                    if (!item.Key.Contains(t))
                    {
                        wasFound = false;
                        break;
                    }
                }

                if (wasFound)
                {
                    Destroy(item.Value.gameObject);
                    keysToRemove.Add(item.Key);
                }
            }
        }

        foreach(var key in keysToRemove)
        {
            drawLabelDict.Remove(key);
        }
    }

    /// <summary>
    /// Destroys draw dimensions that contain the space delimited tags in the key
    /// </summary>
    /// <param name="tags"></param>
    public void DestroyDrawDimensionsWithTags(string nameTags, string ignoreTags = "")
    {
        var searchTags = nameTags.Split(' ');
        string[] searchIgnoreTags = null;

        if (ignoreTags.Length > 0)
        {
            searchIgnoreTags = ignoreTags.Split(' ');
        }

        var keysToRemove = new List<string>();

        foreach (var item in drawDimensionDict)
        {
            bool ignore = false;

            if (ignoreTags.Length > 0)
            {
                foreach (var it in searchIgnoreTags)
                {
                    if (item.Key.Contains(it))
                    {
                        ignore = true;
                        break;
                    }
                }
            }

            if (!ignore)
            {
                bool wasFound = true;
                foreach (var t in searchTags)
                {
                    if (!item.Key.Contains(t))
                    {
                        wasFound = false;
                        break;
                    }
                }

                if (wasFound)
                {
                    item.Value.Clean();
                    Destroy(item.Value.gameObject);
                    keysToRemove.Add(item.Key);
                }
            }
        }

        foreach (var key in keysToRemove)
        {
            drawDimensionDict.Remove(key);
        }
    }

    public void AddDrawLabel(string tagName, string text, Vector3 offset, Vector3 posOffset, bool setPos = false, Vector3? offset2 = null, bool hasNoArrow = false, bool resetArrow = false)
    {
        if (!drawLabelDict.ContainsKey(tagName))
        {
            //Add a label object at the point
            var label = ((GameObject)Instantiate(labelPrefab, posOffset, Quaternion.identity)).GetComponent<LabelTextObject>();
            label.transform.parent = panel.transform;

            //if (setPos == false)
            //{
                label.dragComponent.anchor.transform.position = offset + posOffset;
            //}
            //else
            //{
            //    label.dragComponent.anchor.transform.position = posOffset;
            //}

            label.transform.localRotation = Quaternion.identity;
            label.transform.localScale = Vector3.one;
            label.dragComponent.InitCamera(viewCamera);
            label.dragComponent.panel = panel;
            label.dragComponent.portType = portType;
            label.dragComponent.arrowAnchor.parent = panel.transform;
            label.dragComponent.offset = offset;
            label.dragComponent.viewControl = this;
            label.dragComponent.labelText = label.text;
            label.dragComponent.noArrow = hasNoArrow;

            label.TextColor = MessageQueueTest.instance.textColor;
            label.dragComponent.LineColor = MessageQueueTest.GetDimensionColor();
            label.ShapeName = text;

            if (offset2.HasValue && offset2.Value != Vector3.zero)
            {
                label.dragComponent.useAddLeader = true;
                label.dragComponent.offset2 = offset2.Value;
            }

            drawLabelDict.Add(tagName, label);
        }
        else
        {
            //Change the values of that label
            var label = drawLabelDict[tagName];

            if (label.ShapeName != text || resetArrow)
            {
                label.dragComponent.offset = offset;
            }

            if (setPos)
            {
                label.dragComponent.anchor.transform.position = offset + posOffset;
            }

            label.ShapeName = text;
        }
    }

    public void AddDrawWeldLabel(string tagName, string text, Vector3 offset, Vector3 posOffset, int weldType = 0, string additionalText = "", 
        float leftOffset = 0, float rightOffset = 0, bool setPos = false, Vector3? offset2 = null)
    {
        if (!drawLabelDict.ContainsKey(tagName))
        {
            //Add a label object at the point
            var label = ((GameObject)Instantiate(labelPrefab, posOffset, Quaternion.identity)).GetComponent<LabelTextObject>();
            label.transform.parent = panel.transform;

            if (setPos == false)
                label.dragComponent.anchor.transform.position = offset + posOffset;
            else
                label.dragComponent.anchor.transform.position = posOffset;

            label.transform.localRotation = Quaternion.identity;
            label.transform.localScale = Vector3.one;
            label.dragComponent.InitCamera(viewCamera);
            label.dragComponent.panel = panel;
            label.dragComponent.offset = offset;
            label.dragComponent.viewControl = this;
            label.dragComponent.portType = portType;
            label.dragComponent.arrowAnchor.parent = panel.transform;
            label.dragComponent.labelText = label.text;

            label.dragComponent.isWeld = true;
            label.dragComponent.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            label.dragComponent.weldType = weldType;
            label.dragComponent.arrowObject.renderer.material.color = Color.red;

            label.TextColor = MessageQueueTest.instance.textColor;
            label.ShapeName = text;

            if (offset2.HasValue && offset2.Value != Vector3.zero)
            {
                label.dragComponent.useAddLeader = true;
                label.dragComponent.offset2 = offset2.Value;
            }

            if (leftOffset != 0)
                label.dragComponent.leftTextPosition = new Vector3(leftOffset, 0, 0);

            if (rightOffset != 0)
                label.dragComponent.rightTextPosition = new Vector3(rightOffset, 0, 0);

            if (additionalText != null && additionalText.Length > 0)
            {
                var addedTextObj = (GameObject)Instantiate(additionalTextPrefab);
                addedTextObj.transform.parent = label.transform;
                addedTextObj.transform.localRotation = Quaternion.identity;
                addedTextObj.transform.localScale = Vector3.one;
                //addedText.transform.localPosition = additionalTextPrefab.transform.position; fking unity, <--- line does not work on builds >:(
                (addedTextObj.transform as RectTransform).anchoredPosition = additionalTextPrefab.transform.position;
                addedTextObj.GetComponent<Text>().text = additionalText;

                //Add the additional text reference
                label.dragComponent.additionalTextObj = addedTextObj;
            }

            drawLabelDict.Add(tagName, label);
        }
        else
        {
            var label = drawLabelDict[tagName];

            if (label.ShapeName != text || label.dragComponent.offset != offset)
            {
                //Move the offset
                label.dragComponent.offset = offset;
            }

            label.ShapeName = text;

            if (additionalText.Length > 0)
            {
                if (label.dragComponent.additionalTextObj != null)
                {
                    label.dragComponent.additionalTextObj.GetComponent<Text>().text = additionalText;
                }
                else
                    Debug.Log("Additional Text is NULL!");
            }
        }
    }

    public void AddDrawDimension(string tagName, string text, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col, float aLength = 0.0f, float dLength = 0.0f, Descon.Data.EOffsetType offsetBehave = Descon.Data.EOffsetType.Top)
    {
        if (!drawDimensionDict.ContainsKey(tagName))
        {
            drawDimensionDict.Add(tagName, MessageQueueTest.CreateDimension(viewCamera, text, a, b, c, d, col, 0, 0, portType == CameraViewportType.LEFT, offsetBehave));
        }
        else
        {
            //Edit the existing dimension
            var callout = drawDimensionDict[tagName];

            //callout.viewCamera = viewCamera;
            //callout.isLeftView = isLeftView;
            callout.Text = text;
            callout.viewCamera = viewCamera;
            callout.isLeftView = portType == CameraViewportType.LEFT;
            callout.Resize(a, b, c, d);
            callout.Color = col;
            callout.aLength = aLength;
            callout.dLength = dLength;
        }
    }

    public void AddDrawDimension(string tagName, string text, Vector3 a, Vector3 d, Vector3 forward, float armLength, Color col, float aLength = 0.0f, float dLength = 0.0f, Descon.Data.EOffsetType offsetBehave = Descon.Data.EOffsetType.Top)
    {
        Vector3 direction = (d - a).normalized;

        //Calculate positions b and c based on the arm length
        Vector3 b = a + Vector3.Cross(direction, forward) * armLength;
        Vector3 c = d + Vector3.Cross(direction, forward) * armLength;

        //      A ----------B
        //                  |
        //  P               |
        //  A               Text
        //  R               |
        //  T               |
        //      D ----------C

        if (!drawDimensionDict.ContainsKey(tagName))
        {
            drawDimensionDict.Add(tagName, MessageQueueTest.CreateDimension(viewCamera, text, a, b, c, d, col, aLength, dLength, portType == CameraViewportType.LEFT, offsetBehave));
        }
        else
        {
            //Edit the existing dimension
            var callout = drawDimensionDict[tagName];

            //callout.viewCamera = viewCamera;
            //callout.isLeftView = isLeftView;
            callout.Text = text;
            callout.viewCamera = viewCamera;
            callout.isLeftView = portType == CameraViewportType.LEFT;
            callout.Resize(a, b, c, d);
            callout.Color = col;
            callout.aLength = aLength;
            callout.dLength = dLength;
        }
    }

    public void AddDrawDimension(string tagName, string text, Vector3 a, Vector3 d, float armLength, Color col)
    {
        AddDrawDimension(tagName, text, a, d, viewCamera.transform.forward, armLength, col);
    }

    public void AddDrawDimension(string tagName, string text, Vector3 a, Vector3 d, float armLength, Color col, float aLength, float dlength, Descon.Data.EOffsetType offsetBehave = Descon.Data.EOffsetType.Top)
    {
        AddDrawDimension(tagName, text, a, d, viewCamera.transform.forward, armLength, col, aLength, dlength, offsetBehave);
    }

    public void UpdateDimensionAnchors()
    {
        foreach (var dim in drawDimensionDict)
        {
            if (dim.Value != null)
            {
                dim.Value.UpdateAnchor();
            }
        }
    }

    public void UpdateLines()
    {
        var printBlack = !printInColor;
        if (!isPrinting) printBlack = false;

        //Update the member color
        foreach(var item in MessageQueueTest.instance.memberDictionary)
        {
            item.Value.shapeControl.Color = printBlack ? Color.grey : item.Value.memberColor;

            item.Value.lineDrawing.SetLineColor(GetViewMask(), printBlack ? Color.black : item.Value.memberColor);
            item.Value.lineDrawing.UpdateLineMesh(viewCamera, GetViewMask());

            var conections = item.Value.GetConnectionMembers();

            foreach (var conn in conections)
            {
                conn.shapeControl.Color = printBlack ? Color.grey : conn.memberColor;
                conn.lineDrawing.UpdateLineMesh(viewCamera, GetViewMask());
                conn.lineDrawing.SetLineColor(GetViewMask(), printBlack ? Color.black : conn.memberColor);
            }
        }

        //Update the line to label objects
        foreach (var label in drawLabelDict)
        {
            if (label.Value != null)
            {
                label.Value.TextColor = printBlack ? Color.black : MessageQueueTest.GetTextColor();

                if (!label.Value.dragComponent.isWeld)
                    label.Value.dragComponent.UpdateArrowObject(viewCamera, MessageQueueTest.GetLineSize(), portType, printBlack);
                else
                {
                    label.Value.dragComponent.UpdateWeldObject(viewCamera, MessageQueueTest.GetLineSize(), portType, label.Value.dragComponent.weldType, printBlack);
                }
            }
        }

        //Update the dimensions
        foreach (var dim in drawDimensionDict)
        {
            if (dim.Value != null)
            {
                dim.Value.TextColor = printBlack ? Color.black : MessageQueueTest.GetTextColor();

                dim.Value.Color = (printBlack ? Color.black : MessageQueueTest.GetDimensionColor());

                dim.Value.CreateLines();
            }
        }
    }

    public Rect GetPixelRect()
    {
        Rect temp = viewCamera.pixelRect;

        temp.y = Screen.height - (temp.y + temp.height);

        return temp;
    }

    public void SetCameraRect(Rect rect)
    {
        viewCamera.rect = rect;
    }

    public ViewMask GetViewMask()
    {
        if (portType == CameraViewportType.FRONT) return ViewMask.FRONT;
        if (portType == CameraViewportType.LEFT) return ViewMask.LEFT;
        if (portType == CameraViewportType.TOP) return ViewMask.TOP;
        if (portType == CameraViewportType.D3) return ViewMask.D3;
        if (portType == CameraViewportType.RIGHT) return ViewMask.RIGHT;

        return ViewMask.FRONT;
    }

    string GetViewportText()
    {
        if (portType == CameraViewportType.FRONT) return "Front";
        if (portType == CameraViewportType.LEFT) return "Left";
        if (portType == CameraViewportType.TOP) return "Top";
        if (portType == CameraViewportType.D3) return "3D";
        if (portType == CameraViewportType.RIGHT) return "Right";

        return "Default";
    }
}
