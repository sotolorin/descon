using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawSeat(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        if (!MiscMethods.IsBeam(component.MemberType)) return;
        if (component.WinConnect.ShearSeat != null)
        {
            x = GetPrimaryWidth() * Math.Sign(x);

            var seat = component.WinConnect.ShearSeat;
            var isRight = x > 0;
            var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

            useCustomMaskList = true;

            //TODO: Add the stiffener methods
            var angleFlip = seat.ShortLegOn == EShortLegOn.SupportSide ? true : false;
            //Create the top angle
//            mesh = CreateTopShearSeat(seat, -x, y + yOffset, isRight ? 0 : 180, isRight ? 0 : 0, angleFlip);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            customMaskList.Add(ViewMask.LEFT | ViewMask.RIGHT | ViewMask.FRONT | ViewMask.TOP);

            if (seat.Connection == ESeatConnection.Bolted || seat.Connection == ESeatConnection.Welded)
            {
                //Create the bottom angle
//                mesh = CreateBottomShearSeat(seat, -x, -y + yOffset, isRight ? 180 : 0, isRight ? 180 : 180, angleFlip);
                connectionMeshes.Add(mesh);
                connectionMats.Add(MeshCreator.GetConnectionMaterial());

                customMaskList.Add(ViewMask.LEFT | ViewMask.RIGHT | ViewMask.FRONT);
            }
            else
            {
                //Create the bottom stiffener
                switch(seat.Stiffener)
                {
                    case ESeatStiffener.Tee:
                        mesh = CreateStiffenerTee(seat.StiffenerTee, new Vector3((float)(-x + seat.StiffenerWidth / 2 * Math.Sign(-x)), (float)-y, 0), new Vector3(0, 0, 90), seat.StiffenerWidth);
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        break;
                    case ESeatStiffener.Plate:
                        mesh = CreateStiffenerPlate(seat, new Vector3((float)(-x + seat.PlateWidth / 2 * Math.Sign(-x)), (float)-y + yOffset, 0), new Vector3(0, 0, 0));
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        break;
                }

                customMaskList.Add(ViewMask.LEFT | ViewMask.RIGHT | ViewMask.FRONT);
            }

            var sleg = (float)(angleFlip == false ? seat.TopAngleSupLeg * METRIC : seat.TopAngleBeamLeg * METRIC);
            var bleg = (float)(angleFlip == false ? seat.TopAngleBeamLeg * METRIC : seat.TopAngleSupLeg * METRIC);

            var distFlange = (float)component.WinConnect.Beam.BoltEdgeDistanceOnFlange;

            if (seat.Connection == ESeatConnection.Bolted)
            {
                //Make top the bolts
                //Horizontal bolts
                var boltStartPos = new Vector3((float)(-x + (seat.TopAngle.t * METRIC + boltHeight / 2) * Math.Sign(-x)), (float)y + sleg - (float)seat.Bolt.EdgeDistTransvDir * METRIC + yOffset, (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistLongDir * METRIC));
                var shankLength = (float)(seat.TopAngle.t * METRIC + primaryComponent.Shape.USShape.tf);

                var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(!isRight ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;

                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(!isRight ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Create the beam side bolts
                boltStartPos = new Vector3((float)(-x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange) * Math.Sign(-x)), (float)(y + seat.TopAngle.t * METRIC + boltHeight / 2) + yOffset, (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));
                shankLength = (float)(seat.TopAngle.t * METRIC + component.Shape.USShape.tf);

                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingTransvDir * METRIC * Math.Sign(-x), 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;

                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingTransvDir * METRIC * Math.Sign(-x), 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Combine all the bolts so far
                //I have to do this because the bottom bolts will show in the top view, combing them makes assigning the view mask easier
                //Down below, combine the bottom bolts
                MeshCreator.Reset();
                MeshCreator.Add(boltMeshes.ToArray());

                Mesh topMesh = MeshCreator.Create();
                boltMeshes.Clear();

                //Make the bottom column bolts
                sleg = (float)(angleFlip == false ? seat.Angle.b * METRIC : seat.Angle.d * METRIC);
                bleg = (float)(angleFlip == false ? seat.Angle.d * METRIC : seat.Angle.b * METRIC);

                shankLength = (float)(seat.Angle.t * METRIC + primaryComponent.Shape.USShape.tf);
                boltStartPos = new Vector3((float)(-x + (seat.Angle.t * METRIC + boltHeight / 2) * Math.Sign(-x)), (float)-y - sleg + (float)seat.Bolt.EdgeDistLongDir * METRIC + yOffset, 
                    (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistTransvDir * METRIC));

                //Create the bottom column bolts
                for (int i = 0; i < seat.Bolt.NumberOfLines; ++i)
                {
                    boltMesh = CreateBoltArray(boltStartPos - Vector3.forward * i * (float)seat.Bolt.SpacingTransvDir, Vector3.up * (float)seat.Bolt.SpacingTransvDir * METRIC, angleFlip ? 1 : seat.Bolt.NumberOfRows, (float)seat.Bolt.BoltSize * METRIC, new Vector3(!isRight ? 90 : 270, 90, 0), shankLength);
                    boltMeshes.Add(boltMesh);
                }

                boltStartPos.z *= -1;

                //Make the bottom beam bolts
                shankLength = (float)(seat.Angle.t * METRIC + component.Shape.USShape.tf);
                boltStartPos = new Vector3((float)(-x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange) * Math.Sign(-x)), (float)-y - (float)seat.Angle.t - boltHeight / 2 + yOffset, (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));
                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingLongDir * METRIC * Mathf.Sign((float)x), !angleFlip ? 1 : seat.Bolt.NumberOfRows, (float)seat.Bolt.BoltSize * METRIC, new Vector3(-180, -90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)seat.Bolt.SpacingLongDir * METRIC * Mathf.Sign((float)x), !angleFlip ? 1 : seat.Bolt.NumberOfRows, (float)seat.Bolt.BoltSize * METRIC, new Vector3(-180, -90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                MeshCreator.Reset();
                MeshCreator.Add(boltMeshes.ToArray());

                Mesh bottomMesh = MeshCreator.Create();
                boltMeshes.Clear();

                //Now add the bolt meshes to the bolt list
                boltMeshes.Add(topMesh);
                boltMeshes.Add(bottomMesh);

                //Custom bolt mask list
                customBoltMaskList.Add(ViewMask.LEFT | ViewMask.TOP | ViewMask.RIGHT | ViewMask.FRONT);
                customBoltMaskList.Add(ViewMask.LEFT | ViewMask.RIGHT | ViewMask.FRONT);
            }
            else if(seat.Connection == ESeatConnection.WeldedStiffened)
            {
                if(seat.Stiffener == ESeatStiffener.Plate || seat.Stiffener == ESeatStiffener.Tee)
                {
                    //Draw the two bottom bolts
                    var boltStartPos = new Vector3((float)(-x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback) * Math.Sign(-x)), (float)(-y + yOffset + component.Shape.tf + boltHeight / 2), (float)(component.WinConnect.Beam.GageOnFlange / 2 * METRIC));
                    var shankLength = (float)(seat.PlateThickness * METRIC + component.Shape.tf);

                    var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, -90, 0), shankLength);
                    boltMeshes.Add(boltMesh);

                    boltStartPos.z *= -1;

                    boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)seat.Bolt.SpacingLongDir * METRIC, 1, (float)seat.Bolt.BoltSize * METRIC, new Vector3(0, -90, 0), shankLength);
                    boltMeshes.Add(boltMesh);

                    //Combine the bolt meshes
                    MeshCreator.Reset();
                    MeshCreator.Add(boltMeshes.ToArray());

                    Mesh bottomMesh = MeshCreator.Create();
                    boltMeshes.Clear();

                    //Now add the bolt meshes to the bolt list
                    boltMeshes.Add(bottomMesh);

                    //Custom bolt mask list
                    customBoltMaskList.Add(ViewMask.LEFT | ViewMask.RIGHT | ViewMask.FRONT);
                }
            }

            sleg = (float)(angleFlip == false ? seat.TopAngleSupLeg * METRIC : seat.TopAngleBeamLeg * METRIC);
            bleg = (float)(angleFlip == false ? seat.TopAngleBeamLeg * METRIC : seat.TopAngleSupLeg * METRIC);

            var botSleg = (float)(angleFlip == false ? seat.Angle.b * METRIC : seat.Angle.d * METRIC);
            var botBleg = (float)(angleFlip == false ? seat.Angle.d * METRIC : seat.Angle.b * METRIC);

            var points = new float[50];
            points[0] = (float)(-x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange * METRIC) * Math.Sign(-x));
            points[1] = (float)-x;
            points[2] = (float)(-component.Shape.USShape.d / 2 + yOffset - seat.Angle.t);
            points[3] = (float)(-x + (bleg) * Math.Sign(-x));
            points[4] = (float)-component.Shape.USShape.d / 2 + yOffset - (botSleg - (float)seat.Bolt.EdgeDistLongDir * METRIC);
            points[5] = (float)(-x + (seat.Angle.t * METRIC + boltHeight) * Math.Sign(-x));
            points[6] = (float)(-component.Shape.USShape.d / 2 + yOffset) - botSleg;
            points[7] = -(float)seat.AngleLength * METRIC / 2;
            points[8] = (float)component.WinConnect.Beam.GageOnFlange / 2 * METRIC;//(float)(seat.AngleLength * METRIC / 2 - distFlange * METRIC);
            points[9] = (float)component.Shape.USShape.d / 2 + yOffset;
            points[10] = (float)(seat.AngleLength * METRIC / 2 - seat.Bolt.EdgeDistTransvDir * METRIC);
            points[11] = (float)(component.Shape.USShape.d / 2 + yOffset + seat.TopAngle.t);
            points[12] = (float)(-x + (botBleg) * Math.Sign(-x));
            points[13] = (float)(-component.Shape.USShape.d / 2 + yOffset);
            points[14] = (float)((-component.Shape.USShape.d / 2 + yOffset) - botSleg + seat.Bolt.EdgeDistLongDir * METRIC);
            points[15] = (float)((-component.Shape.USShape.d / 2 + yOffset) - botSleg + seat.Bolt.EdgeDistLongDir + seat.Bolt.SpacingLongDir * METRIC);
            points[16] = (float)(-x + seat.PlateWidth * Math.Sign(-x));
            points[17] = (float)(-y + yOffset - seat.PlateThickness);
            points[18] = (float)(-y + yOffset - seat.PlateThickness - seat.StiffenerLength);
            points[19] = (float)(-x + seat.PlateWidth / 2 * Math.Sign(-x));
            points[20] = (float)(component.Shape.USShape.d / 2 + yOffset);
            points[21] = (float)(-x + (component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback) * Math.Sign(-x));
            points[22] = (float)(-seat.StiffenerWidth);
            points[23] = (float)(-y + yOffset - seat.PlateThickness - seat.StiffenerLength / 2);

            //Stiffener Tee stuff
            points[24] = (float)(-x + (seat.StiffenerWidth) * Math.Sign(-x));
            points[25] = (float)(-y + yOffset - seat.StiffenerTee.d);

            //Double leader
            points[26] = (float)(y + yOffset + sleg);

            //Seat dimensions
            var labelName = "";

            //Destroy old labels
