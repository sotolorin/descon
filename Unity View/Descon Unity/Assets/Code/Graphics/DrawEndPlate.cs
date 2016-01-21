using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawEndPlate(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        object endplate = null;

        if (!MiscMethods.IsBeam(component.MemberType)) endplate = component.WinConnect.ShearEndPlate;
        else endplate = component.WinConnect.ShearEndPlate;
        if (endplate != null)
        {
            if (!isBrace)
            {
                x = GetPrimaryWidth() * Math.Sign(x);
            }

            var gussetTranslation = new Vector3();
            var nonGussetTranslation = new Vector3((float)x, (float)y, 0f);
            if (isBrace && gusset != null)
            {
                //TODO: Will's values, remove when calculations are corrected
                gusset.ColumnSide = 17f;
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
//                mesh = CreateEndPlateMesh(endplate, x, y, gussetTranslation);
            }
            else
            {
                var ePlate = component.WinConnect.ShearEndPlate;

//                mesh = CreateEndPlateMesh(endplate, x + ePlate.Thickness * 2 * METRIC * Math.Sign(x), GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay * (float)ConstNum.METRIC_MULTIPLIER, component.Shape.USShape.d,
//                        ePlate.Length * (float)ConstNum.METRIC_MULTIPLIER, ePlate.Bolt.EdgeDistLongDir * (float)ConstNum.METRIC_MULTIPLIER, ePlate.Position), new Vector3(0, yOffset, 0));
            }

            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());

            var endPlate = component.WinConnect.ShearEndPlate;

            //Create bolts

            //TODO Does Brace to beam always have bolts?
            if (true)//component.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
            {
                //Create the bolts
                var shankLength = (float)(endPlate.Thickness * METRIC + primaryComponent.Shape.USShape.tf);

                var isLeftSide = Math.Sign(-x) >= 0;

                //Add the first vertical line
                var boltStartPos = new Vector3((float)(-x + (endPlate.Thickness * METRIC + boltHeight / 2) * Math.Sign(-x)),
                    GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, endPlate.Length, endPlate.Bolt.EdgeDistLongDir, endPlate.Position) + yOffset, (float)(endPlate.Width * METRIC / 2 - endPlate.Bolt.EdgeDistTransvDir * METRIC)); //(float)component.Shape.USShape.tw / 2 + (float)endPlate.Thickness + boltHeight / 2

                var boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)endPlate.Bolt.SpacingLongDir * METRIC, endPlate.Bolt.NumberOfRows, (float)endPlate.Bolt.BoltSize * METRIC, new Vector3(isLeftSide ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);
                //Add the second
                boltStartPos.z *= -1;//(float)component.Shape.USShape.tw / 2 + (float)endPlate.Thickness + boltHeight / 2

                boltMesh = CreateBoltArray(boltStartPos, Vector3.down * (float)endPlate.Bolt.SpacingLongDir * METRIC, endPlate.Bolt.NumberOfRows, (float)endPlate.Bolt.BoltSize * METRIC, new Vector3(isLeftSide ? 90 : 270, 90, 0), shankLength);
                boltMeshes.Add(boltMesh);
            }
            //else
            //{
            //    //Create the welds
            //    var weldSize = 0.375f;
            //    var plate = component.WinConnect.ShearEndPlate;
            //    //On the plate
            //    connectionMeshes.Add(CreateWeldMesh(new Vector3((float)-x, 0, 0), new Vector3(weldSize / 2.0f, (float)plate.Length + weldSize, (float)plate.Width + weldSize), Vector3.zero));
            //    connectionMats.Add(MeshCreator.GetWeldMaterial());
            //}

            //Add dimensions for the endplate
            if (primaryComponent != null)
            {
                var points = new float[10];
                points[0] = (float)(-x);
                points[1] = (float)(-x + endPlate.Thickness * METRIC * Math.Sign(-x));
                points[2] = GetBoltPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.USShape.d, endPlate.Length, endPlate.Bolt.EdgeDistLongDir, endPlate.Position) + yOffset;
                points[3] = points[2] + (float)endPlate.Bolt.EdgeDistLongDir * METRIC; //Top of plate
                points[4] = points[2] - (float)((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir * METRIC);
                points[5] = points[3] - (float)endPlate.Length * METRIC; //Bottom of plate
                points[6] = -(float)endPlate.Width * METRIC / 2;
                points[7] = (float)endPlate.Width * METRIC / 2;
                points[8] = (float)component.Shape.USShape.d / 2 - points[2] + yOffset * 2;
                points[9] = points[7] - (float)endPlate.Bolt.EdgeDistTransvDir * METRIC;

                var isRight = MessageQueueTest.GetSide(x);

                //Distance to top of bolt
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 1", GetFootString((float)endPlate.Bolt.EdgeDistLongDir),
                    new Vector3(points[1], points[3]), new Vector3(points[1], points[2]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                var labelName = (endPlate.Bolt.NumberOfRows - 1).ToString() + "@" + endPlate.Bolt.SpacingLongDir.ToString() + "=" + ((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir) + unitString;
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 2", labelName,
                    new Vector3(points[1], points[2]), new Vector3(points[1], points[4]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());


                //Add the bolt name callout
                var labelOffset = new Vector3((float)(-x + endPlate.Thickness * Math.Sign(-x)), points[2] - (float)((endPlate.Bolt.NumberOfRows - 1) * endPlate.Bolt.SpacingLongDir), 0);
                labelName = (endPlate.Bolt.NumberOfRows * 2).ToString() + " Bolts"; //TODO: Edit the name
                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " +  Math.Sign(x).ToString() + "endplate bolt label", labelName, labelOffset, new Vector3(15 * Math.Sign(-x), -25, 0));

                var viewSide = (Mathf.Sign((float)-x) < 0) ? CameraViewportType.RIGHT : CameraViewportType.LEFT;

                //Length of endplate
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 3", endPlate.Length + unitString,
                    new Vector3(0, points[3], points[6]), new Vector3(0, points[5], points[6]), 5 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Width of the endplate
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 4", endPlate.Width + unitString,
                    new Vector3(0, points[5], points[6]), new Vector3(0, points[5], points[7]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Draw gage
                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 6", component.GageOnColumn.ToString() + unitString,
                    new Vector3(0, points[4], -points[9]), new Vector3(0, points[4], points[9]), 7 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //TOB to first Bolt
                labelName = component.WinConnect.Beam.DistanceToFirstBoltDisplay + unitString;

                if (endPlate.Position != EPosition.Top)
                {
                    labelName = points[8] + unitString;
                }

                MessageQueueTest.instance.GetView(viewSide).AddDrawDimension("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + isRight.ToString() + "endplate dim 5", labelName,
                    new Vector3(0, (float)component.Shape.USShape.d / 2 + yOffset, points[6]), new Vector3(0, points[2], points[6]), 10 * Math.Sign(-x), MessageQueueTest.GetDimensionColor());

                //Add the end plate name callout
                labelOffset = new Vector3(0, points[5], -(float)endPlate.Width / 2);
                labelName = "PL " + endPlate.Length.ToString() + "in.X " + endPlate.Width.ToString() + "in.X " + endPlate.Thickness.ToString() + "in. " + endPlate.MaterialName;
                MessageQueueTest.instance.GetView(viewSide).AddDrawLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + Math.Sign(x).ToString() + "endplate callout", labelName, labelOffset, new Vector3(0, -8, -25));

                //Test weld symbol
                labelName = GetFootString((float)endPlate.WeldSize);
                labelName = labelName + "\n" + labelName;

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Shear " + MessageQueueTest.GetSide(x) + " EndPlate " + Math.Sign(x).ToString() + "endplate weld", labelName, new Vector3(points[1], points[3] - (float)endPlate.Length / 2, 0), new Vector3(Math.Sign(-x) * 10, -15, 0));

            }
        }
    }
}
