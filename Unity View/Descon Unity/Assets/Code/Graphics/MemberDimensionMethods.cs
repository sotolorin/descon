using UnityEngine;
using System.Collections;
using Descon.Data;
using System;

public class MemberDimensionMethods : MonoBehaviour
{
    public static void AddShearClipAngleDimensions(DetailData component, double x, bool changed)
    {
        var wclip = component.WinConnect.ShearClipAngle;
        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var isLongLegOSL = component.WinConnect.ShearClipAngle.OSL == EOSL.LongLeg;
        var isRight = Math.Sign(x) > 0;
        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;
        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        var beamFirstBoltY = DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, wclip.Length,
            wclip.BoltWebOnBeam.EdgeDistLongDir, wclip.Position) + yOffset;
        var supportFirstBoltY = DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, wclip.Length,
            wclip.BoltOnColumn.EdgeDistLongDir, wclip.Position) + yOffset;

        isRight = !isRight;

        var legLength = isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC;

        var edgeDist = (legLength) - wclip.BoltOslOnSupport.EdgeDistTransvDir * METRIC;

        if (wclip.BeamSideConnection == EConnectionStyle.Bolted)
        {
            //Destroy the weld label
            MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle" + " BeamWelded");

            var edgeDistance = (float)(component.WinConnect.Beam.Lh + component.EndSetback);

            var points = new float[3];
            points[0] = (float)DrawingMethods.GetPrimaryWidth();
            points[1] = edgeDistance;
            points[2] = (points[0] + points[1]) * Math.Sign(x);

            //Distance from top of angle to first bolt - long spacing
            labelName = DrawingMethods.GetFootString((float)wclip.BoltWebOnBeam.EdgeDistLongDir);
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 7" + " BeamBolted", labelName,
                new Vector3(points[2], beamFirstBoltY + (float)wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC), new Vector3(points[2], beamFirstBoltY), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

            var extraV = 0.0f;

            if (wclip.BoltStagger == EBoltStagger.Support)
            {
                extraV = (float)wclip.BoltWebOnBeam.SpacingLongDir * METRIC * 0.5f;
            }

            //Bolt callouts
            var total = (wclip.BoltWebOnBeam.NumberOfRows - 1) * wclip.BoltWebOnBeam.SpacingLongDir * METRIC;
            labelName = (wclip.BoltWebOnBeam.NumberOfRows - 1).ToString() + "@" + wclip.BoltWebOnBeam.SpacingLongDir.ToString() + "=" + DrawingMethods.GetFootString((float)(total / METRIC));
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 8" + " BeamBolted", labelName,
                new Vector3(points[2], beamFirstBoltY - extraV),
                new Vector3(points[2], beamFirstBoltY - (float)(total) - extraV), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

            //Number of bolts callout
            var sgage = edgeDistance;//component.WinConnect.Beam.BoltEdgeDistanceOnFlange;// component.EndSetback + component.WinConnect.Beam.Lh; //This gage value is the distance from the bolt line to the face of the column
            labelName = (wclip.BoltWebOnBeam.NumberOfRows).ToString() + " Bolts\n" + wclip.BoltWebOnBeam.BoltName + "\nGage = " + sgage + unitString;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "num bolts label 2" + " BeamBolted", labelName,
                new Vector3(points[2], beamFirstBoltY - (float)total), new Vector3(8 * Math.Sign(x), -8, 0), false, null, false, changed);

            //Distance from TOB to front plate
            labelName = DrawingMethods.GetFootString((float)component.WinConnect.Beam.DistanceToFirstBoltDisplay);

            var top = CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder ? DrawingMethods.GirderOffset(component) + DrawingMethods.GetTopEl(component) : yOffset;

            var edge2 = (float)(DrawingMethods.GetPrimaryWidth() + ((isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC) - wclip.BoltWebOnBeam.EdgeDistTransvDir * METRIC)) * Math.Sign(x);
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 6" + " BeamBolted", labelName,
                new Vector3(edge2, (float)component.Shape.USShape.d / 2 + top, 0), new Vector3(edge2, beamFirstBoltY, 0), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());
        }
        else
        {
            //Destroy the bolt labels
            MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle" + " BeamBolted");

            //Draw the weld symbol
            var calloutName = "    \n";
            calloutName += DrawingMethods.GetFootString((float)wclip.WeldSizeBeam);
            var yy = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, wclip.Length, wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC, wclip.Position) + yOffset;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip weld" + " BeamWelded", calloutName, 
                new Vector3((float)((DrawingMethods.GetPrimaryWidth() + (!isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC)) * Math.Sign(x)), yy, 0), new Vector3(12 * Math.Sign(x), -15, 0), 4, "Three\nSides", -53, 110);
        }

        var bottomPlatePointY = (float)(beamFirstBoltY + wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC - wclip.Length);

        if (wclip.BeamSideConnection == EConnectionStyle.Welded)
        {
            bottomPlatePointY = DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, wclip.Length, wclip.BoltWebOnBeam.EdgeDistLongDir, wclip.Position) + yOffset;
            bottomPlatePointY = (float)(bottomPlatePointY + wclip.BoltWebOnBeam.EdgeDistLongDir - wclip.Length);
        }

        if (wclip.SizeType == EWebConnectionSize.L2)
            labelName = "2L";
        else
            labelName = "L";

        labelName += wclip.LongLeg + unitString + "X" + wclip.ShortLeg + unitString + "X" + wclip.Thickness + unitString + "X" + wclip.Length + unitString + " - " + wclip.MaterialName;
        MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip label", labelName,
            new Vector3(0, bottomPlatePointY, -(float)(legLength * METRIC + component.Shape.USShape.tw / 2)), new Vector3(0, -10, -15), false, null, false, changed);

        if (wclip.SupportSideConnection == EConnectionStyle.Bolted)
        {
            //Destroy the weld label
            MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle" + " SupportWelded");

            //Distance from top of angle to first bolt - long spacing
            labelName = DrawingMethods.GetFootString((float)wclip.BoltOslOnSupport.EdgeDistLongDir);
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 2" + " SupportBolted", labelName,
                new Vector3(0, supportFirstBoltY + (float)wclip.BoltOslOnSupport.EdgeDistLongDir * METRIC, -(float)edgeDist), new Vector3(0, supportFirstBoltY, -(float)edgeDist), 5 * Math.Sign(x), 
                MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

            var extraV = 0.0f;

            if (wclip.BoltStagger == EBoltStagger.Support)
            {
                extraV = (float)wclip.BoltOslOnSupport.SpacingLongDir * METRIC * 0.5f;
            }

            //Bolt callouts
            var total = (wclip.BoltOslOnSupport.NumberOfRows - 1) * wclip.BoltOslOnSupport.SpacingLongDir * METRIC;
            labelName = (wclip.BoltOslOnSupport.NumberOfRows - 1).ToString() + "@" + wclip.BoltOslOnSupport.SpacingLongDir.ToString() + "=" + DrawingMethods.GetFootString((float)(total / METRIC));
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 3" + " SupportBolted", labelName,
                new Vector3(0, supportFirstBoltY - extraV, -(float)edgeDist),
                new Vector3(0, supportFirstBoltY - (float)(total) - extraV, -(float)edgeDist), 15 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

            //Number of bolts callout
            labelName = (wclip.BoltOslOnSupport.NumberOfRows * (wclip.SizeType == EWebConnectionSize.L2 ? 2 : 1)).ToString() + " Bolts\n" + wclip.BoltOslOnSupport.BoltName;
            MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "num bolts label" + " SupportBolted", labelName,
                new Vector3(0, supportFirstBoltY - (float)total, -(float)(edgeDist + component.Shape.USShape.tw / 2)), new Vector3(0, -5, -10), false, null, false, changed);

            if (wclip.SizeType == EWebConnectionSize.L2)
            {
                //Gage callout
                labelName = DrawingMethods.GetFootString((float)component.GageOnColumn);//(((float)((edgeDist) + component.Shape.USShape.tw / 2) * 2) / METRIC).ToString() + unitString;
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 4" + " SupportBolted", labelName,
                    new Vector3(0, supportFirstBoltY - (float)total, (float)((edgeDist) + component.Shape.USShape.tw / 2)),
                    new Vector3(0, supportFirstBoltY - (float)total, -(float)((edgeDist) + component.Shape.USShape.tw / 2)), -7 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
            }
            else
            {
                //Single clip angle
                //Gage callout
                labelName = DrawingMethods.GetFootString((float)component.GageOnColumn / 2);//(((float)((edgeDist) + component.Shape.USShape.tw / 2) * 2) / METRIC).ToString() + unitString;
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 4" + " SupportBolted", labelName,
                    new Vector3(0, supportFirstBoltY - (float)total, 0),
                    new Vector3(0, supportFirstBoltY - (float)total, -(float)((edgeDist) + component.Shape.USShape.tw / 2)), -7 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
            }

            var top = CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder ? DrawingMethods.GirderOffset(component) + DrawingMethods.GetTopEl(component) : yOffset;

            //Distance from TOB to first bolt
            labelName = DrawingMethods.GetFootString((float)component.WinConnect.Beam.DistanceToFirstBoltDisplay);
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 1" + " SupportBolted", labelName,
                new Vector3(0, (float)component.Shape.USShape.d / 2 + top, -(float)edgeDist), new Vector3(0, supportFirstBoltY, -(float)edgeDist), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

        }
        else
        {
            //Destroy the bolt labels
            MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle" + " SupportBolted");

            //Draw the weld symbol
            var calloutName = "    \n";
            calloutName += "(" + DrawingMethods.GetFootString((float)wclip.WeldSizeSupport) + ")";
            var yy = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, wclip.Length, wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC, wclip.Position) + yOffset;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip weld" + " SupportWelded", calloutName,
                new Vector3((float)(DrawingMethods.GetPrimaryWidth() * Math.Sign(x)), (float)(yy + wclip.Length / 4), 0), new Vector3(-12 * Math.Sign(x), 15, 0), 9, "Typ. Both Angles\nWeld Return: 0.375in on Top", -180, 110);
        }

        var topDistance = (float)(DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, wclip.Length, wclip.BoltWebOnBeam.EdgeDistLongDir, wclip.Position) +
            yOffset + wclip.BoltWebOnBeam.EdgeDistLongDir);

        //Draw distance to top of clip angle
        labelName = DrawingMethods.GetFootString(((float)(yOffset + component.Shape.USShape.d / 2)) - topDistance);

        if (CommonDataStatic.JointConfig != EJointConfiguration.BeamToGirder)
        {
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 10", labelName, new Vector3(0, (float)(yOffset + component.Shape.USShape.d / 2), -(float)legLength / 4),
                new Vector3(0, topDistance, -(float)legLength / 4), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);
        }
        else
        {
            MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 10");
        }

        //Clip front width label
        var clipStart = new Vector3((float)DrawingMethods.GetPrimaryWidth() * Math.Sign(x), bottomPlatePointY, 0);
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + "clip dim 5", DrawingMethods.GetFootString((float)(!isLongLegOSL ? wclip.LongLeg : wclip.ShortLeg)),
            clipStart, clipStart + new Vector3((float)(Math.Sign(x) * (!isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC)), 0, 0), Vector3.forward, Math.Sign(x) * 15.0f,
            MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);
    }

    public static void AddShearEndPlateDimensions(DetailData component, float firstBoltPointY, double x, bool changed)
    {
        var endPlate = component.WinConnect.ShearEndPlate;

        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var isRight = Math.Sign(x) > 0;
        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        isRight = !isRight;

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        var points = new float[10];
        points[0] = (float)(x);
        points[1] = (float)(x + endPlate.Thickness * METRIC * Math.Sign(x));
        points[2] = DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, endPlate.Length, endPlate.Bolt.EdgeDistLongDir, endPlate.Position) + yOffset;
        points[3] = points[2] + (float)endPlate.Bolt.EdgeDistLongDir * METRIC; //Top of plate
        points[4] = points[2] - (float)((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir * METRIC);
        points[5] = points[3] - (float)endPlate.Length * METRIC; //Bottom of plate
        points[6] = -(float)endPlate.Width * METRIC / 2;
        points[7] = (float)endPlate.Width * METRIC / 2;
        points[8] = (float)component.Shape.USShape.d / 2 - points[2] + yOffset * 2;
        points[9] = points[7] - (float)endPlate.Bolt.EdgeDistTransvDir * METRIC;


        //Distance to top of bolt
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 1", DrawingMethods.GetFootString((float)endPlate.Bolt.EdgeDistLongDir),
            new Vector3(points[1], points[3]), new Vector3(points[1], points[2]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        labelName = (endPlate.Bolt.NumberOfRows - 1).ToString() + "@" + endPlate.Bolt.SpacingLongDir.ToString() + "=" + DrawingMethods.GetFootString((float)((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir));
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 2", labelName,
            new Vector3(points[1], points[2]), new Vector3(points[1], points[4]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());


        //Add the bolt name callout
        var labelOffset = new Vector3((float)(x + endPlate.Thickness * Math.Sign(x)), points[2] - (float)((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir), 0);
        labelName = (endPlate.Bolt.NumberOfRows * 2).ToString() + " Bolts\n" + endPlate.Bolt.BoltName;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate bolt label", labelName, labelOffset, new Vector3(15 * Math.Sign(x), -25, 0), false, null, false, changed);

        //Length of endplate
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 3", DrawingMethods.GetFootString((float)endPlate.Length),
            new Vector3(0, points[3], points[7]), new Vector3(0, points[5], points[7]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Width of the endplate
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 4", DrawingMethods.GetFootString((float)endPlate.Width),
            new Vector3(0, points[5], points[6]), new Vector3(0, points[5], points[7]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Draw gage
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 6", DrawingMethods.GetFootString((float)component.GageOnColumn),
            new Vector3(0, points[4], -points[9]), new Vector3(0, points[4], points[9]), 7 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //TOB to first Bolt
        labelName = DrawingMethods.GetFootString((float)component.WinConnect.Beam.DistanceToFirstBoltDisplay);

        if (endPlate.Position != EPosition.Top)
        {
            labelName = DrawingMethods.GetFootString((float)points[8]);
        }

        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate dim 5", labelName,
            new Vector3(0, (float)component.Shape.USShape.d / 2 + yOffset, points[7]), new Vector3(0, points[2], points[7]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Add the end plate name callout
        labelOffset = new Vector3(0, points[5], -(float)endPlate.Width / 2);
        labelName = "PL " + endPlate.Length.ToString() + "in.X " + endPlate.Width.ToString() + "in.X " + endPlate.Thickness.ToString() + "in. " + endPlate.MaterialName;
        MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate callout", labelName, labelOffset, new Vector3(0, -8, -25), false, null, false, changed);

        //Test weld symbol
        labelName = DrawingMethods.GetFootString((float)endPlate.WeldSize);
        labelName = labelName + "\n" + labelName;

        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + "endplate weld", labelName, new Vector3(points[1], points[3] - (float)endPlate.Length / 2, 0), new Vector3(Math.Sign(x) * 10, -15, 0));
    }

    public static void AddShearSinglePlateDimensions(DetailData component, float firstBoltPointY, double x, bool changed)
    {
        var splatewin = component.WinConnect.ShearWebPlate;
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var isRight = Math.Sign(x) > 0;

        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        //Create the dimensions
        var points = new float[30];
        points[0] = (float)(DrawingMethods.GetPrimaryWidth() + splatewin.Width * METRIC) * Math.Sign(x);
        points[1] = (float)(splatewin.Length * METRIC / 2);
        points[2] = (float)(DrawingMethods.GetPrimaryWidth() * Math.Sign(x));
        points[3] = (float)(DrawingMethods.GetPrimaryWidth() + splatewin.Width * METRIC - splatewin.Bolt.EdgeDistTransvDir * METRIC) * Mathf.Sign((float)x);
        points[4] = (float)(component.Shape.USShape.d / 2 - (splatewin.Length * METRIC / 2 - splatewin.Bolt.EdgeDistTransvDir * METRIC));
        points[5] = (float)DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, splatewin.Length, splatewin.Bolt.EdgeDistLongDir, splatewin.Position) + yOffset;
        points[6] = (float)component.Shape.USShape.d / 2 + yOffset;
        points[7] = points[5] - (float)((splatewin.Bolt.NumberOfRows - 1) * splatewin.Bolt.SpacingLongDir * METRIC);
        points[8] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC - (float)splatewin.Length * METRIC;
        points[9] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC;
        points[10] = (float)((splatewin.Bolt.NumberOfLines - 1) * splatewin.Bolt.SpacingTransvDir * METRIC);
        points[11] = points[3] - points[10] * Mathf.Sign((float)x);
        points[12] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC;

        //Stiffeners
        points[13] = (float)(DrawingMethods.GetPrimaryWidth() + splatewin.Width / 2 * METRIC) * Math.Sign(x);
        points[14] = (float)primaryComponent.Shape.USShape.d / 4;

        //Plate length dimension
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " + "splate dim 7", 
            DrawingMethods.GetFootString((float)splatewin.Length), new Vector3(points[0], points[9]), new Vector3(points[0], points[8]), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Number of bolts
        var calloutName = (splatewin.Bolt.NumberOfRows * splatewin.Bolt.NumberOfLines).ToString() + " Bolts\n" + splatewin.Bolt.BoltName;
        MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " + isRight + "splate number of bolts", calloutName, new Vector3(0, points[7]), new Vector3(0, -8, -17), false, null, false, changed);

        calloutName = (splatewin.Bolt.NumberOfRows - 1).ToString() + "@" + splatewin.Bolt.SpacingLongDir.ToString() + "=" + DrawingMethods.GetFootString((float)((splatewin.Bolt.NumberOfRows - 1) * splatewin.Bolt.SpacingLongDir));
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " + "splate dim 11", calloutName, 
            new Vector3(0, points[5]), new Vector3(0, points[7]), -12 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        if (!(component.MomentConnection == EMomentCarriedBy.DirectlyWelded && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane))
        {
            //Destroy the other labels n stuff
            MessageQueueTest.ClearLabelsAndDimensionsIgnore(MessageQueueTest.GetSide(x) + " Shear" + " DWeldedSPlate", "NotDWeldedSPlate");

            var dblPlates = "";
            var plateWidthStr = (splatewin.Width).ToString();

            if(CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice)
            {
                if (splatewin.NumberOfPlates > 1)
                    dblPlates = " - EA. SIDE";

                plateWidthStr = (splatewin.Width * 2).ToString();
            }

            var plateName = "PL" + splatewin.Length.ToString() + unitString + "X" + plateWidthStr + unitString + "X" + splatewin.Thickness.ToString() + unitString + dblPlates + " - " + splatewin.MaterialName;

            if (splatewin.WebPlateStiffener == EWebPlateStiffener.With)
            {
                plateName = "PL" + splatewin.Length.ToString() + unitString + " ~ " + splatewin.Height.ToString() + unitString + "X" + plateWidthStr + unitString + "X" + splatewin.Thickness.ToString() + unitString + dblPlates + " - " + splatewin.MaterialName;

                var stiffLabel = "PL" + (primaryComponent.Shape.USShape.d - primaryComponent.Shape.USShape.tf * 2).ToString() + unitString + "X" +
                    (primaryComponent.Shape.USShape.bf / 2 - primaryComponent.Shape.USShape.tw / 2).ToString() + unitString + "X" +
                        splatewin.StiffenerThickness.ToString() + unitString + " Min. (Top and Bottom)\n" +
                        "Plates welded to shear tab, col. web and flanges\n" +
                        "with two-sided minimum size fillet weld";

                //Create the stiffener dimensions
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "tstiff name label",
                    stiffLabel, new Vector3(points[13], 0, points[14]), new Vector3(5 * Math.Sign(x), 0, 10), false, null, false, changed);

                //Create weld dimension
                //TODO: Fix weld name
                stiffLabel = "Note:\nAll Welds " + "E70XX";
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "tstiff weld note",
                    stiffLabel, new Vector3(-1, 0, 20), Vector3.zero, false, null, true, changed);
            }
            else
            {
                MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "tstiff");
            }

            //Beam splice combines the plates
            if (CommonDataStatic.JointConfig == EJointConfiguration.BeamSplice)
            {
                if (isRight)
                {
                    //Plate name
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " BeamSplice " + "splate name label", plateName, new Vector3(points[0], points[8]), new Vector3(30 * Math.Sign(x), -20, 0), false, null, false, changed);

                    //Plate width dimension
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " BeamSplice " + "splate dim 5",
                        DrawingMethods.GetFootString((float)splatewin.Width * 2), new Vector3(points[0], points[8]), new Vector3(-points[0], points[8]), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);
                }
            }
            else
            {
                //NOTE: I changed it to BeamSplice...May mess up somewhere else?
                MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " BeamSplice " + "splate name label");

                //Plate name
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate name label", 
                    plateName, new Vector3(points[0], points[8]), new Vector3(30 * Math.Sign(x), -20, 0), false, null, false, changed);

                //Single plate width
                //Plate width dimension
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate dim 5",
                    DrawingMethods.GetFootString((float)splatewin.Width), new Vector3(points[0], points[8]), new Vector3(points[2], points[8]), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);
            }

            //Distance to bolt from outside dimension
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate dim 6", DrawingMethods.GetFootString((float)splatewin.Bolt.EdgeDistTransvDir),
                new Vector3(points[0], points[8]), new Vector3(points[3], points[8]), -12 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

            //TOB to plate
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate dim 8", 
                DrawingMethods.GetFootString((float)(points[6] - points[9]) / METRIC), new Vector3(points[0], points[6]), new Vector3(points[0], points[9]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Right);

            //Bolt spacing
            if (points[10] > 0)
            {
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate dim 9", DrawingMethods.GetFootString(points[10]),
                    new Vector3(points[3], points[8]), new Vector3(points[11], points[8]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());
            }
            else
            {
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate", isRight.ToString() + "splate dim 9");
            }

            //Add top of beam to first bolt
            calloutName = DrawingMethods.GetFootString((float)component.WinConnect.Beam.DistanceToFirstBoltDisplay);
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + "splate dim 10", calloutName,
                new Vector3(points[0], points[6]), new Vector3(points[0], points[5]), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

            if (CommonDataStatic.JointConfig != EJointConfiguration.BeamSplice)
            {
                calloutName = DrawingMethods.GetFootString((float)splatewin.SupportWeldSize);
                calloutName = calloutName + "\n" + calloutName;
                var yy = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, splatewin.Length * (float)ConstNum.METRIC_MULTIPLIER, splatewin.Bolt.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, splatewin.Position) + yOffset;
                MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + isRight + "splate weld label", calloutName, new Vector3(0, yy, 0), new Vector3(0, -10, 15));
            }
            else
            {
                MessageQueueTest.ClearLabelsAndDimensions("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + isRight + "splate weld label");
            }
        }
        else
        {
            var plate = component.WinConnect.ShearWebPlate;
            var t = (float)(plate.Thickness * ConstNum.METRIC_MULTIPLIER);

            var plateHeight = (float)(plate.Height * ConstNum.METRIC_MULTIPLIER);
            var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
            var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + plate.Bolt.EdgeDistTransvDir);
            var plate2Height = (float)plate.Length;

            if (plate.WebPlateStiffener == EWebPlateStiffener.With)
                plate2Width = (float)plate.Width - plateWidth;

            //Destroy the other labels n stuff
            MessageQueueTest.ClearLabelsAndDimensionsIgnore(MessageQueueTest.GetSide(x) + " Shear" + " NotDWeldedSPlate", "DWeldedSPlate");

            //Draw the single plate with the weird cutouts and their dimensions
            points[13] = (float)(DrawingMethods.GetPrimaryWidth() + plateWidth) * Math.Sign(x);
            points[14] = -plateHeight / 2 + yOffset - (float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness;
            points[15] = (float)(DrawingMethods.GetPrimaryWidth()) * Math.Sign(x);
            points[16] = points[13] + plate2Width * Math.Sign(x);
            points[17] = yOffset - (float)splatewin.Length / 2;
            points[18] = yOffset;
            points[19] = yOffset + (float)plateHeight / 2;
            points[20] = (float)(DrawingMethods.GetPrimaryWidth() + plateWidth / 2) * Math.Sign(x);
            points[21] = yOffset + (float)plateHeight / 2 + (float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness / 2;
            points[22] = (float)-primaryComponent.Shape.USShape.d / 2;
            points[23] = points[15] + (float)(component.WinConnect.MomentDirectWeld.Top.a + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2) * Math.Sign(x);
            points[24] = (float)component.Shape.USShape.bf / 2;
            points[25] = points[15] + (float)(component.WinConnect.MomentDirectWeld.Top.a * Mathf.Cos(45 * Mathf.Deg2Rad) + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2) * Math.Sign(x);
            points[26] = (float)primaryComponent.Shape.USShape.d / 2 - (float)(primaryComponent.Shape.USShape.tf + component.WinConnect.MomentDirectWeld.Top.a * Mathf.Sin(45 * Mathf.Deg2Rad));
            points[27] = (float)(-primaryComponent.Shape.USShape.d / 2 + primaryComponent.Shape.USShape.tf);

            //Distance to bolt from outside dimension
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate dim 20", 
                DrawingMethods.GetFootString((float)splatewin.Bolt.EdgeDistTransvDir),
                new Vector3(points[3], points[12]), new Vector3(points[0], points[12]), -20 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

            //Draw first plate width
            labelName = DrawingMethods.GetFootString(plateWidth);
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate dim 21", labelName,
                new Vector3(points[13], points[14]), new Vector3(points[15], points[14]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

            //Draw 2nd plate width
            labelName = DrawingMethods.GetFootString(plate2Width);
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate dim 22", labelName,
                new Vector3(points[16], points[14]), new Vector3(points[13], points[14]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

            //Draw the plate name
            labelName = DrawingMethods.GetFootString(t) + " PL - " + splatewin.MaterialName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate name label 2", labelName,
                new Vector3(points[16], points[17]), new Vector3(3 * Math.Sign(x), -12, 0), false, null, false, changed);

            //Add welds
            labelName = DrawingMethods.GetFootString((float)splatewin.SupportWeldSize);
            labelName += "\n" + labelName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate weld 10", labelName,
                new Vector3(points[15], points[18]), new Vector3(30 * Math.Sign(x), -30, 0), 0);

            //Side weld
            labelName = DrawingMethods.GetFootString((float)splatewin.BeamWeldSize);
            labelName += "\n" + labelName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate weld 11", labelName,
                new Vector3(points[20], points[19]), new Vector3(-20 * Math.Sign(x), -10, 0), 5, "Typ.", -50, 100);

            //Top
            labelName = DrawingMethods.GetFootString((float)component.WinConnect.MomentDirectWeld.Top.FilletWeldW2);
            labelName += "\n" + labelName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate weld 12", labelName,
                new Vector3(points[15], points[21]), new Vector3(30 * Math.Sign(x), 10, 0), 5, "Top and\nBottom PL", -90, 100);

            //Make the top dimensions
            //Make the plate length
            labelName = DrawingMethods.GetFootString((float)(component.WinConnect.MomentDirectWeld.Top.a + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2));
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate dim 23", labelName,
                new Vector3(points[15], 0, points[22]), new Vector3(points[23], 0, points[22]), 5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

            //Make the plate width
            labelName = DrawingMethods.GetFootString((float)component.Shape.USShape.bf);
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate dim 24", labelName,
                new Vector3(points[23], 0, -points[24]), new Vector3(points[23], 0, points[24]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

            //Make the top weld symbols
            labelName = ""; //Do nothing
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate weld 13", labelName,
                new Vector3(points[23], 0, points[24] / 2), new Vector3(30 * Math.Sign(x), 0, 20), 6, "TYP.", -60, 90);

            labelName = DrawingMethods.GetFootString((float)splatewin.SupportWeldSize);
            labelName += "\n" + labelName;
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate weld 14", labelName,
                new Vector3(points[20], 0, -points[27]), new Vector3(30 * Math.Sign(x), 0, -20), 5, "Top and Bottom PL\nBoth Flanges", -130, 100);

            //Draw wedge plate names
            labelName = DrawingMethods.GetFootString((float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness) + " Top PL - " + component.WinConnect.MomentDirectWeld.MaterialName;
            labelName += "\n" + DrawingMethods.GetFootString((float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness) + " Bot PL - " + component.WinConnect.MomentDirectWeld.MaterialName;
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + "splate name label 3", labelName,
                new Vector3(points[25], 0, -points[26]), new Vector3(10 * Math.Sign(x), 0, -5), false, null, false, changed);
        }
    }

    public static void AddShearSeatDimensions(DetailData component, double x, bool changed)
    {
        var seat = component.WinConnect.ShearSeat;
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var angleFlip = seat.ShortLegOn == EShortLegOn.SupportSide ? true : false;
        var boltHeight = 0.390625f; // 25/64
        var y = component.Shape.USShape.d / 2;

        var isRight = Math.Sign(x) > 0;

        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        var sleg = (float)(angleFlip == false ? seat.TopAngleSupLeg * METRIC : seat.TopAngleBeamLeg * METRIC);
        var bleg = (float)(angleFlip == false ? seat.TopAngleBeamLeg * METRIC : seat.TopAngleSupLeg * METRIC);

        var botSleg = (float)(angleFlip == false ? seat.Angle.b * METRIC : seat.Angle.d * METRIC);
        var botBleg = (float)(angleFlip == false ? seat.Angle.d * METRIC : seat.Angle.b * METRIC);

        var points = new float[50];
        points[0] = (float)(x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange * METRIC) * Math.Sign(x));
        points[1] = (float)x;
        points[2] = (float)(-component.Shape.USShape.d / 2 + yOffset - seat.Angle.t);
        points[3] = (float)(x + (bleg) * Math.Sign(x));
        points[4] = (float)-component.Shape.USShape.d / 2 + yOffset - (botSleg - (float)seat.Bolt.EdgeDistLongDir * METRIC);
        points[5] = (float)(x + (seat.Angle.t * METRIC + boltHeight) * Math.Sign(x));
        points[6] = (float)(-component.Shape.USShape.d / 2 + yOffset) - botSleg;
        points[7] = -(float)seat.AngleLength * METRIC / 2;
        points[8] = (float)component.WinConnect.Beam.GageOnFlange / 2 * METRIC;//(float)(seat.AngleLength * METRIC / 2 - distFlange * METRIC);
        points[9] = (float)component.Shape.USShape.d / 2 + yOffset;
        points[10] = (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistTransvDir * METRIC);
        points[11] = (float)(component.Shape.USShape.d / 2 + yOffset + seat.TopAngle.t);
        points[12] = (float)(x + (botBleg) * Math.Sign(x));
        points[13] = (float)(-component.Shape.USShape.d / 2 + yOffset);
        points[14] = (float)((-component.Shape.USShape.d / 2 + yOffset) - botSleg + seat.Bolt.EdgeDistLongDir * METRIC);
        points[15] = (float)((-component.Shape.USShape.d / 2 + yOffset) - botSleg + seat.Bolt.EdgeDistLongDir + seat.Bolt.SpacingLongDir * METRIC);
        points[16] = (float)(x + seat.PlateWidth * Math.Sign(x));
        points[17] = (float)(-y + yOffset - seat.PlateThickness);
        points[18] = (float)(-y + yOffset - seat.PlateThickness - seat.StiffenerLength);
        points[19] = (float)(x + seat.PlateWidth / 2 * Math.Sign(x));
        points[20] = (float)(component.Shape.USShape.d / 2 + yOffset);
        points[21] = (float)(x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback) * Math.Sign(x));
        points[22] = (float)(-seat.StiffenerWidth);
        points[23] = (float)(-y + yOffset - seat.PlateThickness - seat.StiffenerLength / 2);

        //Stiffener Tee stuff
        points[24] = (float)(x + (seat.StiffenerWidth) * Math.Sign(x));
        points[25] = (float)(-y + yOffset - seat.StiffenerTee.d);

        //Double leader
        points[26] = (float)(y + yOffset + sleg);

        //Draw weld note
        if (component.MemberType == EMemberType.RightBeam)
        {
            labelName = "Note:\nAll Welds " + seat.WeldName;
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 30" + " WeldNote", labelName,
                new Vector3(0, 0, 20), Vector3.zero, false, null, false, changed);
        }
        else
        {
            var rbeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];

            if (!rbeam.IsActive)
            {
                //Do the same thing
                labelName = "Note:\nAll Welds " + seat.WeldName;
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 30" + " WeldNote", labelName,
                    new Vector3(0, 0, 20), Vector3.zero, false, null, false, changed);
            }
        }

        switch (seat.Connection)
        {
            case ESeatConnection.BoltedStiffenedPlate:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffenerBoltedStiffenedPlate WeldNote");

                break;

            case ESeatConnection.Welded:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffWelded WeldNote");

                break;

            case ESeatConnection.Bolted:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffenerBolted WeldNote");

                //Gage on flange
                labelName = component.GageOnFlange.ToString() + unitString;//((float)(seat.AngleLength * METRIC / 2 - distFlange * METRIC) * 2).ToString() + unitString;
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + "seat dim 4" + " StiffenerBolted", labelName,
                    new Vector3(0, points[9], -points[8]),
                    new Vector3(0, points[9], points[8]), 5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                //Gage on column
                if (seat.Bolt.NumberOfLines <= 2)
                {
                    labelName = component.GageOnColumn.ToString() + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + "seat dim 3" + " StiffenerBolted", labelName, new Vector3(0, points[4], -points[10]),
                        new Vector3(0, points[4], points[10]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                }
                else
                {
                    labelName = (seat.Bolt.NumberOfLines - 1) + "@" + seat.Bolt.SpacingTransvDir + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + "seat dim 3" + " StiffenerBolted", labelName, new Vector3(0, points[4], -points[10]),
                        new Vector3(0, points[4], points[10]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                }

                //Draw number of rows of bolts dimension
                //TODO: Fix this
                if (!angleFlip)
                {
                    //labelName = GetFootString((float)seat.Bolt.SpacingTransvDir);
                    //MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat dim 11" + " StiffenerBolted", labelName, new Vector3(0, points[14], points[7]),
                    //    new Vector3(0, points[15], points[7]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                }
                else
                {
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " Seat" + seat.Connection, isRight.ToString() + "seat dim 11" + " StiffenerBolted");
                }

                //Seat height
                labelName = DrawingMethods.GetFootString(botSleg / METRIC);
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat dim 2" + " StiffenerBolted", labelName, new Vector3(0, points[6], points[7]),
                    new Vector3(0, points[13], points[7]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                //Draw the seat width
                labelName = DrawingMethods.GetFootString((float)component.WinConnect.Beam.BoltEdgeDistanceOnFlange);
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat dim 1" + " StiffenerBolted", labelName, new Vector3(points[1], points[2]),
                    new Vector3(points[0], points[2]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);

                //Add the bottom seat dimension
                labelName = seat.AngleName + "X " + seat.AngleLength + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat label" + " StiffenerBolted", labelName,
                new Vector3(points[12], points[2], 0), new Vector3(8 * Math.Sign(x), -3, 0), false, null, false, changed);

                //Add the seat dimension
                labelName = seat.Bolt.NumberOfLines + " Bolts\n" + seat.Bolt.BoltName;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat bolt label" + " StiffenerBolted", labelName,
                new Vector3(points[5], points[4], 0), new Vector3(8 * Math.Sign(x), -5, 0), false, null, false, changed);

                //Draw the edge dist
                labelName = DrawingMethods.GetFootString((float)seat.Bolt.EdgeDistLongDir);
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + "seat dim 12" + " StiffenerBolted", labelName, new Vector3(0, points[14], points[7]),
                    new Vector3(0, points[6], points[7]), 7.5f * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);
                break;

            case ESeatConnection.WeldedStiffened:

                MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffenerWeldedStiffened WeldNote");

                //Draw top weld
                labelName = "\n" + DrawingMethods.GetFootString((float)seat.WeldSizeBeam);
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 21" + " StiffenerWeldedStiffened", labelName,
                    new Vector3(points[3], points[20]), new Vector3(12 * Math.Sign(x), 15), 8, "Welds may be\nreplaced by bolts.", -120, 110, false, new Vector3(points[1], points[26]));

                //Draw bolt flange spacing
                labelName = DrawingMethods.GetFootString((float)(component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback));
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 22" + " StiffenerWeldedStiffened", labelName,
                    new Vector3(points[1], points[13]), new Vector3(points[21], points[13]), -12 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);

                if (seat.Stiffener == ESeatStiffener.Plate)
                {
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffenerWeldedStiffened StiffPlate");

                    //Draw the plate labels
                    labelName = "PL" + seat.PlateLength + "X" + seat.PlateWidth + "X" + seat.PlateThickness + " " + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 10" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(points[16], points[17], 0), new Vector3(15 * Math.Sign(x), -12), false, null, false, changed);

                    labelName = "PL" + seat.StiffenerLength + "X" + seat.StiffenerWidth + "X" + seat.StiffenerThickness + " " + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 11" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(points[16], points[18], 0), new Vector3(12 * Math.Sign(x), -15), false, null, false, changed);

                    //Draw stiffener width
                    labelName = DrawingMethods.GetFootString((float)seat.StiffenerWidth);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 20" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(points[16], points[18]), new Vector3(points[1], points[18]), 12 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Add bottom weld
                    labelName = DrawingMethods.GetFootString((float)seat.WeldSizeSupport);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 23" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(points[19], points[17]), new Vector3(14 * Math.Sign(x), -17), 5, "1.9in. Min.\neach side", -120, 110);

                    //Draw the plate length
                    labelName = DrawingMethods.GetFootString((float)seat.StiffenerLength);
                    MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 24" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(0, points[17], points[22]), new Vector3(0, points[18], points[22]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

                    //Draw the two weld labels
                    labelName = DrawingMethods.GetFootString((float)seat.WeldSizeSupport);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 25" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(0, points[17], points[22] / 2), new Vector3(0, -17, 14 * Math.Sign(x)), 4, "1.9in. Min.\neach side", -120, 110);

                    //Use the same label name
                    labelName = DrawingMethods.GetFootString((float)seat.WeldSizeSupport);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 26" + " StiffenerWeldedStiffened" + " StiffPlate", labelName,
                        new Vector3(0, points[23], (float)seat.StiffenerThickness / 2), new Vector3(0, -17, -25 * Math.Sign(x)), 0);
                }
                else if (seat.Stiffener == ESeatStiffener.Tee)
                {
                    MessageQueueTest.ClearLabelsAndDimensionsIgnore("Shear " + MessageQueueTest.GetSide(x) + " Seat", "StiffenerWeldedStiffened StiffenerTee");

                    //Draw tee name
                    labelName = seat.StiffenerTeeName + " X " + seat.StiffenerWidth + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 31" + " StiffenerWeldedStiffened" + " StiffenerTee", labelName,
                        new Vector3(points[24], points[25], 0), new Vector3(12 * Math.Sign(x), -15), false, null, false, changed);

                    //Draw tee width
                    labelName = DrawingMethods.GetFootString((float)seat.StiffenerWidth);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 32" + " StiffenerWeldedStiffened" + " StiffenerTee", labelName,
                        new Vector3(points[16], points[25]), new Vector3(points[1], points[25]), -12 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Draw the two weld labels
                    labelName = DrawingMethods.GetFootString((float)seat.WeldSizeBeam);
                    labelName = "\n" + labelName;
                    MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 33" + " StiffenerWeldedStiffened" + " StiffenerTee", labelName,
                        new Vector3(0, points[17], points[22] / 2), new Vector3(0, -17, 14 * Math.Sign(x)), 4, "2.226in. Min.\neach side", -120, 110);

                    //Use the same label name
                    labelName = DrawingMethods.GetFootString((float)seat.WeldSizeBeam);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat label 34" + " StiffenerWeldedStiffened" + " StiffenerTee", labelName,
                        new Vector3(0, points[23], (float)seat.StiffenerThickness / 2), new Vector3(0, -17, -25 * Math.Sign(x)), 0);

                    //Draw the plate length
                    labelName = DrawingMethods.GetFootString((float)seat.StiffenerLength);
                    MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + "seat dim 35" + " StiffenerWeldedStiffened" + " StiffenerTee", labelName,
                        new Vector3(0, points[17], points[22]), new Vector3(0, points[18], points[22]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);
                }
                break;
        }

        //Add the top seat dimension
        labelName = "L" + seat.TopAngleSupLeg.ToString() + "X" + seat.TopAngleBeamLeg.ToString() + "X" + DrawingMethods.GetFootStringSimple((float)seat.TopAngle.t) + " X " + seat.TopAngleLength + unitString;
        var sside = isRight ? "Right" : "Left";
        labelName += "\n(All " + sside + " Side Attachments " + seat.MaterialName + ")";
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat " + "seat label 2", labelName,
        new Vector3(points[3], 0, -points[7]), new Vector3(5 * Math.Sign(x), 0, 10), false, null, false, changed);
    }

    public static void AddShearTeeDimensions(DetailData component, double x, bool changed)
    {
        var tee = component.WinConnect.ShearWebTee;
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];

        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var isRight = Math.Sign(x) > 0;

        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        if (tee.OSLConnection == EConnectionStyle.Welded)
        {
            var weldSize = DrawingMethods.GetFootString((float)tee.WeldSizeFlange);
            //Make welded symbol
            MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel(MessageQueueTest.GetSide(x) + " Tee " + "Tee support weld 1",
                weldSize + "\n" + weldSize, new Vector3(0, DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d,
                    tee.SLength * METRIC, tee.BoltWebOnStem.EdgeDistLongDir * METRIC, tee.Position) + yOffset, (float)(-tee.Size.bf / 2 * METRIC + component.Shape.tw / 2 * METRIC + tee.Size.tw / 2 * METRIC)),
                    new Vector3(0, -20, -15), 5, "Weld Return: 0.375in. on Top", -180, 100);
        }
        else
        {
            MessageQueueTest.instance.GetView(viewSide).DestroyLabel(MessageQueueTest.GetSide(x) + " Tee", "Tee support weld 1");
        }

        //Add dimensions for the tee
        var points = new float[11];
        points[0] = (float)(x);
        points[1] = (float)(x + tee.Size.d * METRIC * Math.Sign(x));
        points[2] = DrawingMethods.GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, tee.SLength * METRIC, tee.BoltWebOnStem.EdgeDistLongDir * METRIC, tee.Position) + yOffset;
        points[3] = points[2] + (float)tee.BoltWebOnStem.EdgeDistLongDir * METRIC; //Top of plate
        points[4] = points[2] - (float)((tee.BoltWebOnStem.NumberOfBolts - 1) * tee.BoltWebOnStem.SpacingLongDir * METRIC);
        points[5] = points[3] - (float)tee.SLength * METRIC; //Bottom of plate
        points[6] = -(float)tee.Size.d * METRIC / 2;
        points[7] = (float)tee.Size.d * METRIC / 2;
        points[8] = (float)component.Shape.USShape.d / 2 - points[2] + yOffset * 2;
        points[9] = -(float)(component.Shape.USShape.tw / 2 + tee.Size.tw / 2 - tee.Size.bf * METRIC / 2 + tee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);
        points[10] = -(float)(component.Shape.USShape.tw / 2 + tee.Size.bf * METRIC / 2 - tee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);

        //Distance to top of bolt
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 1", DrawingMethods.GetFootString((float)tee.BoltWebOnStem.EdgeDistLongDir),
            new Vector3(points[1], points[3]), new Vector3(points[1], points[2]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        labelName = (tee.BoltWebOnStem.NumberOfBolts - 1).ToString() + "@" + tee.BoltWebOnStem.SpacingLongDir.ToString() + "=" + ((tee.BoltWebOnStem.NumberOfBolts - 1) * tee.BoltWebOnStem.SpacingLongDir) + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 2", labelName,
            new Vector3(points[1], points[2]), new Vector3(points[1], points[4]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Number of bolts
        var labelOffset = new Vector3((float)(x + tee.Size.d * Math.Sign(x) - tee.BoltWebOnStem.EdgeDistTransvDir * METRIC * Math.Sign(x)), points[2] - (float)((tee.BoltWebOnStem.NumberOfBolts - 1) * tee.BoltWebOnStem.SpacingLongDir), 0);
        labelName = (tee.BoltWebOnStem.NumberOfBolts).ToString() + " Bolts\n" + tee.BoltWebOnStem.BoltName + "\n" + "Gage = " + tee.BoltGageOnTeeStem * METRIC + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee bolt label", labelName, labelOffset, new Vector3(15 * Math.Sign(x), -25, 0), false, null, false, changed);

        var dist = (float)(component.Shape.USShape.d / 2 + yOffset - points[3]);

        //TOB to plate
        labelName = string.Format("{0:0.###}", dist) + unitString;
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 3", labelName,
            new Vector3(0, (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(0, points[3], 0), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Gage
        if (tee.OSLConnection == EConnectionStyle.Bolted)
        {
            MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 7", component.GageOnColumn.ToString() + unitString,
            new Vector3(0, points[4], points[9]), new Vector3(0, points[4], points[10]), -5 * Math.Sign(x), MessageQueueTest.GetDimensionColor());
        }
        else
        {
            MessageQueueTest.instance.GetView(viewSide).DestroyLabel(MessageQueueTest.GetSide(x) + " Tee", isRight.ToString() + "tee dim 7");
        }

        //Width of the Tee
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 4", tee.Size.d + unitString,
            new Vector3(points[1], points[5], 0), new Vector3(points[0], points[5], 0), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Plate name
        labelName = tee.SizeName + " X " + tee.SLength + unitString + " - " + tee.MaterialName;
        MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 6", labelName,
            new Vector3(0, points[5], (float)(tee.Size.bf / 2 * METRIC - component.Shape.USShape.tw / 2)), new Vector3(0, -10, 10), false, null, false, changed);

        //TOB to first Bolt
        labelName = component.WinConnect.Beam.DistanceToFirstBoltDisplay + unitString;

        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + "tee dim 5", labelName,
            new Vector3(points[1], (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(points[1], points[2], 0), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor());
    }

    public static void AddMomentFlangePlateDimensions(DetailData component, double x, bool changed)
    {
        var fplate = component.WinConnect.MomentFlangePlate;
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var isRight = Math.Sign(x) > 0;
        var primaryWidth = DrawingMethods.GetPrimaryWidth();
        var yOffset = DrawingMethods.GetTopElAndYOffset(component);
        var DistanceToFirstBoltDisplay = (float)(component.EndSetback * METRIC + component.WinConnect.Beam.BoltEdgeDistanceOnFlange * METRIC);
        var topX = Math.Sign(x) * (primaryWidth + component.WinConnect.MomentFlangePlate.TopLength * (float)ConstNum.METRIC_MULTIPLIER / 2);
        var topY = component.Shape.USShape.d / 2 + component.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2 + yOffset;
        var bottomX = Math.Sign(x) * (primaryWidth + component.WinConnect.MomentFlangePlate.BottomLength * (float)ConstNum.METRIC_MULTIPLIER / 2);
        var bottomY = -1 * (component.Shape.USShape.d / 2) - component.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2 + yOffset;

        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        var flangeName = "TOP PL: " + fplate.TopLength + unitString + " X " + fplate.TopWidth + unitString + " X " + fplate.TopThickness + unitString + " - " + component.WinConnect.MomentFlangePlate.MaterialName;
        flangeName += "\n";

        flangeName += "BOT. PL: " + fplate.BottomLength + unitString + " X " + fplate.BottomWidth + unitString + " X " + fplate.BottomThickness + unitString + " - " + component.WinConnect.MomentFlangePlate.MaterialName;

        //Add dimension
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "flange label", flangeName,
            new Vector3((float)(topX + (Math.Sign(x) * fplate.TopLength * METRIC / 2)), 0, (float)-fplate.TopWidth * METRIC / 2), new Vector3(15 * Math.Sign(x), 0, -10), false, null, false, changed);

        var points = new float[8];

        points[0] = (float)(primaryWidth + DistanceToFirstBoltDisplay) * Math.Sign(x);
        points[1] = (float)(component.Shape.USShape.d / 2 + fplate.TopThickness * METRIC) + yOffset;
        points[2] = (float)(primaryWidth * Math.Sign(x));
        points[3] = (float)(points[0] + ((fplate.Bolt.NumberOfRows - 1) * fplate.Bolt.SpacingLongDir * METRIC) * Math.Sign(x));
        points[4] = (float)(fplate.TopWidth * METRIC - (fplate.Bolt.EdgeDistTransvDir * METRIC * 2));
        points[5] = points[4] / 2;
        points[6] = (float)(points[2] + (fplate.TopLength * METRIC) * Math.Sign(x));
        points[7] = (float)(fplate.TopWidth * METRIC / 2);

        //Add distance to first bolt for the top plate
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "fplate dim 1", DrawingMethods.GetFootString((float)DistanceToFirstBoltDisplay / METRIC), new Vector3(points[2], points[1]), new Vector3(points[0], points[1]), -15 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Add bolt callouts
        var calloutName = (fplate.Bolt.NumberOfRows - 1).ToString() + "@" + fplate.Bolt.SpacingLongDir.ToString() + "=" + DrawingMethods.GetFootString((float)((fplate.Bolt.NumberOfRows - 1) * fplate.Bolt.SpacingLongDir));
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "fplate dim 2", calloutName, new Vector3(points[0], points[1]), new Vector3(points[3], points[1]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Add number of bolts callout
        calloutName = (fplate.Bolt.NumberOfRows * 2).ToString() + " Bolts\n" + fplate.Bolt.BoltName;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "num bolts label", calloutName, new Vector3(points[3], points[1]), new Vector3(8 * Math.Sign(x), 4), false, null, false, changed);

        calloutName = DrawingMethods.GetFootString((float)fplate.TopPlateToSupportWeldSize);
        calloutName = calloutName + "\n" + calloutName;


        if (fplate.PlateToSupportWeldType == EWeldType.Fillet)
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "plate fillet weld", calloutName, new Vector3(points[2], points[1]), new Vector3(5 * Math.Sign(x), 5), 5, "Typ.", -62, 92);
        else if (fplate.PlateToSupportWeldType == EWeldType.CJP)
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "plate fillet weld", "", new Vector3(points[2], points[1]), new Vector3(5 * Math.Sign(x), 5), 7, "Typ.", -62, 92);

        //Add top bolt callouts
        calloutName = DrawingMethods.GetFootString(points[4] / METRIC);
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "fplate dim 3", calloutName, new Vector3(points[6], 0, -points[5]), new Vector3(points[6], 0, points[5]), 13 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        calloutName = DrawingMethods.GetFootString((float)fplate.TopWidth);
        MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "fplate dim 4", calloutName, new Vector3(0, points[1], -points[7]), new Vector3(0, points[1], points[7]), -8 * Math.Sign(x), MessageQueueTest.GetDimensionColor());
    }

    public static void AddMomentDirectlyWeldedDimensions(DetailData component, double x, bool changed)
    {
        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        var points = new float[3];

        points[0] = (float)DrawingMethods.GetPrimaryWidth();
        points[1] = (points[0] + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER) * Math.Sign(x);
        points[2] = (float)component.Shape.USShape.d / 2;

        //Add weld
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Moment " + MessageQueueTest.GetSide(x) + " DirectlyWelded " + "dim 1", "",
            new Vector3(points[1], points[2] + yOffset), new Vector3(10 * Math.Sign(x), 5), 6, "Top &\nBot. Flanges", -105, 105);
    }

    public static void AddMomentTeeDimensions(DetailData component, double x, bool changed)
    {
        var tee = component.WinConnect.MomentTee;
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var pWidth = DrawingMethods.GetPrimaryWidth();

        var labelName = "";
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;
        var unitString = DrawingMethods.GetUnitString();
        var isRight = Math.Sign(x) > 0;

        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

        var yOffset = DrawingMethods.GetTopElAndYOffset(component);

        var teeLength = tee.TopLengthAtStem;
        var tw = tee.TopTeeShape.USShape.tw;
        var tf = tee.TopTeeShape.USShape.tf;

        //Draw all the labels
        var points = new float[11];
        points[0] = (float)(pWidth + tee.TopTeeShape.USShape.d) * Math.Sign(x);
        points[1] = -(float)(teeLength / 2);
        points[2] = (float)tee.Beam_g / 2;
        points[3] = (float)((pWidth + tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC) * Math.Sign(x));
        points[4] = points[3] - ((float)tee.BoltBeamStem.SpacingLongDir * METRIC) * Math.Sign(x);
        points[5] = (float)((tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC - tee.BoltBeamStem.SpacingLongDir * METRIC) * Math.Sign(x));
        points[6] = (float)pWidth * Math.Sign(x);
        points[7] = (float)(component.Shape.USShape.d / 2 + tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC + tw / 2) + yOffset;
        points[8] = (float)(tee.Column_g / 2 * METRIC);
        points[9] = (float)(component.Shape.USShape.d / 2 + tw) + yOffset;
        points[10] = (float)(pWidth + tw) * Math.Sign(x);

        labelName = tee.TopTeeShapeName + " - " + tee.TopMaterialName + " (Top and Bottom)";
        labelName += "\n Length = " + teeLength * METRIC + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 1", labelName,
            new Vector3(points[0], 0, points[1]), new Vector3(6 * Math.Sign(x), 0, -6), false, null, false, changed);

        labelName = tee.Beam_g * METRIC + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 2", labelName,
            new Vector3(points[3], 0, -points[2]), new Vector3(points[3], 0, points[2]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        labelName = (float)tee.BoltBeamStem.SpacingLongDir * METRIC + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 3", labelName,
            new Vector3(points[3], 0, points[2]), new Vector3(points[4], 0, points[2]), 6 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        labelName = Mathf.Abs(points[5]) + unitString;
        MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 4", labelName,
            new Vector3(points[4], 0, points[2]), new Vector3(points[6], 0, points[2]), 10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Draw the left/right side gage
        labelName = tee.Column_g + unitString;
        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 5", labelName,
            new Vector3(0, points[7], -points[8]), new Vector3(0, points[7], points[8]), -10 * Math.Sign(x), MessageQueueTest.GetDimensionColor());

        //Draw the tee names
        labelName = tee.BoltBeamStem.NumberOfBolts + " Bolts (TYP.)\n" + tee.BoltBeamStem.BoltName;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 6", labelName,
            new Vector3(points[3], points[9]), new Vector3(8 * Math.Sign(x), 4), false, null, false, changed);

        labelName = tee.BoltColumnFlange.NumberOfBolts + " Bolts (TYP.)\n" + tee.BoltColumnFlange.BoltName;
        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " MTee " + "dim 7", labelName,
            new Vector3(points[10], points[7]), new Vector3(6 * Math.Sign(x), 10), false, null, false, changed);
    }
}
