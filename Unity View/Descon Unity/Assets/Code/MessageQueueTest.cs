using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Threading;
using System.Net.Sockets;
using Descon.Data;
using System.IO.Pipes;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine.UI;
using UnityEngine;
using System;

public class MessageQueueTest : MonoBehaviour
{
    public static MessageQueueTest instance;
    public static bool saveFileChanged = false;
    public static bool prefFileChanged = false;
    public static bool saveDimensions = false;
    public static bool loadDimensions = false;
    public static bool newDrawing = false;
    public static bool newMemberAdded = false;
    private Thread thread;
    public static int dataSizeReceived = 0;
    private NamedPipeClientStream client;
    private static string receivedString = "";
    private SaveFileStructure saveFile;
    private string dimensionVersion = "v11";

    public List<ViewportControl> views;

    public GameObject memberPrefab;
    public Dictionary<string, MemberControl2> memberDictionary;

    public Color textColor = Color.white;
    private Color dimensionColor = Color.green;
    private Color weldColor = Color.red;
    private Color connectionColor = new Color(1, 0, 1);
    private Color boltColor = new Color(1, 1, 0);
    private Color selectionColor = new Color(1, 1, 0);

    private float lineSize = 2;//0.00157f;
    private int lineSizeSelection = 1;

    private int lineSizeSelectionMin = 1;
    private int lineSizeSelectionMax = 2;
    private static bool zoomFitUpdate;
    private static bool zoomFitSelectedUpdate;

    private static bool printing;
    private RenderTexture renderTexture;

    public GameObject simpleLabel;
    public float fontSize = 10.0f;

    public bool debugUpdate;
    public bool loadDebugDrawing;
    public string debugDrawingPath;
    public DimensionCallout calloutPrefab;
    public int currentNumClicks;
    public float clickTime = 0.5f;
    private float currentClickTime;
    private Vector3 prevMousePosition;
    public float checkProcessTime = 3.0f;
    private float currentProcessTime = 0.0f;
    public GameObject mainCanvas;
    public Camera mainCamera;

    public List<GameObject> panels;

    public bool debugMenuActivated = false;
    public Canvas debugMenu;
    public Text mousePosText;
    public Text mouseAxisText;
    public Text mouseRawAxisText;
    public Text mouseManagerAxisText;
    private Color borderColor;
    public float topCutPoint = 0.0f;
    public double prevPrimaryLength;

    public static float GetFontSize()
    {
        return instance.fontSize;
    }

    public static Color GetBorderColor()
    {
        return instance.borderColor;
    }

    public static Color GetDimensionColor()
    {
        return instance.dimensionColor;
    }

    public static Color GetWeldColor()
    {
        return instance.weldColor;
    }

    public static Color GetConnectionColor()
    {
        return instance.connectionColor;
    }

    public static Color GetBoltColor()
    {
        return instance.boltColor;
    }

    public static Color GetTextColor()
    {
        return instance.textColor;
    }

    public static Color GetSelectedColor()
    {
        return instance.selectionColor;
    }

    public static GameObject GetMainCanvas()
    {
        return instance.mainCanvas;
    }

    public static float GetLineSize()
    {
        return instance.lineSize * instance.lineSizeSelection;
    }

    void Awake()
    {
        instance = this;
        Application.targetFrameRate = 60;
    }

	// Use this for initialization
	void Start ()
    {
        memberDictionary = new Dictionary<string, MemberControl2>();

		var loadData = new LoadDataFromXML();
		CommonDataStatic.Preferences = loadData.LoadPreferencesForUnity();
        loadData.LoadShapes();

        saveFileChanged = false;
        prefFileChanged = true;
        zoomFitUpdate = false;
        printing = false;

        if (Application.isEditor)
        {
            saveFileChanged = true;
        }

        //Start the Thread
        thread = new Thread(ThreadUpdate);

        thread.Start();

        clickTime = GetDoubleClickTime();

        LoadColors();

        views[3].selected = true;
	}

    void OnDestroy()
    {
	    if (thread != null)
		    thread.Abort();
        if (client != null)
            client.Close();
    }

