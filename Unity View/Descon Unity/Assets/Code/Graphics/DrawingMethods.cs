using System;
using System.Collections.Generic;
using System.Linq;
using Descon.Data;
using UnityEngine;
using Shape = Descon.Data.Shape;

public partial class DrawingMethods : MonoBehaviour
{
    public static Dictionary<EMemberType, DetailData> DataClass = CommonDataStatic.DetailDataDict;
    public static DrawingMethods instance;
    public bool MeshOn = false;

    private Vector3 translation = Vector3.zero;
    private Mesh mesh = null;
    private List<Mesh> connectionMeshes = new List<Mesh>();
    private List<Mesh> boltMeshes = new List<Mesh>();
    private List<UnityEngine.Material> connectionMats = new List<UnityEngine.Material>();
    private DetailData primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
    private float boltHeight;
    private bool useCustomLines = false;
    private Dictionary<ViewMask, List<CustomLine>> customLines;
    private float customLineOffset = 0.001f;
    private bool isPrimaryInPlane = false;
    private bool updateBaseMesh = false;
    private bool useCustomMask = false;
    private bool useCustomMaskList = false;
    private bool useBoltCustomMaskList = false;
    private List<ViewMask> customMaskList;
    private List<ViewMask> customBoltMaskList;
    private ViewMask customMask;
    private ViewMask defaultMask;

    private float topElInches = 0.0f;
    private float yOffset = 0.0f;
    private EMemberSubType subType = EMemberSubType.Shear;
    private float METRIC = 0.0f;
    private string unitString;

    private DetailData brace0;
    private DetailData brace1;
    private DetailData brace2;
    private bool uL;
    private bool uR;
    private bool lL;
    private bool lR;
    private bool isBrace;
    private BCGussetPlate gusset;
    private bool isShearBolt;

    private bool useCustomConnLines = false;
    private List<Dictionary<ViewMask, List<CustomLine>>> customConnLines;
    private float customConnLineOffset = 0.001f;

    //Called from Unity on scene start
    void Awake()
    {
        instance = this;

        customLines = new Dictionary<ViewMask, List<CustomLine>>();
        customConnLines = new List<Dictionary<ViewMask, List<CustomLine>>>();
        customMaskList = new List<ViewMask>();
        customBoltMaskList = new List<ViewMask>();

        Init();
    }

    private void Init()
    {
        instance.customLines.Add(ViewMask.D3, new List<CustomLine>());
        instance.customLines.Add(ViewMask.FRONT, new List<CustomLine>());
        instance.customLines.Add(ViewMask.LEFT, new List<CustomLine>());
        instance.customLines.Add(ViewMask.RIGHT, new List<CustomLine>());
        instance.customLines.Add(ViewMask.TOP, new List<CustomLine>()); 

        //For now, add 2 connection lines
        AddConnectionLines();
        AddConnectionLines();
    }

    private void AddConnectionLines()
    {
        instance.customConnLines.Add(new Dictionary<ViewMask, List<CustomLine>>());

        var index = instance.customConnLines.Count - 1;

        instance.customConnLines[index].Add(ViewMask.D3, new List<CustomLine>());
        instance.customConnLines[index].Add(ViewMask.TOP, new List<CustomLine>());
        instance.customConnLines[index].Add(ViewMask.FRONT, new List<CustomLine>());
        instance.customConnLines[index].Add(ViewMask.LEFT, new List<CustomLine>());
        instance.customConnLines[index].Add(ViewMask.RIGHT, new List<CustomLine>());
    }

    private void ResetConnectionLines()
    {
        foreach(var conn in instance.customConnLines)
        {
            conn[ViewMask.D3].Clear();
            conn[ViewMask.TOP].Clear();
            conn[ViewMask.FRONT].Clear();
            conn[ViewMask.LEFT].Clear();
            conn[ViewMask.RIGHT].Clear();
        }
    }

    private void Reset()
    {
        instance.translation = Vector3.zero;
        instance.connectionMeshes.Clear();
        instance.boltMeshes.Clear();
        instance.connectionMats.Clear();
        instance.primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember]; 
        instance.useCustomLines = false;
        instance.customLineOffset = 0.001f;
        instance.isPrimaryInPlane = false;
        instance.updateBaseMesh = false;
        instance.METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        instance.unitString = GetUnitString();
        instance.boltHeight = 0.390625f; // 25/64
        instance.isShearBolt = true;
        instance.useCustomMask = false;
        instance.useCustomMaskList = false;
        instance.useBoltCustomMaskList = false;
        instance.customMaskList.Clear();
        instance.customBoltMaskList.Clear();
        instance.defaultMask = ViewMask.FRONT | ViewMask.LEFT | ViewMask.RIGHT | ViewMask.TOP;
        instance.useCustomConnLines = false;
        instance.customConnLineOffset = 0.001f;

        instance.isPrimaryInPlane = instance.primaryComponent.WebOrientation == EWebOrientation.InPlane;
        instance.subType = EMemberSubType.Shear;
        instance.unitString = GetUnitString();

        //Clear the custom lines
        instance.customLines[ViewMask.D3].Clear();
        instance.customLines[ViewMask.FRONT].Clear();
        instance.customLines[ViewMask.LEFT].Clear();
        instance.customLines[ViewMask.RIGHT].Clear();
        instance.customLines[ViewMask.TOP].Clear();

        //Clear the custom connection lines
        ResetConnectionLines();

