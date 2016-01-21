using UnityEngine;
using System.Collections;
using Descon.Data;
using System;
using System.Collections.Generic;

public partial class ConfigDrawMethods : System.Object
{
    public static double GetPrimaryLength()
    {
        if (CommonDataStatic.DetailDataDict != null)
        {
            var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
            var rightComponent = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
            var leftComponent = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

            var primaryWidth = DrawingMethods.GetPrimaryWidth();
            var primaryLength = primaryWidth * 2;
            var extentLength = 16;
            var METRIC = (float)ConstNum.METRIC_MULTIPLIER;

            //Determine the primary column's length
            if (rightComponent.IsActive)
            {
                primaryLength = Math.Max(primaryLength, rightComponent.Shape.USShape.d / 2 + extentLength);
            }

            if (leftComponent.IsActive)
            {
                primaryLength = Math.Max(primaryLength, leftComponent.Shape.USShape.d / 2 + extentLength);
            }

            return primaryLength;
        }
        else
            return 0.0;
    }

    public static double GetBeamLength()
    {
        if (CommonDataStatic.DetailDataDict != null)
        {
            var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
            var rightComponent = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
            var leftComponent = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

            var primaryWidth = DrawingMethods.GetPrimaryWidth();
            var primaryLength = primaryWidth * 2;
            var extentLength = 16;
            var METRIC = (float)ConstNum.METRIC_MULTIPLIER;

            //Determine the primary column's length
            if (rightComponent.IsActive)
            {
                primaryLength = Math.Max(primaryLength, rightComponent.Shape.USShape.d / 2 + extentLength);
            }

            if (leftComponent.IsActive)
            {
                primaryLength = Math.Max(primaryLength, leftComponent.Shape.USShape.d / 2 + extentLength);
            }

            var beamLength = Math.Min(12, primaryLength);

            return beamLength;
        }
        else
            return 0.0;
    }

