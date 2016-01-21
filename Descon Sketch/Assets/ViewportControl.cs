//using UnityEngine;
//using System.Collections;
//using UnityEngine.UI;
//using System.Collections.Generic;
//using Descon.Data;

//public enum CameraViewportType{D3, TOP, LEFT, RIGHT, BOTTOM, FRONT}

///// <summary>
///// Class that controls the placement and resizing of camera views within Unity.
///// </summary>
//public class ViewportControl : MonoBehaviour
//{
//    public Texture2D borderTexture;
//    public float borderWidth = 2.0f;
//    public Color selectedColor;
//    public Color defaultColor;
//    public Camera viewCamera;
//    public CameraViewportType portType;
//    public bool selected;
//    public Color defaultTextColor = Color.white;
//    [HideInInspector]
//    public Color textColor = Color.white;

//    public CameraControl cameraControl;
//    public bool isEnabled = true;
//    public GameObject panel;
//    public Text textPrefab;
//    public string textLayerName;
//    public bool is3DView;

//    //Text objects
//    public Dictionary<string, Dictionary<string, LabelTextObject>> drawLabels = new Dictionary<string, Dictionary<string, LabelTextObject>>();
//    public Dictionary<string, Dictionary<string, DimensionCallout>> dimensions = new Dictionary<string, Dictionary<string, DimensionCallout>>();

//    public GameObject labelPrefab;
//    public bool changed = true;
//    public GameObject additionalTextPrefab;
//    public bool isPrinting = false;

//    // Use this for initialization
//    void Start ()
//    {
//        textColor = defaultTextColor;
//    }

//    void Update()
//    {
//        viewCamera.enabled = isEnabled;

//        if(isEnabled)
//        {
//            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
//            {
//                if (viewCamera.pixelRect.Contains(Input.mousePosition))
//                {
//                    selected = true;
//                }
//                else
//                {
//                    selected = false;
//                }
//            }

//            if (selected)
//            {
//                //Do manipulation
//                if (cameraControl != null)
//                    cameraControl.ControlUpdate();

//                if(cameraControl.changed)
//                {
//                    changed = true;
//                    cameraControl.changed = false;
//                }
//            }
//        }
//    }

//    void LateUpdate()
//    {
//        foreach (var dict in drawLabels)
//        {
//            foreach (var item in dict.Value)
//            {
//                //var pos = viewCamera.WorldToScreenPoint(item.dragComponent.anchor.transform.position) - new Vector3(viewCamera.pixelWidth / 2 + viewCamera.pixelRect.x, viewCamera.pixelHeight / 2 + viewCamera.pixelRect.y, 0);
//                //pos.z = MeshCreator.textOffset;
//                //item.transform.localPosition = pos;
//                item.Value.dragComponent.UpdateAnchor();
//            }
//        }

//        if (changed)
//        {
//            foreach (var dict in dimensions)
//            {
//                foreach (var item in dict.Value)
//                {
//                    item.Value.CreateLines();
//                }
//            }
//        }
//    }

//    public void UpdateLabels()
//    {
//        foreach (var dict in drawLabels)
//        {
//            foreach (var item in dict.Value)
//            {
//                item.Value.dragComponent.UpdateAnchor();
//            }
//        }

//        foreach (var dict in dimensions)
//        {
//            foreach (var item in dict.Value)
//            {
//                item.Value.UpdateAnchor();
//                item.Value.CreateLines();
//            }
//        }

//    }

//    public void ZoomFit()
//    {
//        cameraControl.Reset();
//    }
	
//    // Update is called once per frame
//    void OnGUI()
//    {
//        if (isEnabled)
//            DrawRectangle();
//    }

//    void DrawRectangle()
//    {
//        //Draw a rectangle around the viewport
//        Rect temp = GetPixelRect();