        brace0 = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        brace1 = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        brace2 = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
    }

    private void SetTopElAndYOffset(DetailData component)
    {
        var isNegative = component.WinConnect.Beam.IsTopElNegative;
        instance.topElInches = (float)((component.WinConnect.Beam.TopElFeet * 12.0f) + component.WinConnect.Beam.TopElInches + (component.WinConnect.Beam.TopElNumerator / component.WinConnect.Beam.TopElDenominator));

        if (isNegative)
        {
            instance.topElInches *= -1;
        }

        instance.yOffset = (float)(GetYOffset(component) + instance.topElInches);

        uL = component.MemberType == EMemberType.UpperLeft;
        uR = component.MemberType == EMemberType.UpperRight;
        lL = component.MemberType == EMemberType.LowerLeft;
        lR = component.MemberType == EMemberType.LowerRight;
        isBrace = uL || uR || lL || lR;
        gusset = component.BraceConnect.Gusset;
    }

    public static float GetTopEl(DetailData component)
    {
        var isNegative = component.WinConnect.Beam.IsTopElNegative;
        var topElInches = (float)((component.WinConnect.Beam.TopElFeet * 12.0f) + component.WinConnect.Beam.TopElInches + (component.WinConnect.Beam.TopElNumerator / component.WinConnect.Beam.TopElDenominator));

        if (isNegative)
        {
            topElInches *= -1;
        }

        return (float)(topElInches);
    }

    public static float GetTopElAndYOffset(DetailData component)
    {
        var isNegative = component.WinConnect.Beam.IsTopElNegative;
        var topElInches = (float)((component.WinConnect.Beam.TopElFeet * 12.0f) + component.WinConnect.Beam.TopElInches + (component.WinConnect.Beam.TopElNumerator / component.WinConnect.Beam.TopElDenominator));

        if (isNegative)
        {
            topElInches *= -1;
        }

        return (float)(GetYOffset(component) + topElInches);
    }

    public static float GirderOffset(DetailData component)
    {
        if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
        {
            var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
            return (float)(primaryComponent.Shape.USShape.d - component.Shape.USShape.d) / 2;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// Create a triangular prism for use with fillet weld meshes
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="dim"></param>
    /// <param name="angles"></param>
    /// <returns></returns>
    public static Mesh CreateFilletWeldMesh(Vector3 translation, Vector3 dim, Vector3 angles)
    {
        dim /= 2.0f;

        MeshCreator.Reset();

        MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, -dim.z));

        MeshCreator.Rotate(angles);
        MeshCreator.Translate(translation);

        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a wide flange mesh
    /// </summary>
    /// <param name="component"></param>
    /// <param name="y"></param>
    /// <param name="length"></param>
    /// <param name="isInPlane"></param>
    /// <param name="isDirectlyWelded"></param>
    /// <param name="endSetback"></param>
    /// <param name="isLeft"></param>
    /// <returns></returns>
    public static Mesh CreateWideFlangeMesh(DetailData component, Vector3 position, Vector3 rotation, float length = 0.0f, bool isDirectlyWelded = false, bool useCope = false)
    {
        var shape = component.Shape.USShape;

        //     bf
        //  |-------| Tf     -
        //     | |            |
        //     | |            |
        //     |Tw|           D
        //     | |            |
        //     | |            |
        //  |-------|        -

        var width = (float) (shape.bf/2);
        var iWidth = (float) (shape.tw/2); //Inner Width
        var height = (float) (shape.d/2);
        var iHeight = (float) (height - shape.tf); //Inner Height
        var bevelOffset = isDirectlyWelded ? (float)shape.tf * Mathf.Tan(30 * Mathf.Deg2Rad) : 0;
        var copeOffset = useCope ? (float)component.WinConnect.Beam.TCopeL : 0;


        MeshCreator.Reset();
        //Top////////////////
        MeshCreator.AddQuad(new Vector3(-width, height, -length), new Vector3(-width, height, length - bevelOffset - copeOffset),
            new Vector3(width, height, length - bevelOffset - copeOffset), new Vector3(width, height, -length));
        //Top back
        MeshCreator.AddQuad(new Vector3(-width, iHeight, -length), new Vector3(-width, height, -length),
            new Vector3(width, height, -length), new Vector3(width, iHeight, -length));
        //Top front
        MeshCreator.AddQuad(new Vector3(width, iHeight, length - copeOffset), new Vector3(width, height, length - bevelOffset - copeOffset),
            new Vector3(-width, height, length - bevelOffset - copeOffset), new Vector3(-width, iHeight, length - copeOffset));
        //Left
        MeshCreator.AddQuad(new Vector3(-width, iHeight, length - copeOffset), new Vector3(-width, height, length - bevelOffset - copeOffset),
            new Vector3(-width, height, -length), new Vector3(-width, iHeight, -length));
        //Right
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(width, height, -length),
            new Vector3(width, height, length - bevelOffset - copeOffset), new Vector3(width, iHeight, length - copeOffset));
        //Underside Left
        MeshCreator.AddQuad(new Vector3(-iWidth, iHeight, -length), new Vector3(-iWidth, iHeight, length - copeOffset),
            new Vector3(-width, iHeight, length - copeOffset), new Vector3(-width, iHeight, -length));
        //Underside Right
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(width, iHeight, length - copeOffset),
            new Vector3(iWidth, iHeight, length - copeOffset), new Vector3(iWidth, iHeight, -length));

        var botCopeOffset = (float)component.WinConnect.Beam.BCopeL;

        //Bottom////////////////
        MeshCreator.AddQuad(new Vector3(width, -height, -length), new Vector3(width, -height, length - botCopeOffset),
            new Vector3(-width, -height, length - botCopeOffset), new Vector3(-width, -height, -length));
        //Top back
        MeshCreator.AddQuad(new Vector3(width, -iHeight, -length), new Vector3(width, -height, -length),
            new Vector3(-width, -height, -length), new Vector3(-width, -iHeight, -length));
        //Top front
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, length - bevelOffset - botCopeOffset), new Vector3(-width, -height, length - botCopeOffset),
            new Vector3(width, -height, length - botCopeOffset), new Vector3(width, -iHeight, length - bevelOffset - botCopeOffset));
        //Left
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, -length), new Vector3(-width, -height, -length),
            new Vector3(-width, -height, length - botCopeOffset), new Vector3(-width, -iHeight, length - bevelOffset - botCopeOffset));
        //Right
        MeshCreator.AddQuad(new Vector3(width, -iHeight, length - bevelOffset - botCopeOffset), new Vector3(width, -height, length - botCopeOffset),
            new Vector3(width, -height, -length), new Vector3(width, -iHeight, -length));
        //Underside Left
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, -length), new Vector3(-width, -iHeight, length - bevelOffset - botCopeOffset),
            new Vector3(-iWidth, -iHeight, length - bevelOffset - botCopeOffset), new Vector3(-iWidth, -iHeight, -length));
        //Underside Right
        MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, -iHeight, length - bevelOffset - botCopeOffset),
            new Vector3(width, -iHeight, length - bevelOffset - botCopeOffset), new Vector3(width, -iHeight, -length));

        if (!isDirectlyWelded && !useCope)
        {
            //Column
            //Back
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, iHeight, -length),
                new Vector3(iWidth, iHeight, -length), new Vector3(iWidth, -iHeight, -length));
            //Front
            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, length), new Vector3(iWidth, iHeight, length),
                new Vector3(-iWidth, iHeight, length), new Vector3(-iWidth, -iHeight, length));
            //Right
            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, iHeight, -length),
                new Vector3(iWidth, iHeight, length), new Vector3(iWidth, -iHeight, length));
            //Left
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, length), new Vector3(-iWidth, iHeight, length),
                new Vector3(-iWidth, iHeight, -length), new Vector3(-iWidth, -iHeight, -length));
        }
        else if(useCope)
        {
            var copeLength = (float)component.WinConnect.Beam.TCopeL;
            var copeDepth = (float)component.WinConnect.Beam.TCopeD;

            var botCopeLength = (float)component.WinConnect.Beam.BCopeL;
            var botCopeDepth = (float)(component.WinConnect.Beam.BCopeD - component.Shape.tf);

            //Column
            //Back
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, iHeight, -length),
                new Vector3(iWidth, iHeight, -length), new Vector3(iWidth, -iHeight, -length));
            //Front
            MeshCreator.AddQuad(new Vector3(iWidth, height - copeDepth, length - copeLength), new Vector3(iWidth, iHeight, length - copeLength),
                new Vector3(-iWidth, iHeight, length - copeLength), new Vector3(-iWidth, height - copeDepth, length - copeLength));

            //Indent front
            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight + botCopeDepth, length), new Vector3(iWidth, height - copeDepth, length),
                new Vector3(-iWidth, height - copeDepth, length), new Vector3(-iWidth, -iHeight + botCopeDepth, length));

            //Indent left
            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight + botCopeDepth, length), new Vector3(iWidth, height - copeDepth, length),
                new Vector3(iWidth, height - copeDepth, length - copeLength), new Vector3(iWidth, -iHeight + botCopeDepth, length - copeLength), true);

            //Indent right
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight + botCopeDepth, length), new Vector3(-iWidth, height - copeDepth, length),
                new Vector3(-iWidth, height - copeDepth, length - copeLength), new Vector3(-iWidth, -iHeight + botCopeDepth, length - copeLength));

            var addBotRadius = false;

            if(botCopeDepth > 0)
            {
                MeshCreator.AddQuad(new Vector3(iWidth, -iHeight + botCopeDepth, length), new Vector3(iWidth, -iHeight + botCopeDepth, length - botCopeLength),
                    new Vector3(-iWidth, -iHeight + botCopeDepth, length - botCopeLength), new Vector3(-iWidth, -iHeight + botCopeDepth, length), true);

                MeshCreator.AddQuad(new Vector3(iWidth, -iHeight + botCopeDepth, length - botCopeLength), new Vector3(iWidth, -iHeight, length - botCopeLength),
                    new Vector3(-iWidth, -iHeight, length - botCopeLength), new Vector3(-iWidth, -iHeight + botCopeDepth, length - botCopeLength), true);

                //Bottom part of beam, left/right
                MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, -iHeight + botCopeDepth, -length),
                    new Vector3(iWidth, -iHeight + botCopeDepth, length - botCopeLength), new Vector3(iWidth, -iHeight, length - botCopeLength));

                MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, -iHeight + botCopeDepth, -length),
                    new Vector3(-iWidth, -iHeight + botCopeDepth, length - botCopeLength), new Vector3(-iWidth, -iHeight, length - botCopeLength), true);

                //Add radius
                addBotRadius = true;
            }

            //Indent top
            MeshCreator.AddQuad(new Vector3(-iWidth, height - copeDepth, length), new Vector3(iWidth, height - copeDepth, length),
                new Vector3(iWidth, height - copeDepth, length - copeLength), new Vector3(-iWidth, height - copeDepth, length - copeLength));

            //Right
            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight + botCopeDepth, -length), new Vector3(iWidth, iHeight, -length),
                new Vector3(iWidth, iHeight, length - copeLength), new Vector3(iWidth, -iHeight + botCopeDepth, length - copeLength));
            //Left
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight + botCopeDepth, length - copeLength), new Vector3(-iWidth, iHeight, length - copeLength),
                new Vector3(-iWidth, iHeight, -length), new Vector3(-iWidth, -iHeight + botCopeDepth, -length));

            //Save the current mesh
            var mesh = MeshCreator.Create();

            //Create smooth cut
            Mesh mesh2 = null;

            if (copeDepth > 0)
                mesh2 = MeshCreator.CreateInvertedCorner(new Vector3(0, height - copeDepth, length - copeLength), new Vector3(0, -90, 0), 16, 0.5f, iWidth * 2);

            Mesh mesh3 = null;

            if (addBotRadius)
            {
                mesh3 = MeshCreator.CreateInvertedCorner(new Vector3(0, -iHeight + botCopeDepth, length - botCopeLength), new Vector3(0, -90, -90), 16, 0.5f, iWidth * 2);
            }

            //Add bottom cope

            MeshCreator.Reset();

            //Re-add the geometry
            MeshCreator.Add(mesh);
            MeshCreator.Add(mesh2);
            MeshCreator.Add(mesh3);
        }
        #region Weld Access holes
        else
        {
            var hheight = Mathf.Clamp(Mathf.Max(1.0f * (iWidth * 2), 0.75f), 0, 2.0f);
            var hlength = Math.Max(1.5f * (iWidth * 2), 1.5f);
            var R = 0.28f;// Mathf.Clamp(0.375f, 0, hheight / 2); //3/8 in
            var corner = new Vector3(-iWidth, iHeight, length - hlength);
            var pivot = new Vector3(-iWidth, iHeight - hheight / 2, length - hlength + R);
            var flatLength = 0.6875f - 0; //endSetback

            if (flatLength < 0)
                flatLength = 0;

            //Create the top triangle fan
            var fanCount = 8;

            //Do for both sides
            for (int sides = -1; sides < 2; sides += 2)
            {
                corner.x = sides == 1 ? -iWidth : iWidth;

                for (int i = 0; i < fanCount; ++i)
                {
                    var angle = ((float)i / fanCount) * 90 * Mathf.Deg2Rad;
                    var angle2 = ((float)(i + 1) / fanCount) * 90 * Mathf.Deg2Rad;
                    MeshCreator.AddQuad(new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                        new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)), corner,
                        corner, sides == -1);

                    if (i == fanCount - 1)
                    {
                        var p1 = new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2));
                        var p2 = new Vector3(-iWidth * sides, iHeight, pivot.z - R * Mathf.Cos(angle2));
                        MeshCreator.AddQuad(p1, p2,
                            corner, corner, sides == -1);

                        MeshCreator.AddQuad(p1, new Vector3(-iWidth * sides, p1.y, length - flatLength), new Vector3(-iWidth * sides, iHeight, length - flatLength), p2, sides == -1);

                        //Add bridge
                        if (sides == 1)
                        {
                            MeshCreator.AddQuad(new Vector3(-p1.x, p1.y, p1.z), new Vector3(iWidth, p1.y, length - flatLength), new Vector3(-iWidth, p1.y, length - flatLength), p1);
                        }
                    }

                    //Add bridges
                    if (sides == 1)
                    {
                        MeshCreator.AddQuad(new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                            new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)));
                    }
                }
            }

            corner.y = iHeight - hheight;

            //Do for both sides
            for (int sides = -1; sides < 2; sides += 2)
            {
                corner.x = sides == 1 ? -iWidth : iWidth;

                for (int i = 0; i < fanCount; ++i)
                {
                    var angle = (((float)i / fanCount) * 90 + 270) * Mathf.Deg2Rad;
                    var angle2 = (((float)(i + 1) / fanCount) * 90 + 270) * Mathf.Deg2Rad;
                    MeshCreator.AddQuad(new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                        new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)), corner,
                        corner, sides == -1);

                    if (i == 0)
                    {
                        MeshCreator.AddQuad(new Vector3(-iWidth * sides, iHeight - hheight, length),
                            new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)), corner,
                        corner, sides == -1);
                    }

                    if (i == 0)
                    {
                        //Add bridge
                        if (sides == 1)
                        {
                            MeshCreator.AddQuad(new Vector3(iWidth, iHeight - hheight, length), new Vector3(-iWidth, iHeight - hheight, length),
                                new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                                new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)), true);
                        }
                    }

                    //Add bridges
                    if (sides == 1)
                    {
                        MeshCreator.AddQuad(new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                            new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)));
                    }
                }
            }

            //Create the bottom triangle fan
            pivot.y *= -1;
            corner.y = -iHeight + hheight;

            //Do for both sides
            for (int sides = -1; sides < 2; sides += 2)
            {
                corner.x = sides == 1 ? -iWidth : iWidth;

                for (int i = 0; i < fanCount; ++i)
                {
                    var angle = ((float)i / fanCount) * 90 * Mathf.Deg2Rad;
                    var angle2 = ((float)(i + 1) / fanCount) * 90 * Mathf.Deg2Rad;
                    MeshCreator.AddQuad(new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                        new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)), corner,
                        corner, sides == -1);

                    //Add bridges
                    if (sides == 1)
                    {
                        MeshCreator.AddQuad(new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                            new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)));
                    }

                    if (i == fanCount - 1)
                    {
                        var p1 = new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2));
                        MeshCreator.AddQuad(p1, new Vector3(-iWidth * sides, -iHeight + hheight, length),
                            corner, corner, sides == -1);

                        //Add bridge
                        if (sides == 1)
                        {
                            MeshCreator.AddQuad(new Vector3(p1.x * -1, p1.y, p1.z), new Vector3(iWidth, -iHeight + hheight, length),
                                new Vector3(-iWidth, -iHeight + hheight, length), p1);
                        }
                    }
                }
            }

            corner.y = -iHeight;

            //Do for both sides
            for (int sides = -1; sides < 2; sides += 2)
            {
                corner.x = sides == 1 ? -iWidth : iWidth;

                for (int i = 0; i < fanCount; ++i)
                {
                    var angle = (((float)i / fanCount) * 90 + 270) * Mathf.Deg2Rad;
                    var angle2 = (((float)(i + 1) / fanCount) * 90 + 270) * Mathf.Deg2Rad;
                    MeshCreator.AddQuad(new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                        new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)), corner,
                        corner, sides == -1);

                    //Add bridges
                    if (sides == 1)
                    {
                        MeshCreator.AddQuad(new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                            new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle2), pivot.z - R * Mathf.Cos(angle2)),
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)));
                    }

                    if (i == 0)
                    {
                        MeshCreator.AddQuad(new Vector3(-iWidth * sides, -iHeight, length - bevelOffset),
                            new Vector3(-iWidth * sides, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)),
                            corner, corner, sides == -1);

                        //Add bridge
                        if (sides == 1)
                        {
                            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, length - bevelOffset),
                            new Vector3(iWidth, -iHeight, length - bevelOffset),
                            new Vector3(iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)), 
                            new Vector3(-iWidth, pivot.y + R * Mathf.Sin(angle), pivot.z - R * Mathf.Cos(angle)));
                        }
                    }
                }
            }

            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, iHeight, -length),
                new Vector3(iWidth, iHeight, -length), new Vector3(iWidth, -iHeight, -length));

            //Create the weld access panels
            MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, -iHeight, length - hlength),
                new Vector3(-iWidth, iHeight, length - hlength), new Vector3(-iWidth, iHeight, -length));

            MeshCreator.AddQuad(new Vector3(-iWidth, iHeight - hheight, length), new Vector3(-iWidth, iHeight - hheight, length - hlength),
                new Vector3(-iWidth, -iHeight + hheight, length - hlength), new Vector3(-iWidth, -iHeight + hheight, length));

            //Front
            MeshCreator.AddQuad(new Vector3(iWidth, iHeight - hheight, length), new Vector3(-iWidth, iHeight - hheight, length),
                new Vector3(-iWidth, -iHeight + hheight, length), new Vector3(iWidth, -iHeight + hheight, length));

            MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, -iHeight, length - hlength),
                new Vector3(iWidth, iHeight, length - hlength), new Vector3(iWidth, iHeight, -length), true);

            MeshCreator.AddQuad(new Vector3(iWidth, iHeight - hheight, length), new Vector3(iWidth, iHeight - hheight, length - hlength),
                new Vector3(iWidth, -iHeight + hheight, length - hlength), new Vector3(iWidth, -iHeight + hheight, length), true);

            MeshCreator.AddQuad(new Vector3(iWidth, iHeight, length), new Vector3(iWidth, iHeight, length - flatLength),
                new Vector3(-iWidth, iHeight, length - flatLength), new Vector3(-iWidth, iHeight, length), true);

            var flatHeight = iHeight - hheight / 2 + R * Mathf.Sin(90 * Mathf.Deg2Rad);
            flatHeight = iHeight - flatHeight;

            if (flatHeight < 0)
                flatHeight = 0;

            MeshCreator.AddQuad(new Vector3(iWidth, iHeight, length - flatLength), new Vector3(iWidth, iHeight - flatHeight, length - flatLength),
                new Vector3(-iWidth, iHeight - flatHeight, length - flatLength), new Vector3(-iWidth, iHeight, length - flatLength), true);
        }
        #endregion

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a hollow steel section mesh shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="isInPlane"></param>
    /// <returns></returns>
    public static Mesh CreateHollowSteelSectionMesh(Shape shape, Vector3 position, Vector3 rotation, float length = 0.0f)
    {
        MeshCreator.Reset();

        if (shape.USShape.od.Equals(0) && shape.USShape.id.Equals(0) && !shape.USShape.b.Equals(0))
        {
            //Rectangular shape
            var thick = (float) (shape.USShape.tnom);
            var radius = 2.0f*thick;
            var width = (float) (shape.USShape.B/2);
            var iWidth = (float) (width - thick);
            var height = (float) (shape.USShape.Ht/2);
            var iHeight = (float) (height - thick);

            var numSegments = 4;

            //Top
            MeshCreator.AddQuad(new Vector3(-width + radius, height, -length),
                new Vector3(-width + radius, height, length), new Vector3(width - radius, height, length),
                new Vector3(width - radius, height, -length));
            //Bottom
            MeshCreator.AddQuad(new Vector3(width - radius, iHeight, -length),
                new Vector3(width - radius, iHeight, length), new Vector3(-width + radius, iHeight, length),
                new Vector3(-width + radius, iHeight, -length));
            //Front
            MeshCreator.AddQuad(new Vector3(-width + radius, iHeight, -length),
                new Vector3(-width + radius, height, -length), new Vector3(width - radius, height, -length),
                new Vector3(width - radius, iHeight, -length));
            //Back
            MeshCreator.AddQuad(new Vector3(width - radius, iHeight, length),
                new Vector3(width - radius, height, length), new Vector3(-width + radius, height, length),
                new Vector3(-width + radius, iHeight, length));

            //Left
            MeshCreator.AddQuad(new Vector3(-width, -height + radius, -length),
                new Vector3(-width, -height + radius, length), new Vector3(-width, height - radius, length),
                new Vector3(-width, height - radius, -length));
            //Left back
            MeshCreator.AddQuad(new Vector3(-width, -height + radius, -length),
                new Vector3(-width, height - radius, -length), new Vector3(-width + thick, height - radius, -length),
                new Vector3(-width + thick, -height + radius, -length));
            //Left front
            MeshCreator.AddQuad(new Vector3(-width + thick, -height + radius, length),
                new Vector3(-width + thick, height - radius, length), new Vector3(-width, height - radius, length),
                new Vector3(-width, -height + radius, length));
            //Left Inside
            MeshCreator.AddQuad(new Vector3(-width + thick, height - radius, -length),
                new Vector3(-width + thick, height - radius, length),
                new Vector3(-width + thick, -height + radius, length),
                new Vector3(-width + thick, -height + radius, -length));
            //Right
            MeshCreator.AddQuad(new Vector3(width, height - radius, -length),
                new Vector3(width, height - radius, length), new Vector3(width, -height + radius, length),
                new Vector3(width, -height + radius, -length));
            //Right back
            MeshCreator.AddQuad(new Vector3(width - thick, -height + radius, -length),
                new Vector3(width - thick, height - radius, -length), new Vector3(width, height - radius, -length),
                new Vector3(width, -height + radius, -length));
            //Right front
            MeshCreator.AddQuad(new Vector3(width, -height + radius, length),
                new Vector3(width, height - radius, length), new Vector3(width - thick, height - radius, length),
                new Vector3(width - thick, -height + radius, length));
            //Right Inside
            MeshCreator.AddQuad(new Vector3(width - thick, -height + radius, -length),
                new Vector3(width - thick, -height + radius, length),
                new Vector3(width - thick, height - radius, length),
                new Vector3(width - thick, height - radius, -length));

            //Top left bevel
            MeshCreator.AddBevelExt(new Vector3(-width, height - radius, -length),
                new Vector3(-width, height - radius, length), new Vector3(-width + radius, height, length),
                new Vector3(-width + radius, height, -length),
                new Vector3(-width + radius, height - radius, -length),
                new Vector3(-width + radius, height - radius, length), numSegments, thick);
            //Top right bevel
            MeshCreator.AddBevelExt(new Vector3(width, height - radius, length),
                new Vector3(width, height - radius, -length), new Vector3(width - radius, height, -length),
                new Vector3(width - radius, height, length),
                new Vector3(width - radius, height - radius, length),
                new Vector3(width - radius, height - radius, -length), numSegments, thick);

            //Bottom left bevel
            MeshCreator.AddBevelExt(new Vector3(-width + radius, -height, -length),
                new Vector3(-width + radius, -height, length), new Vector3(-width, -height + radius, length),
                new Vector3(-width, -height + radius, -length),
                new Vector3(-width + radius, -height + radius, -length),
                new Vector3(-width + radius, -height + radius, length), numSegments, thick);
            //Bottom right bevel
            MeshCreator.AddBevelExt(new Vector3(width - radius, -height, length),
                new Vector3(width - radius, -height, -length), new Vector3(width, -height + radius, -length),
                new Vector3(width, -height + radius, length),
                new Vector3(width - radius, -height + radius, length),
                new Vector3(width - radius, -height + radius, -length), numSegments, thick);

            //Bottom
            MeshCreator.AddQuad(new Vector3(width - radius, -height, -length),
                new Vector3(width - radius, -height, length), new Vector3(-width + radius, -height, length),
                new Vector3(-width + radius, -height, -length));
            //Top
            MeshCreator.AddQuad(new Vector3(-width + radius, -iHeight, -length),
                new Vector3(-width + radius, -iHeight, length), new Vector3(width - radius, -iHeight, length),
                new Vector3(width - radius, -iHeight, -length));
            //Front
            MeshCreator.AddQuad(new Vector3(width - radius, -iHeight, -length),
                new Vector3(width - radius, -height, -length), new Vector3(-width + radius, -height, -length),
                new Vector3(-width + radius, -iHeight, -length));
            //Back
            MeshCreator.AddQuad(new Vector3(-width + radius, -iHeight, length),
                new Vector3(-width + radius, -height, length), new Vector3(width - radius, -height, length),
                new Vector3(width - radius, -iHeight, length));

        }
        else
        {
            //Round/Pipe
            var iRadius = (float) shape.USShape.id/2;
            var oRadius = (float) shape.USShape.od/2;

            MeshCreator.AddHollowRing(length, oRadius, 128, (float) shape.USShape.tdes);
        }

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);
        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a single channel mesh shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Mesh CreateSingleChannelMesh(Shape shape, double y = 0, double z = 0)
    {
        //y and z must be greater than zero
        //if (y < 0) y *= -1;
        //if (z < 0) z *= -1;

        var width = (float) (shape.USShape.bf/2); //Flange Width
        var web = (float) (-width + shape.USShape.tf);
        var height = (float) (shape.USShape.d/2); //Height
        var iHeight = (float) (height - shape.USShape.tf); //Inner Height
        var length = (float) z;
        var Y = (float) y;

        MeshCreator.Reset();
        //Top
        MeshCreator.AddQuad(new Vector3(width, height, -length), new Vector3(-width, height, -length),
            new Vector3(-width, height, length), new Vector3(width, height, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(-width, -height, -length), new Vector3(width, -height, -length),
            new Vector3(width, -height, length), new Vector3(-width, -height, length));

        //Left
        MeshCreator.AddQuad(new Vector3(-width, -height, -length), new Vector3(-width, -height, length),
            new Vector3(-width, height, length), new Vector3(-width, height, -length));

        //Front Top
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(-width, iHeight, -length),
            new Vector3(-width, height, -length), new Vector3(width, height, -length));
        //Front Middle
        MeshCreator.AddQuad(new Vector3(web, -iHeight, -length), new Vector3(-width, -iHeight, -length),
            new Vector3(-width, iHeight, -length), new Vector3(web, iHeight, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(width, -height, -length), new Vector3(-width, -height, -length),
            new Vector3(-width, -iHeight, -length), new Vector3(width, -iHeight, -length));

        //Back Top
        MeshCreator.AddQuad(new Vector3(-width, iHeight, length), new Vector3(width, iHeight, length),
            new Vector3(width, height, length), new Vector3(-width, height, length));
        //Back Middle
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, length), new Vector3(web, -iHeight, length),
            new Vector3(web, iHeight, length), new Vector3(-width, iHeight, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(-width, -height, length), new Vector3(width, -height, length),
            new Vector3(width, -iHeight, length), new Vector3(-width, -iHeight, length));

        //Right Top
        MeshCreator.AddQuad(new Vector3(width, iHeight, length), new Vector3(width, iHeight, -length),
            new Vector3(width, height, -length), new Vector3(width, height, length));
        //Right Middle
        MeshCreator.AddQuad(new Vector3(web, -iHeight, length), new Vector3(web, -iHeight, -length),
            new Vector3(web, iHeight, -length), new Vector3(web, iHeight, length));
        //Right Bottom
        MeshCreator.AddQuad(new Vector3(width, -height, length), new Vector3(width, -height, -length),
            new Vector3(width, -iHeight, -length), new Vector3(width, -iHeight, length));

        //Top Underside
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(width, iHeight, length),
            new Vector3(web, iHeight, length), new Vector3(web, iHeight, -length));

        //Bottom Top
        MeshCreator.AddQuad(new Vector3(width, -iHeight, length), new Vector3(width, -iHeight, -length),
            new Vector3(web, -iHeight, -length), new Vector3(web, -iHeight, length));


        MeshCreator.Rotate(new Vector3(90, -90, 0));
        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a single angle mesh shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Mesh CreateSingleAngleMesh(Shape shape, double y = 0, double z = 0)
    {

        //y and z must be greater than zero
        if (y < 0) y *= -1;
        if (z < 0) z *= -1;


        var widthL1 = (float) (shape.USShape.b/2); //Long Leg
        var widthL2 = (float) (shape.USShape.d/2); //Short Leg
        var thickness = (float) (shape.USShape.t/2);
        var iWidthL1 = (float) (widthL1 - thickness); //Long Leg
        var iWidthL2 = (float) (widthL2 - thickness); //Short Leg
        var length = (float) z;
        var Y = (float) y;

        MeshCreator.Reset();
        //Top
        MeshCreator.AddQuad(new Vector3(-iWidthL2, widthL1, length), new Vector3(-iWidthL2, widthL1, -length),
            new Vector3(-widthL2, widthL1, -length), new Vector3(-widthL2, widthL1, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(-widthL2, -widthL1, length), new Vector3(-widthL2, -widthL1, -length),
            new Vector3(widthL2, -widthL1, -length), new Vector3(widthL2, -widthL1, length));

        //Left
        MeshCreator.AddQuad(new Vector3(-widthL2, -widthL1, -length), new Vector3(-widthL2, -widthL1, length),
            new Vector3(-widthL2, widthL1, length), new Vector3(-widthL2, widthL1, -length));

        //Front Middle
        MeshCreator.AddQuad(new Vector3(-iWidthL2, -widthL1, -length), new Vector3(-widthL2, -widthL1, -length),
            new Vector3(-widthL2, widthL1, -length), new Vector3(-iWidthL2, widthL1, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(widthL2, -widthL1, -length), new Vector3(-iWidthL2, -widthL1, -length),
            new Vector3(-iWidthL2, -iWidthL1, -length), new Vector3(widthL2, -iWidthL1, -length));

        //Back Middle
        MeshCreator.AddQuad(new Vector3(-widthL2, -widthL1, length), new Vector3(-iWidthL2, -widthL1, length),
            new Vector3(-iWidthL2, widthL1, length), new Vector3(-widthL2, widthL1, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(-iWidthL2, -widthL1, length), new Vector3(widthL2, -widthL1, length),
            new Vector3(widthL2, -iWidthL1, length), new Vector3(-iWidthL2, -iWidthL1, length));

        //Right Middle
        MeshCreator.AddQuad(new Vector3(-iWidthL2, -iWidthL1, length), new Vector3(-iWidthL2, -iWidthL1, -length),
            new Vector3(-iWidthL2, widthL1, -length), new Vector3(-iWidthL2, widthL1, length));
        //Right Bottom
        MeshCreator.AddQuad(new Vector3(widthL2, -widthL1, length), new Vector3(widthL2, -widthL1, -length),
            new Vector3(widthL2, -iWidthL1, -length), new Vector3(widthL2, -iWidthL1, length));

        //Bottom Overside
        MeshCreator.AddQuad(new Vector3(widthL2, -iWidthL1, length), new Vector3(widthL2, -iWidthL1, -length),
            new Vector3(-iWidthL2, -iWidthL1, -length), new Vector3(-iWidthL2, -iWidthL1, length));

        //MeshCreator.Rotate(new Vector3(0, 0, 0));
        //MeshCreator.Translate(new Vector3(0, 0, 0));
        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a double channel mesh shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Mesh CreateDoubleChannelMesh(Shape shape, double y = 0, double z = 0)
    {
        //if (y < 0) y *= -1;
        //if (z < 0) z *= -1;

        var width = (float) (shape.USShape.bf); //Flange Width
        var flange = (float) (shape.USShape.tf); //Flange Thickness
        var web = (float) (shape.USShape.tw); //Web Thickness
        var height = (float) (shape.USShape.d); //Height
        var iHeight = (float) (height - flange); //Inner Height
        var length = (float) z;
        var Y = (float) y;

        MeshCreator.Reset();

        //Right Channel
        //Top
        MeshCreator.AddQuad(new Vector3(width, height, length), new Vector3(width, height, -length),
            new Vector3(0, height, -length), new Vector3(0, height, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(0, 0, -length), new Vector3(width, 0, -length),
            new Vector3(width, 0, length));

        //Left
        MeshCreator.AddQuad(new Vector3(0, 0, -length), new Vector3(0, 0, length), new Vector3(0, height, length),
            new Vector3(0, height, -length));

        //Front Top
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(0, iHeight, -length),
            new Vector3(0, height, -length), new Vector3(width, height, -length));
        //Front Middle
        MeshCreator.AddQuad(new Vector3(web, flange, -length), new Vector3(0, flange, -length),
            new Vector3(0, iHeight, -length), new Vector3(web, iHeight, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(width, 0, -length), new Vector3(0, 0, -length),
            new Vector3(0, flange, -length), new Vector3(width, flange, -length));

        //Back Top
        MeshCreator.AddQuad(new Vector3(0, iHeight, length), new Vector3(width, iHeight, length),
            new Vector3(width, height, length), new Vector3(0, height, length));
        //Back Middle
        MeshCreator.AddQuad(new Vector3(0, flange, length), new Vector3(web, flange, length),
            new Vector3(web, iHeight, length), new Vector3(0, iHeight, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(width, 0, length),
            new Vector3(width, flange, length), new Vector3(0, flange, length));

        //Right Top
        MeshCreator.AddQuad(new Vector3(width, iHeight, length), new Vector3(width, iHeight, -length),
            new Vector3(width, height, -length), new Vector3(width, height, length));
        //Right Middle
        MeshCreator.AddQuad(new Vector3(web, flange, length), new Vector3(web, flange, -length),
            new Vector3(web, iHeight, -length), new Vector3(web, iHeight, length));
        //Right Bottom
        MeshCreator.AddQuad(new Vector3(width, 0, length), new Vector3(width, 0, -length),
            new Vector3(width, flange, -length), new Vector3(width, flange, length));

        //Top Underside
        MeshCreator.AddQuad(new Vector3(web, iHeight, length), new Vector3(web, iHeight, -length),
            new Vector3(width, iHeight, -length), new Vector3(width, iHeight, length));

        //Bottom Top
        MeshCreator.AddQuad(new Vector3(width, flange, length), new Vector3(width, flange, -length),
            new Vector3(web, flange, -length), new Vector3(web, flange, length));


        //Left Channel
        //Top
        MeshCreator.AddQuad(new Vector3(0, height, length), new Vector3(0, height, -length),
            new Vector3(-width, height, -length), new Vector3(-width, height, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(-width, 0, length), new Vector3(-width, 0, -length),
            new Vector3(0, 0, -length), new Vector3(0, 0, length));

        //Right
        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(0, 0, -length), new Vector3(0, height, -length),
            new Vector3(0, height, length));

        //Front Top
        MeshCreator.AddQuad(new Vector3(0, iHeight, -length), new Vector3(-width, -iHeight, -length),
            new Vector3(-width, height, -length), new Vector3(0, height, -length));
        //Front Middle
        MeshCreator.AddQuad(new Vector3(0, flange, -length), new Vector3(-web, flange, -length),
            new Vector3(-web, iHeight, -length), new Vector3(0, iHeight, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, -length), new Vector3(-width, 0, -length),
            new Vector3(-width, flange, -length), new Vector3(0, flange, -length));

        //Back Top
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, length), new Vector3(0, iHeight, length),
            new Vector3(0, height, length), new Vector3(-width, height, length));
        //Back Middle
        MeshCreator.AddQuad(new Vector3(-web, flange, length), new Vector3(0, flange, length),
            new Vector3(0, iHeight, length), new Vector3(-web, iHeight, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(-width, 0, length), new Vector3(0, 0, length),
            new Vector3(0, flange, length), new Vector3(-width, flange, length));

        //Left Top
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, -length), new Vector3(-width, -iHeight, length),
            new Vector3(-width, height, length), new Vector3(-width, height, -length));
        //Left Middle
        MeshCreator.AddQuad(new Vector3(-web, flange, -length), new Vector3(-web, flange, length),
            new Vector3(-web, iHeight, length), new Vector3(-web, iHeight, -length));
        //Left Bottom
        MeshCreator.AddQuad(new Vector3(-width, 0, -length), new Vector3(-width, 0, length),
            new Vector3(-width, flange, length), new Vector3(-width, flange, -length));

        //Top Underside
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, length), new Vector3(-width, -iHeight, -length),
            new Vector3(-web, iHeight, -length), new Vector3(-web, iHeight, length));

        //Bottom Top
        MeshCreator.AddQuad(new Vector3(-web, flange, length), new Vector3(-web, flange, -length),
            new Vector3(-width, flange, -length), new Vector3(-width, flange, length));


        //MeshCreator.Rotate(new Vector3(0, 0, 0));
        //MeshCreator.Translate(new Vector3(0, 0, 0));
        return MeshCreator.Create();
    }

    /// <summary>
    /// Create double angle mesh shape.
    /// </summary>
    /// <param name="shape"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Mesh CreateDoubleAngleMesh(Shape shape, double y = 0, double z = 0)
    {
        //y and z must be greater than zero

        if (y < 0) y *= -1;
        if (z < 0) z *= -1;


        var widthL1 = (float) (shape.USShape.b); //Long Leg
        var widthL2 = (float) (shape.USShape.d); //Short Leg
        var thickness = (float) (shape.USShape.t);
        var iWidthL1 = (float) (widthL1 - thickness); //Long Leg
        var iWidthL2 = (float) (widthL2 - thickness); //Short Leg
        var length = (float) z;
        var Y = (float) y;

        MeshCreator.Reset();

        //Left Angle
        //Top
        MeshCreator.AddQuad(new Vector3(0, widthL1, length), new Vector3(0, widthL1, -length),
            new Vector3(-thickness, widthL1, -length), new Vector3(-thickness, widthL1, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(-widthL2, 0, length), new Vector3(-widthL2, 0, -length),
            new Vector3(0, 0, -length), new Vector3(0, 0, length));

        //Right
        //MeshCreator.AddQuad(new Vector3(), new Vector3(), new Vector3(), new Vector3());

        //Front Middle
        MeshCreator.AddQuad(new Vector3(0, thickness, -length), new Vector3(-thickness, thickness, -length),
            new Vector3(-thickness, widthL1, -length), new Vector3(0, widthL1, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, -length), new Vector3(-widthL2, 0, -length),
            new Vector3(-widthL2, thickness, -length), new Vector3(0, thickness, -length));

        //Back Middle
        MeshCreator.AddQuad(new Vector3(-thickness, thickness, length), new Vector3(0, thickness, length),
            new Vector3(0, widthL1, length), new Vector3(-thickness, widthL1, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(-widthL2, 0, length), new Vector3(0, 0, length),
            new Vector3(0, thickness, length), new Vector3(-widthL2, thickness, length));

        //Left Middle
        MeshCreator.AddQuad(new Vector3(-thickness, thickness, -length), new Vector3(-thickness, thickness, length),
            new Vector3(-thickness, widthL1, length), new Vector3(-thickness, widthL1, -length));
        //Left Bottom
        MeshCreator.AddQuad(new Vector3(-widthL2, 0, -length), new Vector3(-widthL2, 0, length),
            new Vector3(-widthL2, thickness, length), new Vector3(-widthL2, thickness, -length));

        //Bottom Overside
        MeshCreator.AddQuad(new Vector3(-widthL2, thickness, -length), new Vector3(-widthL2, thickness, length),
            new Vector3(-thickness, thickness, length), new Vector3(-thickness, thickness, -length));




        //Right Angle
        //Top
        MeshCreator.AddQuad(new Vector3(thickness, widthL1, length), new Vector3(thickness, widthL1, -length),
            new Vector3(0, widthL1, -length), new Vector3(0, widthL1, length));

        //Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(0, 0, -length), new Vector3(widthL2, 0, -length),
            new Vector3(widthL2, 0, length));

        //Left
        //MeshCreator.AddQuad(new Vector3(), new Vector3(), new Vector3(), new Vector3());

        //Front Middle
        MeshCreator.AddQuad(new Vector3(thickness, thickness, -length), new Vector3(0, thickness, -length),
            new Vector3(0, widthL1, -length), new Vector3(thickness, widthL1, -length));
        //Front Bottom
        MeshCreator.AddQuad(new Vector3(widthL2, 0, -length), new Vector3(0, 0, -length),
            new Vector3(0, thickness, -length), new Vector3(widthL2, thickness, -length));

        //Back Middle
        MeshCreator.AddQuad(new Vector3(0, thickness, length), new Vector3(thickness, thickness, length),
            new Vector3(thickness, widthL1, length), new Vector3(0, widthL1, length));
        //Back Bottom
        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(widthL2, 0, length),
            new Vector3(widthL2, thickness, length), new Vector3(0, thickness, length));

        //Right Middle
        MeshCreator.AddQuad(new Vector3(thickness, thickness, length), new Vector3(thickness, thickness, -length),
            new Vector3(thickness, widthL1, -length), new Vector3(thickness, widthL1, length));
        //Right Bottom
        MeshCreator.AddQuad(new Vector3(widthL2, 0, length), new Vector3(widthL2, 0, -length),
            new Vector3(widthL2, thickness, -length), new Vector3(widthL2, thickness, length));

        //Bottom Overside
        MeshCreator.AddQuad(new Vector3(widthL2, thickness, length), new Vector3(widthL2, thickness, -length),
            new Vector3(thickness, thickness, -length), new Vector3(thickness, thickness, length));

        //MeshCreator.Rotate(new Vector3(0, 0, 0));
        //MeshCreator.Translate(new Vector3(0, 0, 0));


        return MeshCreator.Create();
    }

    private static Mesh CreateClipAngleMesh(object clipAngle, double y = 0, double z = 0)
    {
        var conY = 0f;
        var conX = 0f;
        var conZ = 0f;
        var t = 0f;
        var clipangle = clipAngle as WCShearClipAngle;
        if (clipangle != null)
        {
            conY = (float)clipangle.LongLeg;
            conX = (float)clipangle.ShortLeg;
            t = (float)clipangle.Thickness;
        }
        var clipanglewin = clipAngle as WCShearClipAngle;
        if (clipanglewin != null)
        {
            conY = (float)clipanglewin.OSLBot;  //Long leg?
            conX = (float)clipanglewin.OSLTop;  // Short leg?
            t = (float)clipanglewin.Thickness;
        }
        conZ = (float) z;

        if (conZ < 0.0f)
            conZ *= -1;

        var Y = (float) y;
        //conZ = conZ.Equals(0) ? 1.5f : conZ;
        MeshCreator.Reset();

        //Long Leg faces - front//
        MeshCreator.AddQuad(new Vector3(0, 0, 0), new Vector3(0, conY, 0), new Vector3(t, conY, 0),
            new Vector3(t, 0, 0));
        //Long Leg bottom
        MeshCreator.AddQuad(new Vector3(t, 0, 0), new Vector3(t, 0, conZ), new Vector3(0, 0, conZ),
            new Vector3(0, 0, 0));
        //Long Leg back
        MeshCreator.AddQuad(new Vector3(0, 0, conZ), new Vector3(0, conY, conZ), new Vector3(0, conY, 0),
            new Vector3(0, 0, 0));
        //Long Leg inner
        MeshCreator.AddQuad(new Vector3(t, t, 0), new Vector3(t, conY, 0), new Vector3(t, conY, conZ),
            new Vector3(t, t, conZ));
        //Long Leg top
        MeshCreator.AddQuad(new Vector3(t, conY, 0), new Vector3(0, conY, 0), new Vector3(0, conY, conZ),
            new Vector3(t, conY, conZ));
        //Long Leg reverse front
        MeshCreator.AddQuad(new Vector3(t, 0, conZ), new Vector3(t, conY, conZ), new Vector3(0, conY, conZ),
            new Vector3(0, 0, conZ));


        //Short Leg faces - front//
        MeshCreator.AddQuad(new Vector3(t, 0, 0), new Vector3(t, t, 0), new Vector3(conX, t, 0),
            new Vector3(conX, 0, 0));
        //Short Leg bottom
        MeshCreator.AddQuad(new Vector3(conX, 0, conZ), new Vector3(t, 0, conZ), new Vector3(t, 0, 0),
            new Vector3(conX, 0, 0));
        //Short Leg reverse front
        MeshCreator.AddQuad(new Vector3(conX, 0, conZ), new Vector3(conX, t, conZ), new Vector3(t, t, conZ),
            new Vector3(t, 0, conZ));
        //Short Leg inner
        MeshCreator.AddQuad(new Vector3(conX, t, 0), new Vector3(t, t, 0), new Vector3(t, t, conZ),
            new Vector3(conX, t, conZ));
        //Short Leg side A
        MeshCreator.AddQuad(new Vector3(conX, 0, 0), new Vector3(conX, t, 0), new Vector3(conX, t, conZ),
            new Vector3(conX, 0, conZ));

        MeshCreator.Rotate(90, 180, 0);
        MeshCreator.Translate(new Vector3(-Y, 0, 0));
        return MeshCreator.Create();
    }

    /// <summary>
    /// Get the clip angle thickness.
    /// </summary>
    /// <param name="clipAngle"></param>
    /// <returns></returns>
    public static float GetClipThickness(object clipAngle)
    {
        var t = 0.0f;

        var bclip = clipAngle as WCShearClipAngle;
        if (bclip != null)
        {
            t = (float)bclip.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        }
        var wclip = clipAngle as WCShearClipAngle;
        if (wclip != null)
        {
            t = (float)wclip.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        }
        var fclip = clipAngle as WCMomentFlangeAngle;
        if (fclip != null)
        {
            t = (float)fclip.Angle.t * (float)ConstNum.METRIC_MULTIPLIER;
        }

        return t;
    }

    /// <summary>
    /// Create a clip angle mesh shape.
    /// </summary>
    /// <param name="clipAngle"></param>
    /// <param name="length"></param>
    /// <param name="position"></param>
    /// <param name="rotation"></param>
    /// <param name="flipLegs"></param>
    /// <returns></returns>
    public static Mesh CreateClipAngleMesh(object clipAngle, double length, Vector3 position, Vector3 rotation, bool flipLegs = false)
    {
        var llen = 0.0f; //Long Leg
        var slen = 0.0f; //Short Leg
        var t = 0.0f; //Thickness
        var Z = (float)length / 2;//Width of the clip angle

        if (Z < 0.0f)
            Z *= -1;

        var bclip = clipAngle as WCShearClipAngle;
        if (bclip != null)
        {
            llen = (float)bclip.LongLeg * (float)ConstNum.METRIC_MULTIPLIER;
            slen = (float)bclip.ShortLeg * (float)ConstNum.METRIC_MULTIPLIER;
            t = (float)bclip.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        }
        var wclip = clipAngle as WCShearClipAngle;
        if (wclip != null)
        {
            llen = (float)(wclip.LongLeg.Equals(0) ? wclip.OSLBot * (float)ConstNum.METRIC_MULTIPLIER : wclip.LongLeg * (float)ConstNum.METRIC_MULTIPLIER);
            slen = (float)(wclip.ShortLeg.Equals(0) ? wclip.OSLTop * (float)ConstNum.METRIC_MULTIPLIER : wclip.ShortLeg * (float)ConstNum.METRIC_MULTIPLIER);
            t = (float)wclip.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        }

        var flangeclip = clipAngle as WCMomentFlangeAngle; 
        if (flangeclip != null)
        {
            llen = (float)flangeclip.Angle.b * (float)ConstNum.METRIC_MULTIPLIER; //Long Leg
            slen = (float)flangeclip.Angle.d * (float)ConstNum.METRIC_MULTIPLIER; //Short Leg
            t = (float)flangeclip.Angle.t * (float)ConstNum.METRIC_MULTIPLIER; //Thickness
        }

        if (flipLegs)
        {
            var temp = llen;
            llen = slen;
            slen = temp;
        }

        //Begin creating the mesh
        MeshCreator.Reset();

        //Magic
        MeshCreator.AddQuad(new Vector3(0, -Z, 0), new Vector3(0, Z, 0), new Vector3(llen, Z, 0), new Vector3(llen, -Z, 0));
        MeshCreator.AddQuad(new Vector3(0, -Z, slen), new Vector3(0, Z, slen), new Vector3(0, Z, 0), new Vector3(0, -Z, 0));
        MeshCreator.AddQuad(new Vector3(0, Z, 0), new Vector3(0, Z, slen), new Vector3(t, Z, slen), new Vector3(t, Z, 0));
        MeshCreator.AddQuad(new Vector3(t, -Z, 0), new Vector3(t, -Z, slen), new Vector3(0, -Z, slen), new Vector3(0, -Z, 0));
        MeshCreator.AddQuad(new Vector3(t, Z, 0), new Vector3(t, Z, t), new Vector3(llen, Z, t), new Vector3(llen, Z, 0));
        MeshCreator.AddQuad(new Vector3(llen, -Z, 0), new Vector3(llen, -Z, t), new Vector3(t, -Z, t), new Vector3(t, -Z, 0));
        MeshCreator.AddQuad(new Vector3(t, Z, t), new Vector3(t, -Z, t), new Vector3(llen, -Z, t), new Vector3(llen, Z, t));
        MeshCreator.AddQuad(new Vector3(t, -Z, t), new Vector3(t, Z, t), new Vector3(t, Z, slen), new Vector3(t, -Z, slen));

        //Cap the ends
        MeshCreator.AddQuad(new Vector3(0, Z, slen), new Vector3(0, -Z, slen), new Vector3(t, -Z, slen), new Vector3(t, Z, slen));
        MeshCreator.AddQuad(new Vector3(llen, Z, 0), new Vector3(llen, Z, t), new Vector3(llen, -Z, t), new Vector3(llen, -Z, 0));

        //Translate to the center of the created angle
        MeshCreator.Translate(new Vector3(-t/2, 0, -t/2));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    public static Mesh CreateSinglePlateMesh(object plate, Vector3 position, Vector3 rotation, bool doubleWidth = false)
    {
        var width = 0f;
        var len = 0f;
        var tk = 0f;

        var wplate = plate as WCShearWebPlate;

        if(wplate != null)
        {
            width = wplate.Width.Equals(0) ? 1.5f : (float)wplate.Width * (float)ConstNum.METRIC_MULTIPLIER;
            len = (float)wplate.Length * (float)ConstNum.METRIC_MULTIPLIER;
            tk = (float)wplate.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        }

        if (doubleWidth) width *= 2;

        return MeshCreator.CreateBoxMesh(position, new Vector3(width, len, tk), rotation);
    }

    public static Mesh CreateDirectlyWeldedSinglePlateMesh(DetailData component, DetailData primaryComponent, Vector3 position, Vector3 rotation)
    {
        MeshCreator.Reset();

        //if(component.WinConnect.ShearWebPlate.WebPlateStiffener == EWebPlateStiffener.Without)
        //{
        //    //Create the first flat piece
        //    var plate = component.WinConnect.ShearWebPlate;
        //    var t = (float)(plate.Thickness * ConstNum.METRIC_MULTIPLIER);

        //    var plateHeight = (float)(plate.Height * ConstNum.METRIC_MULTIPLIER);
        //    var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
        //    var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + plate.Bolt.EdgeDistTransvDir);

        //    var mesh = MeshCreator.CreateBoxMesh(Vector3.zero, new Vector3(plateWidth, plateHeight, t), Vector3.zero);
        //    var mesh2 = MeshCreator.CreateBoxMesh(new Vector3(-(float)(plateWidth + plate2Width) / 2, 0, 0), new Vector3(plate2Width, (float)plate.Length, t), Vector3.zero);
        //    var mesh3 = MeshCreator.CreateInvertedCorner(new Vector3(-plateWidth / 2, (float)plate.Length / 2, 0), new Vector3(0, 0, 90), 16, 0.5f, t);
        //    var mesh4 = MeshCreator.CreateInvertedCorner(new Vector3(-plateWidth / 2, (float)-plate.Length / 2, 0), new Vector3(0, 0, 180), 16, 0.5f, t);

        //    MeshCreator.Add(mesh);
        //    MeshCreator.Add(mesh2);
        //    MeshCreator.Add(mesh3);
        //    //Note: You don't have to add mesh4 because it's data is still in the meshcreator
        //}
        //else
        //{
        //Create the first flat piece
        var plate = component.WinConnect.ShearWebPlate;
        var t = (float)(plate.Thickness * ConstNum.METRIC_MULTIPLIER);

        var plateHeight = (float)(plate.Height * ConstNum.METRIC_MULTIPLIER);
        var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
        var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + plate.Bolt.EdgeDistTransvDir);
        var plate2Height = (float)plate.Length;
        var radius = 0.0f;

        if (plate.WebPlateStiffener == EWebPlateStiffener.With)
        {
            plate2Width = (float)plate.Width - plateWidth;
            radius = 0.5f;
        }


        //Places the top of the bigger plate flush with the beam
        var offset = (plateHeight - (float)component.Shape.d) / 2;
        var offset2 = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.d, plate2Height, plate.Bolt.EdgeDistLongDir, plate.Position);

        var meshes = new List<Mesh>();

        meshes.Add(MeshCreator.CreateBoxMesh(new Vector3(-radius / 2, -offset, 0), new Vector3(plateWidth - radius, plateHeight, t), Vector3.zero));

        if (radius > 0.0f)
        {
            //Have to make the plate indented
            meshes.Add(MeshCreator.CreateBoxMesh(new Vector3(plateWidth / 2 - radius * 0.5f, -offset, 0), new Vector3(radius, plateHeight - radius * 2, t), Vector3.zero));
            meshes.Add(MeshCreator.CreateInvertedCorner(new Vector3(plateWidth / 2 - radius, plateHeight / 2 - radius - offset, 0), new Vector3(0, 180, 90), 16, 0.5f, t));
            meshes.Add(MeshCreator.CreateInvertedCorner(new Vector3(plateWidth / 2 - radius, -plateHeight / 2 + radius - offset, 0), new Vector3(0, 180, 180), 16, 0.5f, t));
        }

        meshes.Add(MeshCreator.CreateBoxMesh(new Vector3(-(float)(plateWidth + plate2Width) / 2, offset2, 0), new Vector3(plate2Width, plate2Height, t), Vector3.zero));
        meshes.Add(MeshCreator.CreateInvertedCorner(new Vector3(-plateWidth / 2, plate2Height / 2 + offset2, 0), new Vector3(0, 0, 90), 16, 0.5f, t));
        meshes.Add(MeshCreator.CreateInvertedCorner(new Vector3(-plateWidth / 2, -plate2Height / 2 + offset2, 0), new Vector3(0, 0, 180), 16, 0.5f, t));

        MeshCreator.Reset();
        MeshCreator.Add(meshes.ToArray());

        //Move the origin
        MeshCreator.Translate(new Vector3(-((float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2) / 2, 0, 0));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    /// <summary>
    /// Create an array of bolt mesh objects
    /// </summary>
    /// <param name="startPos"></param>
    /// <param name="direction"></param>
    /// <param name="numBolts"></param>
    /// <param name="boltDiameter"></param>
    /// <param name="boltAngles"></param>
    /// <param name="shankLength"></param>
    /// <returns></returns>
    public static Mesh CreateBoltArray(Vector3 startPos, Vector3 direction, int numBolts, float boltDiameter, Vector3 boltAngles, float shankLength = 0.0f)
    {
        MeshCreator.Reset();

        if (numBolts < 30)
        {

            for (int i = 0; i < numBolts; ++i)
            {
                MeshCreator.AddBoltAndShank(GetDiameterHeadSize(boltDiameter) / 2, GetBoltHeightSize(boltDiameter), boltDiameter / 2.0f, shankLength, startPos + direction * i, boltAngles);
            }
        }
        else
        {
            UnityEngine.Debug.Log("Too many bolts!");
        }

        return MeshCreator.Create();
    }

    public static float GetDiameterHeadSize(float boltDiameter)
    {
        if (Approx(boltDiameter, 0.5f, 0.01f)) return 0.875f; // 7/8
        if (Approx(boltDiameter, 0.625f, 0.01f)) return 1.0625f; // 1 1/16
        if (Approx(boltDiameter, 0.75f, 0.01f)) return 1.25f; // 1 1/4
        if (Approx(boltDiameter, 0.875f, 0.01f)) return 1.4375f; // 1 7/16
        if (Approx(boltDiameter, 1, 0.01f)) return 1.625f; // 1 5/8
        if (Approx(boltDiameter, 1.125f, 0.01f)) return 1.8125f; // 1 13/16
        if (Approx(boltDiameter, 1.25f, 0.01f)) return 2; // 2
        if (Approx(boltDiameter, 1.375f, 0.01f)) return 2.1875f; // 2 3/16
        if (Approx(boltDiameter, 1.5f, 0.01f)) return 2.375f; // 2 3/8

        return 1.0625f;
    }

    public static float GetBoltHeightSize(float boltDiameter)
    {
        if (Approx(boltDiameter, 0.5f, 0.01f)) return 0.3125f; // 5/16
        if (Approx(boltDiameter, 0.625f, 0.01f)) return 0.390625f; // 25/64
        if (Approx(boltDiameter, 0.75f, 0.01f)) return 0.46875f; // 15/32
        if (Approx(boltDiameter, 0.875f, 0.01f)) return 0.546875f; // 35/64
        if (Approx(boltDiameter, 1, 0.01f)) return 0.609375f; // 39/64
        if (Approx(boltDiameter, 1.125f, 0.01f)) return 0.6875f; // 1 11/16
        if (Approx(boltDiameter, 1.25f, 0.01f)) return 0.78125f; // 25/35
        if (Approx(boltDiameter, 1.375f, 0.01f)) return 0.84375f; // 27/32
        if (Approx(boltDiameter, 1.5f, 0.01f)) return 0.9375f; // 15/16

        return 1.0625f;
    }

    private static bool Approx(float a, float b, float threshold)
    {
        return (Mathf.Abs(a - b) < threshold);
    }

    /// <summary>
    /// Create a end plate mesh shape.
    /// </summary>
    /// <param name="plate"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="additionalTranslation"></param>
    /// <returns></returns>
    public static Mesh CreateEndPlateMesh(object plate, Vector3 position, Vector3 rotation)
    {
        var width = 0f;
        var thick = 0f;
        var length = 0f;

        var eplatewin = plate as WCShearEndPlate;
        if (eplatewin != null)
        {
            thick = (float)eplatewin.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
            length = (float)eplatewin.Length * (float)ConstNum.METRIC_MULTIPLIER;
            width = (float)eplatewin.Width * (float)ConstNum.METRIC_MULTIPLIER;
        }
        var eplatemoment = plate as WCMomentEndPlate;
        if (eplatemoment != null)
        {
            thick = (float)eplatemoment.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
            length = (float)eplatemoment.Length * (float)ConstNum.METRIC_MULTIPLIER;
            width = (float)eplatemoment.Width * (float)ConstNum.METRIC_MULTIPLIER;
        }
        MeshCreator.Reset();

        //front//
        MeshCreator.AddQuad(new Vector3(thick, 0, 0), new Vector3(thick, length, 0), new Vector3(thick, length, width), new Vector3(thick, 0, width));
        //back
        MeshCreator.AddQuad(new Vector3(0, 0, width), new Vector3(0, length, width), new Vector3(0, length, 0), new Vector3(0, 0, 0));
        //top
        MeshCreator.AddQuad(new Vector3(0, length, width), new Vector3(thick, length, width), new Vector3(thick, length, 0), new Vector3(0, length, 0));
        //bottom
        MeshCreator.AddQuad(new Vector3(thick, 0, width), new Vector3(0, 0, width), new Vector3(0, 0, 0), new Vector3(thick, 0, 0));
        //side a
        MeshCreator.AddQuad(new Vector3(thick, 0, width), new Vector3(thick, length, width), new Vector3(0, length, width), new Vector3(0, 0, width));
        //side b
        MeshCreator.AddQuad(new Vector3(0, 0, 0), new Vector3(0, length, 0), new Vector3(thick, length, 0), new Vector3(thick, 0, 0));


        //Center's the shape
        MeshCreator.Translate(new Vector3(-thick / 2, -length / 2, -width / 2));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();

    }

    /// <summary>
    /// Create a tee mesh shape.
    /// </summary>
    /// <param name="tee"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="offsetThickness"></param>
    /// <param name="rotationAngle"></param>
    /// <param name="Lv"></param>
    /// <returns></returns>
    public static Mesh CreateTeeMesh(object tee, Vector3 position, Vector3 rotation)
    {
        MeshCreator.Reset();
        //Tee shape
        //    bf
        // ------------      -
        // |          | tf   |
        // ----   -----      |
        //     |  |          d
        //     |  |          |
        //     |tw|          |
        //     |  |          |
        //      --           -

        var bf = 0.0f;
        var tf = 0.0f;
        var tw = 0.0f;
        var d = 0.0f;
        var Z = 0.0f;

        var teeWin = tee as WCShearWebTee;

        if (teeWin != null)
        {
            bf = (float)teeWin.Size.bf * (float)ConstNum.METRIC_MULTIPLIER / 2;
            tf = (float)teeWin.Size.tf * (float)ConstNum.METRIC_MULTIPLIER / 2;
            tw = (float)teeWin.Size.tw * (float)ConstNum.METRIC_MULTIPLIER / 2;
            d = (float)teeWin.Size.d * (float)ConstNum.METRIC_MULTIPLIER - tf;
            Z = (float)teeWin.SLength * (float)ConstNum.METRIC_MULTIPLIER / 2; //AKA Length of Tee Field
        }

        var teeMom = tee as WCMomentTee;

        if (teeMom != null)
        {
            bf = (float)teeMom.TopTeeShape.bf * (float)ConstNum.METRIC_MULTIPLIER / 2;
            tf = (float)teeMom.TopTeeShape.tf * (float)ConstNum.METRIC_MULTIPLIER / 2;
            tw = (float)teeMom.TopTeeShape.tw * (float)ConstNum.METRIC_MULTIPLIER / 2;
            d = (float)teeMom.TopTeeShape.d * (float)ConstNum.METRIC_MULTIPLIER - tf;
            Z = (float)teeMom.TopLengthAtStem * (float)ConstNum.METRIC_MULTIPLIER / 2; //AKA Length of Tee Field
        }

        //Magic
        MeshCreator.AddQuad(new Vector3(-tf, Z, -bf), new Vector3(-tf, Z, bf), new Vector3(tf, Z, bf), new Vector3(tf, Z, -bf));
        MeshCreator.AddQuad(new Vector3(tf, -Z, -bf), new Vector3(tf, -Z, bf), new Vector3(-tf, -Z, bf), new Vector3(-tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(tf, Z, -bf), new Vector3(tf, Z, bf), new Vector3(tf, -Z, bf), new Vector3(tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, Z, -bf), new Vector3(tf, Z, -bf), new Vector3(tf, -Z, -bf), new Vector3(-tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, -Z, bf), new Vector3(tf, -Z, bf), new Vector3(tf, Z, bf), new Vector3(-tf, Z, bf));
        MeshCreator.AddQuad(new Vector3(-tf, -Z, -bf), new Vector3(-tf, -Z, -tw), new Vector3(-tf, Z, -tw), new Vector3(-tf, Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, Z, bf), new Vector3(-tf, Z, tw), new Vector3(-tf, -Z, tw), new Vector3(-tf, -Z, bf));
        MeshCreator.AddQuad(new Vector3(-d, -Z, tw), new Vector3(-tf, -Z, tw), new Vector3(-tf, Z, tw), new Vector3(-d, Z, tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, -tw), new Vector3(-tf, Z, -tw), new Vector3(-tf, -Z, -tw), new Vector3(-d, -Z, -tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, tw), new Vector3(-d, Z, -tw), new Vector3(-d, -Z, -tw), new Vector3(-d, -Z, tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, tw), new Vector3(-tf, Z, tw), new Vector3(-tf, Z, -tw), new Vector3(-d, Z, -tw));
        MeshCreator.AddQuad(new Vector3(-d, -Z, -tw), new Vector3(-tf, -Z, -tw), new Vector3(-tf, -Z, tw), new Vector3(-d, -Z, tw));

        //Move the origin
        MeshCreator.Translate(new Vector3(-tf, 0, 0));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    public static Mesh CreateStiffenerTee(Shape tee, Vector3 position, Vector3 rotation, double length)
    {
        MeshCreator.Reset();
        //Tee shape
        //    bf
        // ------------      -
        // |          | tf   |
        // ----   -----      |
        //     |  |          d
        //     |  |          |
        //     |tw|          |
        //     |  |          |
        //      --           -

        var bf = 0.0f;
        var tf = 0.0f;
        var tw = 0.0f;
        var d = 0.0f;
        var Z = 0.0f;

        bf = (float)tee.bf * (float)ConstNum.METRIC_MULTIPLIER / 2;
        tf = (float)tee.tf * (float)ConstNum.METRIC_MULTIPLIER / 2;
        tw = (float)tee.tw * (float)ConstNum.METRIC_MULTIPLIER / 2;
        d = (float)tee.d * (float)ConstNum.METRIC_MULTIPLIER - tf;
        Z = (float)length * (float)ConstNum.METRIC_MULTIPLIER / 2; //AKA Length of Tee Field

        //Magic
        MeshCreator.AddQuad(new Vector3(-tf, Z, -bf), new Vector3(-tf, Z, bf), new Vector3(tf, Z, bf), new Vector3(tf, Z, -bf));
        MeshCreator.AddQuad(new Vector3(tf, -Z, -bf), new Vector3(tf, -Z, bf), new Vector3(-tf, -Z, bf), new Vector3(-tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(tf, Z, -bf), new Vector3(tf, Z, bf), new Vector3(tf, -Z, bf), new Vector3(tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, Z, -bf), new Vector3(tf, Z, -bf), new Vector3(tf, -Z, -bf), new Vector3(-tf, -Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, -Z, bf), new Vector3(tf, -Z, bf), new Vector3(tf, Z, bf), new Vector3(-tf, Z, bf));
        MeshCreator.AddQuad(new Vector3(-tf, -Z, -bf), new Vector3(-tf, -Z, -tw), new Vector3(-tf, Z, -tw), new Vector3(-tf, Z, -bf));
        MeshCreator.AddQuad(new Vector3(-tf, Z, bf), new Vector3(-tf, Z, tw), new Vector3(-tf, -Z, tw), new Vector3(-tf, -Z, bf));
        MeshCreator.AddQuad(new Vector3(-d, -Z, tw), new Vector3(-tf, -Z, tw), new Vector3(-tf, Z, tw), new Vector3(-d, Z, tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, -tw), new Vector3(-tf, Z, -tw), new Vector3(-tf, -Z, -tw), new Vector3(-d, -Z, -tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, tw), new Vector3(-d, Z, -tw), new Vector3(-d, -Z, -tw), new Vector3(-d, -Z, tw));
        MeshCreator.AddQuad(new Vector3(-d, Z, tw), new Vector3(-tf, Z, tw), new Vector3(-tf, Z, -tw), new Vector3(-d, Z, -tw));
        MeshCreator.AddQuad(new Vector3(-d, -Z, -tw), new Vector3(-tf, -Z, -tw), new Vector3(-tf, -Z, tw), new Vector3(-d, -Z, tw));

        //Move the origin
        MeshCreator.Translate(new Vector3(-tf, 0, 0));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    public static Mesh CreateStiffenerPlate(WCShearSeat seat, Vector3 position, Vector3 rotation)
    {
        MeshCreator.Reset();

        //Create the bottom seat plate
        var tempMesh = MeshCreator.CreateBoxMesh(Vector3.zero, new Vector3((float)seat.PlateWidth, (float)seat.PlateThickness, (float)seat.PlateLength), Vector3.zero);
        MeshCreator.CreateBoxMesh(new Vector3(0, -(float)seat.PlateThickness / 2 - (float)seat.StiffenerLength / 2, 0), new Vector3((float)seat.StiffenerWidth, (float)seat.StiffenerLength, (float)seat.StiffenerThickness), Vector3.zero);

        MeshCreator.Add(tempMesh);

        //Move the origin
        MeshCreator.Translate(new Vector3(0, -(float)seat.PlateThickness / 2, 0));

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    private static Mesh CreateFabricatedTeeMesh(BCFabricatedTee tee, double y = 0, double z = 0, double? rotationAngle = null)
    {
        MeshCreator.Reset();

        var Y = (float)y;
        var Z = (float)z;
        //stem values
        var stemht = (float)tee.Height;     //height    
        var negstemtk = -(float)tee.Tw/2;   //thickness
        var posstemtk = (float)tee.Tw/2;  

        //flange values
        var flangetk = (float)tee.Tf;       //flange thickness   
        var negflangew = -(float)tee.bf/2;  //flange width  
        var posflangew = (float) tee.bf/2;

        //TODO: Use dummy values to test
        //stemht = 12f;

        //stem plate
        //front//
        MeshCreator.AddQuad(new Vector3(0, 0, posstemtk), new Vector3(0, Z, posstemtk), new Vector3(stemht, Z, posstemtk), new Vector3(stemht, 0, posstemtk));
        //back
        MeshCreator.AddQuad(new Vector3(stemht, 0, negstemtk), new Vector3(stemht, Z, negstemtk), new Vector3(0, Z, negstemtk), new Vector3(0, 0, negstemtk));
        //top
        MeshCreator.AddQuad(new Vector3(0, Z, posstemtk), new Vector3(0, Z, negstemtk), new Vector3(stemht, Z, negstemtk), new Vector3(stemht, Z, posstemtk));
        //bottom
        MeshCreator.AddQuad(new Vector3(0, 0, negstemtk), new Vector3(0, 0, posstemtk), new Vector3(stemht, 0, posstemtk), new Vector3(stemht, 0, negstemtk));
        //side a
        MeshCreator.AddQuad(new Vector3(0, 0, negstemtk), new Vector3(0, Z, negstemtk), new Vector3(0, Z, posstemtk), new Vector3(0, 0, posstemtk));
        //side b
        MeshCreator.AddQuad(new Vector3(stemht, 0, posstemtk), new Vector3(stemht, Z, posstemtk), new Vector3(stemht, Z, negstemtk), new Vector3(stemht, 0, negstemtk));

        //flange plate
        //front//
        MeshCreator.AddQuad(new Vector3(0, 0, posflangew), new Vector3(0, Z, posflangew), new Vector3(flangetk, Z, posflangew), new Vector3(flangetk, 0, posflangew));
        //back
        MeshCreator.AddQuad(new Vector3(flangetk, 0, negflangew), new Vector3(flangetk, Z, negflangew), new Vector3(0, Z, negflangew), new Vector3(0, 0, negflangew));
        //top
        MeshCreator.AddQuad(new Vector3(0, Z, posflangew), new Vector3(0, Z, negflangew), new Vector3(flangetk, Z, negflangew), new Vector3(flangetk, Z, posflangew));
        //bottom
        MeshCreator.AddQuad(new Vector3(0, 0, negflangew), new Vector3(0, 0, posflangew), new Vector3(flangetk, 0, posflangew), new Vector3(flangetk, 0, negflangew));
        //side a
        MeshCreator.AddQuad(new Vector3(0, 0, negflangew), new Vector3(0, Z, negflangew), new Vector3(0, Z, posflangew), new Vector3(0, 0, posflangew));
        //side b
        MeshCreator.AddQuad(new Vector3(flangetk, 0, posflangew), new Vector3(flangetk, Z, posflangew), new Vector3(flangetk, Z, negflangew), new Vector3(flangetk, 0, negflangew));

        if (rotationAngle != null && !((float)rotationAngle).Equals(0))
        {
            MeshCreator.Rotate(0, (double)rotationAngle, 0);
            MeshCreator.Translate(new Vector3(Y, 0, 0));
        }
        else MeshCreator.Translate(new Vector3(Y, 0, 0));            
        return MeshCreator.Create();

    }

    /// <summary>
    /// Creates a Seat mesh with the corner in the bottom left and the legs extending to the top and right
    /// Creates the seat with the long leg going upwards unless the flipLeg bool is true
    /// </summary>
    /// <param name="seat"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="rotationAngle"></param>
    /// <param name="flipLeg"></param>
    /// <returns></returns>
    public static Mesh CreateTopShearSeat(WCShearSeat seat, Vector3 position, Vector3 rotation, bool flipLeg = false)
    {
        // Support Side
        // | |
        // | |
        // | |
        // | |
        // | |______
        // |________|
        // Beam Side
        var thick = (float)seat.TopAngle.t * (float)ConstNum.METRIC_MULTIPLIER;
        var length = (float)seat.TopAngleLength * (float)ConstNum.METRIC_MULTIPLIER / 2;
        var sleg = (float)(flipLeg == false ? seat.TopAngleSupLeg * (float)ConstNum.METRIC_MULTIPLIER : seat.TopAngleBeamLeg * (float)ConstNum.METRIC_MULTIPLIER);
        var bleg = (float)(flipLeg == false ? seat.TopAngleBeamLeg * (float)ConstNum.METRIC_MULTIPLIER : seat.TopAngleSupLeg * (float)ConstNum.METRIC_MULTIPLIER);

        //Magic
        MeshCreator.Reset();
        MeshCreator.AddQuad(new Vector3(0, 0, -length), new Vector3(0, sleg, -length), new Vector3(thick, sleg, -length), new Vector3(thick, 0, -length));
        MeshCreator.AddQuad(new Vector3(thick, 0, -length), new Vector3(thick, thick, -length), new Vector3(bleg, thick, -length), new Vector3(bleg, 0, -length));

        MeshCreator.AddQuad(new Vector3(thick, 0, length), new Vector3(thick, sleg, length), new Vector3(0, sleg, length), new Vector3(0, 0, length));
        MeshCreator.AddQuad(new Vector3(bleg, 0, length), new Vector3(bleg, thick, length), new Vector3(thick, thick, length), new Vector3(thick, 0, length));

        MeshCreator.AddQuad(new Vector3(bleg, thick, length), new Vector3(bleg, thick, -length), new Vector3(thick, thick, -length), new Vector3(thick, thick, length));
        MeshCreator.AddQuad(new Vector3(thick, thick, -length), new Vector3(thick, sleg, -length), new Vector3(thick, sleg, length), new Vector3(thick, thick, length));

        MeshCreator.AddQuad(new Vector3(bleg, 0, -length), new Vector3(bleg, 0, length), new Vector3(0, 0, length), new Vector3(0, 0, -length));
        MeshCreator.AddQuad(new Vector3(bleg, 0, -length), new Vector3(bleg, thick, -length), new Vector3(bleg, thick, length), new Vector3(bleg, 0, length));

        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(0, sleg, length), new Vector3(0, sleg, -length), new Vector3(0, 0, -length));
        MeshCreator.AddQuad(new Vector3(0, sleg, -length), new Vector3(0, sleg, length), new Vector3(thick, sleg, length), new Vector3(thick, sleg, -length));

        //Rotate it around by 180 so it goes in a clockwise fashio around the right beam
        MeshCreator.Rotate(0, 180, 0);

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    public static Mesh CreateBottomShearSeat(WCShearSeat seat, Vector3 position, Vector3 rotation, bool flipLeg = false)
    {
        // Support Side
        // | |
        // | |
        // | |
        // | |
        // | |______
        // |________|
        // Beam Side
        var thick = (float)seat.Angle.t * (float)ConstNum.METRIC_MULTIPLIER;
        var length = (float)seat.AngleLength * (float)ConstNum.METRIC_MULTIPLIER / 2;
        var sleg = (float)(flipLeg == false ? seat.Angle.b * (float)ConstNum.METRIC_MULTIPLIER : seat.Angle.d * (float)ConstNum.METRIC_MULTIPLIER);
        var bleg = (float)(flipLeg == false ? seat.Angle.d * (float)ConstNum.METRIC_MULTIPLIER : seat.Angle.b * (float)ConstNum.METRIC_MULTIPLIER);

        //Magic
        MeshCreator.Reset();
        MeshCreator.AddQuad(new Vector3(0, 0, -length), new Vector3(0, sleg, -length), new Vector3(thick, sleg, -length), new Vector3(thick, 0, -length));
        MeshCreator.AddQuad(new Vector3(thick, 0, -length), new Vector3(thick, thick, -length), new Vector3(bleg, thick, -length), new Vector3(bleg, 0, -length));

        MeshCreator.AddQuad(new Vector3(thick, 0, length), new Vector3(thick, sleg, length), new Vector3(0, sleg, length), new Vector3(0, 0, length));
        MeshCreator.AddQuad(new Vector3(bleg, 0, length), new Vector3(bleg, thick, length), new Vector3(thick, thick, length), new Vector3(thick, 0, length));

        MeshCreator.AddQuad(new Vector3(bleg, thick, length), new Vector3(bleg, thick, -length), new Vector3(thick, thick, -length), new Vector3(thick, thick, length));
        MeshCreator.AddQuad(new Vector3(thick, thick, -length), new Vector3(thick, sleg, -length), new Vector3(thick, sleg, length), new Vector3(thick, thick, length));

        MeshCreator.AddQuad(new Vector3(bleg, 0, -length), new Vector3(bleg, 0, length), new Vector3(0, 0, length), new Vector3(0, 0, -length));
        MeshCreator.AddQuad(new Vector3(bleg, 0, -length), new Vector3(bleg, thick, -length), new Vector3(bleg, thick, length), new Vector3(bleg, 0, length));

        MeshCreator.AddQuad(new Vector3(0, 0, length), new Vector3(0, sleg, length), new Vector3(0, sleg, -length), new Vector3(0, 0, -length));
        MeshCreator.AddQuad(new Vector3(0, sleg, -length), new Vector3(0, sleg, length), new Vector3(thick, sleg, length), new Vector3(thick, sleg, -length));

        //Rotate it around by 180 so it goes in a clockwise fashio around the right beam
        MeshCreator.Rotate(0, 180, 0);

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
    }

    private static Mesh CreateSeatMesh(WCShearSeat seat, double y = 0, double z = 0, bool isStiffener = false, double? rotationAngle = null)
    {
        MeshCreator.Reset();

        var Y = (float)y;
        var Z = (float)z;

        var platedepth = (float)seat.TopAngleLength;
        var posplatedepth = platedepth / 2;
        var negplatedepth = -platedepth/2;
        var platelen = (float)seat.TopAngleBeamLeg;
        var columnlen = (float)seat.TopAngleSupLeg;
        var platetk = (float)seat.TopAngle.kdet;
        var posplatetk = platetk / 2;
        var negplatetk = -platetk / 2;

        var stiffenerlen = (float)seat.StiffenerLength;
        var stiffenertk = (float) seat.StiffenerThickness;
        var postiffenertk = (float)seat.StiffenerThickness / 2;
        var negstiffenertk = -(float)seat.StiffenerThickness / 2;
        var stiffenerdepth = (float)seat.StiffenerWidth;
        var posstiffenerdepth = stiffenerdepth / 2;
        var negstiffenerdepth = -stiffenerdepth / 2;

        if (!isStiffener)
        {
            //vertical plate (attached at columm)
            //front//
            MeshCreator.AddQuad(new Vector3(posplatetk, 0, posplatedepth), new Vector3(posplatetk, columnlen, posplatedepth), new Vector3(negplatetk, columnlen, posplatedepth), new Vector3(negplatetk, 0, posplatedepth));
            //back
            MeshCreator.AddQuad(new Vector3(negplatetk, 0, negplatedepth), new Vector3(negplatetk, columnlen, negplatedepth), new Vector3(posplatetk, columnlen, negplatedepth), new Vector3(posplatetk, 0, negplatedepth));
            //top
            MeshCreator.AddQuad(new Vector3(posplatetk, columnlen, posplatedepth), new Vector3(posplatetk, columnlen, negplatedepth), new Vector3(negplatetk, columnlen, negplatedepth), new Vector3(negplatetk, columnlen, posplatedepth));
            //bottom
            MeshCreator.AddQuad(new Vector3(negplatetk, 0, posplatedepth), new Vector3(negplatetk, 0, negplatedepth), new Vector3(posplatetk, 0, negplatedepth), new Vector3(posplatetk, 0, posplatedepth));
            //column side
            MeshCreator.AddQuad(new Vector3(negplatetk, 0, posplatedepth), new Vector3(negplatetk, columnlen, posplatedepth), new Vector3(negplatetk, columnlen, negplatedepth), new Vector3(negplatetk, 0, negplatedepth));
            //outer side
            MeshCreator.AddQuad(new Vector3(posplatetk, 0, negplatedepth), new Vector3(posplatetk, columnlen, negplatedepth), new Vector3(posplatetk, columnlen, posplatedepth), new Vector3(posplatetk, 0, posplatedepth));

            //horizontal plate (attached at beam)
            //front//
            MeshCreator.AddQuad(new Vector3(platelen, 0, posplatedepth), new Vector3(platelen, platetk, posplatedepth), new Vector3(posplatetk, platetk, posplatedepth), new Vector3(posplatetk, 0, posplatedepth));
            //back
            MeshCreator.AddQuad(new Vector3(posplatetk, 0, negplatedepth), new Vector3(posplatetk, platetk, negplatedepth), new Vector3(platelen, platetk, negplatedepth), new Vector3(platelen, 0, negplatedepth));
            //top
            MeshCreator.AddQuad(new Vector3(platelen, platetk, posplatedepth), new Vector3(platelen, platetk, negplatedepth), new Vector3(posplatetk, platetk, negplatedepth), new Vector3(posplatetk, platetk, posplatedepth));
            //bottom
            MeshCreator.AddQuad(new Vector3(posplatetk, 0, posplatedepth), new Vector3(posplatetk, 0, negplatedepth), new Vector3(platelen, 0, negplatedepth), new Vector3(platelen, 0, posplatedepth));
            //seat wall side
            MeshCreator.AddQuad(new Vector3(posplatetk, 0, posplatedepth), new Vector3(posplatetk, platetk, posplatedepth), new Vector3(posplatetk, platetk, negplatedepth), new Vector3(posplatetk, 0, negplatedepth));
            //outer side
            MeshCreator.AddQuad(new Vector3(platelen, 0, negplatedepth), new Vector3(platelen, platetk, negplatedepth), new Vector3(platelen, platetk, posplatedepth), new Vector3(platelen, 0, posplatedepth));

            //Move the center of the seat to the actual center
            MeshCreator.Translate(new Vector3(0, -posplatetk, 0));
        }
        else
        {
            //stiffener (attached at columm)
            //front//
            MeshCreator.AddQuad(new Vector3(postiffenertk, 0, posstiffenerdepth), new Vector3(postiffenertk, stiffenerlen, posstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, posstiffenerdepth), new Vector3(negstiffenertk, 0, posstiffenerdepth));
            //back
            MeshCreator.AddQuad(new Vector3(negstiffenertk, 0, negstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(postiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(postiffenertk, 0, negstiffenerdepth));
            //top
            MeshCreator.AddQuad(new Vector3(postiffenertk, stiffenerlen, posstiffenerdepth), new Vector3(postiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, posstiffenerdepth));
            //bottom
            MeshCreator.AddQuad(new Vector3(postiffenertk, 0, negstiffenerdepth), new Vector3(postiffenertk, 0, posstiffenerdepth), new Vector3(negstiffenertk, 0, posstiffenerdepth), new Vector3(negstiffenertk, 0, negstiffenerdepth));
            //side a
            MeshCreator.AddQuad(new Vector3(negstiffenertk, 0, posstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, posstiffenerdepth), new Vector3(negstiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(negstiffenertk, 0, negstiffenerdepth));
            //side b
            MeshCreator.AddQuad(new Vector3(postiffenertk, 0, negstiffenerdepth), new Vector3(postiffenertk, stiffenerlen, negstiffenerdepth), new Vector3(postiffenertk, stiffenerlen, posstiffenerdepth), new Vector3(postiffenertk, 0, posstiffenerdepth));
        }

        MeshCreator.Rotate(0, 0, (double) rotationAngle);
        //MeshCreator.Translate(new Vector3(Y, Z, 0));
        //if (rotationAngle != null && rotationAngle != 0)
        //{
        //    var isRightBeam = rotationAngle == 90 || rotationAngle == 180;
        //    var isLowerQuad = rotationAngle == 270 || rotationAngle == 180;
        //    MeshCreator.Rotate(0, 0, (double)rotationAngle);
        //    MeshCreator.Translate(new Vector3(-Y + (isRightBeam ? -1 : 1) * (platetk), (rotationAngle == 90 ? Z : -Z) + (isLowerQuad ? -1 : 1) * platetk / 2, 0));
        //}
        //else 
        MeshCreator.Translate(new Vector3(Y + (isStiffener ? Math.Sign(Y) * (stiffenertk / 2) : Math.Sign(Y) * (platetk)), Z + (isStiffener ? -stiffenerlen : platetk / 2), 0));
        return MeshCreator.Create();
            
        //return MeshCreator.Create();
    }

    /// <summary>
    /// Create the bot flange plate mesh shape.
    /// </summary>
    /// <param name="plate"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <param name="isLeftBeam"></param>
    /// <returns></returns>
    public static Mesh CreateFlangePlateMesh(WCMomentFlangePlate plate, Vector3 position, bool isTop = true, bool isLeftBeam = false)
    {
        var dim = new Vector3((float)(isTop ? plate.TopLength : plate.BottomLength) * (float)ConstNum.METRIC_MULTIPLIER,
            (float)(isTop ? plate.TopThickness : plate.BottomThickness) * (float)ConstNum.METRIC_MULTIPLIER,
            (float)(isTop ? plate.TopWidth : plate.BottomWidth) * (float)ConstNum.METRIC_MULTIPLIER);

        if (plate.PlateToSupportWeldType == EWeldType.CJP)
        {
            dim /= 2.0f;
            MeshCreator.Reset();
            //Modify single face of mesh creator
            if (isLeftBeam)
            {
                MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, -dim.z));
                    
                //Change affected faces
                var dimx2 = dim.x - dim.y;
                MeshCreator.AddQuad(new Vector3(dimx2, dim.y, -dim.z), new Vector3(dimx2, dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(dimx2, dim.y, dim.z), new Vector3(dimx2, dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dimx2, dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z));
                MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(dimx2, dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
            }
            else
            {
                MeshCreator.AddQuad(new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, -dim.z));
                var dimx2 = -(dim.x - dim.y);
                MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dimx2, dim.y, dim.z), new Vector3(dimx2, dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dimx2, -dim.y, dim.z), new Vector3(dimx2, -dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dimx2, dim.y, dim.z));
                MeshCreator.AddQuad(new Vector3(dimx2, dim.y, -dim.z), new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
                MeshCreator.AddQuad(new Vector3(dimx2, dim.y, -dim.z), new Vector3(dimx2, dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, dim.y, -dim.z));
            }

            MeshCreator.Translate(position);
            return MeshCreator.Create();
        }
        return MeshCreator.CreateBoxMesh(position, dim, Vector3.zero);
    }

    /// <summary>
    /// Create the gusset meshs shape.
    /// </summary>
    /// <param name="plate"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static Mesh CreateGussetPlateMesh(BCGussetPlate plate, double x = 0, double y = 0)
    {
        var X = (float) x;
        var Y = (float) y;
        var posZ = (float)plate.Thickness * (float)ConstNum.METRIC_MULTIPLIER / 2;
        var negZ = -(float)plate.Thickness * (float)ConstNum.METRIC_MULTIPLIER / 2;
        var l2 = (float)plate.EdgeDistance * (float)ConstNum.METRIC_MULTIPLIER; //Used for bolt distances
        var l = (float)plate.Length * (float)ConstNum.METRIC_MULTIPLIER;
        var cs = (float)plate.ColumnSide * (float)ConstNum.METRIC_MULTIPLIER;
        var csx = (float)plate.ColumnSideFreeEdgeX * (float)ConstNum.METRIC_MULTIPLIER;
        var csy = (float)plate.ColumnSideFreeEdgeY * (float)ConstNum.METRIC_MULTIPLIER;
        var bs = (float)plate.BeamSide * (float)ConstNum.METRIC_MULTIPLIER;
        var bsx = (float)plate.BeamSideFreeEdgeX * (float)ConstNum.METRIC_MULTIPLIER;
        var bsy = (float)plate.BeamSideFreeEdgeY * (float)ConstNum.METRIC_MULTIPLIER;
        var brs = (float)plate.BraceSide * (float)ConstNum.METRIC_MULTIPLIER;

        //TODO: Values provided by Will for specific case should be removed once calculations are corrected
        posZ = (float)0.5/2;
        negZ = -(float)0.5/2;
        cs = 17f;
        csx = 14.47f;
        csy = 1.3275f;
        bs = 26.75f;
        bsx = 6.2725f;
        bsy = 9.6621f;
        brs = 20.0f;

        MeshCreator.Reset();

        //calculate coordinates for points 3 and 4 (going clockwise and starting at column + beam origin point)
        var pt3x = csx;
        var pt3y = (float) (cs - Math.Sqrt(Math.Pow((csx/Math.Cos(Math.Atan(csy/csx))), 2) - Math.Pow(csx, 2)));
        var pt4y = bsy;
        var pt4x = (float) (bs - (Math.Sqrt(Math.Pow((bsy/Math.Cos(Math.Atan(bsx/bsy))), 2) - Math.Pow(bsy, 2))));

        //col quad front//
        MeshCreator.AddQuad(new Vector3(pt4x, pt4y, posZ), new Vector3(pt3x, pt3y, posZ), new Vector3(0, cs, posZ), 
            new Vector3(0, pt4y, posZ));
        //beam quad front//
        MeshCreator.AddQuad(new Vector3(bs, 0, posZ), new Vector3(pt4x, pt4y, posZ), new Vector3(0, pt4y, posZ), 
            new Vector3(0, 0, posZ));
        //column quad back//
        MeshCreator.AddQuad(new Vector3(0, pt4y, negZ), new Vector3(0, cs, negZ), new Vector3(pt3x, pt3y, negZ),
            new Vector3(pt4x, pt4y, negZ));
        //beam quad back//
        MeshCreator.AddQuad(new Vector3(0, 0, negZ), new Vector3(0, pt4y, negZ), new Vector3(pt4x, pt4y, negZ),
            new Vector3(bs, 0, negZ));
        //col triangle edge face 1
        MeshCreator.AddQuad(new Vector3(0, 0, posZ), new Vector3(0, cs, posZ), new Vector3(0, cs, negZ), new Vector3(0, 0, negZ));
        //col triangle edge face 2
        MeshCreator.AddQuad(new Vector3(0, cs, posZ), new Vector3(pt3x, pt3y, posZ), new Vector3(pt3x, pt3y, negZ),
            new Vector3(0, cs, negZ));
        //brace triangle edge face
        MeshCreator.AddQuad(new Vector3(pt3x, pt3y, posZ), new Vector3(pt4x, pt4y, posZ), new Vector3(pt4x, pt4y, negZ),
            new Vector3(pt3x, pt3y, negZ));
        //free triangle edge face
        MeshCreator.AddQuad(new Vector3(pt4x, pt4y, posZ), new Vector3(bs, 0, posZ), new Vector3(bs, 0, negZ),
            new Vector3(pt4x, pt4y, negZ));
        //beam triangle edge face
        MeshCreator.AddQuad(new Vector3(0, 0, negZ), new Vector3(bs, 0, negZ), new Vector3(bs, 0, posZ), new Vector3(0, 0, posZ));

        return MeshCreator.Create();
    }

    /// <summary>
    /// Create the slipce plate mesh shape.
    /// </summary>
    /// <param name="plate"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    private static Mesh CreateSplicePlateMesh(BCSplicePlates plate, double y = 0, double z = 0)
    {
        var Y = (float) y;
        var t = (float)plate.Thickness * (float)ConstNum.METRIC_MULTIPLIER;
        var Z = (float) z;
        var l = (float)plate.Length * (float)ConstNum.METRIC_MULTIPLIER;

        MeshCreator.Reset();

        //front//
        MeshCreator.AddQuad(new Vector3(0, 0, 0), new Vector3(0, l, 0), new Vector3(0, l, Z), new Vector3(0, 0, Z));
        //back
        MeshCreator.AddQuad(new Vector3(t, 0, Z), new Vector3(t, l, Z), new Vector3(t, l, 0), new Vector3(t, 0, 0));
        //top
        MeshCreator.AddQuad(new Vector3(0, l, 0), new Vector3(t, l, 0), new Vector3(t, l, Z), new Vector3(0, l, Z));
        //bottom
        MeshCreator.AddQuad(new Vector3(t, 0, 0), new Vector3(0, 0, 0), new Vector3(0, 0, Z), new Vector3(t, 0, Z));
        //side a
        MeshCreator.AddQuad(new Vector3(0, 0, Z), new Vector3(0, l, Z), new Vector3(t, l, Z), new Vector3(t, 0, Z));
        //side b
        MeshCreator.AddQuad(new Vector3(t, 0, 0), new Vector3(t, l, 0), new Vector3(0, l, 0), new Vector3(0, 0, 0));

        //MeshCreator.Rotate(0, -90, 0);
        MeshCreator.Translate(new Vector3(Y, -l/2, -Z/2));
        return MeshCreator.Create();
    }

    private static Mesh CreateColumnStiffeners(double y = 0, double z = 0, List<StiffenerFlangeData> plates = null)
    {
        var Y = (float)y;
        var Z = (float)z;
        var stiffener = CommonDataStatic.ColumnStiffener;
        var width = (float)stiffener.SWidth;
        var length = (float)stiffener.SLength;
        var thickness = (float)stiffener.SThickness;
        thickness = 0.5f;

        var ySign = Math.Sign(y);

        MeshCreator.Reset();
        if (plates != null)
        {
            var isbottom = plates.OrderBy(f => f.yOffsetFromOrigin).First();

            var leftYCoord = (float)plates.First(pt => pt.Member == EMemberType.LeftBeam).yOffsetFromOrigin + length;
            var rightYCoord = (float)plates.First(pt => pt.Member == EMemberType.RightBeam).yOffsetFromOrigin + length;

            //Top
            MeshCreator.AddQuad(new Vector3(width, leftYCoord, thickness), new Vector3(-width, leftYCoord, thickness),
                new Vector3(-width, leftYCoord, thickness), new Vector3(width, leftYCoord, thickness));
            //Front
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(-width, -length, thickness),
                new Vector3(-width, leftYCoord, thickness), new Vector3(width, rightYCoord, thickness));
            //Back
            MeshCreator.AddQuad(new Vector3(-width, -length, -thickness), new Vector3(width, -length, -thickness),
                new Vector3(width, rightYCoord, -thickness), new Vector3(-width, leftYCoord, -thickness));
            //Left
            MeshCreator.AddQuad(new Vector3(-width, -length, -thickness), new Vector3(-width, leftYCoord, -thickness),
                new Vector3(-width, leftYCoord, thickness), new Vector3(-width, -length, thickness));
            //Right
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(width, rightYCoord, thickness),
                new Vector3(width, rightYCoord, -thickness), new Vector3(width, -length, -thickness));

            //Bottom
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(width, -length, -thickness),
                new Vector3(-width, -length, -thickness), new Vector3(-width, -length, thickness));
        }
        else
        {
            ////Top
            //MeshCreator.AddQuad(new Vector3(width, length, thickness), new Vector3(-width, length, thickness),
            //    new Vector3(-width, length, thickness), new Vector3(width, length, thickness));
            ////Bottom
            //MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(-width, -length, thickness),
            //    new Vector3(-width, -length, thickness), new Vector3(width, -length, thickness));
            ////Front
            //MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(-width, -length, thickness),
            //    new Vector3(-width, length, thickness), new Vector3(width, length, thickness));
            ////Back
            //MeshCreator.AddQuad(new Vector3(-width, -length, thickness), new Vector3(width, -length, thickness),
            //    new Vector3(width, length, thickness), new Vector3(-width, length, thickness));
            ////Left
            //MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(-width, -length, thickness),
            //    new Vector3(-width, length, thickness), new Vector3(-width, length, thickness));
            ////Right
            //MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(width, -length, thickness),
            //    new Vector3(width, length, thickness), new Vector3(width, length, thickness));

            //Top
            MeshCreator.AddQuad(new Vector3(width, length, thickness), new Vector3(-width, length, thickness),
                new Vector3(-width, length, thickness), new Vector3(width, length, thickness));
            //Front
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(-width, -length, thickness),
                new Vector3(-width, length, thickness), new Vector3(width, length, thickness));
            //Back
            MeshCreator.AddQuad(new Vector3(-width, -length, -thickness), new Vector3(width, -length, -thickness),
                new Vector3(width, length, -thickness), new Vector3(-width, length, -thickness));
            //Left
            MeshCreator.AddQuad(new Vector3(-width, -length, -thickness), new Vector3(-width, length, -thickness),
                new Vector3(-width, length, thickness), new Vector3(-width, -length, thickness));
            //Right
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(width, length, thickness),
                new Vector3(width, length, -thickness), new Vector3(width, -length, -thickness));
            //Bottom
            MeshCreator.AddQuad(new Vector3(width, -length, thickness), new Vector3(width, -length, -thickness),
                new Vector3(-width, -length, -thickness), new Vector3(-width, -length, thickness));
        }

        //MeshCreator.Rotate(0, -90, 0);
        MeshCreator.Translate(new Vector3(0, Y, Z));
        return MeshCreator.Create();
    }

    public void CreateMeshComponent(DetailData component, double x = 0, double y = 0,
        double rotationAngle = 0, EBraceConnectionTypes bctype = EBraceConnectionTypes.DirectlyWelded, bool conBool = false, EMomentCarriedBy momentConnection = EMomentCarriedBy.NoMoment)
    {
        Reset();
        SetTopElAndYOffset(component);

        if (component.IsActive)
        {
            if (component.Shape == null) return;

            if (momentConnection == EMomentCarriedBy.Tee)
            {
                DrawMomentTee(component, x, y, conBool);
            }
            else
            {
                switch (bctype)
                {
                    case EBraceConnectionTypes.ClipAngle:
                        if (momentConnection == EMomentCarriedBy.Angles)
                            DrawMomentAngle(component, x, y);
                        else
                            DrawClipAngle(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.SinglePlate:
                        DrawSinglePlate(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.EndPlate:
                        DrawEndPlate(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.FabricatedTee:
                        DrawTee(component, x, y, conBool, rotationAngle);
                        break;
                    case EBraceConnectionTypes.Seat:
                        DrawSeat(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.SplicePlate:
                        DrawSplicePlate(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.FlangePlate:
                        DrawFlangePlate(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.Stiffener:
                        DrawStiffener(component, x, y, conBool);
                        break;
                    case EBraceConnectionTypes.GussetPlate:
                        DrawGussetPlate(component, x, y, conBool);
                        break;
                    default:
                        DrawBeams(component, x, y, conBool, rotationAngle);
                        break;
                }
            }

            AddMember(component, x, updateBaseMesh);

            if (_gussetCreatorCallback != null) _gussetCreatorCallback();
        }
    }

    private void AddMember(DetailData component, double x, bool updateBaseMesh)
    {

        //Get the member object
        var meshObject = MessageQueueTest.GetMemberControl(component.MemberType);
        meshObject.GetComponent<MemberControl>().memberType = component.MemberType;

        if (component.MemberType != EMemberType.PrimaryMember)
        {
            //Remove from left/right views
            var mask = ViewMask.D3 | ViewMask.FRONT | ViewMask.TOP;

            if (Math.Sign(-x) > 0)
                mask |= ViewMask.LEFT;
            else
                mask |= ViewMask.RIGHT;

            meshObject.GetComponent<MemberControl>().viewMask = mask;
        }
        else
        {
            meshObject.GetComponent<MemberControl>().viewMask = defaultMask;
        }

        if (!useCustomMask)
        {
            customMask = defaultMask;
        }

        if (updateBaseMesh)
        {
            meshObject.GetComponent<MeshFilter>().sharedMesh = mesh;
            meshObject.GetComponent<MeshCollider>().sharedMesh = mesh;
            if (MeshOn) meshObject.GetComponent<MeshFilter>().sharedMesh = ConvertToWireframe(meshObject.GetComponent<MeshFilter>().sharedMesh);
        }

        //Add the connection meshes to the meshObject
        var materialIndex = 0;

        var index = 0;
        foreach (Mesh connMes in connectionMeshes)
        {
            var connMesh = MeshOn? ConvertToWireframe(connMes):connMes;
            if (connectionMats.Count == 0)
            {
                UnityEngine.Debug.Log("No connection material assigned!");
            }
            else
            {
                if (materialIndex < connectionMats.Count)
                {
                    var mask = customMask;

                    if (useCustomMaskList && index < customMaskList.Count)
                    {
                        mask = customMaskList[index];
                    }

                    if(useCustomConnLines)
                        meshObject.GetComponent<MemberControl>().AddConnection(connMesh, connectionMats[materialIndex], component.MemberType, subType, mask, customConnLines[index], customConnLineOffset);
                    else
                        meshObject.GetComponent<MemberControl>().AddConnection(connMesh, connectionMats[materialIndex], component.MemberType, subType, mask);
                }
                else
                {
                    UnityEngine.Debug.Log("Invalid material number!");

                    if (useCustomConnLines)
                        meshObject.GetComponent<MemberControl>().AddConnection(connMesh, connectionMats[0], component.MemberType, subType, customMask, customConnLines[index], customConnLineOffset);
                    else
                        meshObject.GetComponent<MemberControl>().AddConnection(connMesh, connectionMats[0], component.MemberType, subType, customMask);
                }

                materialIndex++;
            }

            index++;
        }

        index = 0;
        //Add bolt connections
        foreach (Mesh boltMesh in boltMeshes)
        {
            var mask = defaultMask;

            if (useCustomMaskList && index < customBoltMaskList.Count)
            {
                mask = customBoltMaskList[index];
            }

            meshObject.GetComponent<MemberControl>().AddConnection(boltMesh, MeshCreator.GetBoltMaterial(), component.MemberType, isShearBolt ? EMemberSubType.BoltShearBeam : EMemberSubType.BoltMomentBeam, mask);

            index++;
        }

        if (useCustomLines)
        {
            var memberControl = meshObject.GetComponent<MemberControl>();
            memberControl.customLines.Clear();

            foreach(var item in customLines)
            {
                memberControl.customLines[item.Key] = new List<CustomLine>(item.Value);
            }

            memberControl.useCustomLines = true;
            memberControl.customLineOffset = customLineOffset;
        }
    }

    private static Action _gussetCreatorCallback;
    static Action CreateGussetMesh(DetailData component)
    {
        _gussetCreatorCallback = null;
        instance.CreateMeshComponent(component, bctype: EBraceConnectionTypes.GussetPlate);
        return null;
    }

    public static float GetDottedLineOffset(double beamLv, double beamHeight, double componentHeight, double boltEdgeLong, EPosition position)
    {
        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
            beamHeight = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.USShape.d;

        var result = 0.0f;

        switch (position)
        {
            case EPosition.Top:
                result = (float)((beamHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.Bottom:
                result = (float)((beamHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.Center:
                result = (float)((beamHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.MatchOtherSideBolts:
                result = (float)((beamHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.NoConnection:
                result = (float)((beamHeight / 2) - (beamLv - boltEdgeLong));
                break;
        }

        return result;
    }

    public static float GetPositionOffset(double beamLv, double beamHeight, double componentHeight, double boltEdgeLong, EPosition position)
    {
        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
            beamHeight = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.USShape.d;

        var result = 0.0f;
        switch(position)
        {
            case EPosition.Top:
                result = (float)((beamHeight / 2 - componentHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.Bottom:
                result = (float)((beamHeight / 2 - componentHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.Center:
                result = (float)((beamHeight / 2 - componentHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.MatchOtherSideBolts:
                result = (float)((beamHeight / 2 - componentHeight / 2) - (beamLv - boltEdgeLong));
                break;

            case EPosition.NoConnection:
                result = (float)((beamHeight / 2 - componentHeight / 2) - (beamLv - boltEdgeLong));
                break;
        }

        return result;
    }

    public static void CreateShapeDottedLineGirder(Dictionary<ViewMask, List<CustomLine>> customLines, ViewMask viewMask, DetailData component, EShearCarriedBy shearConnection, float x, float y, float yOffset)
    {
        var shapeSize = 0.0f;
        var p = 0.0f;
        var cope = (float)component.WinConnect.Beam.TCopeD;
        var botCope = (float)component.WinConnect.Beam.BCopeD;
        var y2 = y;

        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
        {
            yOffset = GetTopEl(component);
            y = (float)CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.USShape.d / 2;
        }

        switch (shearConnection)
        {
            case EShearCarriedBy.SinglePlate:
                var shape = component.WinConnect.ShearWebPlate;
                shapeSize = (float)component.WinConnect.ShearWebPlate.Length * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, shape.Bolt.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, shape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset - cope, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y2 + yOffset + DrawingMethods.GirderOffset(component) + botCope, 0), 0.5f, 0.5f));
                break;

            case EShearCarriedBy.ClipAngle:
                var clipShape = component.WinConnect.ShearClipAngle;
                shapeSize = (float)component.WinConnect.ShearClipAngle.Length * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, clipShape.BoltWebOnBeam.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, clipShape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset - cope, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y2 + yOffset + DrawingMethods.GirderOffset(component) + botCope, 0), 0.5f, 0.5f));
                break;

            case EShearCarriedBy.Tee:
                var teeShape = component.WinConnect.ShearWebTee;
                shapeSize = (float)component.WinConnect.ShearWebTee.SLength * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, teeShape.StemEdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, teeShape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset - cope, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y2 + yOffset + DrawingMethods.GirderOffset(component) + botCope, 0), 0.5f, 0.5f));
                break;

            default:
                customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(x, y + yOffset, 0), new Vector3(x, -y + yOffset)));
                break;
        }
    }

    public static void CreateShapeDottedLine(Dictionary<ViewMask, List<CustomLine>> customLines, ViewMask viewMask, DetailData component, EShearCarriedBy shearConnection, float x, float y, float yOffset)
    {
        var shapeSize = 0.0f;
        var p = 0.0f;

        switch (shearConnection)
        {
            case EShearCarriedBy.SinglePlate:
                var shape = component.WinConnect.ShearWebPlate;
                shapeSize = (float)component.WinConnect.ShearWebPlate.Length * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, shape.Bolt.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, shape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y + yOffset, 0), 0.5f, 0.5f));
            break;

            case EShearCarriedBy.ClipAngle:
                var clipShape = component.WinConnect.ShearClipAngle;
                shapeSize = (float)component.WinConnect.ShearClipAngle.Length * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, clipShape.BoltWebOnBeam.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, clipShape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y + yOffset, 0), 0.5f, 0.5f));
            break;

            case EShearCarriedBy.Tee:
                var teeShape = component.WinConnect.ShearWebTee;
                shapeSize = (float)component.WinConnect.ShearWebTee.SLength * (float)ConstNum.METRIC_MULTIPLIER;
                p = GetDottedLineOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, shapeSize, teeShape.StemEdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, teeShape.Position) + yOffset;
                customLines[viewMask].Add(CustomLine.CreateIntersectLine(new Vector3(x, y + yOffset, 0),
                    new Vector3(x, p, 0),
                    new Vector3(x, p - shapeSize, 0),
                    new Vector3(x, -y + yOffset, 0), 0.5f, 0.5f));
            break;

            default:
            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(x, y + yOffset, 0), new Vector3(x, -y + yOffset)));
            break;
        }
    }

    public static float GetBoltPositionOffset(double beamLv, double beamHeight, double componentHeight, double boltEdgeLong, EPosition position)
    {
        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
            beamHeight = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember].Shape.USShape.d;

        var result = 0.0f;
        beamLv *= (float)ConstNum.METRIC_MULTIPLIER;
        componentHeight *= (float)ConstNum.METRIC_MULTIPLIER;
        boltEdgeLong *= (float)ConstNum.METRIC_MULTIPLIER; 

        switch (position)
        {
            case EPosition.Top:
                result = (float)(beamHeight / 2 - beamLv);
                break;

            case EPosition.Bottom:
                result = (float)(beamHeight / 2 - beamLv);
                break;

            case EPosition.Center:
                result = (float)(beamHeight / 2 - beamLv);
                break;

            case EPosition.MatchOtherSideBolts:
                result = (float)(beamHeight / 2 - beamLv);
                break;

            case EPosition.NoConnection:
                result = (float)(beamHeight / 2 - beamLv);
                break;
        }

        return result;
    }

    public static string GetUnitString()
    {
        if (CommonDataStatic.Units == EUnit.US)
        {
            return "in.";
        }
        else
        {
            return "mm";
        }
    }

    public static string GetFootStringSimple(float inches)
    {
        if (CommonDataStatic.Units == EUnit.US)
        {
            //Figure out what the value would be in feet
            var feet = (int)(inches / 12);
            var inches2 = inches % 12;
            var inch = (int)inches2;
            var fraction = inches2 - inch;

            if (fraction != 0)
            {
                fraction *= 16;
            }

            var result = "";

            if (feet > 0)
                result += feet.ToString() + "\'";

            if (inch > 0)
            {
                if (feet > 0)
                    result += " ";

                result += inch.ToString();
            }

            if (fraction > 0)
            {
                if (inch > 0)
                {
                    result += " ";
                }

                result += GetFraction(fraction, 16, true);
            }

            return result;
        }
        else
        {
            return inches.ToString() + "mm";
        }
    }

    public static string GetFootString(float inches)
    {
        inches = (float)decimal.Round((decimal)inches, 5, MidpointRounding.AwayFromZero);

        if (CommonDataStatic.Units == EUnit.US)
        {
            //Figure out what the value would be in feet
            var feet = (int)(inches / 12);
            var inches2 = inches % 12;
            var inch = (int)inches2;
            var fraction = inches2 - inch;

            if (fraction != 0)
            {
                fraction *= 16;
                fraction = Mathf.Floor(fraction);
            }

            var result = "";

            if (feet > 0)
                result += feet.ToString() + "\'";

            if (inch > 0)
            {
                if (feet > 0)
                    result += " - ";

                result += inch.ToString();

                if (fraction == 0)
                {
                    result += "\"";
                }
            }
            else if(inch == 0)
            {
                if (feet > 0)
                    result += " - ";

                if (fraction == 0)
                {
                    result += "0\"";
                }
            }

            if (fraction > 0)
            {
                if (inch > 0)
                {
                    result += " ";
                }

                result += GetFraction(fraction, 16) + "\"";
            }

            return result;
        }
        else
        {
            return inches.ToString() + "mm";
        }
    }

    public static string GetFraction(float numerator, float denominator, bool condensed = true)
    {
        for (int i = 0; i < 4; ++i)
        {
            if (numerator % 2 == 0 && denominator % 2 == 0)
            {
                numerator /= 2;
                denominator /= 2;
            }
            else
                break;
        }

        return numerator.ToString() + (condensed ? "/" : " / ") + denominator.ToString();
    }

    public static double GetPrimaryThickness()
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var x = 1.0;

        if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                x = primaryComponent.Shape.USShape.tnom;
            }
            else
            {
                if (!isPrimaryInPlane)
                    x = (float)(primaryComponent.Shape.USShape.tf);
                else
                    x = (float)(primaryComponent.Shape.USShape.tw);
            }
        }
        else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
        {
            return 0.0;
        }
        else
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                x = primaryComponent.Shape.USShape.tnom;
            }
            else
            {
                if (isPrimaryInPlane)
                    x = (float)(primaryComponent.Shape.USShape.tf);
                else
                    x = (float)(primaryComponent.Shape.USShape.tw);
            }
        }

        return x;
    }

    public static double GetInitialBeamLength()
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var x = 1.0;

        if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                x = Math.Max(primaryComponent.Shape.USShape.Ht, primaryComponent.Shape.USShape.B);
            }
            else
            {
                x = Math.Max(primaryComponent.Shape.USShape.d, primaryComponent.Shape.USShape.tw);
            }
        }
        else if (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
        {
            return 20.0;
        }
        else
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                x = Math.Max(primaryComponent.Shape.USShape.Ht, primaryComponent.Shape.USShape.B);
            }
            else
            {
                x = Math.Max(primaryComponent.Shape.USShape.d, primaryComponent.Shape.USShape.bf);
            }
        }

        return x;
    }

    public static double GetPrimaryWidth()
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var x = 1.0;

        if(CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder)
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                if (!isPrimaryInPlane)
                    x = primaryComponent.Shape.USShape.Ht / 2;
                else
                    x = primaryComponent.Shape.USShape.B / 2;
            }
            else
            {
                if (!isPrimaryInPlane)
                    x = (float)(primaryComponent.Shape.USShape.d / 2.0f * Math.Sign(x));
                else
                    x = (float)(primaryComponent.Shape.USShape.tw / 2.0f * Math.Sign(x));
            }
        }
        else if(CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamSplice)
        {
            return 0.00000001f;
        }
        else
        {
            if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                if (isPrimaryInPlane)
                    x = primaryComponent.Shape.USShape.Ht / 2;
                else
                    x = primaryComponent.Shape.USShape.B / 2;
            }
            else
            {
                if (isPrimaryInPlane)
                    x = (float)(primaryComponent.Shape.USShape.d / 2.0f * Math.Sign(x));
                else
                    x = (float)(primaryComponent.Shape.USShape.tw / 2.0f * Math.Sign(x));
            }
        }

        return x;
    }

    public static bool BothSidesActive()
    {
        return (CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive && CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive);
    }

    public void Draw()
    {
        //Draw the meshes for both Braces and Win
        //Most stuff is win except for when dealing with the braces

        //BeamToColumnType
        //Joint Configuration
        var primaryMember = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var rightMember = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        var leftMember = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
        var upperRightMember = CommonDataStatic.DetailDataDict[EMemberType.UpperRight];
        var lowerRightMember = CommonDataStatic.DetailDataDict[EMemberType.LowerRight];
        var upperLeftMember = CommonDataStatic.DetailDataDict[EMemberType.UpperLeft];
        var lowerLeftMember = CommonDataStatic.DetailDataDict[EMemberType.LowerLeft];

        var primaryWidth = GetPrimaryWidth();
        var length = primaryWidth * 2;
        var beamLength = Math.Min(12, length);
        var extentLength = 16;

        //Determine the primary column's length
        if (rightMember.IsActive)
        {
            length = Math.Max(length, rightMember.Shape.USShape.d / 2 + extentLength);
        }

        if (leftMember.IsActive)
        {
            length = Math.Max(length, leftMember.Shape.USShape.d / 2 + extentLength);
        }

        //Draw the column
        if (primaryMember != null)
        {
            CreateMeshComponent(primaryMember, 0, length, 0, EBraceConnectionTypes.DirectlyWelded, false);

            if (!primaryMember.IsActive)
            {
                //Delete all the dimensions and labels
                MessageQueueTest.ClearLabels("Column");
            }
        }

        //Right Beam
        if(rightMember != null)
        {
            CreateMeshComponent(rightMember, 12, beamLength, 90, EBraceConnectionTypes.DirectlyWelded, false);
            var rbeamlength = rightMember.Shape.USShape.d / 2;

            if(!rightMember.IsActive)
            {
                //Delete all the dimensions and labels
                MessageQueueTest.ClearLabels("Right");
                MessageQueueTest.ClearLabels("RColumn");
            }

            switch(rightMember.ShearConnection)
            {
                case EShearCarriedBy.SinglePlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Shear", "SinglePlate");
                    CreateMeshComponent(rightMember, beamLength, beamLength, 0, EBraceConnectionTypes.SinglePlate);
                    break;

                case EShearCarriedBy.ClipAngle:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Shear", "ClipAngle");
                    CreateMeshComponent(rightMember, beamLength, beamLength, 0, EBraceConnectionTypes.ClipAngle);
                    break;

                //case EShearCarriedBy.DirectlyWelded:
                    //CreateMeshComponent(rightMember, 12, beamLength, 0, EBraceConnectionTypes.DirectlyWelded, true);
                    //break;

                case EShearCarriedBy.EndPlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Shear", "EndPlate");
                    CreateMeshComponent(rightMember, beamLength, beamLength, 0, EBraceConnectionTypes.EndPlate);
                    break;

                case EShearCarriedBy.Seat:

                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Shear", "Seat");
                        
                    switch (rightMember.WinConnect.ShearSeat.Connection)
                    {
                        case ESeatConnection.Bolted:
                            CreateMeshComponent(rightMember, (primaryWidth), rbeamlength, bctype: EBraceConnectionTypes.Seat, rotationAngle: 90);
                            break;
                        case ESeatConnection.Welded:
                            // Front
                            CreateMeshComponent(rightMember, (primaryWidth), rbeamlength, bctype: EBraceConnectionTypes.Seat, rotationAngle: 90);
                            break;
                        case ESeatConnection.BoltedStiffenedPlate:
                            //Draw Seat
                            CreateMeshComponent(rightMember, (primaryWidth), rbeamlength, bctype: EBraceConnectionTypes.Seat, rotationAngle: 90);
                            break;
                        case ESeatConnection.WeldedStiffened:
                            // Front
                            //Draw Seat
                            CreateMeshComponent(rightMember, (primaryWidth), rbeamlength, bctype: EBraceConnectionTypes.Seat, rotationAngle: 90);
                            break;
                    }
                    break;

                case EShearCarriedBy.Tee:

                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Shear", "Tee");
                    CreateMeshComponent(rightMember, primaryWidth, rbeamlength, 0, EBraceConnectionTypes.FabricatedTee);
                    break;
            }

            switch (rightMember.MomentConnection)
            {
                case EMomentCarriedBy.Tee:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Moment", "Tee");
                    CreateMeshComponent(rightMember, beamLength, beamLength, 0, EBraceConnectionTypes.DirectlyWelded, false, EMomentCarriedBy.Tee);
                    break;
                case EMomentCarriedBy.FlangePlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Moment", "FlangePlate");
                    CreateMeshComponent(rightMember, 10, 20, 0, EBraceConnectionTypes.FlangePlate);
                    break;
                case EMomentCarriedBy.DirectlyWelded:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Moment", "DirectlyWelded");
                    //CreateMeshComponent
                    break;
                case EMomentCarriedBy.Angles:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Moment", "Angle");
                    CreateMeshComponent(rightMember, beamLength, beamLength, 0, EBraceConnectionTypes.ClipAngle, false, EMomentCarriedBy.Angles);
                    break;
                case EMomentCarriedBy.NoMoment:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Right Moment", "Null");
                    break;

            }
        }

        //Left Beam
        if (leftMember != null)
        {
            CreateMeshComponent(leftMember, -beamLength, -beamLength, 90, EBraceConnectionTypes.DirectlyWelded, false);
            var lbeamlength = leftMember.Shape.USShape.d / 2;

            if (!leftMember.IsActive)
            {
                //Delete all the dimensions and labels
                MessageQueueTest.ClearLabels("Left");
                MessageQueueTest.ClearLabels("LColumn");
            }

            switch (leftMember.ShearConnection)
            {
                case EShearCarriedBy.SinglePlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Shear", "SinglePlate");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.SinglePlate);
                    break;

                case EShearCarriedBy.ClipAngle:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Shear", "ClipAngle");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.ClipAngle);
                    break;

                //case EShearCarriedBy.DirectlyWelded:
                    //CreateMeshComponent(leftMember, 12, beamLength, 0, EBraceConnectionTypes.DirectlyWelded, true);
                    //break;

                case EShearCarriedBy.EndPlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Shear", "EndPlate");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.EndPlate);
                    break;

                case EShearCarriedBy.Seat:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Shear", "Seat");
                    switch (leftMember.WinConnect.ShearSeat.Connection)
                    {
                        case ESeatConnection.Bolted:
                            CreateMeshComponent(leftMember, -primaryWidth, lbeamlength, bctype: EBraceConnectionTypes.Seat);
                            break;
                        case ESeatConnection.Welded:
                            CreateMeshComponent(leftMember, -primaryWidth, lbeamlength, bctype: EBraceConnectionTypes.Seat);
                            break;
                        case ESeatConnection.BoltedStiffenedPlate:
                            CreateMeshComponent(leftMember, -primaryWidth, lbeamlength, bctype: EBraceConnectionTypes.Seat);
                            break;
                        case ESeatConnection.WeldedStiffened:
                            CreateMeshComponent(leftMember, -primaryWidth, lbeamlength, bctype: EBraceConnectionTypes.Seat);
                            break;
                    }
                    break;

                case EShearCarriedBy.Tee:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Shear", "Tee");
                    CreateMeshComponent(leftMember, -primaryWidth, lbeamlength, 180, EBraceConnectionTypes.FabricatedTee);
                    break;
            }

            switch (leftMember.MomentConnection)
            {
                case EMomentCarriedBy.Tee:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Moment", "Tee");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.DirectlyWelded, false, EMomentCarriedBy.Tee);
                    break;
                case EMomentCarriedBy.FlangePlate:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Moment", "FlangePlate");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.FlangePlate);
                    break;
                case EMomentCarriedBy.DirectlyWelded:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Moment", "DirectlyWelded");
                    //CreateMeshComponent
                    break;
                case EMomentCarriedBy.Angles:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Moment", "Angle");
                    CreateMeshComponent(leftMember, -beamLength, -beamLength, 0, EBraceConnectionTypes.ClipAngle, false, EMomentCarriedBy.Angles);
                    break;
                case EMomentCarriedBy.NoMoment:
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Left Moment", "Null");
                    break;

            }
        }

        //Upper right brace
        if (upperRightMember != null)
        {
            CreateMeshComponent(upperRightMember, beamLength, 12, 90, EBraceConnectionTypes.DirectlyWelded, false);
            switch (upperRightMember.GussetToColumnConnection)
            {
                case EBraceConnectionTypes.EndPlate:
                    CreateMeshComponent(upperRightMember, 12, beamLength, 0, EBraceConnectionTypes.EndPlate, false);
                    break;
                case EBraceConnectionTypes.SinglePlate:
                    CreateMeshComponent(upperRightMember, 12, beamLength, 0, EBraceConnectionTypes.SinglePlate, false);
                    break;
                case EBraceConnectionTypes.ClipAngle:
                    CreateMeshComponent(upperRightMember, 12, beamLength, 0, EBraceConnectionTypes.ClipAngle, false);
                    break;
            }
        }

        //Lower right brace
        if (lowerRightMember != null)
        {
            CreateMeshComponent(lowerRightMember, beamLength, 12, 90, EBraceConnectionTypes.DirectlyWelded, false);
            switch (lowerRightMember.GussetToColumnConnection)
            {
                case EBraceConnectionTypes.EndPlate:
                    CreateMeshComponent(lowerRightMember, 12, beamLength, 0, EBraceConnectionTypes.EndPlate, false);
                    break;
                case EBraceConnectionTypes.SinglePlate:
                    CreateMeshComponent(lowerRightMember, 12, beamLength, 0, EBraceConnectionTypes.SinglePlate, false);
                    break;
                case EBraceConnectionTypes.ClipAngle:
                    CreateMeshComponent(lowerRightMember, 12, beamLength, 0, EBraceConnectionTypes.ClipAngle, false);
                    break;
            }
        }

        //Upper left brace
        if (upperLeftMember != null)
        {
            CreateMeshComponent(upperLeftMember, beamLength, 12, -90, EBraceConnectionTypes.DirectlyWelded, false);
            switch (upperLeftMember.GussetToColumnConnection)
            {
                case EBraceConnectionTypes.EndPlate:
                    CreateMeshComponent(upperLeftMember, 12, beamLength, 0, EBraceConnectionTypes.EndPlate, false);
                    break;
                case EBraceConnectionTypes.SinglePlate:
                    CreateMeshComponent(upperLeftMember, 12, beamLength, 0, EBraceConnectionTypes.SinglePlate, false);
                    break;
                case EBraceConnectionTypes.ClipAngle:
                    CreateMeshComponent(upperLeftMember, 12, beamLength, 0, EBraceConnectionTypes.ClipAngle, false);
                    break;
            }
        }

        //Lower left brace
        if (lowerLeftMember != null)
        {
            CreateMeshComponent(lowerLeftMember, beamLength, 12, -90, EBraceConnectionTypes.DirectlyWelded, false);
            switch (lowerLeftMember.GussetToColumnConnection)
            {
                case EBraceConnectionTypes.EndPlate:
                    CreateMeshComponent(lowerLeftMember, 12, beamLength, 0, EBraceConnectionTypes.EndPlate, false);
                    break;
                case EBraceConnectionTypes.SinglePlate:
                    CreateMeshComponent(lowerLeftMember, 12, beamLength, 0, EBraceConnectionTypes.SinglePlate, false);
                    break;
                case EBraceConnectionTypes.ClipAngle:
                    CreateMeshComponent(lowerLeftMember, 12, beamLength, 0, EBraceConnectionTypes.ClipAngle, false);
                    break;
            }
        }

        if (CommonDataStatic.ColumnStiffener != null)
        {
            if (CommonDataStatic.ColumnStiffener.TenStiff || CommonDataStatic.ColumnStiffener.CompStiff)
            {
                //MessageQueueTest.ClearLabelsExcept("Column Stiffener");
                CreateMeshComponent(primaryMember, 0, 0, 0, EBraceConnectionTypes.Stiffener);
            }
        }
    }

    public static double GetYOffset(DetailData component)
    {
        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
        {
            return 0.0f;
        }
        else
        {
            double y = 0;
            var brace0 = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
            var brace1 = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
            var brace2 = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
            var colSetting = CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn;
            if (brace1.ShapeName == ConstString.NONE && brace2.ShapeName == ConstString.NONE) return 0;
            if (component != brace0)
            {
                if (!brace1.Shape.USShape.d.Equals(brace2.Shape.USShape.d))
                {
                    var max = Math.Max(brace1.Shape.USShape.d, brace2.Shape.USShape.d);
                    y = max.Equals(component.Shape.USShape.d) ? 0 : max / 2 - component.Shape.USShape.d / 2;
                }
                else y = 0;
            }
            else
            {
                if (colSetting)
                {
                    var max = Math.Max(brace1.Shape.USShape.d, brace2.Shape.USShape.d);
                    y = component.Shape.USShape.d - max / 2;
                }
            }
            return y;
        }
    }

    public static double GetYOffsetWithEl(DetailData component)
    {
        var topElInches = (float)((component.WinConnect.Beam.TopElFeet * 12.0f) + component.WinConnect.Beam.TopElInches + (component.WinConnect.Beam.TopElNumerator / component.WinConnect.Beam.TopElDenominator));

        var isNegative = component.WinConnect.Beam.IsTopElNegative;

        if (isNegative)
        {
            topElInches *= -1;
        }

        var yOffset = (float)(GetYOffset(component) + topElInches);

        return yOffset;
    }

    public static Mesh ConvertToWireframe(Mesh meshObject)
    {
        try
        {
            var mesh = meshObject;

            var vertices = mesh.vertices;
            var triangles = mesh.triangles;
            var lines = new Vector3[triangles.Length];
            int[] indexBuffer;
            var generatedMesh = new Mesh();

            for (var t = 0; t < triangles.Length; t++)
            {
                lines[t] = (vertices[triangles[t]]);
            }

            generatedMesh.vertices = lines;
            generatedMesh.name = "Generated Wireframe";

            var linesLength = lines.Length;
            indexBuffer = new int[linesLength];

            var uvs = new Vector2[linesLength];
            var normals = new Vector3[linesLength];

            for (var m = 0; m < linesLength; m++)
            {
                indexBuffer[m] = m;
                //      uvs[m] = Vector2 (GeneratedMesh.vertices[m].x, GeneratedMesh.vertices[m].z);// sets a Planar UV (VERY SLOW)
                uvs[m] = new Vector2(0.0f, 1.0f);
                normals[m] = new Vector3(1, 1, 1);
            }

            generatedMesh.uv = uvs;
            generatedMesh.normals = normals;
            generatedMesh.SetIndices(indexBuffer, MeshTopology.Lines, 0);
            return generatedMesh;
        }
        catch (Exception)
        {
            return meshObject;
        }
    }
}

public class StiffenerFlangeData
{
    public StiffenerFlangeData()
    {
        Member = 0;
        Plate = null;
        yOffsetFromOrigin = 0;
    }

    public EMemberType Member;
    public WCMomentFlangePlate Plate;
    public double yOffsetFromOrigin;
}



