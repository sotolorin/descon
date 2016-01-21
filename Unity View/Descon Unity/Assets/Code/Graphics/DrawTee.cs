using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawTee(DetailData component, double x = 0, double y = 0, bool conBool = false, double rotationAngle = 0.0)
    {
        var shearTee = component.WinConnect.ShearWebTee;

        if (shearTee != null)
        {
            //mesh = CreateTeeMesh2(tee, x, y, component.Shape.USShape.tw, rotationAngle, component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER);
            mesh = CreateTeeMesh(shearTee, new Vector3(-(float)x,
                GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, shearTee.SLength * METRIC, shearTee.BoltWebOnStem.EdgeDistLongDir * METRIC, shearTee.Position) + yOffset, (float)(component.Shape.USShape.tw / 2 + shearTee.Size.USShape.tw / 2)), 
                new Vector3(0, (float)rotationAngle, 0));
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var viewSide = (Mathf.Sign((float)-x) < 0) ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

            if (shearTee != null)
            {
                //Bolts on beam
                if (shearTee.BeamSideConnection == EConnectionStyle.Bolted)
                {
                    var boltStartPos = new Vector3((float)(-x + (shearTee.Size.d * METRIC - shearTee.BoltWebOnStem.EdgeDistTransvDir * METRIC) * Math.Sign(-x)),
                        GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, shearTee.SLength * METRIC, shearTee.BoltWebOnStem.EdgeDistLongDir * METRIC, shearTee.Position) + yOffset, 
                        (float)(component.Shape.USShape.tw / 2 * METRIC + shearTee.Size.tw * METRIC + boltHeight / 2));
                    var shankLength = (float)(shearTee.Size.tw * METRIC + component.Shape.USShape.tw);

                    var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)shearTee.BoltWebOnStem.SpacingLongDir * METRIC, shearTee.StemNumberOfRows, (float)shearTee.BoltWebOnStem.BoltSize * METRIC, new Vector3(0, 90, 90), shankLength);
                    boltMeshes.Add(boltMesh);
                }

                //Bolts on Support side
                if (shearTee.OSLConnection == EConnectionStyle.Bolted)
                {
                    var boltStartPos = new Vector3((float)(-x + (shearTee.Size.tf * METRIC + boltHeight / 2) * Math.Sign(-x)),
                        GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, shearTee.SLength * METRIC, shearTee.BoltOslOnFlange.EdgeDistLongDir * METRIC, shearTee.Position) + yOffset,
                        (float)(component.Shape.USShape.tw * METRIC / 2 + shearTee.Size.bf * METRIC / 2 - shearTee.BoltOslOnFlange.EdgeDistTransvDir * METRIC));
                    var shankLength = (float)(shearTee.Size.tf * METRIC + primaryComponent.Shape.USShape.tf);

                    var isLeft = x > 0;

                    var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)shearTee.BoltOslOnFlange.SpacingLongDir * METRIC, shearTee.FlangeNumberOfRows, (float)shearTee.BoltOslOnFlange.BoltSize * METRIC, new Vector3(isLeft ? 270 : 90, 90, 0), shankLength);
                    boltMeshes.Add(boltMesh);

                    //Add bolts to the other side
                    boltStartPos.z = (float)(component.Shape.USShape.tw * METRIC - shearTee.Size.bf * METRIC / 2 + shearTee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);

                    boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)shearTee.BoltOslOnFlange.SpacingLongDir * METRIC, shearTee.FlangeNumberOfRows, (float)shearTee.BoltOslOnFlange.BoltSize * METRIC, new Vector3(isLeft ? 270 : 90, 90, 0), shankLength);
                    boltMeshes.Add(boltMesh);
                }
                else
                {
                    var weldSize = GetFootString((float)shearTee.WeldSizeFlange);
                    //Make welded symbol
                    MessageQueueTest.instance.GetView(viewSide).AddDrawWeldLabel(MessageQueueTest.GetSide(x) + " Tee " + "Tee support weld 1", 
                        weldSize + "\n" + weldSize, new Vector3(0, GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, 
                            shearTee.SLength * METRIC, shearTee.BoltWebOnStem.EdgeDistLongDir * METRIC, shearTee.Position) + yOffset, (float)(-shearTee.Size.bf / 2 * METRIC + component.Shape.tw / 2 * METRIC + shearTee.Size.tw / 2 * METRIC)), 
                            new Vector3(0, -20, -15), 5, "Weld Return: 0.375in. on Top", -180, 100);
                }
            }

            //Add dimensions for the tee
            if (primaryComponent != null)
            {
                var points = new float[11];
                points[0] = (float)(-x);
                points[1] = (float)(-x + shearTee.Size.d * METRIC * Math.Sign(-x));
                points[2] = GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * METRIC, component.Shape.USShape.d, shearTee.SLength * METRIC, shearTee.BoltWebOnStem.EdgeDistLongDir * METRIC, shearTee.Position) + yOffset;
                points[3] = points[2] + (float)shearTee.BoltWebOnStem.EdgeDistLongDir * METRIC; //Top of plate
                points[4] = points[2] - (float)((shearTee.BoltWebOnStem.NumberOfBolts - 1) * shearTee.BoltWebOnStem.SpacingLongDir * METRIC);
                points[5] = points[3] - (float)shearTee.SLength * METRIC; //Bottom of plate
                points[6] = -(float)shearTee.Size.d * METRIC / 2;
                points[7] = (float)shearTee.Size.d * METRIC / 2;
                points[8] = (float)component.Shape.USShape.d / 2 - points[2] + yOffset * 2;
                points[9] = (float)(component.Shape.USShape.tw / 2 + shearTee.Size.tw / 2 - shearTee.Size.bf * METRIC / 2 + shearTee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);
                points[10] = (float)(component.Shape.USShape.tw / 2 + shearTee.Size.bf * METRIC / 2 - shearTee.BoltOslOnFlange.EdgeDistTransvDir * METRIC);

                var isRight = MessageQueueTest.GetSide(x);

                //Distance to top of bolt
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 1", GetFootString((float)shearTee.BoltWebOnStem.EdgeDistLongDir),
                    new Vector3(points[1], points[3]), new Vector3(points[1], points[2]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                var labelName = (shearTee.BoltWebOnStem.NumberOfBolts - 1).ToString() + "@" + shearTee.BoltWebOnStem.SpacingLongDir.ToString() + "=" + ((shearTee.BoltWebOnStem.NumberOfBolts - 1) * shearTee.BoltWebOnStem.SpacingLongDir) + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 2", labelName,
                    new Vector3(points[1], points[2]), new Vector3(points[1], points[4]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Number of bolts
                var labelOffset = new Vector3((float)(-x + shearTee.Size.d * Math.Sign(-x) - shearTee.BoltWebOnStem.EdgeDistTransvDir * METRIC * Math.Sign(-x)), points[2] - (float)((shearTee.BoltWebOnStem.NumberOfBolts - 1) * shearTee.BoltWebOnStem.SpacingLongDir), 0);
                labelName = (shearTee.BoltWebOnStem.NumberOfBolts).ToString() + " Bolts\n" + shearTee.BoltWebOnStem.BoltName + "\n" + "Gage = " + shearTee.BoltGageOnTeeStem * METRIC + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Tee " + Math.Sign(x).ToString() + "tee bolt label", labelName, labelOffset, new Vector3(15 * Math.Sign(-x), -25, 0));

                var dist = (float)(component.Shape.USShape.d / 2 + yOffset - points[3]);

                //TOB to plate
                labelName = string.Format("{0:0.###}", dist) + unitString;
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 3", labelName,
                    new Vector3(0, (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(0, points[3], 0), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Gage
                if (shearTee.OSLConnection == EConnectionStyle.Bolted)
                {
                    MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 7", component.GageOnColumn.ToString() + unitString,
                    new Vector3(0, points[4], points[9]), new Vector3(0, points[4], points[10]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());
                }
                else
                {
                    MessageQueueTest.instance.GetView(viewSide).DestroyLabel(MessageQueueTest.GetSide(x) + " Tee", isRight.ToString() + "tee dim 7");
                }

                //Width of the Tee
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 4", shearTee.Size.d + unitString,
                    new Vector3(points[1], points[5], 0), new Vector3(points[0], points[5], 0), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Plate name
                labelName = shearTee.SizeName + " X " + shearTee.SLength + unitString + " - " + shearTee.MaterialName;
                MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 6", labelName,
                    new Vector3(0, points[5], (float)(shearTee.Size.bf / 2 * METRIC + component.Shape.USShape.tw / 2)), new Vector3(0, -10, 10));

                //TOB to first Bolt
                labelName = component.WinConnect.Beam.DistanceToFirstBoltDisplay + unitString;

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "tee dim 5", labelName,
                    new Vector3(points[1], (float)component.Shape.USShape.d / 2 + yOffset, 0), new Vector3(points[1], points[2], 0), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());
            }
        }
    }
}