//        GUI.color = selected == true ? selectedColor : defaultColor;
//        //Top
//        GUI.DrawTexture(new Rect(temp.x, temp.y, temp.width, borderWidth), borderTexture);
//        //Bottom
//        GUI.DrawTexture(new Rect(temp.x, temp.y + temp.height - borderWidth, temp.width, borderWidth), borderTexture);
//        //Left
//        GUI.DrawTexture(new Rect(temp.x, temp.y, borderWidth, temp.height), borderTexture);
//        //Right
//        GUI.DrawTexture(new Rect(temp.x + temp.width - borderWidth, temp.y, borderWidth, temp.height), borderTexture);

//        //Draw label
//        GUI.Label(new Rect(temp.x + 10, temp.y + 10, 200, 30), GetViewportText());
//    }

//    public void DestroyLabels()
//    {
//        foreach (var dict in drawLabels)
//        {
//            foreach (var item in dict.Value)
//            {
//                //item.drawComponent.ClearMesh();
//                Destroy(item.Value.gameObject);
//            }

//            dict.Value.Clear();
//        }

//        foreach (var dict in dimensions)
//        {
//            foreach (var item in dict.Value)
//            {
//                //item.drawComponent.ClearMesh();
//                item.Value.Clean();
//                Destroy(item.Value.gameObject);
//            }

//            dict.Value.Clear();
//        }
//    }

//    public void DestroyLabelsOfType(string keyName)
//    {
//        foreach (var dict in drawLabels)
//        {
//            if (dict.Key.Contains(keyName))
//            {
//                foreach (var item in dict.Value)
//                {
//                    //item.drawComponent.ClearMesh();
//                    Destroy(item.Value.gameObject);
//                }

//                dict.Value.Clear();
//            }
//        }

//        foreach (var dict in dimensions)
//        {
//            if (dict.Key.Contains(keyName))
//            {
//                foreach (var item in dict.Value)
//                {
//                    //item.drawComponent.ClearMesh();
//                    item.Value.Clean();
//                    Destroy(item.Value.gameObject);
//                }

//                dict.Value.Clear();
//            }
//        }
//    }

//    public void DestroyLabelsSelective(string keyName)
//    {
//        var side = keyName.Split(' ');

//        foreach (var dict in drawLabels)
//        {
//            if (dict.Key.Contains(side[0]))
//            {
//                if (!dict.Key.Contains(side[1]))
//                {
//                    foreach (var item in dict.Value)
//                    {
//                        //item.drawComponent.ClearMesh();
//                        Destroy(item.Value.gameObject);
//                    }

//                    dict.Value.Clear();
//                }
//            }
//        }

//        foreach (var dict in dimensions)
//        {
//            if (dict.Key.Contains(side[0]))
//            {
//                if (!dict.Key.Contains(side[1]))
//                {
//                    foreach (var item in dict.Value)
//                    {
//                        //item.drawComponent.ClearMesh();
//                        item.Value.Clean();
//                        Destroy(item.Value.gameObject);
//                    }

//                    dict.Value.Clear();
//                }
//            }
//        }
//    }

//    public void AddLabel(string connectionType, string keyName, string name, Vector3 offset, Vector3 posOffset, bool setPos = false)
//    {
//        if(!drawLabels.ContainsKey(connectionType))
//        {
//            drawLabels.Add(connectionType, new Dictionary<string, LabelTextObject>());
//        }

//        if(!drawLabels[connectionType].ContainsKey(keyName))
//        {
//            //Add a label object at the point
//            var label = ((GameObject)Instantiate(labelPrefab, posOffset, Quaternion.identity)).GetComponent<LabelTextObject>();
//            label.transform.parent = panel.transform;

//            if (setPos == false)
//            {
//                label.dragComponent.anchor.transform.position = offset + posOffset;
//            }
//            else
//            {
//                label.dragComponent.anchor.transform.position = posOffset;
//            }

//            label.transform.localRotation = Quaternion.identity;
//            label.transform.localScale = Vector3.one;
//            label.dragComponent.InitCamera(viewCamera);
//            label.dragComponent.panel = panel;
//            label.dragComponent.offset = offset;
//            label.dragComponent.viewControl = this;

//            label.TextColor = MessageQueueTest.instance.textColor;
//            label.ShapeName = name;

