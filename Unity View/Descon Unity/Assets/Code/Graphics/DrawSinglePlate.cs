using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawSinglePlate(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        object singlePlate = null;
        //TODO: Fix x value given to the single plates
        //x += Math.Sign(x) * primaryComponent.Shape.USShape.tf;
        if (MiscMethods.IsBeam(component.MemberType)) singlePlate = component.WinConnect.ShearWebPlate;
        else singlePlate = component.BraceConnect.BasePlate;
        if (singlePlate != null)
        {
            //Edit: This was not positioning single plates for gussets right for me- Shayla
            if (!isBrace)
            {
                //The plate needs the beam, thickness to offset correctly
                if (primaryComponent != null && primaryComponent.ShapeType == EShapeType.HollowSteelSection)
                {
                    if (isPrimaryInPlane)
                        x = (float)(primaryComponent.Shape.USShape.Ht / 2 * Math.Sign(x));
                    else
                        x = (float)(primaryComponent.Shape.USShape.B / 2 * Math.Sign(x));
                }
                else
                {
                    if (isPrimaryInPlane)
                        x = (float)(primaryComponent.Shape.USShape.d / 2.0f * Math.Sign(x));
                    else
                        x = (float)(primaryComponent.Shape.USShape.tw / 2.0f * Math.Sign(x));
                }
            }


            var primaryWidth = x;

            if (primaryWidth < 0)
                primaryWidth *= -1;

            var gussetTranslation = new Vector3();
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
                if (uR) gussetTranslation = new Vector3(-(float)urX, (float)urY, 0f);
                if (uL) gussetTranslation = new Vector3(-(float)ulX, (float)ulY, 0f);
                if (lR) gussetTranslation = new Vector3(-(float)lrX, (float)lrY, 0f);
                if (lL) gussetTranslation = new Vector3(-(float)lLX, (float)lLY, 0f);
//                mesh = CreateSinglePlateMesh(singlePlate, x, y, component.Shape.USShape.tw, gussetTranslation);
            }
            else
            {
                //Single plate beam offset
                var vector = Vector3.zero;
                var plate = singlePlate as WCShearWebPlate;
                if (plate != null)
                {
                    var zOffset = component.Shape.USShape.tw / 2 + plate.Thickness * METRIC / 2;
                    //vector.z = (float)zOffset;

                    vector.y = yOffset;
                }

                if (component.MomentConnection != EMomentCarriedBy.DirectlyWelded)
                {
//                    mesh = CreateSinglePlateMesh(singlePlate, x, component.Shape.USShape.d, component.Shape.USShape.tw, vector, component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC);
                }
                else
                {
                    mesh = CreateDirectlyWeldedSinglePlateMesh(component, primaryComponent, new Vector3((float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(-x)), yOffset, (float)((component.Shape.USShape.tw + plate.Thickness) / 2)), new Vector3(0, x < 0 ? 180 : 0, 0));

                    //Make custom lines for this shape
                    useCustomConnLines = true;

                    ResetConnectionLines();

                    var t = (float)(plate.Thickness * ConstNum.METRIC_MULTIPLIER);

                    var plateHeight = (float)(plate.Height * ConstNum.METRIC_MULTIPLIER);
                    var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
                    var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + plate.Bolt.EdgeDistTransvDir);
                    var radius = 0.5f;

                    var points = new float[20];

                    points[0] = (float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(-x));
                    points[1] = points[0] + plateWidth * Math.Sign(-x);
                    points[2] = plateHeight / 2 + yOffset;
                    points[3] = -plateHeight / 2 + yOffset;
                    points[4] = ((float)plate.Length / 2) + radius + yOffset;
                    points[5] = -((float)plate.Length / 2) - radius + yOffset;
                    points[6] = points[1] + (float)plate2Width * Math.Sign(-x);
                    points[7] = (float)component.Shape.USShape.tw;
                    points[8] = ((float)plate.Length / 2) + yOffset;
                    points[9] = ((float)-plate.Length / 2) + yOffset;

                    //Create the custom lines
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[2]), new Vector3(points[1], points[2])));
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[3]), new Vector3(points[1], points[3])));
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], points[3]), new Vector3(points[1], points[5])));
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], points[2]), new Vector3(points[1], points[4])));

                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[2]), new Vector3(points[0], points[3])));

                    //Draw the small curves
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(points[1], points[4]), new Vector3(points[1], points[4] - radius),
                        new Vector3(points[1], points[4] - radius), new Vector3(points[1] + radius * Math.Sign(-x), points[4] - radius)));

                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(points[1], points[5]), new Vector3(points[1], points[5] + radius),
                        new Vector3(points[1], points[5] + radius), new Vector3(points[1] + radius * Math.Sign(-x), points[5] + radius)));

                    //Draw the lines for the smaller plate
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[6], points[4] - radius), new Vector3(points[1] + radius * Math.Sign(-x), points[4] - radius)));
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[6], points[5] + radius), new Vector3(points[1] + radius * Math.Sign(-x), points[5] + radius)));
                    customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[6], points[5] + radius), new Vector3(points[6], points[4] - radius)));

                    //Make the lines in the other viewports
                    //Make the side lines
                    var viewSide = Math.Sign(-x) < 0 ? ViewMask.RIGHT : ViewMask.LEFT;

                    //Make the side large plate
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] - t / 2), new Vector3(0, points[2], points[7] + t / 2)));
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] - t / 2), new Vector3(0, points[3], points[7] - t / 2)));
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] + t / 2), new Vector3(0, points[3], points[7] + t / 2)));
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[3], points[7] - t / 2), new Vector3(0, points[3], points[7] + t / 2)));

                    //Make the two lines for the small plate
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[8], points[7] - t / 2), new Vector3(0, points[8], points[7] + t / 2)));
                    customConnLines[0][viewSide].Add(CustomLine.CreateNormalLine(new Vector3(0, points[9], points[7] - t / 2), new Vector3(0, points[9], points[7] + t / 2)));

                    //Make the top lines
                    customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] - t / 2), new Vector3(points[6], 0, points[7] - t / 2), 0.25f, 0.25f));
                    customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] + t / 2), new Vector3(points[0], 0, points[7] - t / 2), 0.25f, 0.25f));
                    customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[7] + t / 2), new Vector3(points[6], 0, points[7] - t / 2), 0.25f, 0.25f));
                    customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] + t / 2), new Vector3(points[6], 0, points[7] + t / 2), 0.25f, 0.25f));

                    //Create the small plate line division
                    customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[7] + t / 2), new Vector3(points[1], 0, points[7] - t / 2), 0.25f, 0.25f));
                }
            }

            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var isRight = Math.Sign(-x) < 0;

            //Create the bolts
            var splate = singlePlate as BCBasePlate;
            if (splate != null)
            {
                //Do stuff
            }

            //TODO: Fix this ugly mess
            var splatewin = singlePlate as WCShearWebPlate;
            if (splatewin != null)
            {
                var boltStartPos = new Vector3((float)(-x + (splatewin.Width * METRIC - splatewin.Bolt.EdgeDistTransvDir * METRIC) * Math.Sign(-x)),
                    (float)GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, splatewin.Length, splatewin.Bolt.EdgeDistLongDir, splatewin.Position) + yOffset, (float)component.Shape.USShape.tw / 2 + (float)splatewin.Thickness * METRIC + boltHeight / 2);

                var shankLength = (float)(splatewin.Thickness * METRIC + component.Shape.USShape.tw);

                for (int i = 0; i < splatewin.Bolt.NumberOfLines; ++i)
                {
                    var boltMesh = CreateBoltArray(boltStartPos + Vector3.left * i * (float)splatewin.Bolt.SpacingTransvDir * METRIC * Math.Sign(-x), Vector3.down * (float)splatewin.Bolt.SpacingLongDir * METRIC,
                        splatewin.Bolt.NumberOfRows, (float)splatewin.Bolt.BoltSize * METRIC, new Vector3(90, 0, 0), shankLength);
                    boltMeshes.Add(boltMesh);
                }

                //Create the dimensions
                var points = new float[30];
                points[0] = (float)(GetPrimaryWidth() + splatewin.Width * METRIC) * Math.Sign(-x);
                points[1] = (float)(splatewin.Length * METRIC / 2);
                points[2] = (float)(GetPrimaryWidth() * Math.Sign(-x));
                points[3] = (float)(GetPrimaryWidth() + splatewin.Width * METRIC - splatewin.Bolt.EdgeDistTransvDir * METRIC) * Mathf.Sign((float)-x);
                points[4] = (float)(component.Shape.USShape.d / 2 - (splatewin.Length * METRIC / 2 - splatewin.Bolt.EdgeDistTransvDir * METRIC));
                points[5] = boltStartPos.y;
                points[6] = (float)component.Shape.USShape.d / 2 + yOffset;
                points[7] = points[5] - (float)((splatewin.Bolt.NumberOfRows - 1) * splatewin.Bolt.SpacingLongDir * METRIC);
                points[8] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC - (float)splatewin.Length * METRIC;
                points[9] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC;
                points[10] = (float)((splatewin.Bolt.NumberOfLines - 1) * splatewin.Bolt.SpacingTransvDir * METRIC);
                points[11] = points[3] - points[10] * Mathf.Sign((float)-x);
                points[12] = points[5] + (float)splatewin.Bolt.EdgeDistLongDir * METRIC;


                //Plate length dimension
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " +  isRight.ToString() + "splate dim 7", GetFootString((float)splatewin.Length), new Vector3(points[0], points[9]), new Vector3(points[0], points[8]), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Number of bolts
                var calloutName = (splatewin.Bolt.NumberOfRows * splatewin.Bolt.NumberOfLines).ToString() + " Bolts\n" + splatewin.Bolt.BoltName;
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " + isRight + "splate number of bolts", calloutName, new Vector3(0, points[7]), new Vector3(0, -8, -17));

                calloutName = (splatewin.Bolt.NumberOfRows - 1).ToString() + "@" + splatewin.Bolt.SpacingLongDir.ToString() + "=" + GetFootString((float)((splatewin.Bolt.NumberOfRows - 1) * splatewin.Bolt.SpacingLongDir));
                MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate " +  isRight.ToString() + "splate dim 11", calloutName, new Vector3(0, points[5]), new Vector3(0, points[7]), 12 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                if (component.MomentConnection != EMomentCarriedBy.DirectlyWelded)
                {
                    //Destroy the other labels n stuff
                    //MessageQueueTest.DestroyLabelsWithTag(false, "DWeldedSPlate");

                    var plateName = "PL" + splatewin.Length.ToString() + unitString + "X" + splatewin.Width.ToString() + unitString + "X" + splatewin.Thickness.ToString() + unitString + " - " + splatewin.MaterialName;

                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + Math.Sign(x).ToString() + "splate name label", plateName, new Vector3(points[0], points[8]), new Vector3(30 * Math.Sign(-x), -20, 0));

                    //Plate width dimension
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " +  isRight.ToString() + "splate dim 5",
                        GetFootString((float)splatewin.Width), new Vector3(points[0], points[8]), new Vector3(points[2], points[8]), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);

                    //Distance to bolt from outside dimension
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " +  isRight.ToString() + "splate dim 6", GetFootString((float)splatewin.Bolt.EdgeDistTransvDir),
                        new Vector3(points[0], points[8]), new Vector3(points[3], points[8]), 12 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);

                    //TOB to plate
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " +  isRight.ToString() + "splate dim 8", GetFootString((float)(points[6] - points[9]) / METRIC), new Vector3(points[0], points[6]), new Vector3(points[0], points[9]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Right);

                    //Bolt spacing
                    if (points[10] > 0)
                    {
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " +  isRight.ToString() + "splate dim 9", GetFootString(points[10]),
                            new Vector3(points[3], points[8]), new Vector3(points[11], points[8]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());
                    }
                    else
                    {
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyLabel(MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate", isRight.ToString() + "splate dim 9");
                    }

                    //Add top of beam to first bolt
                    calloutName = GetFootString((float)component.WinConnect.Beam.DistanceToFirstBoltDisplay);
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " +  isRight.ToString() + "splate dim 10", calloutName,
                        new Vector3(points[0], points[6]), new Vector3(points[0], points[5]), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    calloutName = GetFootString((float)splatewin.SupportWeldSize);
                    calloutName = calloutName + "\n" + calloutName;
                    var yy = GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d, splatewin.Length * (float)ConstNum.METRIC_MULTIPLIER, splatewin.Bolt.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, splatewin.Position) + yOffset;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " NotDWeldedSPlate " + isRight + "splate weld label", calloutName, new Vector3(0, yy, 0), new Vector3(0, -10, 15));
                }
                else
                {
                    var t = (float)(splatewin.Thickness * ConstNum.METRIC_MULTIPLIER);
                    var plateHeight = (float)(splatewin.Height * ConstNum.METRIC_MULTIPLIER);
                    var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
                    var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + splatewin.Bolt.EdgeDistTransvDir);
                    var radius = 0.5f;

                    //Destroy the other labels n stuff
                    //MessageQueueTest.DestroyLabelsWithTag(false, "NotDWeldedSPlate");

                    //Draw the single plate with the weird cutouts and their dimensions
                    points[13] = (float)(GetPrimaryWidth() + plateWidth) * Math.Sign(-x);
                    points[14] = -plateHeight / 2 + yOffset - (float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness;
                    points[15] = (float)(GetPrimaryWidth()) * Math.Sign(-x);
                    points[16] = points[13] + plate2Width * Math.Sign(-x);
                    points[17] = yOffset - (float)splatewin.Length / 2;
                    points[18] = yOffset;
                    points[19] = yOffset + (float)plateHeight / 2;
                    points[20] = (float)(GetPrimaryWidth() + plateWidth / 2) * Math.Sign(-x);
                    points[21] = yOffset + (float)plateHeight / 2 + (float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness / 2;
                    points[22] = (float)-primaryComponent.Shape.USShape.d / 2;
                    points[23] = points[15] + (float)(component.WinConnect.MomentDirectWeld.Top.a + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2) * Math.Sign(-x);
                    points[24] = (float)component.Shape.USShape.bf / 2;
                    points[25] = points[15] + (float)(component.WinConnect.MomentDirectWeld.Top.a * Mathf.Cos(45 * Mathf.Deg2Rad) + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2) * Math.Sign(-x);
                    points[26] = (float)primaryComponent.Shape.USShape.d / 2 - (float)(primaryComponent.Shape.USShape.tf + component.WinConnect.MomentDirectWeld.Top.a * Mathf.Sin(45 * Mathf.Deg2Rad));
                    points[27] = (float)(-primaryComponent.Shape.USShape.d / 2 + primaryComponent.Shape.USShape.tf);

                    //Distance to bolt from outside dimension
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " +  isRight.ToString() + "splate dim 20", GetFootString((float)splatewin.Bolt.EdgeDistTransvDir),
                        new Vector3(points[3], points[12]), new Vector3(points[0], points[12]), 20 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

                    //Draw first plate width
                    var labelName = GetFootString(plateWidth);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " +  isRight.ToString() + "splate dim 21", labelName,
                        new Vector3(points[13], points[14]), new Vector3(points[15], points[14]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Draw 2nd plate width
                    labelName = GetFootString(plate2Width);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " +  isRight.ToString() + "splate dim 22", labelName,
                        new Vector3(points[16], points[14]), new Vector3(points[13], points[14]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Draw the plate name
                    labelName = GetFootString(t) + " PL - " + splatewin.MaterialName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate name label 2", labelName, 
                        new Vector3(points[16], points[17]), new Vector3(3 * Math.Sign(-x), -12, 0));

                    //Add welds
                    labelName = GetFootString((float)splatewin.SupportWeldSize);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate weld 10", labelName,
                        new Vector3(points[15], points[18]), new Vector3(30 * Math.Sign(-x), -30, 0), 0);

                    //Side weld
                    labelName = GetFootString((float)splatewin.BeamWeldSize);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate weld 11", labelName,
                        new Vector3(points[20], points[19]), new Vector3(-20 * Math.Sign(-x), -10, 0), 5, "Typ.", -50, 100);

                    //Top
                    labelName = GetFootString((float)component.WinConnect.MomentDirectWeld.Top.FilletWeldW2);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate weld 12", labelName,
                        new Vector3(points[15], points[21]), new Vector3(30 * Math.Sign(-x), 10, 0), 5, "Top and\nBottom PL", -90, 100);

                    //Make the top dimensions
                    //Make the plate length
                    labelName = GetFootString((float)(component.WinConnect.MomentDirectWeld.Top.a + (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2));
                    MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " +  isRight.ToString() + "splate dim 23", labelName,
                        new Vector3(points[15], 0, points[22]), new Vector3(points[23], 0, points[22]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

                    //Make the plate width
                    labelName = GetFootString((float)component.Shape.USShape.bf);
                    MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " +  isRight.ToString() + "splate dim 24", labelName,
                        new Vector3(points[23], 0, -points[24]), new Vector3(points[23], 0, points[24]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

                    //Make the top weld symbols
                    labelName = ""; //Do nothing
                    MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate weld 13", labelName,
                        new Vector3(points[23], 0, -points[24]/2), new Vector3(30 * Math.Sign(-x), 0, -20), 6, "TYP.", -60, 90);

                    labelName = GetFootString((float)splatewin.SupportWeldSize);
                    labelName += "\n" + labelName;
                    MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate weld 14", labelName,
                        new Vector3(points[20], 0, points[27]), new Vector3(30 * Math.Sign(-x), 0, 20), 5, "Top and Bottom PL\nBoth Flanges", -130, 100);

                    //Draw wedge plate names
                    labelName = GetFootString((float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness) + " Top PL - " + component.WinConnect.MomentDirectWeld.MaterialName;
                    labelName += "\n" + GetFootString((float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness) + " Bot PL - " + component.WinConnect.MomentDirectWeld.MaterialName;
                    MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " SinglePlate" + " DWeldedSPlate " + Math.Sign(x).ToString() + "splate name label 3", labelName,
                        new Vector3(points[25], 0, points[26]), new Vector3(10 * Math.Sign(-x), 0, 5));
                }
            }
        }
    }
}