    public static void DrawBeamToColumn()
    {
        //Draw the meshes for both Braces and Win
        //Most stuff is win except for when dealing with the braces

        //BeamToColumnType
        //Joint Configuration
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var rightComponent = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        var leftComponent = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

        var primaryWidth = DrawingMethods.GetPrimaryWidth();
        var primaryLength = DrawingMethods.GetInitialBeamLength();
        var extentLength = 16;
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var topNearBeam = CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn;
        var topCut = (float)CommonDataStatic.ColumnStiffener.TopOfBeamToColumn;
        var leftTopPoint = 0.0f;
        var rightTopPoint = 0.0f;

        var changed = false;

        //Use the l/r beam length instead so set a lower value
        if (rightComponent.IsActive || leftComponent.IsActive)
            primaryLength = 15;

        //Determine the primary column's length
        if (rightComponent.IsActive)
        {
            primaryLength = Math.Max(primaryLength, rightComponent.Shape.USShape.d / 2 + extentLength);
            rightTopPoint = (float)(rightComponent.Shape.USShape.d / 2 + DrawingMethods.GetYOffsetWithEl(rightComponent));
        }

        if (leftComponent.IsActive)
        {
            primaryLength = Math.Max(primaryLength, leftComponent.Shape.USShape.d / 2 + extentLength);

            leftTopPoint = (float)(leftComponent.Shape.USShape.d / 2 + DrawingMethods.GetYOffsetWithEl(leftComponent));
        }

        if (topNearBeam)
        {
            var oldLength = (float)primaryLength;

            var topPoint = Mathf.Max(leftTopPoint - DrawingMethods.GetTopEl(leftComponent), rightTopPoint - DrawingMethods.GetTopEl(rightComponent));
            //Cut the primary length
            MessageQueueTest.instance.topCutPoint = topPoint + topCut;
            topCut = (float)(primaryLength - topPoint) - topCut;
            primaryLength -= topCut / 2;

            topCut = oldLength - (float)primaryLength;

            //Draw dimensions
            var distance = (float)CommonDataStatic.ColumnStiffener.TopOfBeamToColumn;

            if (distance > 0)
            {
                var points = new float[5];
                points[0] = (float)DrawingMethods.GetPrimaryWidth();
                points[1] = leftTopPoint;
                points[2] = rightTopPoint;
                points[3] = (float)primaryLength - topCut; //Top of column

                //Left side
                var value = points[3] - points[1];

                if (value > 0)
                {
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Left Column Top Dist", DrawingMethods.GetFootString(value), new Vector3(-points[0], points[1]),
                        new Vector3(-points[0], points[1] + value), -4, MessageQueueTest.GetDimensionColor());
                }
                else
                    MessageQueueTest.ClearLabelsAndDimensions("Left Column Top Dist");

                value = points[3] - points[2];

                if (value > 0)
                {
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Right Column Top Dist", DrawingMethods.GetFootString(value), new Vector3(points[0], points[2]),
                        new Vector3(points[0], points[2] + value), 4, MessageQueueTest.GetDimensionColor());
                }
                else
                    MessageQueueTest.ClearLabelsAndDimensions("Right Column Top Dist");
            }
            else
                MessageQueueTest.ClearLabelsAndDimensions("Dist");
        }
        else
        {
            MessageQueueTest.instance.topCutPoint = 0.0f;
            MessageQueueTest.ClearLabelsAndDimensions("Dist");
        }

        var beamLength = Math.Min(12, primaryLength);

        if (MessageQueueTest.instance.prevPrimaryLength != primaryLength)
        {
            MessageQueueTest.instance.prevPrimaryLength = primaryLength;
            changed = true;
        }


        if (MessageQueueTest.GetMemberControl(EMemberType.PrimaryMember).MemberChanged)
        {
            changed = true;
        }

        //Draw the primary member
        if (primaryComponent.IsActive && primaryComponent.ShapeType != EShapeType.None)
        {
            var member = MessageQueueTest.GetMemberControl(primaryComponent.MemberType);
            member.memberType = EMemberType.PrimaryMember;
            member.subType = EMemberSubType.Main;

            //Move these lines farther away from the camera
            member.lineDrawing.lineOffset = 0.1f;

            var isInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;

            //Create the member mesh
            if (primaryComponent.ShapeType == EShapeType.WideFlange)
            {
                MessageQueueTest.ClearLabelsAndDimensions("HSS");

                member.shapeControl.Mesh = DrawingMethods.CreateWideFlangeMesh(primaryComponent, new Vector3(0, -topCut, 0), new Vector3(90, isInPlane ? 90 : 0, 0), (float)primaryLength, false);

                //Create the lines
                MemberLineMethods.CreatePrimaryBeamLines(member.lineDrawing, primaryComponent, primaryLength);
            }
            else if (primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Column", "Main");

                member.shapeControl.Mesh = DrawingMethods.CreateHollowSteelSectionMesh(primaryComponent.Shape, new Vector3(0, -topCut, 0), new Vector3(90, isInPlane ? 90 : 0, 0), (float)primaryLength);

                //Create the lines
                MemberLineMethods.CreatePrimaryHSSLines(member.lineDrawing, primaryComponent, primaryLength);
            }

            //Create the primary member name label
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Column Main label", primaryComponent.ShapeName + " - " + primaryComponent.Material.Name,
                new Vector3((float)DrawingMethods.GetPrimaryWidth(), MessageQueueTest.instance.topCutPoint > 0 ? MessageQueueTest.instance.topCutPoint : (float)primaryLength, 0),
                new Vector3(10, 8, 0), changed, null, false, changed);

            //Create the stiffeners
            CreateStiffeners(member);

            if((!leftComponent.IsActive && !rightComponent.IsActive))
            {
                MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener");
            }
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Column");
        }

        //Draw the Right Beam
        if(rightComponent.IsActive)
        {
            CreateBeam(rightComponent, beamLength, primaryWidth, true);

            //Create the right beam label
            var yOffset = DrawingMethods.GetTopElAndYOffset(rightComponent);
            var beam = rightComponent.Shape;
            var labelOffset = new Vector3((float)(DrawingMethods.GetPrimaryWidth() + beamLength * 2), -(float)beam.d / 2 * METRIC + yOffset, 0);

            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Right Beam Label", rightComponent.ShapeName + " - " + rightComponent.Material.Name + "\nEnd Offset = "
                + rightComponent.EndSetback.ToString() + " in", labelOffset, new Vector3(10, -8, 0), changed, null, false, changed);
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Right");
        }

        //Draw the Left Beam
        if (leftComponent.IsActive)
        {
            CreateBeam(leftComponent, beamLength, -primaryWidth, true);

            //Create the left beam label
            var yOffset = DrawingMethods.GetTopElAndYOffset(leftComponent);
            var beam = leftComponent.Shape;
            var labelOffset = new Vector3(-(float)(DrawingMethods.GetPrimaryWidth() + beamLength * 2), -(float)beam.d / 2 * METRIC + yOffset, 0);

            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Left Beam Label", leftComponent.ShapeName + " - " + leftComponent.Material.Name + "\nEnd Offset = "
                + leftComponent.EndSetback.ToString() + " in", labelOffset, new Vector3(-10, -8, 0), changed, null, false, changed);
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Left");
        }

        //Draw the elevation labels
        if (leftComponent.IsActive && rightComponent.IsActive)
        {
            var rBeamEl = DrawingMethods.GetTopEl(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam]);
            var lBeamEl = DrawingMethods.GetTopEl(CommonDataStatic.DetailDataDict[EMemberType.RightBeam]);
            var rBeamYOffset = (float)DrawingMethods.GetYOffset(leftComponent);
            var lBeamYOffset = (float)DrawingMethods.GetYOffset(rightComponent);
            var rBeamHeight = (float)leftComponent.Shape.USShape.d / 2;
            var lBeamHeight = (float)rightComponent.Shape.USShape.d / 2;
            var endSetBack = (float)leftComponent.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;
            var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;

            //Extender config
            var shearPlate = leftComponent.WinConnect.ShearWebPlate;
            var extend = false;

            if (shearPlate != null && shearPlate.ExtendedConfiguration == true && !isPrimaryInPlane)
            {
                extend = true;
            }

            var horizPoint = (float)(DrawingMethods.GetPrimaryWidth());

            if (extend)
            {
                horizPoint = (float)(primaryComponent.Shape.USShape.bf / 2);
            }

            var totalLength = 0.0f;

            if (lBeamEl < rBeamEl)
            {
                totalLength = rBeamEl - lBeamEl;
            }
            else
            {
                totalLength = lBeamEl - rBeamEl;
            }

            if (lBeamEl < rBeamEl)
            {
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Column Elev 1", DrawingMethods.GetFootString(totalLength), new Vector3(horizPoint, rBeamHeight + rBeamEl + rBeamYOffset),
                    new Vector3(horizPoint, lBeamHeight + lBeamYOffset + lBeamEl), -20, MessageQueueTest.GetDimensionColor(), horizPoint * 2 + endSetBack, 0, EOffsetType.Right);
            }
            else if (lBeamEl > rBeamEl)
            {
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Column Elev 1", DrawingMethods.GetFootString(totalLength), new Vector3(-horizPoint, lBeamHeight + lBeamYOffset + lBeamEl),
                    new Vector3(-horizPoint, rBeamHeight + rBeamEl + rBeamYOffset), 20, MessageQueueTest.GetDimensionColor(), horizPoint * 2 + endSetBack, 0, EOffsetType.Right);
            }

            if (totalLength == 0.0f)
            {
                //Delete those 
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawDimensionsWithTags("Column Elev");
            }
        }
        else
        {
            //Delete those 
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawDimensionsWithTags("Column Elev");
        }
    }

    public static void DrawBeamToGirder()
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var rightComponent = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        var leftComponent = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

        var primaryWidth = DrawingMethods.GetPrimaryWidth();
        var primaryLength = 15; //Length is actually doubled
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();

        var beamLength = 20;
        var useCope = true;

        var changed = false;

        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice)
            useCope = false;

        if (MessageQueueTest.GetMemberControl(EMemberType.PrimaryMember).MemberChanged)
        {
            changed = true;
        }

        //Draw the primary member
        if (primaryComponent.IsActive && primaryComponent.ShapeType != EShapeType.None)
        {
            var member = MessageQueueTest.GetMemberControl(primaryComponent.MemberType);
            member.memberType = EMemberType.PrimaryMember;
            member.subType = EMemberSubType.Main;

            //Move these lines farther away from the camera
            member.lineDrawing.lineOffset = 0.1f;

            var isInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;

            //Create the member mesh
            if (primaryComponent.ShapeType == EShapeType.WideFlange)
            {
                MessageQueueTest.ClearLabelsAndDimensions("HSS");

                member.shapeControl.Mesh = DrawingMethods.CreateWideFlangeMesh(primaryComponent, new Vector3(), new Vector3(0, !isInPlane ? 90 : 0, 0), (float)primaryLength, false);

                //Create the lines
                MemberLineMethods.CreateGirderPrimaryBeamLines(member.lineDrawing, primaryComponent, primaryLength);
            }
            else if (primaryComponent.ShapeType == EShapeType.HollowSteelSection)
            {
                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Column", "Main");

                member.shapeControl.Mesh = DrawingMethods.CreateHollowSteelSectionMesh(primaryComponent.Shape, new Vector3(), new Vector3(0, !isInPlane ? 90 : 0, 0), (float)primaryLength);

                //Create the lines
                MemberLineMethods.CreatePrimaryHSSLines(member.lineDrawing, primaryComponent, primaryLength);
            }

            //Create the primary member name label
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Column Main label", primaryComponent.ShapeName + " - " + primaryComponent.Material.Name,
                new Vector3(isInPlane ? (float)primaryComponent.Shape.USShape.bf / 2 : primaryLength, (float)(primaryComponent.Shape.USShape.d / 2), 0), new Vector3(10, 8, 0), changed, null, false, changed);
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Column");
        }

        var isGirder = (CommonDataStatic.BeamToColumnType == EJointConfiguration.BeamToGirder);

        //Draw the Right Beam
        if (rightComponent.IsActive)
        {
            CreateBeam(rightComponent, beamLength, primaryWidth, useCope);

            //Create the right beam label
            var yOffset = DrawingMethods.GetTopElAndYOffset(rightComponent) + DrawingMethods.GirderOffset(rightComponent);
            var beam = rightComponent.Shape;
            var labelOffset = new Vector3((float)(DrawingMethods.GetPrimaryWidth() + beamLength * 2), -(float)beam.d / 2 * METRIC + yOffset, 0);

            var copeText = rightComponent.WinConnect.Beam.TCopeL.ToString() + unitString + " X " + rightComponent.WinConnect.Beam.TCopeD.ToString() + unitString;

            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Right Beam Label", rightComponent.ShapeName + " - " + rightComponent.Material.Name + "\nEnd Offset = "
                + rightComponent.EndSetback.ToString() + " in." + (isGirder ? "\nTop Cope: " + copeText : ""), labelOffset, new Vector3(10, -8, 0), changed, null, false, changed);
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Right");
        }

        //Draw the Left Beam
        if (leftComponent.IsActive)
        {
            CreateBeam(leftComponent, beamLength, -primaryWidth, useCope);

            //Create the left beam label
            var yOffset = DrawingMethods.GetTopElAndYOffset(leftComponent) + DrawingMethods.GirderOffset(leftComponent);
            var beam = leftComponent.Shape;
            var labelOffset = new Vector3(-(float)(DrawingMethods.GetPrimaryWidth() + beamLength * 2), -(float)beam.d / 2 * METRIC + yOffset, 0);

            var copeText = leftComponent.WinConnect.Beam.TCopeL.ToString() + unitString + " X " + leftComponent.WinConnect.Beam.TCopeD.ToString() + unitString;

            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Left Beam Label", leftComponent.ShapeName + " - " + leftComponent.Material.Name + "\nEnd Offset = "
                + leftComponent.EndSetback.ToString() + " in" + (isGirder ? "\nTop Cope: " + copeText : ""), labelOffset, new Vector3(-10, -8, 0), changed, null, false, changed);
        }
        else
        {
            //Clear the labels and dimensions
            MessageQueueTest.ClearLabelsAndDimensions("Left");
        }

        MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener");
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawDimensionsWithTags("Column Elev");
    }

    static void CreateBeam(DetailData component, double beamLength, double x, bool useCope = false)
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var boltHeight = 0.390625f; // 25/64
        var isLeftSide = x < 0;
        var memberType = isLeftSide ? EMemberType.LeftBeam : EMemberType.RightBeam;

        var side = "Right";

        if(x < 0)
            side = "Left";

        var primaryWidth = DrawingMethods.GetPrimaryWidth();

        var beamXOffset = primaryWidth;

        if (primaryComponent.WebOrientation == EWebOrientation.OutOfPlane)
        {
            if(component.ShearConnection == EShearCarriedBy.SinglePlate && component.WinConnect.ShearWebPlate.ExtendedConfiguration)
                beamXOffset = primaryComponent.Shape.USShape.bf / 2;
        }

        var member = MessageQueueTest.GetMemberControl(component.MemberType);
        member.memberType = memberType;
        member.subType = EMemberSubType.Main;

        var shearConnectionChanged = member.ShearConnChanged;
        var momentConnectionChanged = member.MomentConnChanged;

        var isDirectlyWelded = component.MomentConnection == EMomentCarriedBy.DirectlyWelded && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane;
        var useCutoutWeld = (component.MomentConnection == EMomentCarriedBy.DirectlyWelded && primaryComponent.WebOrientation == EWebOrientation.InPlane);

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);
        var beamOffset = yOffset;

        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
        {
            yOffset = DrawingMethods.GetTopEl(component);
            beamOffset = DrawingMethods.GirderOffset(component) + yOffset;
        }

        var directlyWeldedOffset = (float)((primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2 + component.WinConnect.MomentDirectWeld.Top.a);

        //Create the member mesh
        member.shapeControl.Mesh = DrawingMethods.CreateWideFlangeMesh(component, new Vector3((float)(beamXOffset + beamLength + (isDirectlyWelded ? directlyWeldedOffset : component.EndSetback)) * Math.Sign(x),
            beamOffset, 0), new Vector3(0, isLeftSide ? 90 : -90, 0), (float)beamLength, useCutoutWeld, useCope);

        //Create the lines
        if (x > 0)
            MemberLineMethods.CreateRightBeamLines(member.lineDrawing, component, beamLength, useCope);
        else
        {
            MemberLineMethods.CreateLeftBeamLines(member.lineDrawing, component, beamLength, useCope);
        }

        //Move these lines farther from the camera
        member.lineDrawing.lineOffset = 0.1f;

        var posOffset = 0.0f;

        //Create backing bars
        //Add backing bars
        if (useCutoutWeld)
        {
            var dimensions = new Vector3(0.6875f, 0.2f, (float)component.Shape.USShape.bf);
  
            //Create the connection object
            var backMember = MessageQueueTest.CreateMemberControl();
            //add it to the connection controls of the parent object
            member.AddConnectionMember(backMember);

            backMember.memberType = memberType;
            backMember.subType = EMemberSubType.Main;

            backMember.lineDrawing.lineOffset = -0.01f;

            var mesh1 = MeshCreator.CreateBoxMesh(new Vector3((float)(DrawingMethods.GetPrimaryWidth() + dimensions.x / 2) * Math.Sign(x), (float)(component.Shape.USShape.d / 2 - component.Shape.USShape.tf - dimensions.y / 2) + yOffset, 0), dimensions, Vector3.zero);
            var mesh2 = MeshCreator.CreateBoxMesh(new Vector3((float)(DrawingMethods.GetPrimaryWidth() + dimensions.x / 2) * Math.Sign(x), (float)(-component.Shape.USShape.d / 2 - dimensions.y / 2) + yOffset, 0), dimensions, Vector3.zero);

            MeshCreator.Reset();
            MeshCreator.Add(mesh1);
            MeshCreator.Add(mesh2);

            //Apply the mesh
            backMember.shapeControl.Mesh = MeshCreator.Create();

            //Create the lines
            MemberLineMethods.CreateMeshLines(backMember.lineDrawing, backMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

            //Set the color
            backMember.memberColor = MessageQueueTest.GetConnectionColor();
        }

        //Bolt stuff
        var boltStartPos = Vector3.zero;
        var edgeDist = 0.0;
        var shankLength = 0.0f;
        var boltMeshes = new List<Mesh>();
        Mesh boltMesh = null;
        MemberControl2 boltMember = null;

        //Create the connection meshes
        switch (component.ShearConnection)
        {
            case EShearCarriedBy.ClipAngle:

                //Clear the other right side shear dimensions and labels
                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Shear", "ClipAngle");

                #region clipAngle Meshes
                //Create the connection object
                var clipMember = MessageQueueTest.CreateMemberControl();
                //Add it to the connection controls of the parent object
                member.AddConnectionMember(clipMember);

                clipMember.memberType = memberType;
                clipMember.subType = EMemberSubType.Shear;

                clipMember.lineDrawing.lineOffset = -0.01f;

                //Create the first clip angle mesh
                var clipAngle = component.WinConnect.ShearClipAngle;
                var isLongLegOSL = component.WinConnect.ShearClipAngle.OSL == EOSL.LongLeg;
                var t = DrawingMethods.GetClipThickness(clipAngle);
                //Get the clip position
                var pos = new Vector3((float)(primaryWidth + t / 2) * Math.Sign(x),
                    DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d * METRIC, clipAngle.Length * METRIC, clipAngle.BoltWebOnBeam.EdgeDistLongDir * METRIC, clipAngle.Position)
                    + yOffset,-((float)component.Shape.USShape.tw / 2 + t / 2));

                clipMember.shapeControl.Mesh = DrawingMethods.CreateClipAngleMesh(clipAngle, clipAngle.Length * METRIC, pos, new Vector3(!isLeftSide ? 180 : 0, !isLeftSide ? 0 : 180, 0), isLongLegOSL);

                //Create the lines
                MemberLineMethods.CreateMeshLines(clipMember.lineDrawing, clipMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Create the custom top lines
                MemberLineMethods.CreateTopClipAngleLines(clipMember.lineDrawing, component, x, false); 

                clipMember.memberColor = MessageQueueTest.GetConnectionColor();

                if (clipAngle.SizeType == EWebConnectionSize.L2)
                {
                    //Create the second clip angle behind
                    clipMember = MessageQueueTest.CreateMemberControl();
                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(clipMember);

                    clipMember.memberType = memberType;
                    clipMember.subType = EMemberSubType.Shear;

                    pos.z = ((float)component.Shape.USShape.tw / 2 + t / 2);

                    clipMember.shapeControl.Mesh = DrawingMethods.CreateClipAngleMesh(clipAngle, clipAngle.Length * METRIC, pos, new Vector3(0, !isLeftSide ? 0 : 0, !isLeftSide ? 0 : 180), isLongLegOSL);
                    clipMember.memberColor = MessageQueueTest.GetConnectionColor();

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(clipMember.lineDrawing, clipMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    //Create the custom top lines
                    MemberLineMethods.CreateTopClipAngleLines(clipMember.lineDrawing, component, x, true); 
                }

                #endregion

                #region BoltMeshes

                boltMeshes.Clear();

                edgeDist = (isLongLegOSL ? clipAngle.LongLeg * METRIC : clipAngle.ShortLeg * METRIC) - clipAngle.BoltOslOnSupport.EdgeDistTransvDir * METRIC;
                shankLength = (float)(clipAngle.Thickness * METRIC + DrawingMethods.GetPrimaryThickness());

                //Create the column side bolts
                if (clipAngle.SupportSideConnection == EConnectionStyle.Bolted)
                {
                    if (CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice && DrawingMethods.BothSidesActive())
                        shankLength *= 2;

                    if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder && DrawingMethods.BothSidesActive())
                        shankLength += t;

                    boltHeight = DrawingMethods.GetBoltHeightSize((float)clipAngle.BoltOslOnSupport.BoltSize);

                    //Bolts on column (OSL)
                    boltStartPos = new Vector3((float)(primaryWidth + t + boltHeight / 2) * Math.Sign(x),
                        DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, clipAngle.Length, clipAngle.BoltOslOnSupport.EdgeDistLongDir, clipAngle.Position) + yOffset,
                        (float)((edgeDist) + component.Shape.USShape.tw / 2));

                    if (clipAngle.BoltStagger == EBoltStagger.Support)
                    {
                        boltStartPos.y -= (float)clipAngle.BoltOslOnSupport.SpacingLongDir * METRIC * 0.5f;
                    }

                    if (clipAngle.SizeType == EWebConnectionSize.L2)
                    {
                        for (int i = 0; i < clipAngle.BoltOslOnSupport.NumberOfLines; ++i)
                        {
                            boltMesh = DrawingMethods.CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(-x) * (float)clipAngle.BoltOslOnSupport.SpacingTransvDir * METRIC + (float)(Math.Sign(-x) * 0)),
                                Vector3.down * (float)clipAngle.BoltOslOnSupport.SpacingLongDir * METRIC, clipAngle.BoltOslOnSupport.NumberOfRows, (float)clipAngle.BoltOslOnSupport.BoltSize * METRIC, new Vector3(isLeftSide ? 90 : 270, -90, 0), shankLength);
                            boltMeshes.Add(boltMesh);
                        }
                    }


                    boltStartPos.z *= -1;

                    for (int i = 0; i < clipAngle.BoltOslOnSupport.NumberOfLines; ++i)
                    {
                        boltMesh = DrawingMethods.CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(-x) * (float)clipAngle.BoltOslOnSupport.SpacingTransvDir * METRIC + (float)(Math.Sign(-x) * 0)),
                            Vector3.down * (float)clipAngle.BoltOslOnSupport.SpacingLongDir * METRIC, clipAngle.BoltOslOnSupport.NumberOfRows, (float)clipAngle.BoltOslOnSupport.BoltSize * METRIC, new Vector3(isLeftSide ? 90 : 270, -90, 0), shankLength);
                        boltMeshes.Add(boltMesh);
                    }
                }

                //Create the column side bolt
                //Combine the bolts
                MeshCreator.Reset();
                boltMesh = MeshCreator.Combine(boltMeshes.ToArray());

                //Create a connection object for those bolts
                boltMember = MessageQueueTest.CreateMemberControl();

                //Add it to the connection controls of the parent object
                member.AddConnectionMember(boltMember);

                boltMember.lineDrawing.lineOffset = -0.01f;
                boltMember.shapeControl.Mesh = boltMesh;
                boltMember.memberColor = MessageQueueTest.GetBoltColor();
                boltMember.memberType = memberType;
                boltMember.subType = EMemberSubType.BoltShearSupport;

                MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                boltMeshes.Clear();

                //Could also be component.WinConnect.Beam.Lh + Endsetback
                edgeDist = component.WinConnect.Beam.Lh + component.EndSetback;
                //edgeDist = component.WinConnect.Beam.BoltEdgeDistanceOnFlange;//(isLongLegOSL ? clipAngle.LongLeg * METRIC : clipAngle.ShortLeg * METRIC) - clipAngle.BoltWebOnBeam.EdgeDistTransvDir * METRIC;

                //Create the beam side bolts
                if (clipAngle.BeamSideConnection == EConnectionStyle.Bolted)
                {
                    boltHeight = DrawingMethods.GetBoltHeightSize((float)clipAngle.BoltWebOnBeam.BoltSize);

                    boltStartPos = new Vector3((float)(primaryWidth + edgeDist) * Math.Sign(x),
                        DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, clipAngle.Length, clipAngle.BoltWebOnBeam.EdgeDistLongDir, clipAngle.Position) + yOffset, 
                        (float)(t + component.Shape.USShape.tw / 2 + boltHeight / 2)); //z - (float)(bclip.ShortLeg - bclip.BoltOnColumn.EdgeDistLongDir)
                    shankLength = (float)(t * 2 + component.Shape.USShape.tw);

                    var oneLessRow = clipAngle.BoltStagger == EBoltStagger.OneLessRow;

                    if (clipAngle.BoltStagger == EBoltStagger.Beam || oneLessRow)
                    {
                        boltStartPos.y -= (float)clipAngle.BoltWebOnBeam.SpacingLongDir * METRIC * 0.5f;
                    }

                    //Draw the bolts on the beam side
                    for (int i = 0; i < clipAngle.BoltWebOnBeam.NumberOfLines; ++i)
                    {
                        //splate.BoltToHorizEdgeDist
                        boltMesh = DrawingMethods.CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(-x) * (float)clipAngle.BoltWebOnBeam.SpacingTransvDir * METRIC + (float)(Math.Sign(-x) * 0)),
                            Vector3.down * (float)clipAngle.BoltWebOnBeam.SpacingLongDir * METRIC, clipAngle.BoltWebOnBeam.NumberOfRows - (oneLessRow ? 1 : 0), (float)clipAngle.BoltWebOnBeam.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, isLeftSide ? -180 : 0, 0), shankLength);
                        boltMeshes.Add(boltMesh);
                    }
                }

                //Combine the bolts
                MeshCreator.Reset();
                boltMesh = MeshCreator.Combine(boltMeshes.ToArray());

                //Create a connection object for those bolts
                boltMember = MessageQueueTest.CreateMemberControl();

                //Add it to the connection controls of the parent object
                member.AddConnectionMember(boltMember);

                boltMember.lineDrawing.lineOffset = -0.01f;
                boltMember.shapeControl.Mesh = boltMesh;
                boltMember.memberColor = MessageQueueTest.GetBoltColor();
                boltMember.memberType = memberType;
                boltMember.subType = EMemberSubType.BoltShearBeam;

                //Create the bolt lines
                MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                boltMeshes.Clear();

                #endregion

                //Create the dimensions
                MemberDimensionMethods.AddShearClipAngleDimensions(component, x, shearConnectionChanged);
                break;

            case EShearCarriedBy.EndPlate:

                //Clear the other right side shear dimensions and labels
                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Shear", "EndPlate");

                //Create the connection object
                var endMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(endMember);

                endMember.memberType = memberType;
                endMember.subType = EMemberSubType.Shear;

                endMember.lineDrawing.lineOffset = -0.01f;

                //Create the end plate mesh
                var endPlate = component.WinConnect.ShearEndPlate;

                posOffset = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d * METRIC, endPlate.Length * METRIC, endPlate.Bolt.EdgeDistLongDir, endPlate.Position);

                //Apply the mesh
                endMember.shapeControl.Mesh = DrawingMethods.CreateEndPlateMesh(endPlate, new Vector3((float)(primaryWidth + endPlate.Thickness / 2) * Math.Sign(x), posOffset + yOffset, 0), new Vector3(0, 0, 0));

                //Create the lines
                MemberLineMethods.CreateMeshLines(endMember.lineDrawing, endMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                endMember.memberColor = MessageQueueTest.GetConnectionColor();

                #region boltMeshes

                //Create the bolts
                shankLength = (float)(endPlate.Thickness * METRIC + DrawingMethods.GetPrimaryThickness());

                if(CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice || CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
                {
                    if(DrawingMethods.BothSidesActive())
                        shankLength += (float)(endPlate.Thickness * METRIC);
                }

                boltHeight = DrawingMethods.GetBoltHeightSize((float)endPlate.Bolt.BoltSize);

                //Add the first vertical line
                boltStartPos = new Vector3((float)(primaryWidth + (endPlate.Thickness * METRIC + boltHeight / 2) * 1) * Math.Sign(x),
                    DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, endPlate.Length, endPlate.Bolt.EdgeDistLongDir, endPlate.Position) + yOffset, 
                    (float)component.GageOnColumn / 2);

                boltMeshes.Clear();

                boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)endPlate.Bolt.SpacingLongDir * METRIC, endPlate.Bolt.NumberOfRows, (float)endPlate.Bolt.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));
                //Add the second
                boltStartPos.z *= -1;

                boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)endPlate.Bolt.SpacingLongDir * METRIC, endPlate.Bolt.NumberOfRows, (float)endPlate.Bolt.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));

                //Combine the bolts
                MeshCreator.Reset();
                boltMesh = MeshCreator.Combine(boltMeshes.ToArray());

                //Create a connection object for those bolts
                boltMember = MessageQueueTest.CreateMemberControl();

                //Add it to the connection controls of the parent object
                member.AddConnectionMember(boltMember);

                boltMember.lineDrawing.lineOffset = -0.01f;
                boltMember.shapeControl.Mesh = boltMesh;
                boltMember.memberColor = MessageQueueTest.GetBoltColor();
                boltMember.memberType = memberType;
                boltMember.subType = EMemberSubType.BoltShearSupport;

                //Create the lines
                MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                #endregion

                //Create the dimensions
                MemberDimensionMethods.AddShearEndPlateDimensions(component, boltStartPos.y, x, shearConnectionChanged);

                break;

            case EShearCarriedBy.Seat:

                //Clear the other right side shear dimensions and labels
                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Shear", "Seat");

                //Create the connection object
                var seatMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(seatMember);

                seatMember.memberType = memberType;
                seatMember.subType = EMemberSubType.Shear;

                seatMember.lineDrawing.lineOffset = -0.01f;

                //Create the seat mesh
                var seat = component.WinConnect.ShearSeat;

                var angleFlip = seat.ShortLegOn == EShortLegOn.SupportSide ? true : false;
                var isRight = x > 0;

                //Some seat variables
                var sleg = (float)(angleFlip == false ? seat.TopAngleSupLeg * METRIC : seat.TopAngleBeamLeg * METRIC);
                var bleg = (float)(angleFlip == false ? seat.TopAngleBeamLeg * METRIC : seat.TopAngleSupLeg * METRIC);
                var distFlange = (float)component.WinConnect.Beam.BoltEdgeDistanceOnFlange;
                var y = (float)component.Shape.USShape.d / 2;

                posOffset = 0.0f;

                //Apply the mesh
                seatMember.shapeControl.Mesh = DrawingMethods.CreateTopShearSeat(seat, new Vector3((float)primaryWidth * Math.Sign(x), (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(0, isRight ? 180 : 0, 0), angleFlip);

                //Create the lines
                MemberLineMethods.CreateMeshLines(seatMember.lineDrawing, seatMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                seatMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bottom seat
                if (seat.Connection == ESeatConnection.Bolted || seat.Connection == ESeatConnection.Welded)
                {
                    //Create the connection object
                    seatMember = MessageQueueTest.CreateMemberControl();
                    //add it to the connection controls of the parent object
                    member.AddConnectionMember(seatMember);

                    seatMember.memberType = memberType;
                    seatMember.subType = EMemberSubType.Shear;

                    seatMember.lineDrawing.lineOffset = -0.01f;

                    seatMember.shapeControl.Mesh = DrawingMethods.CreateBottomShearSeat(seat, new Vector3((float)primaryWidth * Math.Sign(x), -y + yOffset), new Vector3(0, isRight ? 0 : 180, 180), angleFlip);

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(seatMember.lineDrawing, seatMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    //Set the color
                    seatMember.memberColor = MessageQueueTest.GetConnectionColor();
                }
                else
                {
                    //Create the bottom stiffener
                    switch (seat.Stiffener)
                    {
                        case ESeatStiffener.Tee:
                            //Create the connection object
                            seatMember = MessageQueueTest.CreateMemberControl();
                            //add it to the connection controls of the parent object
                            member.AddConnectionMember(seatMember);

                            seatMember.memberType = memberType;
                            seatMember.subType = EMemberSubType.Shear;

                            seatMember.lineDrawing.lineOffset = -0.01f;

                            seatMember.shapeControl.Mesh = DrawingMethods.CreateStiffenerTee(seat.StiffenerTee, new Vector3((float)(primaryWidth + seat.StiffenerWidth / 2) * Math.Sign(x), (float)-y, 0), new Vector3(0, 0, 90), seat.StiffenerWidth);

                            //Create the lines
                            MemberLineMethods.CreateMeshLines(seatMember.lineDrawing, seatMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                            //Set the color
                            seatMember.memberColor = MessageQueueTest.GetConnectionColor();

                            break;
                        case ESeatStiffener.Plate:
                            //Create the connection object
                            seatMember = MessageQueueTest.CreateMemberControl();
                            //add it to the connection controls of the parent object
                            member.AddConnectionMember(seatMember);

                            seatMember.memberType = memberType;
                            seatMember.subType = EMemberSubType.Shear;

                            seatMember.lineDrawing.lineOffset = -0.01f;

                            seatMember.shapeControl.Mesh = DrawingMethods.CreateStiffenerPlate(seat, new Vector3((float)(primaryWidth + seat.PlateWidth / 2) * Math.Sign(x), (float)-y + yOffset, 0), new Vector3(0, 0, 0));

                            //Create the lines
                            MemberLineMethods.CreateMeshLines(seatMember.lineDrawing, seatMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                            //Set the color
                            seatMember.memberColor = MessageQueueTest.GetConnectionColor();
                            break;
                    }

                }

                //Create the bolts for the top seat
                if (seat.Connection == ESeatConnection.Bolted)
                {
                    #region Top bolt mesh
                    boltMeshes.Clear();

                    boltHeight = DrawingMethods.GetBoltHeightSize((float)seat.Bolt.BoltSize);

                    //Make top the bolts
                    //Horizontal bolts
                    boltStartPos = new Vector3((float)(primaryWidth + (seat.TopAngle.t * METRIC + boltHeight / 2)) * Math.Sign(x), (float)y + sleg - (float)seat.Bolt.EdgeDistTransvDir * METRIC + yOffset, (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistLongDir * METRIC));
                    shankLength = (float)(seat.TopAngle.t * METRIC + DrawingMethods.GetPrimaryThickness());

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(isRight ? 90 : 270, 90, 0), shankLength));

                    boltStartPos.z *= -1;

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(isRight ? 90 : 270, 90, 0), shankLength));

                    //Make support side bolts
                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearSupport;

                    //Create the bolt lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    boltMeshes.Clear();

                    //Create the beam side bolts
                    boltStartPos = new Vector3((float)(primaryWidth + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange)) * Math.Sign(x), (float)(y + seat.TopAngle.t * METRIC + boltHeight / 2) + yOffset, (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));
                    shankLength = (float)(seat.TopAngle.t * METRIC + component.Shape.USShape.tf);

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingTransvDir * METRIC * Math.Sign(x), 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength));

                    boltStartPos.z *= -1;

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingTransvDir * METRIC * Math.Sign(x), 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength));

                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearBeam;

                    //Create the bolt lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    #endregion

                    #region Bottom bolt mesh
                    //Create the BOTTOM seat bolts
                    boltMeshes.Clear();

                    //Setup the legs and shank length
                    sleg = (float)(angleFlip == false ? seat.Angle.b * METRIC : seat.Angle.d * METRIC);
                    bleg = (float)(angleFlip == false ? seat.Angle.d * METRIC : seat.Angle.b * METRIC);

                    shankLength = (float)(seat.Angle.t * METRIC + DrawingMethods.GetPrimaryThickness());
                    boltStartPos = new Vector3((float)(primaryWidth + (seat.Angle.t * METRIC + boltHeight / 2)) * Math.Sign(x), (float)-y - sleg + (float)seat.Bolt.EdgeDistLongDir * METRIC + yOffset,
                        (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistTransvDir * METRIC));

                    //Create the bottom column bolts
                    for (int i = 0; i < seat.Bolt.NumberOfLines; ++i)
                    {
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos - Vector3.forward * i * (float)seat.Bolt.SpacingTransvDir, Vector3.up * (float)seat.Bolt.SpacingTransvDir * METRIC, angleFlip ? 1 : seat.Bolt.NumberOfRows,
                            (float)seat.Bolt.BoltSize * METRIC, new Vector3(isRight ? 90 : 270, 90, 0), shankLength));
                    }

                    //Column side bolts
                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearSupport;

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    boltMeshes.Clear();

                    boltStartPos.z *= -1;

                    //Make the bottom beam bolts
                    shankLength = (float)(seat.Angle.t * METRIC + component.Shape.USShape.tf);
                    boltStartPos = new Vector3((float)(primaryWidth + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange)) * Math.Sign(x), (float)-y - (float)seat.Angle.t - boltHeight / 2 + yOffset,
                        (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingLongDir * METRIC * Mathf.Sign((float)-x), !angleFlip ? 1 : seat.Bolt.NumberOfRows,
                        (float)seat.Bolt.BoltSize * METRIC, new Vector3(-180, -90, 0), shankLength));

                    boltStartPos.z *= -1;
                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingLongDir * METRIC * Mathf.Sign((float)-x), !angleFlip ? 1 : seat.Bolt.NumberOfRows, (float)seat.Bolt.BoltSize * METRIC, new Vector3(-180, -90, 0), shankLength));

                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearBeam;

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    #endregion
                }
                else if (seat.Connection == ESeatConnection.WeldedStiffened)
                {
                    if (seat.Stiffener == ESeatStiffener.Plate || seat.Stiffener == ESeatStiffener.Tee)
                    {
                        //Create two bottom bolts
                        boltMeshes.Clear();

                        boltHeight = DrawingMethods.GetBoltHeightSize((float)seat.Bolt.BoltSize);

                        boltStartPos = new Vector3((float)((primaryWidth + component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback) * Math.Sign(x)), (float)(-y + yOffset + component.Shape.tf + boltHeight / 2), 
                            (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));

                        shankLength = (float)(seat.PlateThickness * METRIC + component.Shape.tf);

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, -90, 0), shankLength));

                        boltStartPos.z *= -1;

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, -90, 0), shankLength));

                        //Combine all the bolts into one mesh
                        MeshCreator.Reset();
                        MeshCreator.Add(boltMeshes.ToArray());

                        boltMesh = MeshCreator.Create();

                        //Create a connection object for those bolts
                        boltMember = MessageQueueTest.CreateMemberControl();

                        //Add it to the connection controls of the parent object
                        member.AddConnectionMember(boltMember);

                        boltMember.lineDrawing.lineOffset = -0.01f;
                        boltMember.shapeControl.Mesh = boltMesh;
                        boltMember.memberColor = MessageQueueTest.GetBoltColor();
                        boltMember.memberType = memberType;
                        boltMember.subType = EMemberSubType.BoltShearBeam;

                        //Create the lines
                        MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                    }
                }

                MemberDimensionMethods.AddShearSeatDimensions(component, x, shearConnectionChanged);

                break;

            case EShearCarriedBy.SinglePlate:

                //Clear the other right side shear dimensions and labels
                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Shear", "SinglePlate");

                //Create the connection object
                var singleMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(singleMember);

                singleMember.memberType = memberType;
                singleMember.subType = EMemberSubType.Shear;

                singleMember.lineDrawing.lineOffset = -0.01f;

                //Create the single plate mesh
                var singlePlate = component.WinConnect.ShearWebPlate;

                posOffset = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d * METRIC, singlePlate.Length * METRIC, singlePlate.Bolt.EdgeDistLongDir, singlePlate.Position);

                if (CommonDataStatic.JointConfig != EJointConfiguration.BeamSplice)
                {
                    //Apply the mesh
                    if (!isDirectlyWelded && singlePlate.WebPlateStiffener == EWebPlateStiffener.Without)
                    {
                        singleMember.shapeControl.Mesh = DrawingMethods.CreateSinglePlateMesh(singlePlate, new Vector3((float)(primaryWidth + singlePlate.Width / 2) * Math.Sign(x), posOffset + yOffset,
                            -(float)(component.Shape.USShape.tw + singlePlate.Thickness) / 2.0f), Vector3.zero);

                        //Create the second plate
                        if (singlePlate.NumberOfPlates > 1)
                        {
                            var mesh1 = singleMember.shapeControl.Mesh;

                            //Create second plate
                            var mesh2 = DrawingMethods.CreateSinglePlateMesh(singlePlate, new Vector3((float)(primaryWidth + singlePlate.Width / 2) * Math.Sign(x), posOffset + yOffset,
                            (float)(component.Shape.USShape.tw + singlePlate.Thickness) / 2.0f), Vector3.zero);

                            MeshCreator.Add(mesh1);
                            MeshCreator.Add(mesh2);

                            singleMember.shapeControl.Mesh = MeshCreator.Create();
                            mesh1 = null;
                            mesh2 = null;
                        }

                        //Create the lines
                        MemberLineMethods.CreateMeshLines(singleMember.lineDrawing, singleMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                    }
                    else
                    {
                        //Create the lines
                        MemberLineMethods.CreateDirectlyWeldedSinglePlateLines(singleMember.lineDrawing, component, x);

                        var meshes = new List<Mesh>();

                        meshes.Add(DrawingMethods.CreateDirectlyWeldedSinglePlateMesh(component, primaryComponent, new Vector3((float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(x)), yOffset, 
                            -(float)((component.Shape.USShape.tw + singlePlate.Thickness) / 2)), new Vector3(0, !isLeftSide ? 180 : 0, 0)));

                        //Create stiffener plates
                        if (singlePlate.WebPlateStiffener == EWebPlateStiffener.With)
                        {
                            var primaryShape = primaryComponent.Shape.USShape;
                            var plateHeight = (float)(singlePlate.Height * ConstNum.METRIC_MULTIPLIER);
                            var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
                            var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + singlePlate.Bolt.EdgeDistTransvDir);
                            var plate2Height = (float)singlePlate.Length;

                            if (singlePlate.WebPlateStiffener == EWebPlateStiffener.With)
                                plate2Width = (float)singlePlate.Width - plateWidth;

                            //Places the top of the bigger plate flush with the beam
                            var offset = (plateHeight - (float)component.Shape.d) / 2;
                            var offset2 = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.d, plate2Height, singlePlate.Bolt.EdgeDistLongDir, singlePlate.Position);

                            //TODO: Move stiffeners away from beam
                            meshes.Add(MeshCreator.CreateBoxMesh(new Vector3((float)((plateWidth / 2 + primaryComponent.Shape.USShape.tw / 2) * Math.Sign(x)), (float)((plateHeight + singlePlate.StiffenerThickness) / 2) - offset + yOffset, 0), new Vector3(plateWidth,
                                (float)singlePlate.StiffenerThickness, (float)(primaryShape.d - primaryShape.tf * 2)), Vector3.zero));

                            meshes.Add(MeshCreator.CreateBoxMesh(new Vector3((float)((plateWidth / 2 + primaryComponent.Shape.USShape.tw / 2) * Math.Sign(x)), (float)(-(plateHeight + singlePlate.StiffenerThickness) / 2) - offset + yOffset, 0), new Vector3(plateWidth,
                                (float)singlePlate.StiffenerThickness, (float)(primaryShape.d - primaryShape.tf * 2)), Vector3.zero));

                            MemberLineMethods.AddMeshLines(singleMember.lineDrawing, meshes[1], new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                            MemberLineMethods.AddMeshLines(singleMember.lineDrawing, meshes[2], new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                        }

                        MeshCreator.Reset();
                        MeshCreator.Add(meshes.ToArray());

                        singleMember.shapeControl.Mesh = MeshCreator.Create();
                    }
                }
                else
                {
                    //Create a solid single plate
                    if(isLeftSide)
                    {
                        singleMember.shapeControl.Mesh = DrawingMethods.CreateSinglePlateMesh(singlePlate, new Vector3(0, posOffset + yOffset,
                            -(float)(component.Shape.USShape.tw + singlePlate.Thickness) / 2.0f), Vector3.zero, true);

                        //Create the second plate
                        if (singlePlate.NumberOfPlates > 1)
                        {
                            var mesh1 = singleMember.shapeControl.Mesh;

                            //Create second plate
                            var mesh2 = DrawingMethods.CreateSinglePlateMesh(singlePlate, new Vector3(0, posOffset + yOffset,
                            (float)(component.Shape.USShape.tw + singlePlate.Thickness) / 2.0f), Vector3.zero, true);

                            MeshCreator.Add(mesh1);
                            MeshCreator.Add(mesh2);

                            singleMember.shapeControl.Mesh = MeshCreator.Create();
                            mesh1 = null;
                            mesh2 = null;
                        }

                        //Create the lines
                        MemberLineMethods.CreateMeshLines(singleMember.lineDrawing, singleMember.shapeControl.Mesh, new ViewMask[4] { ViewMask.FRONT, ViewMask.TOP, ViewMask.LEFT, ViewMask.RIGHT });
                    }
                }

                //Set the color
                singleMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bolts
                #region boltMeshes

                boltHeight = DrawingMethods.GetBoltHeightSize((float)singlePlate.Bolt.BoltSize);

                boltStartPos = new Vector3((float)(primaryWidth + (singlePlate.Width * METRIC - singlePlate.Bolt.EdgeDistTransvDir * METRIC)) * Math.Sign(x),
                    (float)DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, singlePlate.Length, singlePlate.Bolt.EdgeDistLongDir, singlePlate.Position) + yOffset,
                    -(float)(component.Shape.USShape.tw + boltHeight) / 2 - (float)singlePlate.Thickness * METRIC);

                shankLength = (float)(singlePlate.Thickness * METRIC + component.Shape.USShape.tw);

                if (singlePlate.NumberOfPlates > 1)
                {
                    shankLength += (float)(singlePlate.Thickness) * METRIC;
                }

                boltMeshes.Clear();

                for (int i = 0; i < singlePlate.Bolt.NumberOfLines; ++i)
                {
                    boltMesh = DrawingMethods.CreateBoltArray(boltStartPos + Vector3.left * i * (float)singlePlate.Bolt.SpacingTransvDir * METRIC * Math.Sign(x), Vector3.down * (float)singlePlate.Bolt.SpacingLongDir * METRIC,
                        singlePlate.Bolt.NumberOfRows, (float)singlePlate.Bolt.BoltSize * METRIC, new Vector3(-90, 0, 0), shankLength);
                    boltMeshes.Add(boltMesh);
                }

                //Combine all the bolts into one mesh
                MeshCreator.Reset();
                MeshCreator.Add(boltMeshes.ToArray());

                boltMesh = MeshCreator.Create();

                //Create a connection object for those bolts
                boltMember = MessageQueueTest.CreateMemberControl();

                //Add it to the connection controls of the parent object
                member.AddConnectionMember(boltMember);

                boltMember.lineDrawing.lineOffset = -0.01f;
                boltMember.shapeControl.Mesh = boltMesh;
                boltMember.memberColor = MessageQueueTest.GetBoltColor();
                boltMember.memberType = memberType;
                boltMember.subType = EMemberSubType.BoltShearBeam;

                //Create the lines
                MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                #endregion

                //Create the dimensions
                MemberDimensionMethods.AddShearSinglePlateDimensions(component, boltStartPos.y, x, shearConnectionChanged);

                break;

            case EShearCarriedBy.Tee:

                //Clear the other right side shear dimensions and labels
                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Shear", "Tee");

                //Create the connection object
                var teeMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(teeMember);

                teeMember.memberType = memberType;
                teeMember.subType = EMemberSubType.Shear;

                teeMember.lineDrawing.lineOffset = -0.01f;

                //Create the single plate mesh
                var tee = component.WinConnect.ShearWebTee;

                posOffset = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d * METRIC, tee.SLength * METRIC, tee.BoltWebOnStem.EdgeDistLongDir, tee.Position);

                //Apply the mesh
                teeMember.shapeControl.Mesh = DrawingMethods.CreateTeeMesh(tee, new Vector3((float)primaryWidth * Math.Sign(x), posOffset + yOffset, -(float)(component.Shape.USShape.tw / 2 + tee.Size.USShape.tw / 2)), new Vector3(0, !isLeftSide ? 180 : 0, 0));

                //Create the lines
                //TODO Directly welded case
                MemberLineMethods.CreateMeshLines(teeMember.lineDrawing, teeMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                teeMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bolts
                #region Bolts

                boltMeshes.Clear();

                var viewSide = (Mathf.Sign((float)x) < 0) ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

                if (tee != null)
                {
                    //Bolts on beam
                    if (tee.BeamSideConnection == EConnectionStyle.Bolted)
                    {
                        boltHeight = DrawingMethods.GetBoltHeightSize((float)tee.BoltWebOnStem.BoltSize);
                        boltStartPos = new Vector3((float)(primaryWidth + (tee.Size.d * METRIC - tee.BoltWebOnStem.EdgeDistTransvDir * METRIC)) * Math.Sign(x),
                            DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, tee.SLength * METRIC, tee.BoltWebOnStem.EdgeDistLongDir * METRIC, tee.Position) + yOffset,
                            -(float)(component.Shape.USShape.tw / 2 * METRIC + tee.Size.tw * METRIC + boltHeight / 2));
                        shankLength = (float)(tee.Size.tw * METRIC + component.Shape.USShape.tw);

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)tee.BoltWebOnStem.SpacingLongDir * METRIC, tee.StemNumberOfRows, (float)tee.BoltWebOnStem.BoltSize * METRIC, new Vector3(0, -90, 90), shankLength));
                    }

                    //Beam bolt meshes
                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearBeam;

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                    boltMeshes.Clear();

                    //Bolts on Support side
                    if (tee.OSLConnection == EConnectionStyle.Bolted)
                    {
                        boltHeight = DrawingMethods.GetBoltHeightSize((float)tee.BoltOslOnFlange.BoltSize);
                        boltStartPos = new Vector3((float)(primaryWidth + (tee.Size.tf * METRIC + boltHeight / 2)) * Math.Sign(x),
                            DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, tee.SLength * METRIC, tee.BoltOslOnFlange.EdgeDistLongDir * METRIC, tee.Position) + yOffset,
                            -(float)(component.Shape.USShape.tw * METRIC / 2 + tee.Size.bf * METRIC / 2 - tee.BoltOslOnFlange.EdgeDistTransvDir * METRIC));
                        shankLength = (float)(tee.Size.tf * METRIC + DrawingMethods.GetPrimaryThickness());

                        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice || CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
                        {
                            if(DrawingMethods.BothSidesActive())
                                shankLength += (float)(tee.Size.tf * METRIC);
                        }

                        var isLeft = x < 0;

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)tee.BoltOslOnFlange.SpacingLongDir * METRIC, tee.FlangeNumberOfRows, (float)tee.BoltOslOnFlange.BoltSize * METRIC, new Vector3(isLeft ? 270 : 90, 90, 0), shankLength));

                        //Add bolts to the other side
                        boltStartPos.z = -(float)(component.Shape.USShape.tw * METRIC - tee.Size.bf * METRIC / 2 + tee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * (float)tee.BoltOslOnFlange.SpacingLongDir * METRIC, tee.FlangeNumberOfRows, (float)tee.BoltOslOnFlange.BoltSize * METRIC, new Vector3(isLeft ? 270 : 90, 90, 0), shankLength));
                    }

                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltShearSupport;

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                }


                #endregion

                //Create dimendions
                MemberDimensionMethods.AddShearTeeDimensions(component, x, shearConnectionChanged);

                break;
        }

        switch (component.MomentConnection)
        {
            case EMomentCarriedBy.FlangePlate:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "FlangePlate");

                //Create the connection object
                var fplateMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(fplateMember);

                fplateMember.memberType = memberType;
                fplateMember.subType = EMemberSubType.Moment;

                fplateMember.lineDrawing.lineOffset = -0.01f;

                //Create the single plate mesh
                var fplate = component.WinConnect.MomentFlangePlate;

                //Apply the mesh
                fplateMember.shapeControl.Mesh = DrawingMethods.CreateFlangePlateMesh(fplate, new Vector3((float)(primaryWidth + fplate.TopLength * METRIC / 2) * Math.Sign(x),
                    (float)(component.Shape.USShape.d / 2 + yOffset + fplate.TopThickness / 2 * METRIC)), true, isLeftSide ? true : false);

                //Create the lines
                MemberLineMethods.CreateMeshLines(fplateMember.lineDrawing, fplateMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                fplateMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bottom plate
                fplateMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(fplateMember);

                fplateMember.memberType = memberType;
                fplateMember.subType = EMemberSubType.Moment;

                fplateMember.lineDrawing.lineOffset = -0.01f;

                //Apply the mesh
                fplateMember.shapeControl.Mesh = DrawingMethods.CreateFlangePlateMesh(fplate, new Vector3((float)(primaryWidth + fplate.TopLength * METRIC / 2) * Math.Sign(x),
                    (float)(-component.Shape.USShape.d / 2 + yOffset - fplate.TopThickness / 2 * METRIC)), false, isLeftSide ? true : false);

                //Create the lines
                MemberLineMethods.CreateMeshLines(fplateMember.lineDrawing, fplateMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                fplateMember.memberColor = MessageQueueTest.GetConnectionColor();


                //Create the bolts
                #region Bolts

                if (fplate != null)
                {
                    boltMeshes.Clear();

                    var DistanceToFirstBoltDisplay = (float)(component.EndSetback * METRIC + component.WinConnect.Beam.BoltEdgeDistanceOnFlange * METRIC);

                    boltHeight = DrawingMethods.GetBoltHeightSize((float)fplate.Bolt.BoltSize);
                    boltStartPos = new Vector3((float)((primaryWidth + DistanceToFirstBoltDisplay) * Math.Sign(x)), (float)(component.Shape.USShape.d / 2 + fplate.TopThickness * METRIC + boltHeight / 2) + yOffset, (float)(fplate.TopWidth * METRIC / 2 - fplate.Bolt.EdgeDistTransvDir * METRIC));

                    shankLength = (float)(fplate.TopThickness * METRIC + component.Shape.USShape.tf);

                    //Create 1st set of Flange bolts
                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                        (float)fplate.Bolt.BoltSize * METRIC, Vector3.zero, shankLength));

                    //Create 2st set of Flange bolts
                    boltStartPos.z *= -1;

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                        (float)fplate.Bolt.BoltSize * METRIC, Vector3.zero, shankLength));

                    //Create the bottom set of bolts
                    boltStartPos.y = (float)(component.Shape.USShape.d / 2 + fplate.BottomThickness * METRIC + boltHeight / 2) + yOffset;
                    boltStartPos.z = (float)(fplate.BottomWidth * METRIC / 2 - fplate.Bolt.EdgeDistTransvDir * METRIC);
                    boltStartPos.y = -(float)(component.Shape.USShape.d / 2 + fplate.BottomThickness * METRIC + boltHeight / 2) + yOffset;

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                        (float)fplate.Bolt.BoltSize * METRIC, new Vector3(180, 0, 0), shankLength));

                    //Create 4rd set of Flange bolts
                    boltStartPos.z *= -1;

                    boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                        (float)fplate.Bolt.BoltSize * METRIC, new Vector3(180, 0, 0), shankLength));

                    //Combine all the bolts into one mesh
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    boltMesh = MeshCreator.Create();

                    //Create a connection object for those bolts
                    boltMember = MessageQueueTest.CreateMemberControl();

                    //Add it to the connection controls of the parent object
                    member.AddConnectionMember(boltMember);

                    boltMember.lineDrawing.lineOffset = -0.01f;
                    boltMember.shapeControl.Mesh = boltMesh;
                    boltMember.memberColor = MessageQueueTest.GetBoltColor();
                    boltMember.memberType = memberType;
                    boltMember.subType = EMemberSubType.BoltMomentBeam;

                    //Create the lines
                    MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                }

                #endregion

                //Create dimendions
                MemberDimensionMethods.AddMomentFlangePlateDimensions(component, x, momentConnectionChanged);

                break;

            case EMomentCarriedBy.Tee:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "MTee");

                //Create the connection object
                var teeMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(teeMember);

                teeMember.memberType = memberType;
                teeMember.subType = EMemberSubType.Moment;

                teeMember.lineDrawing.lineOffset = -0.01f;

                //Create the single plate mesh
                var tee = component.WinConnect.MomentTee;

                //Apply the mesh
                teeMember.shapeControl.Mesh = DrawingMethods.CreateTeeMesh(tee, new Vector3((float)(primaryWidth) * Math.Sign(x),
                    (float)(component.Shape.USShape.d / 2 + yOffset + tee.TopTeeShape.USShape.tw / 2 * METRIC)), new Vector3(90, isLeftSide ? 0 : 180, 0));

                //Create the lines
                MemberLineMethods.CreateMeshLines(teeMember.lineDrawing, teeMember.shapeControl.Mesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                teeMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bottom tee
                teeMember = MessageQueueTest.CreateMemberControl();
                //add it to the connection controls of the parent object
                member.AddConnectionMember(teeMember);

                teeMember.memberType = memberType;
                teeMember.subType = EMemberSubType.Moment;

                teeMember.lineDrawing.lineOffset = -0.01f;

                //Apply the mesh
                teeMember.shapeControl.Mesh = DrawingMethods.CreateTeeMesh(tee, new Vector3((float)(primaryWidth) * Math.Sign(x),
                    (float)(-component.Shape.USShape.d / 2 + yOffset - tee.BottomTeeShape.USShape.tw / 2 * METRIC)), new Vector3(90, isLeftSide ? 0 : 180, 0));

                //Create the lines
                MemberLineMethods.CreateMeshLines(teeMember.lineDrawing, teeMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                //Set the color
                teeMember.memberColor = MessageQueueTest.GetConnectionColor();

                //Create the bolts
                #region Bolts

                if (tee != null)
                {
                    boltMeshes.Clear();

                    var tw = tee.TopTeeShape.USShape.tw;
                    var tf = tee.TopTeeShape.USShape.tf;

                    var teeLength = tee.TopLengthAtStem;

                    //Create the bolts
                    if (tee.TeeConnectionStyle == EConnectionStyle.Bolted)
                    {
                        boltHeight = DrawingMethods.GetBoltHeightSize((float)tee.BoltColumnFlange.BoltSize);
                        boltStartPos = new Vector3((float)((primaryWidth + tf + boltHeight / 2) * Math.Sign(x)), (float)(component.Shape.USShape.d / 2 + tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC + tw / 2) + yOffset, (float)(tee.Column_g / 2 * METRIC));
                        shankLength = (float)(tf + DrawingMethods.GetPrimaryThickness());

                        var spacing = (float)(tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC) * 2;

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));

                        boltStartPos.z *= -1;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.down * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));

                        boltStartPos.y = (float)(-component.Shape.USShape.d / 2 - tee.TopTeeShape.USShape.bf / 2 + tee.BoltColumnFlange.EdgeDistTransvDir * METRIC - tw / 2) + yOffset;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.up * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));

                        boltStartPos.z *= -1;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.up * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, 90, 0), shankLength));

                        //Support bolts
                        //Combine all the bolts into one mesh
                        MeshCreator.Reset();
                        MeshCreator.Add(boltMeshes.ToArray());

                        boltMesh = MeshCreator.Create();

                        //Create a connection object for those bolts
                        boltMember = MessageQueueTest.CreateMemberControl();

                        //Add it to the connection controls of the parent object
                        member.AddConnectionMember(boltMember);

                        boltMember.lineDrawing.lineOffset = -0.01f;
                        boltMember.shapeControl.Mesh = boltMesh;
                        boltMember.memberColor = MessageQueueTest.GetBoltColor();
                        boltMember.memberType = memberType;
                        boltMember.subType = EMemberSubType.BoltMomentSupport;

                        //Create the lines
                        MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });

                        boltMeshes.Clear();

                        //Create beam bolts
                        boltHeight = DrawingMethods.GetBoltHeightSize((float)tee.BoltBeamStem.BoltSize);
                        boltStartPos = new Vector3((float)((primaryWidth + tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC) * Math.Sign(x)), (float)(component.Shape.USShape.d / 2 + tw) + yOffset + boltHeight / 2, (float)(-tee.Beam_g / 2));
                        shankLength = (float)(tw + component.Shape.USShape.tf);

                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength));

                        boltStartPos.z *= -1;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength));

                        boltStartPos.y = -(float)(component.Shape.USShape.d / 2 + tw) + yOffset - boltHeight / 2;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(180, 90, 0), shankLength));

                        boltStartPos.z *= -1;
                        boltMeshes.Add(DrawingMethods.CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(180, 90, 0), shankLength));

                        //Combine all the bolts into one mesh
                        MeshCreator.Reset();
                        MeshCreator.Add(boltMeshes.ToArray());

                        boltMesh = MeshCreator.Create();

                        //Create a connection object for those bolts
                        boltMember = MessageQueueTest.CreateMemberControl();

                        //Add it to the connection controls of the parent object
                        member.AddConnectionMember(boltMember);

                        boltMember.lineDrawing.lineOffset = -0.01f;
                        boltMember.shapeControl.Mesh = boltMesh;
                        boltMember.memberColor = MessageQueueTest.GetBoltColor();
                        boltMember.memberType = memberType;
                        boltMember.subType = EMemberSubType.BoltMomentBeam;

                        //Create the lines
                        MemberLineMethods.CreateMeshLines(boltMember.lineDrawing, boltMesh, new ViewMask[3] { ViewMask.FRONT, ViewMask.TOP, isLeftSide ? ViewMask.LEFT : ViewMask.RIGHT });
                    }
                }

                #endregion

                //Create dimendions
                MemberDimensionMethods.AddMomentTeeDimensions(component, x, momentConnectionChanged);

                break;

            case EMomentCarriedBy.Angles:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "MAngle");

                break;

            case EMomentCarriedBy.DirectlyWelded:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "DirectlyWelded");

                if (isDirectlyWelded)
                {
                    //Create the connection object
                    var dMember = MessageQueueTest.CreateMemberControl();
                    //add it to the connection controls of the parent object
                    member.AddConnectionMember(dMember);

                    dMember.memberType = memberType;
                    dMember.subType = EMemberSubType.Moment;

                    dMember.lineDrawing.lineOffset = -0.01f;

                    MeshCreator.Reset();

                    //Create the continuity plates
                    //Make those wedge things
                    var points = new float[20];
                    points[0] = (float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(x));
                    points[1] = (float)(component.WinConnect.ShearWebPlate.Height / 2 + yOffset + component.WinConnect.MomentDirectWeld.Top.StiffenerThickness / 2);
                    points[2] = (float)component.WinConnect.ShearWebPlate.Clip;
                    points[3] = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
                    points[4] = (float)component.WinConnect.MomentDirectWeld.Top.a;
                    points[5] = (float)component.Shape.USShape.bf;
                    points[6] = (float)(primaryComponent.Shape.USShape.d - primaryComponent.Shape.USShape.tf * 2);
                    points[7] = (float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness;
                    points[8] = (float)component.WinConnect.MomentDirectWeld.Bottom.a;
                    points[9] = (float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness;
                    points[10] = -(float)component.WinConnect.ShearWebPlate.Height / 2 + yOffset - points[9] / 2;
                    points[11] = (float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf);
                    points[12] = (float)(primaryComponent.Shape.USShape.bf / 2 * Math.Sign(x));
                    points[13] = points[12] + points[4] * Math.Sign(x);

                    boltMeshes.Clear();

                    //Create the top wedge
                    boltMeshes.Add(MeshCreator.CreateWedgePlate(new Vector3(points[0], points[1], 0), points[2], points[3], points[4], points[5], points[6], points[7], new Vector3(0, !isLeftSide ? 180 : 0, 0)));

                    //Create the bottom wedge
                    boltMeshes.Add(MeshCreator.CreateWedgePlate(new Vector3(points[0], points[10], 0), points[2], points[3], points[8], points[5], points[6], points[9], new Vector3(0, !isLeftSide ? 180 : 0, 0)));

                    //Apply the mesh
                    dMember.shapeControl.Mesh = MeshCreator.Combine(boltMeshes.ToArray());

                    //Create the lines
                    MemberLineMethods.CreateContinuityPlateLines(dMember.lineDrawing, component, x);

                    //Set the color
                    dMember.memberColor = MessageQueueTest.GetConnectionColor();
                }

                if(useCutoutWeld)
                {
                    //Add the dimensions
                    MemberDimensionMethods.AddMomentDirectlyWeldedDimensions(component, x, momentConnectionChanged);
                }

                break;

            case EMomentCarriedBy.NoMoment:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "NoMoment");

                break;

            case EMomentCarriedBy.EndPlate:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore(side + " Moment", "MEndPlate");

                break;
        }
    }

    private static void CreateStiffeners(MemberControl2 mainColumnMember)
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();

        var component = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        //Flipped
        var rBeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        var lBeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];

        var leftStiffenerTop = 0.0f;
        var rightStiffenerTop = 0.0f;

        var leftStiffenerBot = 0.0f;
        var rightStiffenerBot = 0.0f;

        Mesh mesh = null;
        List<Mesh> connectionMeshes = new List<Mesh>();

        if (!lBeam.IsActive && !rBeam.IsActive)
            return;

        if (lBeam.Moment.Equals(0) && rBeam.Moment.Equals(0))
        {
            MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener");
            return;
        }

        if (lBeam.IsActive && !rBeam.IsActive)
        {
            leftStiffenerTop = (float)(lBeam.Shape.USShape.d / 2 + DrawingMethods.GetYOffsetWithEl(lBeam));

            if (lBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
                leftStiffenerTop += (float)(lBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            else
                leftStiffenerTop -= (float)lBeam.Shape.USShape.tf / 2;

            rightStiffenerTop = leftStiffenerTop;

            leftStiffenerBot = -(float)(lBeam.Shape.USShape.d / 2 + (float)DrawingMethods.GetYOffsetWithEl(lBeam));

            if (lBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
                leftStiffenerBot -= (float)(lBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            else
                leftStiffenerBot += (float)lBeam.Shape.USShape.tf / 2;

            rightStiffenerBot = leftStiffenerBot;
        }
        else if (!lBeam.IsActive && rBeam.IsActive)
        {
            leftStiffenerTop = (float)(rBeam.Shape.USShape.d / 2 + (float)DrawingMethods.GetYOffsetWithEl(rBeam));

            if (rBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
                leftStiffenerTop += (float)(rBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            else
                leftStiffenerTop -= (float)rBeam.Shape.USShape.tf / 2;

            rightStiffenerTop = leftStiffenerTop;

            leftStiffenerBot = -(float)(rBeam.Shape.USShape.d / 2 + (float)DrawingMethods.GetYOffsetWithEl(rBeam));

            if (rBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
                leftStiffenerBot -= (float)(rBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            else
                leftStiffenerBot += (float)rBeam.Shape.USShape.tf / 2;

            rightStiffenerBot = leftStiffenerBot;
        }
        else
        {
            leftStiffenerTop = (float)(lBeam.Shape.USShape.d / 2) + (float)DrawingMethods.GetYOffsetWithEl(lBeam);
            rightStiffenerTop = (float)(rBeam.Shape.USShape.d / 2) + (float)DrawingMethods.GetYOffsetWithEl(rBeam);

            leftStiffenerBot = -(float)(lBeam.Shape.USShape.d / 2 )+ (float)DrawingMethods.GetYOffsetWithEl(lBeam);
            rightStiffenerBot = -(float)(rBeam.Shape.USShape.d / 2) + (float)DrawingMethods.GetYOffsetWithEl(rBeam);

            if (lBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
            {
                leftStiffenerTop += (float)(lBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
                leftStiffenerBot -= (float)(lBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            }
            else
            {
                leftStiffenerTop -= (float)lBeam.Shape.USShape.tf / 2;
                leftStiffenerBot += (float)lBeam.Shape.USShape.tf / 2;
            }

            if (rBeam.MomentConnection == EMomentCarriedBy.FlangePlate)
            {
                rightStiffenerTop += (float)(rBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
                rightStiffenerBot -= (float)(rBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2);
            }
            else
            {
                rightStiffenerTop -= (float)rBeam.Shape.USShape.tf / 2;
                rightStiffenerBot += (float)rBeam.Shape.USShape.tf / 2;
            }
        }

        var columnStiffener = CommonDataStatic.ColumnStiffener;

        if (columnStiffener != null)
        {
            //Stiffener sizes
            var width = (float)columnStiffener.SWidth * (float)ConstNum.METRIC_MULTIPLIER;
            var length = (float)columnStiffener.SLength * (float)ConstNum.METRIC_MULTIPLIER;
            var thickness = (float)columnStiffener.SThickness * (float)ConstNum.METRIC_MULTIPLIER;
            var frontZOffset = 0.0f;
            var rearZOffset = 0.0f;
            var calloutText = "";

            //Front backing plates
            if (columnStiffener.DNumberOfPlates >= 1)
            {
                frontZOffset = (float)columnStiffener.DThickness;

                //Create the backing plate
                mesh = MeshCreator.CreateBoxMesh(new Vector3(0, 0, -(float)(component.Shape.USShape.tw / 2 + columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER / 2)), new Vector3((float)columnStiffener.DHorizontal * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DVertical * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER), Vector3.zero);
                connectionMeshes.Add(mesh);

                calloutText = (columnStiffener.DNumberOfPlates > 1 ? "2" : "") + "PL " + columnStiffener.DVertical * METRIC + unitString + "X " + columnStiffener.DHorizontal * METRIC + unitString + "X " +
                    columnStiffener.DThickness * METRIC + unitString + " - " + columnStiffener.MaterialName;

                calloutText += "\nIf using groove weld,";
                calloutText += "\nPL. thick. = 0.1875 " + unitString;
                calloutText += "\nPlate to Flange Weld:";
                calloutText += "\n" + DrawingMethods.GetFootString((float)columnStiffener.DFlangeWeldSize) + " Fillet or equivalent Groove weld";
                calloutText += "\nPl. to Stiff. Weld: " + DrawingMethods.GetFootString((float)columnStiffener.DWebWeldSize);
                calloutText += "\nFillet or equivalent Groove weld";

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Column Stiffener doubler text", calloutText, new Vector3(0, 0, 0),
                    new Vector3(-20, -20, 0));
            }
            else
            {
                MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener doubler text");
            }

            //Rear backing plates
            if (columnStiffener.DNumberOfPlates > 1)
            {
                rearZOffset = (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER;

                mesh = MeshCreator.CreateBoxMesh(new Vector3(0, 0, (float)(component.Shape.USShape.tw / 2 + columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER / 2)), new Vector3((float)columnStiffener.DHorizontal * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DVertical * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER), Vector3.zero);
                connectionMeshes.Add(mesh);
            }

            //Top stiffener
            if (columnStiffener.TenStiff)
            {
                var leftPoint = new Vector3(-(float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), leftStiffenerTop, 0);
                var rightPoint = new Vector3((float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), rightStiffenerTop, 0);

                if(columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
                {
                    if (columnStiffener.LeftStiffenerRequired && !columnStiffener.RightStiffenerRequired)
                        rightPoint.x = 0;

                    if (!columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                        leftPoint.x = 0;
                }

                //First, if only one side needs stiffener
                if (CommonDataStatic.ColumnStiffener.StiffenerType == EStiffenerType.HoritzotalTwo)
                {
                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                }
                else
                {
                    if (columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                    {
                        //Inclined
                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);

                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                    }
                    else
                    {
                        //Horizontal stiffeners
                        if (columnStiffener.LeftStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                        }
                        else if (columnStiffener.RightStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                        }
                    }
                }

                var clip = Math.Max(primaryComponent.Shape.kdet - primaryComponent.Shape.tf, columnStiffener.DThickness);

                //Create the callout description
                calloutText = "PL " + length + unitString + "X " + width + unitString + "X " + thickness + unitString + "(TYP. 4) - " + columnStiffener.MaterialName;
                calloutText += "\n" + "Clip inside corners " + clip * METRIC + unitString + " Max";
                calloutText += "\n" + "Plate to Flange Weld: " + DrawingMethods.GetFootString((float)columnStiffener.SFlangeWeldSize) + " Double Fillet";
                calloutText += "\n" + "Stiff. Pl to Dbl.Pl Weld: " + DrawingMethods.GetFootString((float)columnStiffener.SWebWeldSize) + " Fillet";
                calloutText += "\n" + "Stiff. Pl to Web Weld: " + DrawingMethods.GetFootString((float)columnStiffener.SWebWeldSize) + " Fillet";

                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Column Stiffener " + " stiffener text top", calloutText, new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2),
                    new Vector3(10, 0, 15));
            }
            else
            {
                MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener " + " stiffener text top");
            }

            //Bottom Stiffener
            if (columnStiffener.CompStiff)
            {
                var leftPoint = new Vector3(-(float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), leftStiffenerBot, 0);
                var rightPoint = new Vector3((float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), rightStiffenerBot, 0);

                if (columnStiffener.StiffenerLength == EStiffenerLength.PartialLengthOfWeb)
                {
                    if (columnStiffener.LeftStiffenerRequired && !columnStiffener.RightStiffenerRequired)
                        rightPoint.x = 0;

                    if (!columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                        leftPoint.x = 0;
                }

                //First, if only one side needs stiffener
                if (CommonDataStatic.ColumnStiffener.StiffenerType == EStiffenerType.HoritzotalTwo)
                {
                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                }
                else
                {
                    if (columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                    {
                        //Inclined
                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);

                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                    }
                    else
                    {
                        //Horizontal stiffeners
                        if (columnStiffener.LeftStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                        }
                        else if (columnStiffener.RightStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - frontZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + rearZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                        }
                    }
                }

            }
        }
        else
        {
            //MessageQueueTest.ClearLabelsAndDimensions("Column Stiffener " + " stiffener text");
        }

        //Create the connection object
        var stiffMember = MessageQueueTest.CreateMemberControl();
        //add it to the connection controls of the parent object
        mainColumnMember.AddConnectionMember(stiffMember);

        stiffMember.memberType = EMemberType.PrimaryMember;
        stiffMember.subType = EMemberSubType.Stiffener;

        stiffMember.lineDrawing.lineOffset = -0.01f;

        MeshCreator.Reset();

        //Apply the mesh
        stiffMember.shapeControl.Mesh = MeshCreator.Combine(connectionMeshes.ToArray());

        //Create the lines
        //TODO Directly welded case
        MemberLineMethods.CreateMeshLines(stiffMember.lineDrawing, stiffMember.shapeControl.Mesh, new ViewMask[2] { ViewMask.FRONT, ViewMask.TOP});

        //Set the color
        stiffMember.memberColor = MessageQueueTest.GetConnectionColor();
    }
}