    public void ThreadUpdate()
    {
		client = new NamedPipeClientStream(".", ConstString.UNITY_PIPE_NAME_SEND, PipeDirection.InOut, PipeOptions.Asynchronous) {ReadMode = PipeTransmissionMode.Message};
	    client.Connect();

        const int BUFFSIZE = 100;

        while(true)
        {
            byte[] buffer = new byte[BUFFSIZE];
            int count = client.Read(buffer, 0, BUFFSIZE);

            if (count > 0 && count < BUFFSIZE)
            {
                receivedString = System.Text.Encoding.ASCII.GetString(buffer, 0, count);
	            // This will switch on the data passed over and later can do different things depending on the message
				switch (receivedString)
	            {
		            case ConstString.UNITY_UPDATE:
						saveFileChanged = true;
						break;
                    case ConstString.UNITY_USER_SAVED:
                        saveDimensions = true;
                        break;
                    //case ConstString.UNITY_NEW_FILE_LOADED:
                    //    loadDimensions = true;
                    //    fileLoaded = true;
                    //    break;
					case ConstString.UNITY_PREFERENCES_UPDATE:
                        prefFileChanged = true;
						break;
                    //case ConstString.UNITY_THICK_LINES:
                    //    // Use thick lines. If already thick, do nothing
                    //    lineSizeSelection = Mathf.Clamp(lineSizeSelection + 1, lineSizeSelectionMin, lineSizeSelectionMax);
                    //    saveFileChanged = true;
                    //    break;
                    //case ConstString.UNITY_THIN_LINES:
                    //    // Use thin lines. If already thin, do nothing
                    //    lineSizeSelection = Mathf.Clamp(lineSizeSelection - 1, lineSizeSelectionMin, lineSizeSelectionMax);
                    //    saveFileChanged = true;
                    //    break;
					case ConstString.UNITY_ZOOM_TO_FIT:
						// Zoom to fit all views
                        zoomFitUpdate = true;
						break;
                    case ConstString.UNITY_ZOOM_TO_FIT_SELECTED:
                        zoomFitSelectedUpdate = true;
                        break;
                    //case ConstString.UNITY_MESH_ON:
                    //    DrawingMethods.instance.MeshOn = true;
                    //    saveFileChanged = true;
                    //    break;
                    //case ConstString.UNITY_MESH_OFF:
                    //    DrawingMethods.instance.MeshOn = false;
                    //    saveFileChanged = true;
                    //    break;
					case ConstString.UNITY_CREATE_IMAGE:
                        printing = true;
						break;
                    case ConstString.UNITY_NEW_DRAWING:
                        newDrawing = true;
                        break;
                    case ConstString.UNITY_JOINT_CONFIG_CHANGE:
                        break;
                    case ConstString.UNITY_NEW_MEMBER_ADDED:
                        newMemberAdded = true;
                        break;
				}

                client.Flush();
            }
        }
    }

    [DllImport("user32.dll")]
    static extern uint GetDoubleClickTime();

    void Update()
    {
        if(debugUpdate)
        {
            saveFileChanged = true;
            prefFileChanged = true;
        }

        if (saveFileChanged)
        {
            if (newDrawing)
            {
                DestroyAllLabelsAndDimensions();
                newDrawing = false;

                //Reset zoom flags on cameras
                foreach (var view in views)
                {
                    view.cameraControl.hasMoved = false;
                }

                //Load the dimensions
                var sfile = new LoadDataFromXML().LoadDesconDrawing(ConstString.FILE_UNITY_DIMENSIONS, false);

                LoadDimensions(sfile.DimensionData);

                //Load the camera positions
                LoadCameras(sfile.CameraData);
            }

           //Load the save file structure
            if(!loadDebugDrawing)
                saveFile = new LoadDataFromXML().LoadDesconDrawing(ConstString.FILE_UNITY_DRAWING, false);
            else
                saveFile = new LoadDataFromXML().LoadDesconDrawing(debugDrawingPath, false);

            if (saveFile != null)
            {
                //Destroy the connection meshes to prevent leaks
                foreach(var member in memberDictionary)
                {
                    member.Value.DestroyConnectionMeshes();
                }

                //TODO: Do other configs
                switch(CommonDataStatic.BeamToColumnType)
                {
                    case EJointConfiguration.BraceToColumn:
                    case EJointConfiguration.BeamToColumnFlange:
                    case EJointConfiguration.BeamToHSSColumn:
                    case EJointConfiguration.BeamToColumnWeb:
                        ConfigDrawMethods.DrawBeamToColumn();
                        break;
                    case EJointConfiguration.BeamToGirder:
                    case EJointConfiguration.BeamSplice:
                        ConfigDrawMethods.DrawBeamToGirder();
                        break;
                    //case EJointConfiguration.BeamSplice: 
                        //ConfigDrawMethods.DrawBeamSplice();
                        //break;
                }

                //Set active state of gameObjects
                foreach (var detail in CommonDataStatic.DetailDataDict)//.Where(itm => itm.Value.IsActive))
                {
                    var memberObject = GetMemberControl(detail.Value.MemberType);

                    memberObject.SetVisible(detail.Value.IsActive);

                    //Set the shape types for each member
//                    memberObject.subType = detail.Value.ShapeType;
                }

                //Update the member color
                foreach (var detail in CommonDataStatic.DetailDataDict.Where(itm => itm.Value.IsActive))
                {
                    var memberObject = GetMemberControl(detail.Value.MemberType);// + GetKeyNamePart(detail.Value.GussetToColumnConnection));

                    if (detail.Key == EMemberType.PrimaryMember)
                    {
                        memberObject.memberColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.Columns);
                    }
                    else
                    {
                        memberObject.memberColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.BeamsBraces);
                    }
                }
            }

