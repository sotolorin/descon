using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawFlangePlate(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        if (component.WinConnect.MomentFlangePlate != null)
        {
            int signTrans = -1;
            if (component.MemberType == EMemberType.LeftBeam) signTrans = 1;
            var topX = signTrans * (GetPrimaryWidth() + component.WinConnect.MomentFlangePlate.TopLength * (float)ConstNum.METRIC_MULTIPLIER / 2);
            var topY = component.Shape.USShape.d / 2 + component.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2 + yOffset;
            var bottomX = signTrans * (GetPrimaryWidth() + component.WinConnect.MomentFlangePlate.BottomLength * (float)ConstNum.METRIC_MULTIPLIER / 2);
            var bottomY = -1 * (component.Shape.USShape.d / 2) - component.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2 + yOffset;
            subType = EMemberSubType.Moment;
            isShearBolt = false;

            //Top Flange Plate 
//            mesh = CreateTopFlangePlateMesh(component.WinConnect.MomentFlangePlate, (float)topX, (float)topY, component.MemberType == EMemberType.LeftBeam);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            //Bottom Flange Plate
//            mesh = CreateBottomFlangePlateMesh(component.WinConnect.MomentFlangePlate, (float)bottomX, (float)bottomY, component.MemberType == EMemberType.LeftBeam);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var fplate = component.WinConnect.MomentFlangePlate;

            var DistanceToFirstBoltDisplay = (float)(component.EndSetback * METRIC + component.WinConnect.Beam.BoltEdgeDistanceOnFlange * METRIC);

            var boltStartPos = new Vector3((float)((GetPrimaryWidth() + DistanceToFirstBoltDisplay) * Math.Sign(-x)), (float)(component.Shape.USShape.d / 2 + fplate.TopThickness * METRIC + boltHeight / 2) + yOffset, (float)(fplate.TopWidth * METRIC / 2 - fplate.Bolt.EdgeDistTransvDir * METRIC));

            var shankLength = (float)(fplate.TopThickness * METRIC + component.Shape.USShape.tf);

            //Create 1st set of Flange bolts
            var boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(-x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                (float)fplate.Bolt.BoltSize * METRIC, Vector3.zero, shankLength);

            boltMeshes.Add(boltMesh);

            //Create 2st set of Flange bolts
            boltStartPos.z *= -1;

            boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(-x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                (float)fplate.Bolt.BoltSize * METRIC, Vector3.zero, shankLength);

            boltMeshes.Add(boltMesh);

            //Create the bottom set of bolts
            boltStartPos.y = (float)(component.Shape.USShape.d / 2 + fplate.BottomThickness * METRIC + boltHeight / 2) + yOffset;
            boltStartPos.z = (float)(fplate.BottomWidth * METRIC / 2 - fplate.Bolt.EdgeDistTransvDir * METRIC);
            boltStartPos.y = -(float)(component.Shape.USShape.d / 2 + fplate.BottomThickness * METRIC + boltHeight / 2) + yOffset;

            boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(-x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                (float)fplate.Bolt.BoltSize * METRIC, new Vector3(180, 0, 0), shankLength);

            boltMeshes.Add(boltMesh);

            //Create 4rd set of Flange bolts
            boltStartPos.z *= -1;

            boltMesh = CreateBoltArray(boltStartPos, Vector3.right * (float)(Math.Sign(-x) * fplate.Bolt.SpacingLongDir * METRIC), fplate.Bolt.NumberOfRows,
                (float)fplate.Bolt.BoltSize * METRIC, new Vector3(180, 0, 0), shankLength);

            boltMeshes.Add(boltMesh);


            var flangeName = "TOP PL: " + fplate.TopLength + unitString + " X " + fplate.TopWidth + unitString + " X " + fplate.TopThickness + unitString + " - " + component.WinConnect.MomentFlangePlate.MaterialName;
            flangeName += "\n";

            flangeName += "BOT. PL: " + fplate.BottomLength + unitString + " X " + fplate.BottomWidth + unitString + " X " + fplate.BottomThickness + unitString + " - " + component.WinConnect.MomentFlangePlate.MaterialName;

            //Add dimension
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + "flange label", flangeName,
                new Vector3((float)(topX + (Math.Sign(-x) * fplate.TopLength * METRIC / 2)), 0, (float)-fplate.TopWidth * METRIC / 2), new Vector3(15 * Math.Sign(-x), 0, -10));

            var points = new float[8];

            points[0] = (float)(GetPrimaryWidth() + DistanceToFirstBoltDisplay) * Math.Sign(-x);
            points[1] = (float)(component.Shape.USShape.d / 2 + fplate.TopThickness * METRIC) + yOffset;
            points[2] = (float)(GetPrimaryWidth() * Math.Sign(-x));
            points[3] = (float)(points[0] + ((fplate.Bolt.NumberOfRows - 1) * fplate.Bolt.SpacingLongDir * METRIC) * Math.Sign(-x));
            points[4] = (float)(fplate.TopWidth * METRIC - (fplate.Bolt.EdgeDistTransvDir * METRIC * 2));
            points[5] = points[4] / 2;
            points[6] = (float)(points[2] + (fplate.TopLength * METRIC) * Math.Sign(-x));
            points[7] = (float)(fplate.TopWidth * METRIC / 2);

            var isRight = (Math.Sign(-x) < 0);

            //Add distance to first bolt for the top plate
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + isRight.ToString() + "fplate dim 1", GetFootString((float)DistanceToFirstBoltDisplay / METRIC), new Vector3(points[2], points[1]), new Vector3(points[0], points[1]), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

            //Add bolt callouts
            var calloutName = (fplate.Bolt.NumberOfRows - 1).ToString() + "@" + fplate.Bolt.SpacingLongDir.ToString() + "=" + GetFootString((float)((fplate.Bolt.NumberOfRows - 1) * fplate.Bolt.SpacingLongDir));
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + isRight.ToString() + "fplate dim 2", calloutName, new Vector3(points[0], points[1]), new Vector3(points[3], points[1]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

            //Add number of bolts callout
            calloutName = (fplate.Bolt.NumberOfRows * 2).ToString() + " Bolts\n" + fplate.Bolt.BoltName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + Math.Sign(x).ToString() + "num bolts label", calloutName, new Vector3(points[3], points[1]), new Vector3(8 * Math.Sign(-x), 4));

            calloutName = GetFootString((float)fplate.TopPlateToSupportWeldSize);
            calloutName = calloutName + "\n" + calloutName;

            if (fplate.PlateToSupportWeldType == EWeldType.Fillet)
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel(MessageQueueTest.GetSide(x) + " FlangePlate " + Math.Sign(x).ToString() + "plate fillet weld", calloutName, new Vector3(points[2], points[1]), new Vector3(5 * Math.Sign(-x), 5), 5, "Typ.", -62, 92);
            else if (fplate.PlateToSupportWeldType == EWeldType.CJP)
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel(MessageQueueTest.GetSide(x) + " FlangePlate " + Math.Sign(x).ToString() + "plate fillet weld", "", new Vector3(points[2], points[1]), new Vector3(5 * Math.Sign(-x), 5), 7, "Typ.", -62, 92);

            //Add top bolt callouts
            calloutName = GetFootString(points[4] / METRIC);
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + isRight.ToString() + "fplate dim 3", calloutName, new Vector3(points[6], 0, -points[5]), new Vector3(points[6], 0, points[5]), 13 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

            calloutName = GetFootString((float)fplate.TopWidth);
            MessageQueueTest.instance.GetView(isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " FlangePlate " + isRight.ToString() + "fplate dim 4", calloutName, new Vector3(0, points[1], -points[7]), new Vector3(0, points[1], points[7]), 8, MessageQueueTest.GetDimensionColor());
        }
    }
}
