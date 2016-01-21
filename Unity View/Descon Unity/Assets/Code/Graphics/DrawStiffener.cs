using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawStiffener(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        var rBeam = brace1;
        var lBeam = brace2;
        var leftStiffenerTop = 0.0f;
        var rightStiffenerTop = 0.0f;

        var leftStiffenerBot = 0.0f;
        var rightStiffenerBot = 0.0f;

        if (!lBeam.IsActive && !rBeam.IsActive)
            return;

        if (lBeam.Moment.Equals(0) && rBeam.Moment.Equals(0)) return;

        useCustomMask = true;
        customMask = ViewMask.D3 | ViewMask.FRONT | ViewMask.TOP;

        if (lBeam.IsActive && !rBeam.IsActive)
        {
            leftStiffenerTop = (float)(lBeam.Shape.USShape.d / 2 + lBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(lBeam);
            rightStiffenerTop = leftStiffenerTop;

            leftStiffenerBot = -(float)(lBeam.Shape.USShape.d / 2 + lBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(lBeam);
            rightStiffenerBot = leftStiffenerBot;
        }
        else if (!lBeam.IsActive && rBeam.IsActive)
        {
            leftStiffenerTop = (float)(rBeam.Shape.USShape.d / 2 + rBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(rBeam);
            rightStiffenerTop = leftStiffenerTop;

            leftStiffenerBot = -(float)(rBeam.Shape.USShape.d / 2 + rBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(rBeam);
            rightStiffenerBot = leftStiffenerBot;
        }
        else
        {
            leftStiffenerTop = (float)(lBeam.Shape.USShape.d / 2 + lBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(lBeam);
            rightStiffenerTop = (float)(rBeam.Shape.USShape.d / 2 + rBeam.WinConnect.MomentFlangePlate.TopThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(rBeam);

            leftStiffenerBot = -(float)(lBeam.Shape.USShape.d / 2 + lBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(lBeam);
            rightStiffenerBot = -(float)(rBeam.Shape.USShape.d / 2 + rBeam.WinConnect.MomentFlangePlate.BottomThickness * (float)ConstNum.METRIC_MULTIPLIER / 2) + (float)GetYOffsetWithEl(rBeam);
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
                mesh = MeshCreator.CreateBoxMesh(new Vector3(0, 0, (float)(component.Shape.USShape.tw / 2 + columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER / 2)), new Vector3((float)columnStiffener.DHorizontal * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DVertical * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER), Vector3.zero);
                connectionMeshes.Add(mesh);
                connectionMats.Add(MeshCreator.GetConnectionMaterial());

                calloutText = (columnStiffener.DNumberOfPlates > 1 ? "2" : "") + "PL " + columnStiffener.DVertical * METRIC + unitString + "X " + columnStiffener.DHorizontal * METRIC + unitString + "X " +
                    columnStiffener.DThickness * METRIC + unitString + " - " + columnStiffener.MaterialName;

                calloutText += "\nIf using groove weld,";
                calloutText += "\nPL. thick. = 0.1875 " + unitString;
                calloutText += "\nPlate to Flange Weld:";
                calloutText += "\n" + GetFootString((float)columnStiffener.DFlangeWeldSize) + " Fillet or equivalent Groove weld";
                calloutText += "\nPl. to Stiff. Weld: " + GetFootString((float)columnStiffener.DWebWeldSize);
                calloutText += "\nFillet or equivalent Groove weld";

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Column Stiffener " + " stiffener text", calloutText, new Vector3(0, 0, 0),
                    new Vector3(10, -20, 0));
            }

            //Rear backing plates
            if (columnStiffener.DNumberOfPlates > 1)
            {
                rearZOffset = (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER;

                mesh = MeshCreator.CreateBoxMesh(new Vector3(0, 0, -(float)(component.Shape.USShape.tw / 2 + columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER / 2)), new Vector3((float)columnStiffener.DHorizontal * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DVertical * (float)ConstNum.METRIC_MULTIPLIER, (float)columnStiffener.DThickness * (float)ConstNum.METRIC_MULTIPLIER), Vector3.zero);
                connectionMeshes.Add(mesh);
                connectionMats.Add(MeshCreator.GetConnectionMaterial());
            }

            //Top stiffener
            if (columnStiffener.TenStiff)
            {
                var leftPoint = new Vector3((float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), leftStiffenerTop, 0);
                var rightPoint = new Vector3(-(float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), rightStiffenerTop, 0);

                //First, if only one side needs stiffener
                if (CommonDataStatic.ColumnStiffener.StiffenerType == EStiffenerType.HoritzotalTwo)
                {
                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());
                }
                else
                {
                    if (columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                    {
                        //Inclined
                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());

                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());
                    }
                    else
                    {
                        //Horizontal stiffeners
                        if (columnStiffener.LeftStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        }
                        else if (columnStiffener.RightStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        }
                    }
                }

                var clip = Math.Max(primaryComponent.Shape.kdet - primaryComponent.Shape.tf, columnStiffener.DThickness);

                //Create the callout description
                calloutText = "PL " + length + unitString + "X " + width + unitString + "X " + thickness + unitString + "(TYP. 4) - " + columnStiffener.MaterialName;
                calloutText += "\n" + "Clip inside corners " + clip * METRIC + unitString + " Max";
                calloutText += "\n" + "Plate to Flange Weld: " + GetFootString((float)columnStiffener.SFlangeWeldSize) + " Double Fillet";
                calloutText += "\n" + "Stiff. Pl to Dbl.Pl Weld: " + GetFootString((float)columnStiffener.SWebWeldSize) + " Fillet";
                calloutText += "\n" + "Stiff. Pl to Web Weld: " + GetFootString((float)columnStiffener.SWebWeldSize) + " Fillet";

                MessageQueueTest.instance.GetView(CameraViewportType.TOP).AddDrawLabel("Column Stiffener " + " stiffener text top", calloutText, new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2),
                    new Vector3(10, 0, 30));
            }

            //Bottom Stiffener
            if (columnStiffener.CompStiff)
            {
                var leftPoint = new Vector3((float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), leftStiffenerBot, 0);
                var rightPoint = new Vector3(-(float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf), rightStiffenerBot, 0);

                //First, if only one side needs stiffener
                if (CommonDataStatic.ColumnStiffener.StiffenerType == EStiffenerType.HoritzotalTwo)
                {
                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    //Create the smaller plates 1
                    mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                    connectionMeshes.Add(mesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());
                }
                else
                {
                    if (columnStiffener.LeftStiffenerRequired && columnStiffener.RightStiffenerRequired)
                    {
                        //Inclined
                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());

                        //Create the smaller plates 1
                        mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, rightPoint, thickness, width, Vector3.zero);
                        connectionMeshes.Add(mesh);
                        connectionMats.Add(MeshCreator.GetConnectionMaterial());
                    }
                    else
                    {
                        //Horizontal stiffeners
                        if (columnStiffener.LeftStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), leftPoint, new Vector3(rightPoint.x, leftPoint.y, rightPoint.z), thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        }
                        else if (columnStiffener.RightStiffenerRequired)
                        {
                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, (float)component.Shape.USShape.tw / 2 + frontZOffset + (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());

                            //Create the smaller plates 1
                            mesh = MeshCreator.CreateSkewedBoxMesh(new Vector3(0, 0, -(float)component.Shape.USShape.tw / 2 - rearZOffset - (float)width / 2), new Vector3(leftPoint.x, rightPoint.y, leftPoint.z), rightPoint, thickness, width, Vector3.zero);
                            connectionMeshes.Add(mesh);
                            connectionMats.Add(MeshCreator.GetConnectionMaterial());
                        }
                    }
                }

            }
        }
    }
}
