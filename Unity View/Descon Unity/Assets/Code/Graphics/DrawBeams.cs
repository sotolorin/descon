using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawBeams(DetailData component, double x = 0, double y = 0, bool conBool = false, double rotationAngle = 0.0)
    {
        updateBaseMesh = true;
        var isInPlane = component.WebOrientation == EWebOrientation.InPlane;
        //var topElInches = (float)((component.WinConnect.Beam.TopElFeet * 12.0f) + component.WinConnect.Beam.TopElInches + (component.WinConnect.Beam.TopElNumerator / component.WinConnect.Beam.TopElDenominator));
        var z = rotationAngle == 0 ? y : x;
        var isDirectlyWelded = false;
        var endSetBack = (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;

        if (component.MemberType != EMemberType.PrimaryMember && component.MomentConnection == EMomentCarriedBy.DirectlyWelded)
        {
            isDirectlyWelded = true;

            //Add backing bars
            if (isPrimaryInPlane)
            {
                if (component.MemberType == EMemberType.LeftBeam || component.MemberType == EMemberType.RightBeam)
                {
                    var dimensions = new Vector3(0.6875f, 0.2f, (float)component.Shape.USShape.bf);
                    var tempMesh = MeshCreator.CreateBoxMesh(new Vector3((float)(GetPrimaryWidth() + dimensions.x / 2) * Math.Sign(-x), (float)(component.Shape.USShape.d / 2 - component.Shape.USShape.tf - dimensions.y / 2) + yOffset, 0), dimensions, Vector3.zero);
                    connectionMeshes.Add(tempMesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());

                    tempMesh = MeshCreator.CreateBoxMesh(new Vector3((float)(GetPrimaryWidth() + dimensions.x / 2) * Math.Sign(-x), (float)(-component.Shape.USShape.d / 2 - dimensions.y / 2) + yOffset, 0), dimensions, Vector3.zero);
                    connectionMeshes.Add(tempMesh);
                    connectionMats.Add(MeshCreator.GetConnectionMaterial());
                }
            }
            else
            {
                useCustomConnLines = true;
                //Make those wedge things
                var points = new float[20];
                points[0] = (float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(-x));
                points[1] = (float)(component.WinConnect.ShearWebPlate.Height / 2 + yOffset + component.WinConnect.MomentDirectWeld.Top.StiffenerThickness / 2);
                points[2] = (float)component.WinConnect.ShearWebPlate.Clip;
                points[3] = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
                points[4] = (float)component.WinConnect.MomentDirectWeld.Top.a;
                points[5] = (float)component.Shape.USShape.bf;
                points[6] = (float)(primaryComponent.Shape.USShape.d - primaryComponent.Shape.USShape.tf * 2);
                points[7] = (float)component.WinConnect.MomentDirectWeld.Top.StiffenerThickness;
                points[8] = (float)component.WinConnect.MomentDirectWeld.Bottom.a;
                points[9] = (float)component.WinConnect.MomentDirectWeld.Bottom.StiffenerThickness;
                points[10] = -(float)component.WinConnect.ShearWebPlate.Height / 2 + yOffset - points[9] / 2;
                points[11] = (float)(primaryComponent.Shape.USShape.d / 2 - primaryComponent.Shape.USShape.tf);
                points[12] = (float)(primaryComponent.Shape.USShape.bf / 2 * Math.Sign(-x));
                points[13] = points[12] + points[4] * Math.Sign(-x);

                var tempMesh = MeshCreator.CreateWedgePlate(new Vector3(points[0], points[1], 0), points[2], points[3], points[4], points[5], points[6], points[7], new Vector3(0, x < 0 ? 180 : 0, 0));

                connectionMeshes.Add(tempMesh);
                connectionMats.Add(MeshCreator.GetConnectionMaterial());

                //Create the bottom wedge
                tempMesh = MeshCreator.CreateWedgePlate(new Vector3(points[0], points[10], 0), points[2], points[3], points[8], points[5], points[6], points[9], new Vector3(0, x < 0 ? 180 : 0, 0));

                connectionMeshes.Add(tempMesh);
                connectionMats.Add(MeshCreator.GetConnectionMaterial());

                ResetConnectionLines();

                //Create the custom lines
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[12], 0, -points[11]), new Vector3(points[0] + points[2] * Math.Sign(-x), 0, -points[11])));
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[12], 0, points[11]), new Vector3(points[0] + points[2] * Math.Sign(-x), 0, points[11])));

                //Clip lines
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[0], 0, -points[11] + points[2]), new Vector3(points[0] + points[2] * Math.Sign(-x), 0, -points[11])));
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[0], 0, points[11] - points[2]), new Vector3(points[0] + points[2] * Math.Sign(-x), 0, points[11])));

                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[12], 0, -points[11]), new Vector3(points[13], 0, -points[5] / 2)));
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[12], 0, points[11]), new Vector3(points[13], 0, points[5] / 2)));

                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[13], 0, -points[5] / 2), new Vector3(points[13], 0, points[5] / 2)));
                customConnLines[0][ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[0], 0, -points[11] + points[2]), new Vector3(points[0], 0, points[11] - points[2])));

                //Create the side lines
                var viewside = x < 0 ? ViewMask.LEFT : ViewMask.RIGHT;
                customConnLines[0][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[1] + points[7] / 2, -points[11]), new Vector3(0, points[1] + points[7] / 2, points[11])));
                customConnLines[0][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, -points[11]), new Vector3(0, points[1] - points[7] / 2, points[11])));
                customConnLines[0][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, -points[11]), new Vector3(0, points[1] + points[7] / 2, -points[11])));
                customConnLines[0][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, points[11]), new Vector3(0, points[1] + points[7] / 2, points[11])));

                //Create front lines
                customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[1] + points[7] / 2), new Vector3(points[13], points[1] + points[7] / 2)));
                customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[1] - points[7] / 2), new Vector3(points[13], points[1] - points[7] / 2)));
                customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[1] - points[7] / 2), new Vector3(points[0], points[1] + points[7] / 2)));
                customConnLines[0][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[13], points[1] - points[7] / 2), new Vector3(points[13], points[1] + points[7] / 2)));

                //Create bottom side lines
                customConnLines[1][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[10] + points[9] / 2, -points[11]), new Vector3(0, points[10] + points[9] / 2, points[11])));
                customConnLines[1][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, -points[11]), new Vector3(0, points[10] - points[9] / 2, points[11])));
                customConnLines[1][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, -points[11]), new Vector3(0, points[10] + points[9] / 2, -points[11])));
                customConnLines[1][viewside].Add(CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, points[11]), new Vector3(0, points[10] + points[9] / 2, points[11])));

                //Create bottom front lines
                customConnLines[1][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[10] + points[9] / 2), new Vector3(points[13], points[10] + points[9] / 2)));
                customConnLines[1][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[10] - points[9] / 2), new Vector3(points[13], points[10] - points[9] / 2)));
                customConnLines[1][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[0], points[10] - points[9] / 2), new Vector3(points[0], points[10] + points[9] / 2)));
                customConnLines[1][ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[13], points[10] - points[9] / 2), new Vector3(points[13], points[10] + points[9] / 2)));

            }
        }

        if (!isBrace && component.MemberType != EMemberType.PrimaryMember)
        {
            z = 12.0;//rotationAngle == 0 ? y : x;
        }

        switch (component.ShapeType)
        {
            case EShapeType.SingleChannel:
                mesh = CreateSingleChannelMesh(component.Shape, 0, z);
                break;
            case EShapeType.DoubleChannel:
                mesh = CreateDoubleChannelMesh(component.Shape, 0, z);
                break;
            case EShapeType.WideFlange:
                //var yOffset = (float)(GetYOffset(component) - topElInches);
//                mesh = CreateWideFlangeMesh(component, component.MemberType == EMemberType.PrimaryMember ? -yOffset : yOffset, z, isInPlane, isDirectlyWelded && isPrimaryInPlane ? true : false, endSetBack, x < 0);
                if (component.MemberType == EMemberType.PrimaryMember)
                {
                    useCustomLines = true;
                    customLineOffset = 0.1f;
                    //Create some custom lines for the HSS
                    //FRONT
                    var width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.d : component.Shape.USShape.bf) / 2);
                    var innerWidth = (float)((isPrimaryInPlane == true ? component.Shape.USShape.tf : (component.Shape.USShape.bf - component.Shape.USShape.tw) / 2));
                    var extent = width + 4;
                    //Top
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0)));
                    //Outside lines
                    var outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.2f);
                    var outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.8f);

                    var lbeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
                    //Left side
                    if(lbeam.IsActive && lbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
                    {
                        //((float)plate.Length / 2) + radius + yOffset;
                        var plate = lbeam.WinConnect.ShearWebPlate;
                        var yoff = GetTopElAndYOffset(component);

                        customLines[ViewMask.FRONT].Add(CustomLine.CreateIntersectLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
                    }
                    else
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0)));

                    var rbeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
                    //Left side
                    if (rbeam.IsActive && rbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
                    {
                        //((float)plate.Length / 2) + radius + yOffset;
                        var plate = rbeam.WinConnect.ShearWebPlate;
                        var yoff = GetTopElAndYOffset(component);

                        customLines[ViewMask.FRONT].Add(CustomLine.CreateIntersectLine(new Vector3(-width, outerLeftHeight.y, 0), new Vector3(-width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(-width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(-width, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
                    }
                    else
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(-width, outerRightHeight.y, 0), new Vector3(-width, -outerLeftHeight.y - 2 * yOffset, 0)));

                    //Inner lines
                    if (isPrimaryInPlane)
                    {
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0)));
                    }
                    else
                    {
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateDottedLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
                    }

                    //Bottom Bezier
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(extent, -1 - (float)z - yOffset, 0), new Vector3(width, 1 - (float)z - yOffset, 0), new Vector3(-width, -1 - (float)z - yOffset, 0), new Vector3(-extent, 1 - (float)z - yOffset, 0)));

                    width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.bf : component.Shape.USShape.d) / 2);
                    innerWidth = (float)((isPrimaryInPlane == true ? ((component.Shape.USShape.bf - component.Shape.USShape.tw) / 2) : component.Shape.USShape.tf));
                    extent = width + 4;
                    //Top
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent)));
                    //Outside lines
                    outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.2f);
                    outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.8f);

                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width)));
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width)));

                    //Inner lines
                    if (isPrimaryInPlane)
                    {
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth), 1.0f, 1.0f));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth), 1.0f, 1.0f));
                    }
                    else
                    {
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth)));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth)));
                    }

                    //Bottom Bezier
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z - yOffset, extent), new Vector3(0, 1 - (float)z - yOffset, width), new Vector3(0, -1 - (float)z - yOffset, -width), new Vector3(0, 1 - (float)z - yOffset, -extent)));

                    //Top
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent)));
                    //Outside lines
                    outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.2f);
                    outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.8f);

                    customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width)));
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width)));

                    //Inner lines
                    if (isPrimaryInPlane)
                    {
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth), 1.0f, 1.0f));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth), 1.0f, 1.0f));
                    }
                    else
                    {
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth)));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth)));
                    }

                    //Bottom Bezier
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z - yOffset, extent), new Vector3(0, 1 - (float)z - yOffset, width), new Vector3(0, -1 - (float)z - yOffset, -width), new Vector3(0, 1 - (float)z - yOffset, -extent)));

                    //More lines
                    width = (float)(component.Shape.USShape.d / 2);
                    var length = (float)(component.Shape.USShape.bf / 2);
                    var flangeThick = (float)(component.Shape.USShape.tf);
                    var webThick = (float)(component.Shape.USShape.tw / 2);
                    var angles = new Vector3(0, isPrimaryInPlane ? 0 : 90, 0);

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, webThick), new Vector3(width - flangeThick, 0, webThick), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -webThick), new Vector3(width - flangeThick, 0, -webThick), angles));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, webThick), new Vector3(-width + flangeThick, 0, length), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, webThick), new Vector3(width - flangeThick, 0, length), angles));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, length), new Vector3(-width, 0, length), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, length), new Vector3(width, 0, length), angles));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width, 0, length), new Vector3(-width, 0, -length), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width, 0, length), new Vector3(width, 0, -length), angles));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -webThick), new Vector3(-width + flangeThick, 0, -length), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, -webThick), new Vector3(width - flangeThick, 0, -length), angles));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -length), new Vector3(-width, 0, -length), angles));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, -length), new Vector3(width, 0, -length), angles));

                    //Delete the HSS lines
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawLabelsAndDimensionsWithTags("HSS Column");
                    MessageQueueTest.instance.GetView(CameraViewportType.LEFT).DestroyDrawLabelsAndDimensionsWithTags("HSS Column");
                    MessageQueueTest.instance.GetView(CameraViewportType.RIGHT).DestroyDrawLabelsAndDimensionsWithTags("HSS Column");
                }
                else if (component.MemberType == EMemberType.LeftBeam)
                {
                    //Left beam custom lines
                    useCustomLines = true;
                    customLineOffset = 0.01f;

                    //     bf
                    //  |-------| Tf     -
                    //     | |            |
                    //     | |            |
                    //     |Tw|           D
                    //     | |            |
                    //     | |            |
                    //  |-------|        -

                    //Extender config
                    var shearPlate = component.WinConnect.ShearWebPlate;
                    var extend = false;

                    if (shearPlate != null && shearPlate.ExtendedConfiguration == true && !isPrimaryInPlane)
                    {
                        extend = true;
                    }

                    var points = new float[9];
                    points[0] = (float)(GetPrimaryWidth());// + (extend ? primaryComponent.Shape.USShape.bf / 2 : 0));

                    if (extend)
                    {
                        points[0] = (float)(primaryComponent.Shape.USShape.bf / 2);
                    }

                    points[1] = points[0] + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;
                    points[2] = (float)component.Shape.USShape.d / 2;
                    points[3] = Mathf.Abs((float)z * 2); //The Z is multiplied by 2 because the Create Flange method doesn't divide by 2
                    points[4] = points[1] + points[3];
                    points[5] = (float)component.Shape.USShape.tf - points[2];
                    points[6] = (float)component.Shape.USShape.tw / 2;
                    points[7] = points[2] - (float)component.Shape.USShape.tf;
                    points[8] = (float)component.Shape.USShape.bf / 2;
                    var extent = 4.0f;
                    var bevel = component.MomentConnection == EMomentCarriedBy.DirectlyWelded && isPrimaryInPlane ? (float)component.Shape.USShape.tf * Mathf.Tan(30 * Mathf.Deg2Rad) : 0;

                    if (component.WebOrientation == EWebOrientation.InPlane)
                    {
                        if (component.MomentConnection != EMomentCarriedBy.DirectlyWelded || primaryComponent.WebOrientation == EWebOrientation.OutOfPlane)
                        {

                            CreateShapeDottedLine(customLines, ViewMask.FRONT, component, component.ShearConnection, points[1], points[2], yOffset);

                            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawLabelsWithTags("Column lbeam weld");
                        }
                        else
                        {
                            //Add weld
                            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Column lbeam weld", "",
                                new Vector3(points[1], points[2] + yOffset), new Vector3(10 * Math.Sign(-x), 5), 6, "Top &\nBot. Flanges", -105, 105);

                            //Draw the weld holes
                            var flatLength = 0.6875f - (float)component.EndSetback;
                            var R = 0.28f;
                            var iWidth = (float)(component.Shape.USShape.tw / 2);
                            var hheight = Mathf.Clamp(Mathf.Max(1.0f * (iWidth * 2), 0.75f), 0, 2.0f);
                            var hlength = Math.Max(1.5f * (iWidth * 2), 1.5f);
                            var fanCount = 8;
                            var iHeight = (float)(component.Shape.USShape.d / 2 - component.Shape.USShape.tf);
                            var pivotY = iHeight - hheight / 2;
                            var flatHeight = iHeight - hheight / 2 + R * Mathf.Sin(90 * Mathf.Deg2Rad);
                            flatHeight = iHeight - flatHeight;

                            if (flatHeight < 0)
                                flatHeight = 0;

                            if (flatLength < 0)
                                flatLength = 0;

                            for (int i = 0; i < fanCount * 2; ++i)
                            {
                                var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                                var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                                customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(angle) + hlength - R, pivotY + R * Mathf.Sin(angle) + yOffset),
                                    new Vector3(points[1] + R * Mathf.Cos(angle2) + hlength - R, pivotY + R * Mathf.Sin(angle2) + yOffset)));
                            }

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + hlength - R, pivotY + R + yOffset),
                                    new Vector3(points[1] + flatLength, pivotY + R + yOffset)));

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + flatLength, points[7] + yOffset),
                                    new Vector3(points[1] + flatLength, pivotY + R + yOffset)));

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(-90 * Mathf.Deg2Rad) + hlength - R, pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1], points[7] - hheight + yOffset)));

                            //Vertical dotted line
                            CreateShapeDottedLine(customLines, ViewMask.FRONT, component, component.ShearConnection, points[1], points[7] - hheight, yOffset);

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(90 * Mathf.Deg2Rad) + hlength - R, -pivotY + R * Mathf.Sin(90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1], -points[7] + hheight + yOffset)));

                            for (int i = 0; i < fanCount * 2; ++i)
                            {
                                var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                                var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                                customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(angle) + hlength - R, -pivotY + R * Mathf.Sin(angle) + yOffset),
                                    new Vector3(points[1] + R * Mathf.Cos(angle2) + hlength - R, -pivotY + R * Mathf.Sin(angle2) + yOffset)));
                            }

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(-90 * Mathf.Deg2Rad) + hlength - R, -pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1] + bevel, -points[7] + yOffset)));
                        }

                        //Top and bottom lines
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + bevel, points[2] + yOffset, 0), new Vector3(points[4], points[2] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[2] + yOffset, 0), new Vector3(points[4], -points[2] + yOffset)));

                        //Bottom tf/bevel lines
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + bevel, points[2] + yOffset, 0), new Vector3(points[1], points[7] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[2] + yOffset, 0), new Vector3(points[1] + bevel, -points[7] + yOffset)));

                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] + bevel, points[5] + yOffset, 0), new Vector3(points[4], points[5] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[5] + yOffset, 0), new Vector3(points[4], -points[5] + yOffset)));

                        //Add the bezier curve on the end
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(points[4] - 1, points[2] + extent + yOffset, 0), new Vector3(points[4] + 1, points[2] + 1 + yOffset),
                            new Vector3(points[4] - 1, -points[2] - 1 + yOffset, 0), new Vector3(points[4] + 1, -points[2] - extent + yOffset, 0)));

                        //Right side view
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[6])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[6])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, points[7] + yOffset, -points[8])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, points[7] + yOffset, points[8])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[8])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[8])));

                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, -points[8])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[8]), new Vector3(0, points[2] + yOffset, points[8])));

                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[2] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, points[8])));

                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, -points[8])));
                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

                        customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[2] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

                        //Top lines
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[1], 0, points[8]), new Vector3(points[1], 0, -points[8])));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[4], 0, points[8]), new Vector3(points[1], 0, points[8])));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[4], 0, -points[8]), new Vector3(points[1], 0, -points[8])));

                        //Dotted lines
                        customLines[ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[6]), new Vector3(points[4], 0, points[6]), 1, 1));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[1], 0, -points[6]), new Vector3(points[4], 0, -points[6]), 1, 1));

                        customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(points[4] - 1, 0, points[8] + extent), new Vector3(points[4] + 1, 0, points[8] + 1),
                            new Vector3(points[4] - 1, 0, -points[8] - 1), new Vector3(points[4] + 1, 0, -points[8] - extent)));
                    }
                }
                else if (component.MemberType == EMemberType.RightBeam)
                {
                    //Right beam custom lines
                    useCustomLines = true;
                    customLineOffset = 0.01f;
                    //     bf
                    //  |-------| Tf     -
                    //     | |            |
                    //     | |            |
                    //     |Tw|           D
                    //     | |            |
                    //     | |            |
                    //  |-------|        -

                    //Extender config
                    var shearPlate = component.WinConnect.ShearWebPlate;
                    var extend = false;

                    if (shearPlate != null && shearPlate.ExtendedConfiguration == true && !isPrimaryInPlane)
                    {
                        extend = true;
                    }

                    var points = new float[9];
                    points[0] = (float)(GetPrimaryWidth());// + (extend ? primaryComponent.Shape.USShape.bf / 2 : 0));

                    if (extend)
                    {
                        points[0] = (float)(primaryComponent.Shape.USShape.bf / 2);
                    }

                    points[1] = -points[0] - (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;
                    points[2] = (float)component.Shape.USShape.d / 2;
                    points[3] = Mathf.Abs((float)z * 2); //The Z is multiplied by 2 because the Create Flange method doesn't divide by 2
                    points[4] = points[1] - points[3];
                    points[5] = (float)component.Shape.USShape.tf - points[2];
                    points[6] = (float)component.Shape.USShape.tw / 2;
                    points[7] = points[2] - (float)component.Shape.USShape.tf;
                    points[8] = (float)component.Shape.USShape.bf / 2;
                    var extent = 4.0f;
                    var bevel = component.MomentConnection == EMomentCarriedBy.DirectlyWelded && isPrimaryInPlane ? (float)component.Shape.USShape.tf * Mathf.Tan(30 * Mathf.Deg2Rad) : 0;

                    if (component.WebOrientation == EWebOrientation.InPlane)
                    {
                        if (component.MomentConnection != EMomentCarriedBy.DirectlyWelded || primaryComponent.WebOrientation == EWebOrientation.OutOfPlane)
                        {
                            CreateShapeDottedLine(customLines, ViewMask.FRONT, component, component.ShearConnection, points[1], points[2], yOffset);

                            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawLabelsWithTags("Column rbeam weld");
                        }
                        else
                        {
                            //Add weld
                            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawWeldLabel("Column rbeam weld", "",
                                new Vector3(points[1], points[2] + yOffset), new Vector3(10 * Math.Sign(-x), 5), 6, "Top &\nBot. Flanges", -105, 105);

                            //Draw the weld holes
                            var flatLength = 0.6875f - (float)component.EndSetback;
                            var R = 0.28f;
                            var iWidth = (float)(component.Shape.USShape.tw / 2);
                            var hheight = Mathf.Clamp(Mathf.Max(1.0f * (iWidth * 2), 0.75f), 0, 2.0f);
                            var hlength = Math.Max(1.5f * (iWidth * 2), 1.5f);
                            var fanCount = 8;
                            var iHeight = (float)(component.Shape.USShape.d / 2 - component.Shape.USShape.tf);
                            var pivotY = iHeight - hheight / 2;
                            var flatHeight = iHeight - hheight / 2 + R * Mathf.Sin(90 * Mathf.Deg2Rad);
                            flatHeight = iHeight - flatHeight;

                            if (flatHeight < 0)
                                flatHeight = 0;

                            if (flatLength < 0)
                                flatLength = 0;

                            for (int i = 0; i < fanCount * 2; ++i)
                            {
                                var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                                var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                                customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(angle) - hlength + R, pivotY + R * Mathf.Sin(angle) + yOffset),
                                    new Vector3(points[1] - R * Mathf.Cos(angle2) - hlength + R, pivotY + R * Mathf.Sin(angle2) + yOffset)));
                            }

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - hlength + R, pivotY + R + yOffset),
                                    new Vector3(points[1] - flatLength, pivotY + R + yOffset)));

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - flatLength, points[7] + yOffset),
                                    new Vector3(points[1] - flatLength, pivotY + R + yOffset)));

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(-90 * Mathf.Deg2Rad) - hlength + R, pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1], points[7] - hheight + yOffset)));

                            //Vertical dotted line
                            CreateShapeDottedLine(customLines, ViewMask.FRONT, component, component.ShearConnection, points[1], points[7] - hheight, yOffset);

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(90 * Mathf.Deg2Rad) - hlength + R, -pivotY + R * Mathf.Sin(90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1], -points[7] + hheight + yOffset)));

                            for (int i = 0; i < fanCount * 2; ++i)
                            {
                                var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                                var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                                customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(angle) - hlength + R, -pivotY + R * Mathf.Sin(angle) + yOffset),
                                    new Vector3(points[1] - R * Mathf.Cos(angle2) - hlength + R, -pivotY + R * Mathf.Sin(angle2) + yOffset)));
                            }

                            customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(-90 * Mathf.Deg2Rad) - hlength + R, -pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                                    new Vector3(points[1] - bevel, -points[7] + yOffset)));
                        }

                        //Top and bottom lines
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - bevel, points[2] + yOffset, 0), new Vector3(points[4], points[2] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[2] + yOffset, 0), new Vector3(points[4], -points[2] + yOffset)));

                        //Bottom tf/bevel lines
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - bevel, points[2] + yOffset, 0), new Vector3(points[1], points[7] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[2] + yOffset, 0), new Vector3(points[1] - bevel, -points[7] + yOffset)));

                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1] - bevel, points[5] + yOffset, 0), new Vector3(points[4], points[5] + yOffset)));
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(points[1], -points[5] + yOffset, 0), new Vector3(points[4], -points[5] + yOffset)));

                        //Add the bezier curve on the end
                        customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(points[4] - 1, points[2] + extent + yOffset, 0), new Vector3(points[4] + 1, points[2] + 1 + yOffset),
                            new Vector3(points[4] - 1, -points[2] - 1 + yOffset, 0), new Vector3(points[4] + 1, -points[2] - extent + yOffset, 0)));

                        //Right side view
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[6])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[6])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, points[7] + yOffset, -points[8])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, points[7] + yOffset, points[8])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[8])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[8])));

                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, -points[8])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[8]), new Vector3(0, points[2] + yOffset, points[8])));

                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, points[2] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, points[8])));

                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, -points[8])));
                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

                        customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, -points[2] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

                        //Top lines
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[1], 0, points[8]), new Vector3(points[1], 0, -points[8])));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[4], 0, points[8]), new Vector3(points[1], 0, points[8])));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(points[4], 0, -points[8]), new Vector3(points[1], 0, -points[8])));

                        //Dotted lines
                        customLines[ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[6]), new Vector3(points[4], 0, points[6]), 1, 1));
                        customLines[ViewMask.TOP].Add(CustomLine.CreateDottedLine(new Vector3(points[1], 0, -points[6]), new Vector3(points[4], 0, -points[6]), 1, 1));

                        customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(points[4] - 1, 0, points[8] + extent), new Vector3(points[4] + 1, 0, points[8] + 1),
                            new Vector3(points[4] - 1, 0, -points[8] - 1), new Vector3(points[4] + 1, 0, -points[8] - extent)));
                    }
                }

                break;
            case EShapeType.HollowSteelSection:
//                mesh = CreateHollowSteelSectionMesh(component.Shape, 0, z, isInPlane);
                useCustomLines = true;

                //Create some custom lines for the HSS
                //FRONT
                if (useCustomLines)
                {
                    var width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.Ht : component.Shape.USShape.B) / 2);
                    var innerWidth = (float)component.Shape.USShape.tnom;
                    var extent = width + 4;
                    //Top
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0)));
                    //Outside lines
                    var outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0), 0.2f);
                    var outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0), 0.8f);

                    customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, -outerRightHeight.y, 0)));
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateNormalLine(new Vector3(-width, outerRightHeight.y, 0), new Vector3(-width, -outerLeftHeight.y, 0)));

                    //Inner lines
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateDottedLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y, 0), 1f, 1f));
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y, 0), 1f, 1f));

                    //Bottom Bezier
                    customLines[ViewMask.FRONT].Add(CustomLine.CreateBezierLine(new Vector3(extent, -1 - (float)z, 0), new Vector3(width, 1 - (float)z, 0), new Vector3(-width, -1 - (float)z, 0), new Vector3(-extent, 1 - (float)z, 0)));

                    width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.B : component.Shape.USShape.Ht) / 2);
                    extent = width + 4;
                    //Top
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent)));
                    //Outside lines
                    outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.2f);
                    outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.8f);

                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y, width)));
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y, -width)));

                    //Inner lines
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y, width - innerWidth), 1f, 1f));
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y, -width + innerWidth), 1f, 1f));

                    //Bottom Bezier
                    customLines[ViewMask.RIGHT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z, extent), new Vector3(0, 1 - (float)z, width), new Vector3(0, -1 - (float)z, -width), new Vector3(0, 1 - (float)z, -extent)));

                    //Top
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent)));
                    //Outside lines
                    outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.2f);
                    outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.8f);

                    customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y, width)));
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y, -width)));

                    //Inner lines
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y, width - innerWidth), 1f, 1f));
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y, -width + innerWidth), 1f, 1f));

                    //Bottom Bezier
                    customLines[ViewMask.LEFT].Add(CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z, extent), new Vector3(0, 1 - (float)z, width), new Vector3(0, -1 - (float)z, -width), new Vector3(0, 1 - (float)z, -extent)));

                    //More lines
                    width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.Ht : component.Shape.USShape.B) / 2);
                    var length = (float)((isPrimaryInPlane == true ? component.Shape.USShape.B : component.Shape.USShape.Ht) / 2);
                    var thick = (float)(component.Shape.USShape.tnom);
                    var radius = 2.0f * thick;

                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width, 0, length - radius), new Vector3(width, 0, -length + radius)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width, 0, length - radius), new Vector3(-width, 0, -length + radius)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - radius, 0, length), new Vector3(-width + radius, 0, length)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - radius, 0, -length), new Vector3(-width + radius, 0, -length)));

                    //Curves TODO: Change the curves to use a Slerp instead
                    customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(width, 0, length - radius), new Vector3(width, 0, length), new Vector3(width, 0, length), new Vector3(width - radius, 0, length)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(-width, 0, length - radius), new Vector3(-width, 0, length), new Vector3(-width, 0, length), new Vector3(-width + radius, 0, length)));

                    customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(width, 0, -length + radius), new Vector3(width, 0, -length), new Vector3(width, 0, -length), new Vector3(width - radius, 0, -length)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateBezierLine(new Vector3(-width, 0, -length + radius), new Vector3(-width, 0, -length), new Vector3(-width, 0, -length), new Vector3(-width + radius, 0, -length)));

                    //Inside box TODO: Fix inner curve ie. it has none
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - thick, 0, length - thick), new Vector3(width - thick, 0, -length + thick)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(-width + thick, 0, length - thick), new Vector3(-width + thick, 0, -length + thick)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - thick, 0, length - thick), new Vector3(-width + thick, 0, length - thick)));
                    customLines[ViewMask.TOP].Add(CustomLine.CreateNormalLine(new Vector3(width - thick, 0, -length + thick), new Vector3(-width + thick, 0, -length + thick)));

                    //Add the HSS dimension
                    var armLength = 5;
                    MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("HSS 1", (width * 2).ToString() + "\"",
                        new Vector3(width, -(float)z, 0),
                        new Vector3(-width, -(float)z, 0),
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).viewCamera.transform.forward,
                        armLength, MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    MessageQueueTest.instance.GetView(CameraViewportType.LEFT).AddDrawDimension("HSS 2", (length * 2).ToString() + "\"",
                        new Vector3(0, -(float)z, -length),
                        new Vector3(0, -(float)z, length),
                        MessageQueueTest.instance.GetView(CameraViewportType.LEFT).viewCamera.transform.forward,
                        armLength, MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);

                    MessageQueueTest.instance.GetView(CameraViewportType.RIGHT).AddDrawDimension("HSS 3", (length * 2).ToString() + "\"",
                        new Vector3(0, -(float)z, length),
                        new Vector3(0, -(float)z, -length),
                        MessageQueueTest.instance.GetView(CameraViewportType.RIGHT).viewCamera.transform.forward,
                        armLength, MessageQueueTest.GetDimensionColor(), 0, 0, EOffsetType.Bottom);
                }

                break;
            default:
