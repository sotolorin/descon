using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

/// <summary>
/// Draw Angles and Plates for Descon Brace
/// </summary>
public partial class DrawingMethods
{
    public void DrawClipAngle(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        object clipAngle = null;
        if (component.WinConnect.ShearClipAngle.Size != null) clipAngle = component.WinConnect.ShearClipAngle;
        else if (component.WinConnect.ShearClipAngle.Size != null && !conBool) clipAngle = component.WinConnect.ShearClipAngle;
        else if (conBool) clipAngle = component.WinConnect.MomentFlangeAngle;

        if (clipAngle != null)
        {
            if (!isBrace)
            {
                //The plate needs the beam, thickness to offset correctly
                x = GetPrimaryWidth() * Math.Sign(x);
            }

            var primaryWidth = GetPrimaryWidth();

            var t = GetClipThickness(clipAngle);
            t = t.Equals(0) ? 1.5f : t;   //Some times there is no thickness specifically for shear clip angles
            //x = component.MemberType == EMemberType.LeftBeam ? -brace0.Shape.USShape.d / 2 : brace0.Shape.USShape.d / 2;
            var flip = component.MemberType == EMemberType.LeftBeam;

            var clipZ = y;
            if (clipZ < 0)
                clipZ *= -1;

            var wclip = clipAngle as WCShearClipAngle;
            if (wclip != null)
            {
                clipZ = wclip.Length * (float)ConstNum.METRIC_MULTIPLIER;
            }

            var gussetTranslation1 = new Vector3();
            var gussetTranslation2 = new Vector3();
            var nonGussetTranslation1 = new Vector3(0, yOffset, 0);
            var nonGussetTranslation2 = new Vector3(0, yOffset, 0);

            if (wclip != null)
            {
                nonGussetTranslation1 = new Vector3((float)(primaryWidth + t / 2) * Math.Sign(-x),
                    GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, clipZ, wclip.BoltOslOnSupport.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, wclip.Position) + yOffset,
                    -((float)component.Shape.USShape.tw / 2 + t / 2));
                nonGussetTranslation2 = new Vector3((float)(primaryWidth + t / 2) * Math.Sign(-x),
                    GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, clipZ, wclip.BoltOslOnSupport.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, wclip.Position) + yOffset, ((float)component.Shape.USShape.tw / 2) + t / 2);
            }

            if (isBrace && gusset != null)
            {
                //TODO: Will's values, remove when calculations are corrected
                gusset.ColumnSide = 17f * (float)ConstNum.METRIC_MULTIPLIER;
                var urX = brace0.Shape.USShape.d / 2; //UR
                var lrX = brace0.Shape.USShape.d / 2; //LR
                var urY = brace1.Shape.USShape.d / 2 + gusset.ColumnSide / 2; //UR
                var lrY = -brace1.Shape.USShape.d / 2 - gusset.ColumnSide / 2; //LR
                var ulX = -brace0.Shape.USShape.d / 2; //UL
                var lLX = -brace0.Shape.USShape.d / 2; //lL
                var ulY = brace2.Shape.USShape.d / 2 + gusset.ColumnSide / 2; //UL
                var lLY = -brace2.Shape.USShape.d / 2 - gusset.ColumnSide / 2; //lL
                if (uR)
                {
                    gussetTranslation1 = new Vector3(-(float)urX, (float)urY, 0 + nonGussetTranslation1.z);
                    gussetTranslation2 = new Vector3(-(float)urX, (float)urY, 0 + nonGussetTranslation2.z);
                }
                if (uL)
                {
                    gussetTranslation1 = new Vector3(-(float)ulX, (float)ulY, 0 + nonGussetTranslation1.z);
                    gussetTranslation2 = new Vector3(-(float)ulX, (float)ulY, 0 + nonGussetTranslation2.z);
                    flip = true;
                }
                if (lR)
                {
                    gussetTranslation1 = new Vector3(-(float)lrX, (float)lrY, 0 + nonGussetTranslation1.z);
                    gussetTranslation2 = new Vector3(-(float)lrX, (float)lrY, 0 + nonGussetTranslation2.z);
                }
                if (lL)
                {
                    gussetTranslation1 = new Vector3(-(float)lLX, (float)lLY, 0 + nonGussetTranslation1.z);
                    gussetTranslation2 = new Vector3(-(float)lLX, (float)lLY, 0 + nonGussetTranslation2.z);
                    flip = true;
                }
            }

