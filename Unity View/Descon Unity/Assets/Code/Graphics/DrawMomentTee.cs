using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawMomentTee(DetailData component, double x = 0, double y = 0, bool conBool = false, double rotationAngle = 0.0)
    {
        var tee = component.WinConnect.MomentTee;
        var isLeft = x < 0;

        x = GetPrimaryWidth() * Math.Sign(x);
        var pWidth = GetPrimaryWidth();

        if(tee != null)
        {
            //Create top tee
            mesh = CreateTeeMesh(tee, new Vector3((float)-x, (float)(component.Shape.USShape.d / 2 + tee.TopTeeShape.USShape.tw / 2) + yOffset, 0), new Vector3(90, isLeft ? 180 : 0, 0));
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            //Create bottom tee
            mesh = CreateTeeMesh(tee, new Vector3((float)-x, (float)(-component.Shape.USShape.d / 2 - tee.TopTeeShape.USShape.tw / 2) + yOffset, 0), new Vector3(90, isLeft ? 180 : 0, 0));
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var tw = tee.TopTeeShape.USShape.tw;
            var tf = tee.TopTeeShape.USShape.tf;

            var teeLength = tee.TopLengthAtStem;

            //Create the bolts
            if (tee.TeeConnectionStyle == EConnectionStyle.Bolted)
            {
                var boltStartPos = new Vector3((float)((pWidth + tf + boltHeight / 2) * Math.Sign(-x)), (float)(component.Shape.USShape.d / 2 + tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC + tw / 2) + yOffset, (float)(tee.Column_g / 2 * METRIC));
                var shankLength = (float)(tf + primaryComponent.Shape.USShape.tf);

                var spacing = (float)(tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC) * 2;

                var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(isLeft ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(isLeft ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.y = (float)(-component.Shape.USShape.d / 2 - tee.TopTeeShape.USShape.bf / 2 + tee.BoltColumnFlange.EdgeDistTransvDir * METRIC - tw / 2) + yOffset;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.up * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(isLeft ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.up * spacing, 2, (float)tee.BoltColumnFlange.BoltSize * METRIC, new Vector3(isLeft ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Create beam bolts
                boltStartPos = new Vector3((float)((pWidth + tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC) * Math.Sign(-x)), (float)(component.Shape.USShape.d / 2 + tw) + yOffset + boltHeight / 2, (float)(-tee.Beam_g / 2));
                shankLength = (float)(tw + component.Shape.USShape.tf);

                boltMesh = CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(-x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(-x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.y = -(float)(component.Shape.USShape.d / 2 + tw) + yOffset - boltHeight / 2;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(-x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(180, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.left * (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(-x), tee.BoltBeamStem.NumberOfRows, (float)tee.BoltBeamStem.BoltSize * METRIC, new Vector3(180, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Draw all the labels
                var points = new float[11];
                points[0] = (float)(pWidth + tee.TopTeeShape.USShape.d) * Math.Sign(-x);
                points[1] = -(float)(teeLength / 2);
                points[2] = (float)tee.Beam_g / 2;
                points[3] = (float)((pWidth + tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC) * Math.Sign(-x));
                points[4] = points[3] + (float)tee.BoltBeamStem.SpacingLongDir * METRIC * Math.Sign(x);
                points[5] = (float)((tee.TopTeeShape.USShape.d - tee.BoltBeamStem.EdgeDistLongDir * METRIC - tee.BoltBeamStem.SpacingLongDir * METRIC) * Math.Sign(-x));
                points[6] = (float)pWidth * Math.Sign(-x);
                points[7] = (float)(component.Shape.USShape.d / 2 + tee.TopTeeShape.USShape.bf / 2 - tee.BoltColumnFlange.EdgeDistTransvDir * METRIC + tw / 2) + yOffset;
                points[8] = (float)(tee.Column_g / 2 * METRIC);
                points[9] = (float)(component.Shape.USShape.d / 2 + tw) + yOffset;
                points[10] = (float)(pWidth + tw) * Math.Sign(-x);

                var isRight = MessageQueueTest.GetSide(x);
                var labelName = tee.TopTeeShapeName + " - " + tee.TopMaterialName + " (Top and Bottom)";
                labelName += "\n Length = " + teeLength * METRIC + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 1", labelName, 
                    new Vector3(points[0], 0, points[1]), new Vector3(6 * Math.Sign(-x), 0, -6));

                labelName = tee.Beam_g * METRIC + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 2", labelName,
                    new Vector3(points[3], 0, -points[2]), new Vector3(points[3], 0, points[2]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                labelName = (float)tee.BoltBeamStem.SpacingLongDir * METRIC + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 3", labelName,
                    new Vector3(points[3], 0, points[2]), new Vector3(points[4], 0, points[2]), 6 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                labelName = points[5] + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 4", labelName,
                    new Vector3(points[4], 0, points[2]), new Vector3(points[6], 0, points[2]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                var viewSide = isLeft ? CameraViewportType.LEFT : CameraViewportType.RIGHT;

                //Draw the left/right side gage
                labelName = tee.Column_g + unitString;
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 5", labelName,
                    new Vector3(0, points[7], -points[8]), new Vector3(0, points[7], points[8]), -10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Draw the tee names
                labelName = tee.BoltBeamStem.NumberOfBolts + " Bolts (TYP.)\n" + tee.BoltBeamStem.BoltName;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 6", labelName,
                    new Vector3(points[3], points[9]), new Vector3(8 * Math.Sign(-x), 4));

                labelName = tee.BoltColumnFlange.NumberOfBolts + " Bolts (TYP.)\n" + tee.BoltColumnFlange.BoltName;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Tee " + isRight.ToString() + "mtee dim 7", labelName,
                    new Vector3(points[10], points[7]), new Vector3(6 * Math.Sign(-x), 10));
            }
        }
    }
}