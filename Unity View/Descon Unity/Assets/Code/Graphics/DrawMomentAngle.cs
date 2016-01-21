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
    public void DrawMomentAngle(DetailData component, double x = 0, double y = 0)
    {
        //Do Moment shear
        var mclip = component.WinConnect.MomentFlangeAngle;

        if (mclip != null)
        {
            x = GetPrimaryWidth() * Math.Sign(-x);
            var t = GetClipThickness(mclip);
            var primaryWidth = GetPrimaryWidth();

            var isLongLegOSL = false; //TODO: component.WinConnect.MomentFlangeAngle.OSL == EOSL.LongLeg;
            var clipZ = mclip.Length * (float)ConstNum.METRIC_MULTIPLIER;

            if (clipZ == 0)
                clipZ = 3;

            var translation = new Vector3((float)x + t / 2 * Math.Sign(x), yOffset + (float)component.Shape.d / 2 + t / 2, 0);
            var flip = component.MemberType == EMemberType.LeftBeam;

            //Create the top angle
            mesh = CreateClipAngleMesh(mclip, clipZ, translation, new Vector3(flip ? -90 : -90, flip ? 0 : 180, 0), isLongLegOSL);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            //Create the bottom angle
            translation = new Vector3((float)x + t / 2 * Math.Sign(x), yOffset - (float)component.Shape.d / 2 - t / 2, 0);
            mesh = CreateClipAngleMesh(mclip, clipZ, translation, new Vector3(flip ? 90 : 90, flip ? 0 : 180, 0), isLongLegOSL);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var isRight = MessageQueueTest.GetSide(x);
            var points = new float[9];
            var labelName = "";

            //Draw beam side bolts
            if(mclip.BeamConnection == EConnectionStyle.Bolted)
            {
                var boltStartPos = new Vector3((float)(primaryWidth + mclip.LongLeg - mclip.BeamBolt.EdgeDistTransvDir) * Math.Sign(x), (float)(yOffset + component.Shape.d / 2 + t + boltHeight / 2), (float)(-clipZ / 2 + mclip.BeamBolt.EdgeDistLongDir));

                var shankLength = (float)(mclip.Angle.tw * METRIC + component.Shape.USShape.tf);

                //First row
                var boltMesh = CreateBoltArray(boltStartPos, Vector3.right * Math.Sign(-x) * (float)mclip.BeamBolt.SpacingTransvDir * METRIC, mclip.BeamBolt.NumberOfRows, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Second row
                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * Math.Sign(-x) * (float)mclip.BeamBolt.SpacingTransvDir * METRIC, mclip.BeamBolt.NumberOfRows, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(0, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Bottom second row
                boltStartPos.y *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * Math.Sign(-x) * (float)mclip.BeamBolt.SpacingTransvDir * METRIC, mclip.BeamBolt.NumberOfRows, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(0, 90, 180), shankLength);
                boltMeshes.Add(boltMesh);

                //Bottom first row
                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.right * Math.Sign(-x) * (float)mclip.BeamBolt.SpacingTransvDir * METRIC, mclip.BeamBolt.NumberOfRows, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(0, 90, 180), shankLength);
                boltMeshes.Add(boltMesh);

                //Draw dimensions
                points[0] = (float)(primaryWidth + mclip.LongLeg - mclip.BeamBolt.EdgeDistTransvDir) * Math.Sign(x);
                points[1] = (float)(clipZ / 2 - mclip.BeamBolt.EdgeDistLongDir);
                points[2] = (float)(yOffset + component.Shape.d / 2 + mclip.ShortLeg - mclip.ColumnBolt.EdgeDistTransvDir);
                points[3] = (float)(yOffset + component.Shape.d / 2);
                points[4] = (float)(primaryWidth + t) * Math.Sign(x);
                points[5] = (float)(clipZ / 2 - mclip.ColumnBolt.EdgeDistLongDir);
                points[6] = (float)(primaryWidth + mclip.LongLeg - mclip.BeamBolt.EdgeDistTransvDir - mclip.BeamBolt.SpacingTransvDir) * Math.Sign(x);
                points[7] = (float)(yOffset + component.Shape.d / 2 + t);
                points[8] = (float)(primaryWidth) * Math.Sign(x);

                var viewSide = (Mathf.Sign((float)x) < 0) ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

                labelName = GetFootString(points[1] * 2);
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 11", labelName,
                    new Vector3(points[0], 0, points[1]), new Vector3(points[0], 0, -points[1]), 22 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0);

                labelName = mclip.BeamBolt.NumberOfBolts + " Bolts\n" + mclip.BeamBolt.BoltName + " (Typ.)";
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 12", labelName,
                    new Vector3(points[0], 0, points[1]), new Vector3(-6 * Math.Sign(-x), 0, 12));

                labelName = mclip.ColumnBolt.NumberOfBolts + " Bolts\n" + mclip.ColumnBolt.BoltName + " (Typ.)";
                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 13", labelName,
                    new Vector3(points[4], 0, -points[5]), new Vector3(-6 * Math.Sign(-x), 0, -12));

                //Draw the front spacing for the bolts
                labelName = GetFootString((float)mclip.BeamBolt.SpacingTransvDir);
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 14", labelName,
                    new Vector3(points[0], points[7]), new Vector3(points[6], points[7]), 15 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Bottom);

                labelName = GetFootString((float)(mclip.LongLeg - mclip.BeamBolt.EdgeDistTransvDir - mclip.BeamBolt.SpacingTransvDir));
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 15", labelName,
                    new Vector3(points[6], points[7]), new Vector3(points[8], points[7]), 24 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Left);
            }

            //Draw OSL/column side bolts
            if (mclip.ColumnConnection == EConnectionStyle.Bolted)
            {
                var boltStartPos = new Vector3((float)(primaryWidth + t + boltHeight / 2) * Math.Sign(x), (float)(yOffset + component.Shape.d / 2 + mclip.ShortLeg - mclip.ColumnBolt.EdgeDistTransvDir), (float)(-clipZ / 2 + mclip.ColumnBolt.EdgeDistLongDir));

                var shankLength = (float)(mclip.Angle.tw * METRIC + primaryComponent.Shape.USShape.tf);

                var isLeft = x > 0;

                //First row
                var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)mclip.ColumnBolt.SpacingTransvDir * METRIC, 1, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(isLeft ? 90 : -90, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)mclip.ColumnBolt.SpacingTransvDir * METRIC, 1, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(isLeft ? 90 : -90, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.y *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)mclip.ColumnBolt.SpacingTransvDir * METRIC, 1, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(isLeft ? 90 : -90, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                boltStartPos.z *= -1;
                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)mclip.ColumnBolt.SpacingTransvDir * METRIC, 1, (float)mclip.BeamBolt.BoltSize * METRIC, new Vector3(isLeft ? 90 : -90, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);

                //Draw dimensions
                points[0] = (float)(primaryWidth) * Math.Sign(x);
                points[1] = (float)(clipZ / 2 - mclip.ColumnBolt.EdgeDistLongDir);
                points[2] = (float)(yOffset + component.Shape.d / 2 + mclip.ShortLeg - mclip.ColumnBolt.EdgeDistTransvDir);
                points[3] = (float)(yOffset + component.Shape.d / 2);

                var viewSide = (Mathf.Sign((float)x) < 0) ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

                labelName = GetFootString(points[1] * 2);
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 1", labelName,
                    new Vector3(0, points[2], points[1]), new Vector3(0, points[2], -points[1]), -10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Bottom);

                labelName = GetFootString((float)(mclip.ShortLeg - mclip.ColumnBolt.EdgeDistTransvDir));
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 2", labelName,
                    new Vector3(0, points[2], -points[1]), new Vector3(0, points[3], -points[1]), -10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor(), 0, 0, Descon.Data.EOffsetType.Right);
            }

            points[0] = (float)(primaryWidth + mclip.LongLeg) * Math.Sign(x);
            points[1] = (float)(primaryWidth) * Math.Sign(x);
            points[2] = (float)(clipZ / 2);
            points[3] = (float)(yOffset + component.Shape.d / 2 + t);

            //Draw the length of the angle dimension
            MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawDimension("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 20", GetFootString((float)(mclip.LongLeg)),
                new Vector3(points[0], 0, points[2]), new Vector3(points[1], 0, points[2]), -10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

            //Draw the part name label
            labelName = mclip.AngleName + " X " + clipZ + unitString + "\n" + "(Typ.) - " + mclip.MaterialName;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Moment " + MessageQueueTest.GetSide(x) + " Angle " + isRight.ToString() + "angle dim 21", labelName,
                new Vector3(points[0], points[3]), new Vector3(-6 * Math.Sign(-x), 6));
        }
    }
}