//            MessageQueueTest.DestroyLabelsSelective(MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener, false);

            switch(seat.Connection)
            {
                case ESeatConnection.Bolted:
                    //Gage on flange
                    labelName = component.GageOnFlange.ToString() + unitString;//((float)(seat.AngleLength * METRIC / 2 - distFlange * METRIC) * 2).ToString() + unitString;
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + isRight.ToString() + "seat dim 4", labelName,
                        new Vector3(0, points[9], -points[8]),
                        new Vector3(0, points[9], points[8]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Gage on column
                    if (seat.Bolt.NumberOfLines <= 2)
                    {
                        labelName = component.GageOnColumn.ToString() + unitString;
                        MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + isRight.ToString() + "seat dim 3", labelName, new Vector3(0, points[4], -points[10]),
                            new Vector3(0, points[4], points[10]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                    }
                    else
                    {
                        labelName = (seat.Bolt.NumberOfLines - 1) + "@" + seat.Bolt.SpacingTransvDir + unitString;
                        MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + isRight.ToString() + "seat dim 3", labelName, new Vector3(0, points[4], -points[10]),
                            new Vector3(0, points[4], points[10]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                    }

                    //Draw number of rows of bolts dimension
                    //TODO: Fix this
                    if (!angleFlip)
                    {
                        //labelName = GetFootString((float)seat.Bolt.SpacingTransvDir);
                        //MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat dim 11", labelName, new Vector3(0, points[14], points[7]),
                        //    new Vector3(0, points[15], points[7]), -5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                    }
                    else
                    {
                        MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).DestroyLabel(MessageQueueTest.GetSide(x) + " Seat" + seat.Connection,isRight.ToString() + "seat dim 11");
                    }

                    //Seat height
                    labelName = GetFootString(botSleg / METRIC);
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat dim 2", labelName, new Vector3(0, points[6], points[7]),
                        new Vector3(0, points[13], points[7]), -10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    //Draw the seat width
                    labelName = GetFootString((float)component.WinConnect.Beam.BoltEdgeDistanceOnFlange);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat dim 1", labelName, new Vector3(points[1], points[2]),
                        new Vector3(points[0], points[2]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

                    //Add the bottom seat dimension
                    labelName = seat.AngleName + "X " + seat.AngleLength + unitString;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat label", labelName,
                    new Vector3(points[12], points[2], 0), new Vector3(8 * Math.Sign(-x), -3, 0));

                    //Add the seat dimension
                    labelName = seat.Bolt.NumberOfLines + " Bolts\n" + seat.Bolt.BoltName;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat bolt label", labelName,
                    new Vector3(points[5], points[4], 0), new Vector3(8 * Math.Sign(-x), -5, 0));

                    //Draw the edge dist
                    labelName = GetFootString((float)seat.Bolt.EdgeDistLongDir);
                    MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + " " + isRight.ToString() + "seat dim 12", labelName, new Vector3(0, points[14], points[7]),
                        new Vector3(0, points[6], points[7]), 7.5f * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Left);
                break;

                case ESeatConnection.WeldedStiffened:

                    //Draw top weld
                    labelName = "\n" + GetFootString((float)seat.WeldSizeBeam);
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 21", labelName,
                        new Vector3(points[3], points[20]), new Vector3(12 * Math.Sign(-x), 15), 8, "Welds may be\nreplaced by bolts.", -120, 110, false, new Vector3(points[1], points[26]));

                    //Draw bolt flange spacing
                    labelName = GetFootString((float)(component.WinConnect.Beam.BoltEdgeDistanceOnFlange + component.EndSetback));
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 22", labelName,
                        new Vector3(points[1], points[13]), new Vector3(points[21], points[13]), 12 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Right);

                    if (seat.Stiffener == ESeatStiffener.Plate)
                    {
                        //Draw the plate labels
                        labelName = "PL" + seat.PlateLength + "X" + seat.PlateWidth + "X" + seat.PlateThickness + " " + unitString;
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 10", labelName,
                            new Vector3(points[16], points[17], 0), new Vector3(15 * Math.Sign(-x), -12));

                        labelName = "PL" + seat.StiffenerLength + "X" + seat.StiffenerWidth + "X" + seat.StiffenerThickness + " " + unitString;
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 11", labelName,
                            new Vector3(points[16], points[18], 0), new Vector3(12 * Math.Sign(-x), -15));

                        //Draw stiffener width
                        labelName = GetFootString((float)seat.StiffenerWidth);
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 20", labelName,
                            new Vector3(points[16], points[18]), new Vector3(points[1], points[18]), 12 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                        //Add bottom weld
                        labelName = GetFootString((float)seat.WeldSizeSupport);
                        labelName += "\n" + labelName;
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 23", labelName,
                            new Vector3(points[19], points[17]), new Vector3(14 * Math.Sign(-x), -17), 5, "1.9in. Min.\neach side", -120, 110);

                        //Draw the plate length
                        labelName = GetFootString((float)seat.StiffenerLength);
                        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 24", labelName,
                            new Vector3(0, points[17], points[22]), new Vector3(0, points[18], points[22]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);

                        //Draw the two weld labels
                        labelName = GetFootString((float)seat.WeldSizeSupport);
                        labelName += "\n" + labelName;
                        MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 25", labelName,
                            new Vector3(0, points[17], points[22] / 2), new Vector3(0, -17, 14 * Math.Sign(-x)), 4, "1.9in. Min.\neach side", -120, 110);

                        //Use the same label name
                        labelName = GetFootString((float)seat.WeldSizeSupport);
                        labelName += "\n" + labelName;
                        MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 26", labelName,
                            new Vector3(0, points[23], (float)seat.StiffenerThickness / 2), new Vector3(0, -17, -25 * Math.Sign(-x)), 0);
                    }
                    else if(seat.Stiffener == ESeatStiffener.Tee)
                    {
                        //Draw tee name
                        labelName = seat.StiffenerTeeName + " X " + seat.StiffenerWidth + unitString;
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 31", labelName,
                            new Vector3(points[24], points[25], 0), new Vector3(12 * Math.Sign(-x), -15));

                        //Draw tee width
                        labelName = GetFootString((float)seat.StiffenerWidth);
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 32", labelName,
                            new Vector3(points[16], points[25]), new Vector3(points[1], points[25]), 12 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                        //Draw the two weld labels
                        labelName = GetFootString((float)seat.WeldSizeBeam);
                        labelName = "\n" + labelName;
                        MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 33", labelName,
                            new Vector3(0, points[17], points[22] / 2), new Vector3(0, -17, 14 * Math.Sign(-x)), 4, "2.226in. Min.\neach side", -120, 110);

                        //Use the same label name
                        labelName = GetFootString((float)seat.WeldSizeBeam);
                        labelName += "\n" + labelName;
                        MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat label 34", labelName,
                            new Vector3(0, points[23], (float)seat.StiffenerThickness / 2), new Vector3(0, -17, -25 * Math.Sign(-x)), 0);

                        //Draw the plate length
                        labelName = GetFootString((float)seat.StiffenerLength);
                        MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Seat" + seat.Connection + seat.Stiffener + " " + isRight.ToString() + "seat dim 35", labelName,
                            new Vector3(0, points[17], points[22]), new Vector3(0, points[18], points[22]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Top);
                    }

                break;

                case ESeatConnection.BoltedStiffenedPlate:
                break;

                case ESeatConnection.Welded:
                break;
            }

            //TODO: The destroy selected will reset the position on the seat labels
            //Add the top seat dimension
            labelName = "L" + seat.TopAngleSupLeg.ToString() + "X" + seat.TopAngleBeamLeg.ToString() + "X" + GetFootStringSimple((float)seat.TopAngle.t) + " X " + seat.TopAngleLength + unitString;
            var sside = isRight ? "Right" : "Left";
            labelName += "\n(All " + sside + " Side Attachments " + seat.MaterialName + ")";
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Seat " + isRight.ToString() + "seat label 2", labelName,
            new Vector3(points[3], 0, -points[7]), new Vector3(5 * Math.Sign(-x), 0, 15));
        }
    }
}