//            drawLabels[connectionType].Add(keyName, label);
//        }
//        else
//        {
//            //Change the values of that label
//            var label = drawLabels[connectionType][keyName];

//            label.ShapeName = name;
//            label.dragComponent.offset = offset;
//        }
//    }

//    public void AddWeldLabel(string connectionType, string keyName, string name, Vector3 offset, Vector3 posOffset, int weldType = 0, bool useAdditionalText = false, string moreText = "", float leftOffset = 0, float rightOffset = 0, bool setPos = false)
//    {
//        if (!drawLabels.ContainsKey(connectionType))
//        {
//            drawLabels.Add(connectionType, new Dictionary<string, LabelTextObject>());
//        }

//        if (!drawLabels[connectionType].ContainsKey(keyName))
//        {
//            //Add a label object at the point
//            var label = ((GameObject)Instantiate(labelPrefab, posOffset, Quaternion.identity)).GetComponent<LabelTextObject>();
//            label.transform.parent = panel.transform;

//            if (setPos == false)
//                label.dragComponent.anchor.transform.position = offset + posOffset;
//            else
//                label.dragComponent.anchor.transform.position = posOffset;

//            label.transform.localRotation = Quaternion.identity;
//            label.transform.localScale = Vector3.one;
//            label.dragComponent.InitCamera(viewCamera);
//            label.dragComponent.panel = panel;
//            label.dragComponent.offset = offset;
//            label.dragComponent.viewControl = this;

//            label.dragComponent.isWeld = true;
//            label.dragComponent.rectTransform.pivot = new Vector2(0.5f, 0.5f);
//            label.dragComponent.weldType = weldType;
//            label.dragComponent.arrowObject.renderer.material.color = Color.red;

//            label.TextColor = MessageQueueTest.instance.textColor;
//            label.ShapeName = name;

//            if (leftOffset != 0)
//                label.dragComponent.leftTextPosition = new Vector3(leftOffset, 0, 0);

//            if (rightOffset != 0)
//                label.dragComponent.rightTextPosition = new Vector3(rightOffset, 0, 0);

//            if (useAdditionalText)
//            {
//                var addedText = (GameObject)Instantiate(additionalTextPrefab);
//                addedText.transform.parent = label.transform;
//                addedText.transform.localRotation = Quaternion.identity;
//                addedText.transform.localScale = Vector3.one;
//                //addedText.transform.localPosition = additionalTextPrefab.transform.position; fking unity, this line does not work on builds
//                (addedText.transform as RectTransform).anchoredPosition = additionalTextPrefab.transform.position;

//                label.dragComponent.additionalText = addedText;

//                if (moreText.Length > 0)
//                {
//                    addedText.GetComponent<Text>().text = moreText;
//                }
//            }

//            drawLabels[connectionType].Add(keyName, label);
//        }
//        else
//        {
//            var label = drawLabels[connectionType][keyName];

//            label.ShapeName = name;

//            if (useAdditionalText)
//            {
//                if (label.dragComponent.additionalText != null)
//                {
//                    if (moreText.Length > 0)
//                    {
//                        label.dragComponent.additionalText.GetComponent<Text>().text = moreText;
//                    }
//                }
//                else
//                    Debug.Log("Additional Text is NULL!");
//            }
//        }
//    }

//    public void AddDimension(string connectionType, string keyname, string text, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col, float aLength = 0.0f, float dLength = 0.0f)
//    {
//        if (!dimensions.ContainsKey(connectionType))
//        {
//            dimensions.Add(connectionType, new Dictionary<string, DimensionCallout>());
//        }

//        if (!dimensions[connectionType].ContainsKey(keyname))
//        {
//            dimensions[connectionType].Add(keyname, MessageQueueTest.CreateDimension(viewCamera, text, a, b, c, d, col, 0, 0, portType == CameraViewportType.LEFT));
//        }
//        else
//        {
//            //Edit the existing dimension
//            var callout = dimensions[connectionType][keyname];