            for (int i = 0; i < views.Count; ++i)
            {
                if (!views[i].is3DView)
                    views[i].changed = true;
            }

            //Consume the change
            saveFileChanged = false;

            SendUnityData(ConstString.UNITY_UPDATE_DONE);
        }

        if (prefFileChanged)
        {
            LoadColors();

            //Change line size
            lineSizeSelection = CommonDataStatic.Preferences.ViewSettings.ThinLines ? lineSizeSelectionMin : lineSizeSelectionMax;

            var prevViewStates = new bool[5];
            for (int i = 0; i < 5; ++i)
            {
                prevViewStates[i] = views[i].isEnabled;
            }

            //Update the toggled views
            //Left
            views[0].isEnabled = CommonDataStatic.Preferences.ViewSettings.ShowLeft;
            //Top
            views[1].isEnabled = CommonDataStatic.Preferences.ViewSettings.ShowTop;
            //Right
            views[2].isEnabled = CommonDataStatic.Preferences.ViewSettings.ShowRight;
            //Front
            views[3].isEnabled = CommonDataStatic.Preferences.ViewSettings.ShowFront;
            //3d
            views[4].isEnabled = CommonDataStatic.Preferences.ViewSettings.Show3D;

            var changed = false;
            //Check if views changed
            for (int i = 0; i < 5; ++i)
            {
                if(prevViewStates[i] != views[i].isEnabled)
                {
                    //Changed
                    changed = true;
                    break;
                }
            }

            var changedSelected = false;

            if(changed)
            {
                for (int i = 0; i < 5; ++i)
                {
                    views[i].changed = true;

                    if(!changedSelected && views[i].isEnabled)
                    {
                        views[i].selected = true;
                        changedSelected = true;
                    }
                    else
                    {
                        views[i].selected = false;
                    }
                }
            }

            //Update the camera views
            UpdateCameraViews();

            //Update the canvas panels for drawing
            for (int i = 0; i < panels.Count; ++i)
            {
                panels[i].SetActive(views[i].isEnabled);

                if(views[i].isEnabled)
                {
                    views[i].UpdateLabelsAndDimensions();
                }
            }

            //Adjust callout visibility
            var visible = CommonDataStatic.Preferences.ViewSettings.Callouts;
            foreach(var view in views)
            {
                view.SetDrawLabelVisible(visible);
            }

            //Adjust dimension visibility
            visible = CommonDataStatic.Preferences.ViewSettings.Dimensions;
            foreach (var view in views)
            {
                view.SetDrawDimensionVisible(visible);
            }

            //Adjust weld visibility
            visible = CommonDataStatic.Preferences.ViewSettings.Welds;
            foreach (var view in views)
            {
                view.SetDrawLabelVisible(visible, true);
            }

            //Consume the change
            prefFileChanged = false;
        }

        if (saveDimensions)
        {
            SaveDimensions();

            saveDimensions = false;
        }

        if (loadDimensions)
        {
            var sfile = new LoadDataFromXML().LoadDesconDrawing(ConstString.FILE_UNITY_DIMENSIONS, false);

            DestroyAllLabelsAndDimensions();

            LoadDimensions(sfile.DimensionData);

            //Load the camera positions
            LoadCameras(sfile.CameraData);

            loadDimensions = false;
        }

        debugUpdate = false;

        if (zoomFitSelectedUpdate)
        {
            ZoomFitSelected(true);
            zoomFitSelectedUpdate = false;
        }

        if (zoomFitUpdate)
        {
            ZoomFitAll(true);

            zoomFitUpdate = false;
        }

        if (newMemberAdded)
        {
            //Zoom to fit all cameras that haven't moved
            ZoomFitAll(false);

            newMemberAdded = false;
        }

        //if (fileLoaded)
        //{
        //    fileLoaded = false;

        //    var sfile = new LoadDataFromXML().LoadDesconDrawing(ConstString.FILE_UNITY_DIMENSIONS);

        //    DestroyAllLabelsAndDimensions();

        //    LoadDimensions(sfile.DimensionData);

        //    ZoomFitAll();
        //}

        if(printing)
        {
            PrintDrawing();

            printing = false;
        }

        var doubleClicked = false;

        //Double click
        if(Input.GetMouseButtonUp(0))
        {
            if(currentNumClicks == 0)
            {
                currentNumClicks++;
                currentClickTime = clickTime;
                prevMousePosition = Input.mousePosition;
            }
            else
            {
                if (Vector3.Distance(Input.mousePosition, prevMousePosition) <= 3)
                {
                    //Double clicked
                    doubleClicked = true;
                    currentNumClicks = 0;
                    currentClickTime = 0;
                }
                else
                {
                    doubleClicked = false;
                    currentNumClicks = 0;
                    currentClickTime = 0;
                }
            }
        }

        if (currentNumClicks > 0)
        {
            currentClickTime -= Time.deltaTime;

            if(currentClickTime <= 0)
            {
                currentNumClicks = 0;
                currentClickTime = 0;
            }
        }

        
        //Update the selection of the mouse
        for (int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled)
            {
                //Determine if the mouse is in that quadrant
                if (views[i].viewCamera.pixelRect.Contains(Input.mousePosition))
                {
                    //Check the raycast
                    RaycastHit hit;
                    Physics.Raycast(views[i].viewCamera.ViewportPointToRay(views[i].viewCamera.ScreenToViewportPoint(Input.mousePosition)), out hit, 1000.0f);

                    GameObject hitObject = null;

                    if (hit.collider != null)
                        hitObject = hit.collider.gameObject;

                    foreach (var member in memberDictionary)
                    {
                        if (member.Value != null && member.Value.gameObject.activeSelf)
                        {
                            if (Input.GetMouseButtonUp(0))
                            {
                                member.Value.SetSelected(hitObject, true, 0, doubleClicked);
                            }
                            else if(Input.GetMouseButtonUp(1))
                            {
                                member.Value.SetSelected(hitObject, true, 1, doubleClicked);
                            }
                            else
                            {
                                member.Value.SetHover(hitObject);
                            }

                            //Check the connection meshes
                            foreach (var connection in member.Value.GetConnectionMembers())
                            {
                                if (Input.GetMouseButtonUp(0))
                                {
                                    connection.SetSelected(hitObject, true, 0, doubleClicked);
                                }
                                else if(Input.GetMouseButtonUp(1))
                                {
                                    connection.SetSelected(hitObject, true, 1, doubleClicked);
                                }
                                else
                                {
                                    connection.SetHover(hitObject);
                                }
                            }
                        }
                    }
                }
            }
        }

        //Detect Descon close
        if (!Application.isEditor)
        {
            currentProcessTime += Time.deltaTime;

            if (currentProcessTime >= checkProcessTime)
            {
                var detectedDescon = false;
                var procList = System.Diagnostics.Process.GetProcesses();

                for (int i = 0; i < procList.Length; i++)
                {
                    try
                    {
                        if (procList[i].ProcessName.Contains("Descon"))
                        {
                            if (System.Diagnostics.Process.GetCurrentProcess().Id != procList[i].Id)
                            //Locks up descon
                            //if (procList[i].MainWindowTitle == "Descon 8")
                            {
                                detectedDescon = true;
                                break;
                            }
                        }
                    }
                    catch { }
                }

                currentProcessTime = 0.0f;

                if (detectedDescon == false)
                {
                    Application.Quit();
                }
            }
        }

        //Debug menu
        if(Input.GetKeyDown(KeyCode.U))
        {
            debugMenuActivated = !debugMenuActivated;

            debugMenu.gameObject.SetActive(debugMenuActivated);
        }

        if(debugMenuActivated)
        {
            mousePosText.text = Input.mousePosition.ToString();
            mouseAxisText.text = Input.GetAxis("Mouse X").ToString() + ", " + Input.GetAxis("Mouse Y").ToString();
            mouseRawAxisText.text = Input.GetAxisRaw("Mouse X").ToString() + ", " + Input.GetAxisRaw("Mouse Y").ToString();
            mouseManagerAxisText.text = MouseManager.GetMouseAxisX().ToString() + ", " + MouseManager.GetMouseAxisY().ToString();

            if (Input.GetKeyDown(KeyCode.P))
            {
                //StartCoroutine("TakeScreenShot");
                PrintDrawing();
            }

            if (Input.GetKeyDown(KeyCode.K))
            {
                saveDimensions = true;
            }

            if (Input.GetKeyDown(KeyCode.L))
            {
                loadDimensions = true;
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                saveFileChanged = true;
                newMemberAdded = true;
            }

            //Recreate dimensions
            if (Input.GetKeyDown(KeyCode.D))
            {
                DestroyAllLabelsAndDimensions();

                saveFileChanged = true;
            }
        }
    }

    public void LoadCameras(List<CameraData> cameraData)
    {
        foreach (var item in cameraData)
        {
            var cam = GetView(item.cameraIndex).viewCamera;

            cam.transform.position = new Vector3(item.posX, item.posY, item.posZ);
            cam.orthographicSize = item.zoom;

            var control = cam.GetComponent<CameraControl>();
            control.nextOrthoValue = item.zoom;
            control.ApplyZoom();
            control.hasMoved = item.hasMoved;
        }
    }

    public void LoadDimensions(List<DimensionData> dimensionData)
    {
        foreach(var item in dimensionData)
        {
            if(item.IsDimension)
            {
                var view = GetView(item.ViewIndex);

                if (item.version != null && item.version == dimensionVersion)
                {
                    view.AddDrawDimension(item.tagName, item.Text, GetVec3(item.PointA), GetVec3(item.PointB), GetVec3(item.PointC), GetVec3(item.PointD),
                    MessageQueueTest.GetDimensionColor(), item.ALen, item.DLen, item.OffsetType);
                }
                else
                {
                    //Ignore the dimension
                }
            }
            else //It is a labelTextObject
            {
                var view = GetView(item.ViewIndex);

                if (item.version != null && item.version == dimensionVersion)
                {
                    if (item.IsWeld)
                    {
                        view.AddDrawWeldLabel(item.tagName, item.Text, GetVec3(item.PointA), GetVec3(item.PointB), item.WeldType, item.AddText, item.LeftOffset, item.RightOffset, true, GetVec3(item.PointC));
                    }
                    else
                    {
                        //Not a weld
                        view.AddDrawLabel(item.tagName, item.Text, GetVec3(item.PointA), GetVec3(item.PointB), true, GetVec3(item.PointC));
                    }
                }
                else
                {
                    //Ignore the dimension
                }
            }
        }

        foreach(var view in views)
        {
            view.changed = true;
        }
    }

    public void LoadColors()
    {
        CommonDataStatic.Preferences = new LoadDataFromXML().LoadPreferencesForUnity();

        if (CommonDataStatic.Preferences != null)
        {
            //Change the colors
            instance.boltColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.Bolts);
            instance.connectionColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.ConnectionElements);
            instance.dimensionColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.DimensionLinesLeaders);
            instance.textColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.Text);
            instance.weldColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.WeldSymbols);
            instance.selectionColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.Highlight);

            var backColor = HexToColor(CommonDataStatic.Preferences.ColorSettings.Background);

            //Change the background color
            for (int i = 0; i < views.Count; ++i)
            {
                views[i].viewCamera.backgroundColor = backColor;
            }

            //Set border color
            if (backColor == Color.white)
                borderColor = Color.black;
            else
                borderColor = Color.white;
        }
    }

    public void SaveDimensions()
    {
        List<DimensionData> dimensionData = new List<DimensionData>();

        foreach(var view in views)
        {
            foreach (var item in view.drawLabelDict)
            {
                var labelObject = item.Value;
                var data = new DimensionData();

                data.version = dimensionVersion;
                data.tagName = item.Key;
                data.IsDimension = false;
                data.Text = labelObject.ShapeName;
                data.IsWeld = labelObject.dragComponent.isWeld;
                data.WeldType = labelObject.dragComponent.weldType;
                data.UseAddLeader = labelObject.dragComponent.useAddLeader;

                if (labelObject.dragComponent.additionalTextObj != null)
                    data.AddText = labelObject.dragComponent.additionalTextObj.GetComponent<Text>().text;

                data.LeftOffset = labelObject.dragComponent.leftTextPosition.x;
                data.RightOffset = labelObject.dragComponent.rightTextPosition.x;

                //Offset
                data.PointA = ConvertVec3(labelObject.dragComponent.offset);
                //Offset2
                data.PointC = ConvertVec3(labelObject.dragComponent.offset2);
                //PosOffset
                data.PointB = ConvertVec3(labelObject.dragComponent.anchor.transform.position);
                data.ViewIndex = GetViewIndex(view.portType);

                dimensionData.Add(data);
            }
        }

        for (int i = 0; i < views.Count; ++i)
        {
            var view = views[i];

            foreach (var item in view.drawDimensionDict)
            {
                var dimension = item.Value;
                var data = new DimensionData();

                data.version = dimensionVersion;
                data.tagName = item.Key;
                data.IsDimension = true;
                data.ViewIndex = i;
                data.Text = dimension.Text;
                data.PointA = ConvertVec3(dimension.pointA.transform.position);
                data.PointB = ConvertVec3(dimension.pointB.transform.position);
                data.PointC = ConvertVec3(dimension.pointC.transform.position);
                data.PointD = ConvertVec3(dimension.pointD.transform.position);
                data.ALen = dimension.aLength;
                data.DLen = dimension.dLength;
                data.OffsetType = dimension.offsetType;

                dimensionData.Add(data);
            }
        }

        List<CameraData> cameraData = new List<CameraData>();

        for (int i = 0; i < 5; ++i)
        {
            var camData = new CameraData();

            var cam = GetView(i).viewCamera;

            camData.cameraIndex = i;
            camData.posX = cam.transform.position.x;
            camData.posY = cam.transform.position.y;
            camData.posZ = cam.transform.position.z;
            camData.zoom = cam.orthographicSize;
            camData.hasMoved = cam.GetComponent<CameraControl>().hasMoved;

            cameraData.Add(camData);
        }

        //Save the data to a new file
        new SaveDataToXML().SaveDimensionsForUnity(dimensionData, cameraData);

        //Tell Descon we're done
        SendUnityData(ConstString.UNITY_DONE_SAVING);
    }

    public Vector3 GetVec3(float[] val)
    {
        if (val != null && val.Length == 3)
        {
            return new Vector3(val[0], val[1], val[2]);
        }
        else
            return Vector3.zero;
    }

    public float[] ConvertVec3(Vector3 val)
    {
        return new float[3] { val.x, val.y, val.z };
    }

    public void PrintDrawing()
    {
        if (CommonDataStatic.Preferences.ReportSettings.NoViewsSelected == false)
        {
            bool[] prevViewStates = new bool[views.Count];
            bool[] printViewStates = new bool[views.Count];

            for (int i = 0; i < views.Count; ++i)
            {
                prevViewStates[i] = views[i].isEnabled;
            }

            printViewStates[GetViewIndex(CameraViewportType.TOP)] = CommonDataStatic.Preferences.ReportSettings.ShowTopView;
            printViewStates[GetViewIndex(CameraViewportType.LEFT)] = CommonDataStatic.Preferences.ReportSettings.ShowLeftSideView;
            printViewStates[GetViewIndex(CameraViewportType.RIGHT)] = CommonDataStatic.Preferences.ReportSettings.ShowRightSideView;
            printViewStates[GetViewIndex(CameraViewportType.FRONT)] = CommonDataStatic.Preferences.ReportSettings.ShowFrontView;
            printViewStates[GetViewIndex(CameraViewportType.D3)] = CommonDataStatic.Preferences.ReportSettings.Show3DView;

            //Set the active state of the views
            for (int i = 0; i < views.Count; ++i)
            {
                views[i].isEnabled = printViewStates[i];
            }

            var oldLineSelection = lineSizeSelection;
            //Bold the lines
            if (lineSizeSelection != 2)
                instance.lineSizeSelection++;

            //Update the camera views
            UpdateCameraViews();

            if (CommonDataStatic.Preferences.ReportSettings.CombineDrawingViews)
            {
                //Combine all the views
                RenderImage(ConstString.FILE_UNITY_IMAGE_ALL);
            }
            else
            {
                //Render an image for each active view
                for (int i = 0; i < views.Count; ++i)
                {
                    if (printViewStates[i])
                    {
                        SetViewActiveSelective(i, true);
                        RenderImage(ViewIndexToFilePath(i));
                    }
                }
            }

            //Reset the views
            for (int i = 0; i < views.Count; ++i)
            {
                views[i].isEnabled = prevViewStates[i];
            }

            UpdateCameraViews();

            //Reset the line size
            instance.lineSizeSelection = oldLineSelection;

            //Reset the lines
            for (int i = 0; i < views.Count; ++i)
            {
                if (views[i].isEnabled)
                {
                    //This will refresh the lines and anchors
                    views[i].changed = true;
                    //views[i].UpdateDimensionAnchors();
                    //views[i].UpdateLabelsAndDimensions();
                    //views[i].UpdateLines();
                }
            }

            SendUnityData(ConstString.UNITY_CREATE_IMAGE_DONE);
        }
    }

    private void RenderImage(string filePath)
    {
        //var screenShotSize = 2400; //Corrseponds to 8 inches printed
        renderTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 24);
        Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

        //Set the render texture
        RenderTexture.active = renderTexture;

        GL.Clear(true, true, Color.black);

        var canvas = GetMainCanvas().GetComponent<Canvas>();
        canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceCamera;
        canvas.planeDistance = 1;

        var printInColor = CommonDataStatic.Preferences.ReportSettings.ShowDrawingInColor;

        for (int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled)
            {
                views[i].isPrinting = true;
                views[i].printInColor = printInColor;
                views[i].UpdateTextColors();
                views[i].SetSizeAndStyle(16, UnityEngine.FontStyle.Bold);
                views[i].UpdateDimensionAnchors();
                views[i].UpdateLines();

                var oldBackColor = views[i].viewCamera.backgroundColor;
                views[i].viewCamera.backgroundColor = Color.white;

                views[i].viewCamera.targetTexture = renderTexture;

                //Render the camera
                views[i].viewCamera.Render();
                views[i].viewCamera.targetTexture = null;
                views[i].viewCamera.backgroundColor = oldBackColor;
            }
        }

        //Render the UI
        //Canvas.ForceUpdateCanvases();
        canvas.worldCamera = mainCamera;
        canvas.scaleFactor = canvas.scaleFactor + 0.0000001f; // force canvas update scale
        mainCamera.targetTexture = renderTexture;
        mainCamera.Render();
        mainCamera.targetTexture = null;

        //Change the text color back
        for (int i = 0; i < views.Count; ++i)
        {
            views[i].isPrinting = false;
            views[i].UpdateTextColors();
            views[i].SetSizeAndStyle(12, UnityEngine.FontStyle.Normal);
            views[i].UpdateDimensionAnchors();
            views[i].UpdateLines();
        }

        canvas.renderMode = UnityEngine.RenderMode.ScreenSpaceOverlay;

        screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        screenShot.Apply();

        RenderTexture.active = null;

        var bytes = screenShot.EncodeToPNG();
        Destroy(screenShot);
        System.IO.File.WriteAllBytes(filePath, bytes);
        RenderTexture.ReleaseTemporary(renderTexture);
    }

    private string ViewIndexToFilePath(int index)
    {
        switch(index)
        {
            case 0: return ConstString.FILE_UNITY_IMAGE_LEFT;
            case 1: return ConstString.FILE_UNITY_IMAGE_TOP;
            case 2: return ConstString.FILE_UNITY_IMAGE_RIGHT;
            case 3: return ConstString.FILE_UNITY_IMAGE_FRONT;
            case 4: return ConstString.FILE_UNITY_IMAGE_3D;
        }

        return ConstString.FILE_UNITY_IMAGE_ALL;
    }

    public static string GetClickString(EMemberType memberType, EMemberSubType subType, EClickType clickType)
    {
        var commonList = CommonDataStatic.CommonLists;

        string str = commonList.CompleteMemberList.First(c => c.Key == memberType).Value + "|" +
        commonList.MemberSubType.First(c => c.Key == subType).Value + "|" +
        commonList.ClickType.First(c => c.Key == clickType).Value;
        return str;
    }

    public static void DestroyAllLabelsAndDimensions()
    {
        foreach (var view in instance.views)
        {
            view.DestroyAllLabelsAndDimensions();
        }
    }

    public static void ClearLabels(string keyName)
    {
        foreach (var view in instance.views)
        {
            view.DestroyDrawLabelsWithTags(keyName);
        }
    }

    public static void ClearLabelsAndDimensions(string tagNames)
    {
        foreach (var view in instance.views)
        {
            view.DestroyDrawLabelsWithTags(tagNames);
            view.DestroyDrawDimensionsWithTags(tagNames);
        }
    }

    public static void ClearLabelsAndDimensionsIgnore(string tagNames, string ignoreTags)
    {
        foreach (var view in instance.views)
        {
            view.DestroyDrawLabelsAndDimensionsWithTags(tagNames, ignoreTags);
        }
    }

    public static string GetSide(double x)
    {
        if (System.Math.Sign(x) > 0)
        {
            return "Right";
        }
        else
            return "Left";
    }

    public void SendUnityData(string command)
    {
		var sendToDescon = new NamedPipeClientStream(".", ConstString.UNITY_PIPE_NAME_RECEIVE, PipeDirection.Out, PipeOptions.Asynchronous) { ReadMode = PipeTransmissionMode.Message };
		sendToDescon.Connect();

        //if (sendToDescon.IsConnected)
        //{
			sendToDescon.BeginWrite(System.Text.Encoding.ASCII.GetBytes(command), 0, command.Length, AsyncSend, sendToDescon);
        //}
    }

    private void AsyncSend(System.IAsyncResult iar)
    {
		var sendToDescon = (NamedPipeClientStream)iar.AsyncState;

        // End the write
		sendToDescon.EndWrite(iar);
		sendToDescon.Flush();
		sendToDescon.Close();
		sendToDescon.Dispose();
    }

    public ViewportControl GetView(int index)
    {
        return views[index];
    }

    public static ViewportControl GetView(ViewMask mask)
    {
        switch(mask)
        {
            case ViewMask.LEFT:
                return instance.views[0];

            case ViewMask.TOP:
                return instance.views[1];

            case ViewMask.RIGHT:
                return instance.views[2];

            case ViewMask.FRONT:
                return instance.views[3];

            case ViewMask.D3:
                return instance.views[4];
        }

        return null;
    }

    public int GetViewIndex(CameraViewportType portType)
    {
        switch(portType)
        {
            case CameraViewportType.LEFT:
                return 0;

            case CameraViewportType.TOP:
                return 1;

            case CameraViewportType.RIGHT:
                return 2;

            case CameraViewportType.FRONT:
                return 3;

            case CameraViewportType.D3:
                return 4;

            case CameraViewportType.BOTTOM:
                return 5;
        }

        return -1;
    }

    public ViewportControl GetView(CameraViewportType portType)
    {
        foreach(var view in views)
        {
            if(view.portType == portType)
            {
                return view;
            }
        }

        return null;
    }

    public void ZoomFitAll(bool reset = false)
    {
        for (int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled)
            {
                views[i].ZoomFit(reset);
            }
        }
    }

    public void ZoomFitSelected(bool reset = false)
    {
        for (int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled && views[i].selected)
            {
                views[i].ZoomFit(reset);
            }
        }
    }

    public static GameObject CreateTextObject(GameObject panel, string text, Vector3 position)
    {
        var obj = (GameObject)Instantiate(instance.simpleLabel, position, Quaternion.identity);
        obj.GetComponent<UnityEngine.UI.Text>().text = text;
        obj.transform.position = position;
        obj.transform.parent = panel.transform;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        return obj;
    }

    public static DimensionCallout CreateDimension(Camera viewCamera, string text, Vector3 a, Vector3 b, Vector3 c, Vector3 d, Color col, float aLength = 0.0f, float dLength = 0.0f, bool isLeftView = false, Descon.Data.EOffsetType offsetBehave = Descon.Data.EOffsetType.Top)
    {
        var obj = (GameObject)Instantiate(instance.calloutPrefab.gameObject, (a + d) / 2, Quaternion.identity);
        var callout = obj.GetComponent<DimensionCallout>();
        callout.Init(viewCamera, isLeftView);
        callout.Text = text;
        callout.viewType = viewCamera.GetComponent<ViewportControl>().portType;
        callout.SetPoints(a, b, c, d);
        callout.offsetType = offsetBehave;
        callout.Color = col;
        callout.aLength = aLength;
        callout.dLength = dLength;

        return callout;
    }

    /// <summary>
    /// Create a new member gameObject if the key is empty. Instead of creating new GameObjects, just use
    /// the gameObject that was already created, just change the mesh.
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
    public static MemberControl2 GetMemberControl(EMemberType type)
    {
        MemberControl2 member = null;

        var name = type.ToString();

        if (!instance.memberDictionary.ContainsKey(name))
        {
            //Create the gameObject for that key
            var meshObject = (GameObject)Instantiate(instance.memberPrefab, Vector3.zero, Quaternion.identity);
            meshObject.name = name;
            member = meshObject.GetComponent<MemberControl2>();
            instance.memberDictionary.Add(name, member);
        }
        else
        {
            member = instance.memberDictionary[name];
        }

        return member;
    }

    public static MemberControl2 CreateMemberControl()
    {
        return ((GameObject)Instantiate(instance.memberPrefab, Vector3.zero, Quaternion.identity)).GetComponent<MemberControl2>();
    }

    public void SetViewActiveSelective(int viewIndex, bool update = false)
    {
        for (int i = 0; i < views.Count; ++i)
        {
            views[i].isEnabled = (i == viewIndex ? true : false);
        }

        if(update)
        {
            UpdateCameraViews();
        }
    }

    public void UpdateCameraViews()
    {
        var activeViews = new List<ViewportControl>();

        for(int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled)
            {
                activeViews.Add(views[i]);
            }
        }

        switch (activeViews.Count)
        {
            case 0:
                break;

            case 1:
                activeViews[0].SetCameraRect(new Rect(0, 0, 1, 1));
                break;
            case 2:
                activeViews[0].SetCameraRect(new Rect(0, 0, 0.5f, 1));
                activeViews[1].SetCameraRect(new Rect(0.5f, 0, 0.5f, 1));
                break;
            case 3:
                activeViews[0].SetCameraRect(new Rect(0, 0.5f, 0.5f, 0.5f));
                activeViews[1].SetCameraRect(new Rect(0.5f, 0.5f, 0.5f, 0.5f));
                activeViews[2].SetCameraRect(new Rect(0, 0, 1, 0.5f));
                break;
            case 4:
                activeViews[0].SetCameraRect(new Rect(0, 0.5f, 0.5f, 0.5f));
                activeViews[1].SetCameraRect(new Rect(0.5f, 0.5f, 0.5f, 0.5f));
                activeViews[2].SetCameraRect(new Rect(0, 0, 0.5f, 0.5f));
                activeViews[3].SetCameraRect(new Rect(0.5f, 0, 0.5f, 0.5f));
                break;
            case 5:
                activeViews[0].SetCameraRect(new Rect(0, 0.5f, 0.33f, 0.5f));
                activeViews[1].SetCameraRect(new Rect(0.33f, 0.5f, 0.33f, 0.5f));
                activeViews[2].SetCameraRect(new Rect(0.66f, 0.5f, 0.34f, 0.5f));

                activeViews[3].SetCameraRect(new Rect(0, 0, 0.5f, 0.5f));
                activeViews[4].SetCameraRect(new Rect(0.5f, 0, 0.5f, 0.5f));
                break;
        }

        for (int i = 0; i < views.Count; ++i)
        {
            if (views[i].isEnabled)
            {
                views[i].panel.SetActive(true);

                var panel = views[i].panel.GetComponent<PanelResizer>();

                if (panel != null)
                {
                    panel.UpdateBounds();
                }
            }
            else
            {
                views[i].panel.SetActive(false);
            }
        }
    }

    public Color HexToColor(string hex)
    {
        if (hex != null)
        {
            byte r = byte.Parse(hex.Substring(3, 2), System.Globalization.NumberStyles.HexNumber);
            byte g = byte.Parse(hex.Substring(5, 2), System.Globalization.NumberStyles.HexNumber);
            byte b = byte.Parse(hex.Substring(7, 2), System.Globalization.NumberStyles.HexNumber);
            byte a = byte.Parse(hex.Substring(1, 2), System.Globalization.NumberStyles.HexNumber);

            return new Color(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }
        else
        {
            return Color.magenta;
        }
    }
}