//                mesh = CreateWideFlangeMesh(component, 0, isBrace ? y : z);
                break;

            //TODO: Implement Brace placement for every shape type of mesh
        }

        if (!rotationAngle.Equals(0) && !isBrace)
        {
            var len = z;

            if (len < 0.0f)
                len *= -1;

            if (component.ShearConnection == EShearCarriedBy.SinglePlate && component.WinConnect.ShearWebPlate.ExtendedConfiguration == true)
            {
                var width = isPrimaryInPlane ? primaryComponent.Shape.USShape.d / 2 : primaryComponent.Shape.USShape.bf / 2;

                translation = new Vector3(0, (float)(width + (component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER) + len) * Math.Sign(x), 0);
            }
            else
            {
                if (primaryComponent.ShapeType != EShapeType.HollowSteelSection)
                {
                    translation = new Vector3(topElInches, (float)(isPrimaryInPlane == true ? primaryComponent.Shape.USShape.d : primaryComponent.Shape.USShape.tw) / 2 * Math.Sign(x) + (float)len * Math.Sign(x) +
                        (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER * Math.Sign(x), 0);
                }
                else
                {
                    //Use the HSS properties
                    var shape = primaryComponent.Shape.USShape;
                    var horizOffset = (float)(isPrimaryInPlane == true ? shape.Ht : shape.B) / 2;

                    translation = new Vector3(topElInches, horizOffset * Math.Sign(x) + (float)len * Math.Sign(x) +
                        (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER * Math.Sign(x), 0);
                }
            }

            //UnityEngine.Debug.Log(component.ShapeName + " " + translation + " " + x.ToString() + " " + y.ToString());

            MeshCreator.Translate(translation);
            MeshCreator.Rotate(0, 0, (float)rotationAngle);
            MeshCreator.Create(mesh);
        }
        else if (isBrace && gusset != null)
        {
            //TODO: Will's values, remove when calculations are corrected
            gusset.ColumnSide = 17f * (float)ConstNum.METRIC_MULTIPLIER;
            gusset.ColumnSideFreeEdgeX = 14.47f * (float)ConstNum.METRIC_MULTIPLIER;
            gusset.ColumnSideFreeEdgeY = 1.3275f * (float)ConstNum.METRIC_MULTIPLIER;
            gusset.BeamSide = 26.75 * (float)ConstNum.METRIC_MULTIPLIER;
            gusset.BeamSideFreeEdgeX = 6.2725f * (float)ConstNum.METRIC_MULTIPLIER;
            gusset.BeamSideFreeEdgeY = 9.6621f * (float)ConstNum.METRIC_MULTIPLIER;
            var urX = brace0.Shape.USShape.d / 2 + gusset.ColumnSideFreeEdgeX + component.BraceX + component.AngleX; //UR
            var lrX = brace0.Shape.USShape.d / 2 + gusset.ColumnSideFreeEdgeX + component.BraceX + component.AngleX; //LR
            var urY = brace1.Shape.USShape.d / 2 + gusset.BeamSideFreeEdgeY + component.BraceY + component.AngleY; //UR
            var lrY = -brace1.Shape.USShape.d / 2 - gusset.BeamSideFreeEdgeY + component.BraceY + component.AngleY; //LR
            var ulX = -brace0.Shape.USShape.d / 2 - gusset.ColumnSideFreeEdgeX + component.BraceX + component.AngleX; //UL
            var lLX = -brace0.Shape.USShape.d / 2 - gusset.ColumnSideFreeEdgeX + component.BraceX + component.AngleX; //lL
            var ulY = brace2.Shape.USShape.d / 2 + gusset.BeamSideFreeEdgeY + component.BraceY + component.AngleY; //UL
            var lLY = -brace2.Shape.USShape.d / 2 - gusset.BeamSideFreeEdgeY + component.BraceY + component.AngleY; //lL
            var braceRotAngle = component.Angle;

            if (uR) translation = new Vector3(-(float)urX, (float)urY, 0);
            if (uL) translation = new Vector3(-(float)ulX, (float)ulY, 0);
            if (lR) translation = new Vector3(-(float)lrX, (float)lrY, 0);
            if (lL) translation = new Vector3(-(float)lLX, (float)lLY, 0);
            MeshCreator.Rotate(0, 0, (float)braceRotAngle);
            MeshCreator.Translate(translation);
            MeshCreator.Create(mesh);

            //Add gusset plate mesh
            _gussetCreatorCallback = CreateGussetMesh(component);
        }

        //Create the label object
        if (component.MemberType == EMemberType.PrimaryMember)
        {
            var column = component.Shape;
            MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("Column main column label", component.ShapeName + " - " + component.Material.Name, new Vector3((float)-(isPrimaryInPlane ? component.Shape.USShape.d / 2 : component.Shape.USShape.bf / 2), (float)y, 0), new Vector3(-20, 8, 0));
        }

        if (primaryComponent != null)
        {
            if (component.MemberType == EMemberType.LeftBeam)
            {
                var beam = component.Shape;
                var labelOffset = new Vector3((float)GetPrimaryWidth() + (float)z * 2, -(float)beam.d / 2 * METRIC + yOffset, 0);

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("LColumn lbeam label", component.ShapeName + " - " + component.Material.Name + "\nEnd Offset = " + component.EndSetback.ToString() + " in", labelOffset, new Vector3(20, -8, 0));
            }

            if (component.MemberType == EMemberType.RightBeam)
            {
                var beam = component.Shape;
                var labelOffset = new Vector3(-(float)GetPrimaryWidth() - (float)z * 2, -(float)beam.d / 2 * METRIC + yOffset, 0);

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawLabel("RColumn rbeam label", component.ShapeName + " - " + component.Material.Name + "\nEnd Offset = " + component.EndSetback.ToString() + " in", labelOffset, new Vector3(-20, -8, 0));
            }
        }


        //Draw the top beam el values
        if (component.MemberType != EMemberType.PrimaryMember)
        {
            if(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam].IsActive && CommonDataStatic.DetailDataDict[EMemberType.RightBeam].IsActive)
            {
                if(component.MemberType == EMemberType.LeftBeam)
                {
                    var lBeamEl = DrawingMethods.GetTopEl(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam]);
                    var rBeamEl = DrawingMethods.GetTopEl(CommonDataStatic.DetailDataDict[EMemberType.RightBeam]);
                    var lBeamYOffset = (float)DrawingMethods.GetYOffset(CommonDataStatic.DetailDataDict[EMemberType.LeftBeam]);
                    var rBeamYOffset = (float)DrawingMethods.GetYOffset(CommonDataStatic.DetailDataDict[EMemberType.RightBeam]);
                    var lBeamHeight = (float)component.Shape.USShape.d / 2;
                    var rBeamHeight = (float)CommonDataStatic.DetailDataDict[EMemberType.RightBeam].Shape.USShape.d / 2;

                    //Extender config
                    var shearPlate = component.WinConnect.ShearWebPlate;
                    var extend = false;

                    if (shearPlate != null && shearPlate.ExtendedConfiguration == true && !isPrimaryInPlane)
                    {
                        extend = true;
                    }

                    var horizPoint = (float)(GetPrimaryWidth());// + (extend ? primaryComponent.Shape.USShape.bf / 2 : 0));

                    if (extend)
                    {
                        horizPoint = (float)(primaryComponent.Shape.USShape.bf / 2);
                    }

                    var totalLength = 0.0f;

                    if (lBeamEl < rBeamEl)
                    {
                        totalLength = rBeamEl - lBeamEl;
                    }
                    else
                    {
                        totalLength = lBeamEl - rBeamEl;
                    }

                    if (lBeamEl < rBeamEl)
                    {
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Column elev 1", GetFootString(totalLength), new Vector3(horizPoint, rBeamHeight + rBeamEl + rBeamYOffset),
                            new Vector3(horizPoint, lBeamHeight + lBeamYOffset + lBeamEl), 10, MessageQueueTest.GetDimensionColor(), horizPoint * 2 + endSetBack, 0);
                    }
                    else if (lBeamEl > rBeamEl)
                    {
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).AddDrawDimension("Column elev 1", GetFootString(totalLength), new Vector3(-horizPoint, lBeamHeight + lBeamYOffset + lBeamEl),
                            new Vector3(-horizPoint, rBeamHeight + rBeamEl + rBeamYOffset), -10, MessageQueueTest.GetDimensionColor(), horizPoint * 2 + endSetBack, 0);
                    }

                    if(totalLength == 0.0f)
                    {
                        //Delete those 
                        MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawDimensionsWithTags("Column elev");
                    }
                }
            }
        }
    }
}