//            //callout.viewCamera = viewCamera;
//            //callout.isLeftView = isLeftView;
//            callout.Text = text;
//            callout.viewCamera = viewCamera;
//            callout.isLeftView = portType == CameraViewportType.LEFT;
//            callout.Resize(a, b, c, d);
//            callout.SetLineColor(col);
//            callout.aLength = aLength;
//            callout.dLength = dLength;
//        }
//    }

//    public void AddDimension(string connectionType, string keyname, string text, Vector3 a, Vector3 d, Vector3 forward, float armLength, Color col, float aLength = 0.0f, float dLength = 0.0f)
//    {
//        Vector3 direction = (d - a).normalized;

//        //Calculate positions b and c based on the arm length
//        Vector3 b = a + Vector3.Cross(direction, forward) * armLength;
//        Vector3 c = d + Vector3.Cross(direction, forward) * armLength;

//        //      A ----------B
//        //                  |
//        //  P               |
//        //  A               Text
//        //  R               |
//        //  T               |
//        //      D ----------C

//        if (!dimensions.ContainsKey(connectionType))
//        {
//            dimensions.Add(connectionType, new Dictionary<string, DimensionCallout>());
//        }

//        if (!dimensions[connectionType].ContainsKey(keyname))
//        {
//            dimensions[connectionType].Add(keyname, MessageQueueTest.CreateDimension(viewCamera, text, a, b, c, d, col, aLength, dLength, portType == CameraViewportType.LEFT));
//        }
//        else
//        {
//            //Edit the existing dimension
//            var callout = dimensions[connectionType][keyname];

//            //callout.viewCamera = viewCamera;
//            //callout.isLeftView = isLeftView;
//            callout.Text = text;
//            callout.viewCamera = viewCamera;
//            callout.isLeftView = portType == CameraViewportType.LEFT;
//            callout.Resize(a, b, c, d);
//            callout.SetLineColor(col);
//            callout.aLength = aLength;
//            callout.dLength = dLength;
//        }
//    }

//    public void AddDimension(string connectionType, string keyname, string text, Vector3 a, Vector3 d, float armLength, Color col)
//    {
//        AddDimension(connectionType, keyname, text, a, d, viewCamera.transform.forward, armLength, col);
//    }

//    public void AddDimension(string connectionType, string keyname, string text, Vector3 a, Vector3 d, float armLength, Color col, float aLength, float dlength)
//    {
//        AddDimension(connectionType, keyname, text, a, d, viewCamera.transform.forward, armLength, col, aLength, dlength);
//    }

//    public void UpdateLineColors()
//    {
//        if (MessageQueueTest.instance != null)
//        {
//            foreach (var item in MessageQueueTest.instance.memberDictionary)
//            {
//                if (item.Value != null && item.Value.gameObject.activeSelf == true)
//                {
//                    var drawable = item.Value.drawables[GetViewMask()];

//                    if (drawable != null)
//                    {
//                        drawable.LineColor = isPrinting ? Color.black : item.Value.gameObject.GetComponent<MeshFilter>().renderer.material.color;
//                    }

//                    foreach (var item2 in item.Value.connectionObjects)
//                    {
//                        var drawable2 = item2.drawables[GetViewMask()];
//                        drawable2.LineColor = isPrinting ? Color.black : item2.gameObject.GetComponent<MeshFilter>().renderer.material.color;
//                    }
//                }
//            }
//        }
//    }

//    public void ConfigureLines()
//    {
//        if (MessageQueueTest.instance != null)
//        {
//            foreach(var item in MessageQueueTest.instance.memberDictionary)
//            {
//                if(item.Value != null && item.Value.gameObject.activeSelf == true)
//                {
//                    var drawable = item.Value.drawables[GetViewMask()];

//                    if (drawable != null)
//                    {
//                        drawable.lineObject.SetActive(item.Value.gameObject.activeSelf);
//                        var flag = item.Value.viewMask & GetViewMask();
//                        if (flag == GetViewMask())
//                        {
//                            //drawable.LineColor = item.Value.isSelected ? MessageQueueTest.instance.selectedLineColor : MessageQueueTest.instance.lineColor;
//                            drawable.LineColor = isPrinting ? Color.black : item.Value.gameObject.GetComponent<MeshFilter>().renderer.material.color;