            var isLongLegOSL = component.WinConnect.ShearClipAngle.OSL == EOSL.LongLeg;

            //Clip angle creates the short leg on the column
            mesh = CreateClipAngleMesh(clipAngle, clipZ, isBrace ? gussetTranslation1 : nonGussetTranslation1, new Vector3(flip ? 180 : 0, flip ? 0 : 180, 0), isLongLegOSL);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            mesh = CreateClipAngleMesh(clipAngle, clipZ, isBrace ? gussetTranslation2 : nonGussetTranslation2, new Vector3(0, flip ? 0 : 0, flip ? 0 : 180), isLongLegOSL);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var boltStartPos = Vector3.zero;

            wclip = clipAngle as WCShearClipAngle;
            if (wclip != null)
            {
                var shankLength = (float)(wclip.Thickness * METRIC + primaryComponent.Shape.USShape.tf);

                var isLeftSide = Math.Sign(-x) >= 0;

                var edgeDist = (isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC) - wclip.BoltOslOnSupport.EdgeDistTransvDir * METRIC;

                var labelName = "";
                var isRight = Math.Sign(-x) < 0;

                if (wclip.SupportSideConnection == EConnectionStyle.Bolted)
                {
                    //Bolts on column (OSL)
                    boltStartPos = new Vector3((float)(primaryWidth + t + boltHeight / 2) * Math.Sign(-x),
                        GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, clipZ, wclip.BoltOslOnSupport.EdgeDistLongDir, wclip.Position) + yOffset, (float)((edgeDist) + component.Shape.USShape.tw / 2));

                    if (wclip.BoltStagger == EBoltStagger.Support)
                    {
                        boltStartPos.y -= (float)wclip.BoltWebOnBeam.SpacingLongDir * METRIC * 0.5f;
                    }

                    for (int i = 0; i < wclip.BoltOslOnSupport.NumberOfLines; ++i)
                    {
                        //splate.BoltToHorizEdgeDist
                        var boltMesh = CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(x) * (float)wclip.BoltOslOnSupport.SpacingTransvDir * METRIC + (float)(Math.Sign(x) * 0)),
                            Vector3.down * (float)wclip.BoltOslOnSupport.SpacingLongDir * METRIC, wclip.BoltOslOnSupport.NumberOfRows, (float)wclip.BoltOslOnSupport.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, -90, 0), shankLength);
                        boltMeshes.Add(boltMesh);
                    }

                    boltStartPos.z *= -1;

                    for (int i = 0; i < wclip.BoltOslOnSupport.NumberOfLines; ++i)
                    {
                        //splate.BoltToHorizEdgeDist
                        var boltMesh = CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(x) * (float)wclip.BoltOslOnSupport.SpacingTransvDir * METRIC + (float)(Math.Sign(x) * 0)),
                            Vector3.down * (float)wclip.BoltOslOnSupport.SpacingLongDir * METRIC, wclip.BoltOslOnSupport.NumberOfRows, (float)wclip.BoltOslOnSupport.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, -90, 0), shankLength);
                        boltMeshes.Add(boltMesh);
                    }
                }

                var firstBoltPointY = boltStartPos.y;

                if (wclip.BeamSideConnection == EConnectionStyle.Bolted)
                {
                    boltStartPos = new Vector3((float)(primaryWidth * Math.Sign(-x) + (wclip.LongLeg * METRIC - wclip.BoltWebOnBeam.EdgeDistTransvDir * METRIC) * Math.Sign(-x)),
                        GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, clipZ, wclip.BoltWebOnBeam.EdgeDistLongDir, wclip.Position) + yOffset, (float)(t + component.Shape.USShape.tw / 2 + boltHeight / 2)); //z - (float)(bclip.ShortLeg - bclip.BoltOnColumn.EdgeDistLongDir)
                    shankLength = (float)(t * 2 + component.Shape.USShape.tw);

                    var oneLessRow = wclip.BoltStagger == EBoltStagger.OneLessRow;

                    if (wclip.BoltStagger == EBoltStagger.Beam || oneLessRow)
                    {
                        boltStartPos.y -= (float)wclip.BoltWebOnBeam.SpacingLongDir * METRIC * 0.5f;
                    }

                    firstBoltPointY = boltStartPos.y;

                    //Draw the bolts on the beam side
                    for (int i = 0; i < wclip.BoltWebOnBeam.NumberOfLines; ++i)
                    {
                        //splate.BoltToHorizEdgeDist
                        var boltMesh = CreateBoltArray(boltStartPos + Vector3.left * ((float)i * (float)Math.Sign(x) * (float)wclip.BoltWebOnBeam.SpacingTransvDir * METRIC + (float)(Math.Sign(x) * 0)),
                            Vector3.down * (float)wclip.BoltWebOnBeam.SpacingLongDir * METRIC, wclip.BoltWebOnBeam.NumberOfRows - (oneLessRow ? 1 : 0), (float)wclip.BoltWebOnBeam.BoltSize * METRIC, new Vector3(!isLeftSide ? 90 : 270, isLeftSide ? -180 : 0, 0), shankLength);
                        boltMeshes.Add(boltMesh);
                    }

                    var points = new float[3];
                    points[0] = (float)GetPrimaryWidth();
                    points[1] = (float)((isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC) - wclip.BoltWebOnBeam.EdgeDistTransvDir * METRIC);
                    points[2] = (points[0] + points[1]) * Math.Sign(-x);

                    //Delete the weld symbol
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", x.ToString() + "clip weld");

                    //Distance from top of angle to first bolt - long spacing
                    labelName = (wclip.BoltWebOnBeam.EdgeDistLongDir).ToString() + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 7", labelName,
                        new Vector3(points[2], firstBoltPointY + (float)wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC), new Vector3(points[2], firstBoltPointY), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                    var extraV = 0.0f;

                    if (wclip.BoltStagger == EBoltStagger.Support)
                    {
                        extraV = (float)wclip.BoltWebOnBeam.SpacingLongDir * METRIC * 0.5f;
                    }

                    firstBoltPointY = boltStartPos.y;

                    //Bolt callouts
                    var total = (wclip.BoltWebOnBeam.NumberOfRows - 1) * wclip.BoltWebOnBeam.SpacingLongDir * METRIC;
                    labelName = (wclip.BoltWebOnBeam.NumberOfRows - 1).ToString() + "@" + wclip.BoltWebOnBeam.SpacingLongDir.ToString() + "=" + (total / METRIC) + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 8", labelName,
                        new Vector3(points[2], firstBoltPointY - extraV),
                        new Vector3(points[2], firstBoltPointY - (float)(total) - extraV), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                    //Number of bolts callout
                    var sgage = component.EndSetback + component.WinConnect.Beam.Lh; //This gage value is the distance from the bolt line to the face of the column
                    labelName = (wclip.BoltWebOnBeam.NumberOfRows).ToString() + " Bolts\n" + wclip.BoltWebOnBeam.BoltName + "\nGage = " + sgage + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "num bolts label 2", labelName,
                        new Vector3(points[2], firstBoltPointY - (float)total), new Vector3(8 * Math.Sign(-x), -8, 0));

                    //Distance from TOB to front plate
                    labelName = (component.WinConnect.Beam.DistanceToFirstBoltDisplay) + unitString;

                    var edge2 = (float)(GetPrimaryWidth() + ((isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC) - wclip.BoltWebOnBeam.EdgeDistTransvDir * METRIC)) * Math.Sign(-x);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 6", labelName,
                        new Vector3(edge2, (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(edge2, firstBoltPointY, 0), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());
                }
                else
                {
                    //Draw the weld symbol
                    var calloutName = "    \n";
                    calloutName += GetFootString((float)wclip.WeldSizeBeam);
                    var yy = GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, clipZ, wclip.BoltWebOnBeam.EdgeDistLongDir * METRIC, wclip.Position) + yOffset;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + x.ToString() + "clip weld", calloutName, new Vector3((float)((GetPrimaryWidth() + (!isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC)) * Math.Sign(-x)), yy, 0), new Vector3(12 * Math.Sign(-x), -15, 0), 4, "", -53, 110);

                    //Delete the bolted dimensions
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 7");
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 8");
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "num bolts label 2");
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 6");

                }

                firstBoltPointY = boltStartPos.y;
                var bottomPlatePointY = (float)(firstBoltPointY + wclip.BoltOslOnSupport.EdgeDistLongDir * METRIC - clipZ);

                if (wclip.BeamSideConnection == EConnectionStyle.Welded)
                {
                    bottomPlatePointY = (float)(component.Shape.d / 2 + yOffset + wclip.BoltWebOnBeam.EdgeDistLongDir - component.WinConnect.Beam.DistanceToFirstBoltDisplay - clipZ);
                }

                labelName = "2L" + wclip.LongLeg + unitString + "X" + wclip.ShortLeg + unitString + "X" + wclip.Thickness + unitString + "X" + wclip.Length + unitString + " - " + wclip.MaterialName;
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip label", labelName,
                    new Vector3(0, bottomPlatePointY, (float)(wclip.ShortLeg * METRIC + component.Shape.USShape.tw / 2)), new Vector3(0, -10, 15));

                //Distance from TOB to plate
                labelName = (component.WinConnect.Beam.DistanceToFirstBoltDisplay) + unitString;
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 1", labelName,
                    new Vector3(0, (float)component.Shape.USShape.d / 2 + yOffset, -(float)edgeDist), new Vector3(0, firstBoltPointY, -(float)edgeDist), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                if (wclip.SupportSideConnection == EConnectionStyle.Bolted)
                {
                    //Distance from top of angle to first bolt - long spacing
                    labelName = (wclip.BoltOslOnSupport.EdgeDistLongDir).ToString() + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 2", labelName,
                        new Vector3(0, firstBoltPointY + (float)wclip.BoltOslOnSupport.EdgeDistLongDir * METRIC, -(float)edgeDist), new Vector3(0, firstBoltPointY, -(float)edgeDist), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                    var extraV = 0.0f;

                    if (wclip.BoltStagger == EBoltStagger.Support)
                    {
                        extraV = (float)wclip.BoltOslOnSupport.SpacingLongDir * METRIC * 0.5f;
                    }

                    //Bolt callouts
                    var total = (wclip.BoltOslOnSupport.NumberOfRows - 1) * wclip.BoltOslOnSupport.SpacingLongDir * METRIC;
                    labelName = (wclip.BoltOslOnSupport.NumberOfRows - 1).ToString() + "@" + wclip.BoltOslOnSupport.SpacingLongDir.ToString() + "=" + (total / METRIC) + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 3", labelName,
                        new Vector3(0, firstBoltPointY - extraV, -(float)edgeDist),
                        new Vector3(0, firstBoltPointY - (float)(total) - extraV, -(float)edgeDist), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                    //Number of bolts callout
                    labelName = (wclip.BoltOslOnSupport.NumberOfRows * 2).ToString() + " Bolts\n" + wclip.BoltOslOnSupport.BoltName;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "num bolts label", labelName,
                        new Vector3(0, firstBoltPointY - (float)total, -(float)(edgeDist + component.Shape.USShape.tw / 2)), new Vector3(0, -10, -10));

                    //Gage callout
                    labelName = component.GageOnColumn.ToString() + unitString;//(((float)((edgeDist) + component.Shape.USShape.tw / 2) * 2) / METRIC).ToString() + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 4", labelName,
                        new Vector3(0, firstBoltPointY - (float)total, -(float)((edgeDist) + component.Shape.USShape.tw / 2)),
                        new Vector3(0, firstBoltPointY - (float)total, (float)((edgeDist) + component.Shape.USShape.tw / 2)), 7 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                }
                else
                {
                    //Welded?
                    //Delete the bolted dimensions
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 2");
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 3");
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "num bolts label");
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " ClipAngle", isRight.ToString() + "clip dim 4");
                }

                //Clip front width label
                var clipStart = new Vector3((float)primaryWidth * Math.Sign(-x), bottomPlatePointY, 0);
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " ClipAngle " + isRight.ToString() + "clip dim 5", (!isLongLegOSL ? wclip.LongLeg : wclip.ShortLeg).ToString() + unitString, 
                    clipStart, clipStart + new Vector3((float)(Math.Sign(-x) * (!isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC)), 0, 0), Vector3.forward, Math.Sign(-x) * 15.0f, 
                    MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
            }
        }
    }
}