//                            //Draw the lines TODO: Replace with custom drawing code for each type of mesh and for each view
//                            if (!item.Value.useCustomLines)
//                            {
//                                if (item.Value.gameObject.GetComponent<MeshFilter>().sharedMesh != null)
//                                    MeshCreator.DrawLineObject(drawable.LineMesh, item.Value.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices, item.Value.gameObject.transform.position, item.Value.gameObject.transform.rotation, camera, MessageQueueTest.instance.GetLineSize(), 0.01f);
//                            }
//                            else
//                            {
//                                //Draw custom lines logic
//                                MeshCreator.DrawCustomLineObject(drawable.LineMesh, item.Value, item.Value.gameObject.transform.position, item.Value.gameObject.transform.rotation, camera, MessageQueueTest.instance.GetLineSize(), GetViewMask(), item.Value.customLineOffset);
//                            }

//                            //Draw all the connections
//                            foreach (var item2 in item.Value.connectionObjects)
//                            {
//                                var drawable2 = item2.drawables[GetViewMask()];
//                                drawable2.LineColor = isPrinting ? Color.black : item2.gameObject.GetComponent<MeshFilter>().renderer.material.color;
//                                var control = item2.gameObject.GetComponent<ConnectionControl>();

//                                if ((control.viewMask & GetViewMask()) == GetViewMask())
//                                {
//                                    MeshCreator.DrawLineObject(drawable2.LineMesh, item2.gameObject.GetComponent<MeshFilter>().sharedMesh.vertices, item2.gameObject.transform.position, item2.gameObject.transform.rotation, camera, MessageQueueTest.instance.GetLineSize(), 0.0f);
//                                }
//                            }
//                        }
//                    }
//                }
//                else if(item.Value != null)
//                {
//                    for (int i = 0; i < 5; ++i)
//                    {
//                        //Disable the line object so that lines do not persist after disablin
//                        foreach(var conn in item.Value.drawables)
//                        {
//                            conn.Value.lineObject.SetActive(item.Value.gameObject.activeSelf);
//                        }
//                    }
//                }
//            }
//        }

//        //Update the line to label objects
//        foreach (var dict in drawLabels)
//        {
//            foreach (var label in dict.Value)
//            {
//                if (!label.Value.dragComponent.isWeld)
//                    label.Value.dragComponent.UpdateArrowObject(viewCamera, MessageQueueTest.instance.GetLineSize(), portType);
//                else
//                {
//                    label.Value.dragComponent.UpdateWeldObject(viewCamera, MessageQueueTest.instance.GetLineSize(), portType, label.Value.dragComponent.weldType);
//                }
//            }
//        }
//    }

//    void OnPreCull()
//    {
//        UpdateLineColors();

//        if (changed)
//        {
//            cameraControl.ApplyZoom();
//            ConfigureLines();

//            changed = false;
//        }
//    }

//    public Rect GetPixelRect()
//    {
//        Rect temp = viewCamera.pixelRect;

//        temp.y = Screen.height - (temp.y + temp.height);

//        return temp;
//    }

//    public void SetCameraRect(Rect rect)
//    {
//        viewCamera.rect = rect;
//    }

//    public ViewMask GetViewMask()
//    {
//        if (portType == CameraViewportType.FRONT) return ViewMask.FRONT;
//        if (portType == CameraViewportType.LEFT) return ViewMask.LEFT;
//        if (portType == CameraViewportType.TOP) return ViewMask.TOP;
//        if (portType == CameraViewportType.D3) return ViewMask.D3;
//        if (portType == CameraViewportType.RIGHT) return ViewMask.RIGHT;

//        return ViewMask.FRONT;
//    }

//    string GetViewportText()
//    {
//        if (portType == CameraViewportType.FRONT) return "Front";
//        if (portType == CameraViewportType.LEFT) return "Left";
//        if (portType == CameraViewportType.TOP) return "Top";
//        if (portType == CameraViewportType.D3) return "3D";
//        if (portType == CameraViewportType.RIGHT) return "Right";

//        return "Default";
//    }
//}
