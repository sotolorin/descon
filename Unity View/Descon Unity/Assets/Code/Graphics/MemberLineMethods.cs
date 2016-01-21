using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public class MemberLineMethods : System.Object
{
    #region BeamToColumn
    public static void CreatePrimaryBeamLines(LineDrawing drawing, DetailData component, double z = 0.0)
    {
        drawing.ClearAllLines();

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = 0.0f;

        //Create some custom lines for the WideFlange
        //FRONT
        var width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.d : component.Shape.USShape.bf) / 2);
        var innerWidth = (float)((isPrimaryInPlane == true ? component.Shape.USShape.tf : (component.Shape.USShape.bf - component.Shape.USShape.tw) / 2));
        var extent = width + 4;
        //Top
        if (!CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0)));

        //Outside lines
        var outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.2f);
        var outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.8f);

        var topLeftPoint = outerLeftHeight.y;
        var topRightPoint = outerRightHeight.y;

        if(CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
        {
            topLeftPoint = MessageQueueTest.instance.topCutPoint;
            topRightPoint = MessageQueueTest.instance.topCutPoint;

            //Top line
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width, topLeftPoint, 0), new Vector3(width, topRightPoint, 0)));
        }

        var rbeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
        //Right side
        if (rbeam.IsActive && rbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
        {
            var plate = rbeam.WinConnect.ShearWebPlate;
            var yoff = DrawingMethods.GetTopElAndYOffset(component);

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateIntersectLine(new Vector3(width, topLeftPoint, 0), new Vector3(width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0), 0.5f, 0.5f));
        }
        else if(rbeam.IsActive && (rbeam.ShearConnection == EShearCarriedBy.SinglePlate && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane))
        {
            var plate = rbeam.WinConnect.ShearWebPlate;
            var yoff = DrawingMethods.GetTopElAndYOffset(component);
            var plate2Height = (float)plate.Length;
            var offset2 = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.d, plate2Height, plate.Bolt.EdgeDistLongDir, plate.Position);

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateIntersectLine(new Vector3(width, topLeftPoint, 0), new Vector3(width, yoff + (float)plate.Length / 2 - offset2), new Vector3(width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0), 0.5f, 0.5f));
        }
        else
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width, topLeftPoint, 0), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0)));

        var lbeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
        //Left side
        if (lbeam.IsActive && lbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
        {
            var plate = lbeam.WinConnect.ShearWebPlate;
            var yoff = DrawingMethods.GetTopElAndYOffset(component);

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateIntersectLine(new Vector3(-width, topRightPoint, 0), new Vector3(-width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(-width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(-width, -outerRightHeight.y - 2 * yOffset, 0), 0.5f, 0.5f));
        }
        else if (lbeam.IsActive && (lbeam.ShearConnection == EShearCarriedBy.SinglePlate && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane))
        {
            var plate = rbeam.WinConnect.ShearWebPlate;
            var yoff = DrawingMethods.GetTopElAndYOffset(component);
            var plate2Height = (float)plate.Length;
            var offset2 = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.d, plate2Height, plate.Bolt.EdgeDistLongDir, plate.Position);

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateIntersectLine(new Vector3(-width, topRightPoint, 0), new Vector3(-width, yoff + (float)plate.Length / 2 - offset2), new Vector3(-width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(-width, -outerRightHeight.y - 2 * yOffset, 0), 0.5f, 0.5f));
        }
        else
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width, topRightPoint, 0), new Vector3(-width, -outerLeftHeight.y - 2 * yOffset, 0)));

        //Inner lines
        if (isPrimaryInPlane)
        {
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width - innerWidth, topLeftPoint, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + innerWidth, topRightPoint, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0)));
        }
        else
        {
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateDottedLine(new Vector3(width - innerWidth, topLeftPoint, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, topRightPoint, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
        }

        //Bottom Bezier
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(extent, -1 - (float)z - yOffset, 0), new Vector3(width, 1 - (float)z - yOffset, 0), new Vector3(-width, -1 - (float)z - yOffset, 0), new Vector3(-extent, 1 - (float)z - yOffset, 0)));

        width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.bf : component.Shape.USShape.d) / 2);
        innerWidth = (float)((isPrimaryInPlane == true ? ((component.Shape.USShape.bf - component.Shape.USShape.tw) / 2) : component.Shape.USShape.tf));
        extent = width + 4;

        //Top
        if (!CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent)));
        else
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, -width), new Vector3(0, topRightPoint, width)));

        //Outside lines
        outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.2f);
        outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.8f);

        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, width), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width)));
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, topRightPoint, -width), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width)));

        //Inner lines
        if (isPrimaryInPlane)
        {
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(0, topLeftPoint, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth), 1.0f, 1.0f));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(0, topRightPoint, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth), 1.0f, 1.0f));
        }
        else
        {
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth)));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, topRightPoint, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth)));
        }

        //Bottom Bezier
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z - yOffset, extent), new Vector3(0, 1 - (float)z - yOffset, width), new Vector3(0, -1 - (float)z - yOffset, -width), new Vector3(0, 1 - (float)z - yOffset, -extent)));

        //Top
        if (!CommonDataStatic.ColumnStiffener.BeamNearTopOfColumn)
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent)));
        else
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, -width), new Vector3(0, topRightPoint, width)));

        //Outside lines
        outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.2f);
        outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z - yOffset, extent), new Vector3(0, 1 + (float)z - yOffset, width), new Vector3(0, -1 + (float)z - yOffset, -width), new Vector3(0, 1 + (float)z - yOffset, -extent), 0.8f);

        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, width), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width)));
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, topRightPoint, -width), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width)));

        //Inner lines
        if (isPrimaryInPlane)
        {
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateDottedLine(new Vector3(0, topLeftPoint, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth), 1.0f, 1.0f));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateDottedLine(new Vector3(0, topRightPoint, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth), 1.0f, 1.0f));
        }
        else
        {
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, topLeftPoint, width - innerWidth), new Vector3(0, -outerRightHeight.y - 2 * yOffset, width - innerWidth)));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, topRightPoint, -width + innerWidth), new Vector3(0, -outerLeftHeight.y - 2 * yOffset, -width + innerWidth)));
        }

        //Bottom Bezier
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z - yOffset, extent), new Vector3(0, 1 - (float)z - yOffset, width), new Vector3(0, -1 - (float)z - yOffset, -width), new Vector3(0, 1 - (float)z - yOffset, -extent)));

        //More lines
        width = (float)(component.Shape.USShape.d / 2);
        var length = (float)(component.Shape.USShape.bf / 2);
        var flangeThick = (float)(component.Shape.USShape.tf);
        var webThick = (float)(component.Shape.USShape.tw / 2);
        var angles = new Vector3(0, isPrimaryInPlane ? 0 : 90, 0);

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, webThick), new Vector3(width - flangeThick, 0, webThick), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -webThick), new Vector3(width - flangeThick, 0, -webThick), angles));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, webThick), new Vector3(-width + flangeThick, 0, length), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, webThick), new Vector3(width - flangeThick, 0, length), angles));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, length), new Vector3(-width, 0, length), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, length), new Vector3(width, 0, length), angles));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width, 0, length), new Vector3(-width, 0, -length), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width, 0, length), new Vector3(width, 0, -length), angles));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -webThick), new Vector3(-width + flangeThick, 0, -length), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, -webThick), new Vector3(width - flangeThick, 0, -length), angles));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, 0, -length), new Vector3(-width, 0, -length), angles));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, 0, -length), new Vector3(width, 0, -length), angles));
    }

    public static void CreatePrimaryHSSLines(LineDrawing drawing, DetailData component, double z = 0.0)
    {
        drawing.ClearAllLines();

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = 0.0f;

        //Create some custom lines for the HSS
        var width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.Ht : component.Shape.USShape.B) / 2);
        var innerWidth = (float)component.Shape.USShape.tnom;
        var extent = width + 4;
        //Top
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0)));
        //Outside lines
        var outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0), 0.2f);
        var outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z, 0), new Vector3(width, 1 + (float)z, 0), new Vector3(-width, -1 + (float)z, 0), new Vector3(-extent, 1 + (float)z, 0), 0.8f);

        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, -outerRightHeight.y, 0)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width, outerRightHeight.y, 0), new Vector3(-width, -outerLeftHeight.y, 0)));

        //Inner lines
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateDottedLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y, 0), 1f, 1f));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y, 0), 1f, 1f));

        //Bottom Bezier
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(extent, -1 - (float)z, 0), new Vector3(width, 1 - (float)z, 0), new Vector3(-width, -1 - (float)z, 0), new Vector3(-extent, 1 - (float)z, 0)));

        width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.B : component.Shape.USShape.Ht) / 2);
        extent = width + 4;
        //Top
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent)));
        //Outside lines
        outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.2f);
        outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.8f);

        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y, width)));
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y, -width)));

        //Inner lines
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y, width - innerWidth), 1f, 1f));
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y, -width + innerWidth), 1f, 1f));

        //Bottom Bezier
        drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z, extent), new Vector3(0, 1 - (float)z, width), new Vector3(0, -1 - (float)z, -width), new Vector3(0, 1 - (float)z, -extent)));

        //Top
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateBezierLine(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent)));
        //Outside lines
        outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.2f);
        outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(0, -1 + (float)z, extent), new Vector3(0, 1 + (float)z, width), new Vector3(0, -1 + (float)z, -width), new Vector3(0, 1 + (float)z, -extent), 0.8f);

        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, outerLeftHeight.y, width), new Vector3(0, -outerRightHeight.y, width)));
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, outerRightHeight.y, -width), new Vector3(0, -outerLeftHeight.y, -width)));

        //Inner lines
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateDottedLine(new Vector3(0, outerLeftHeight.y, width - innerWidth), new Vector3(0, -outerRightHeight.y, width - innerWidth), 1f, 1f));
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateDottedLine(new Vector3(0, outerRightHeight.y, -width + innerWidth), new Vector3(0, -outerLeftHeight.y, -width + innerWidth), 1f, 1f));

        //Bottom Bezier
        drawing.AddLine(ViewMask.LEFT, CustomLine.CreateBezierLine(new Vector3(0, -1 - (float)z, extent), new Vector3(0, 1 - (float)z, width), new Vector3(0, -1 - (float)z, -width), new Vector3(0, 1 - (float)z, -extent)));

        //More lines
        width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.Ht : component.Shape.USShape.B) / 2);
        var length = (float)((isPrimaryInPlane == true ? component.Shape.USShape.B : component.Shape.USShape.Ht) / 2);
        var thick = (float)(component.Shape.USShape.tnom);
        var radius = 2.0f * thick;

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width, 0, length - radius), new Vector3(width, 0, -length + radius)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width, 0, length - radius), new Vector3(-width, 0, -length + radius)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - radius, 0, length), new Vector3(-width + radius, 0, length)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - radius, 0, -length), new Vector3(-width + radius, 0, -length)));

        //Curves TODO: Change the curves to use a Slerp instead
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(width, 0, length - radius), new Vector3(width, 0, length), new Vector3(width, 0, length), new Vector3(width - radius, 0, length)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(-width, 0, length - radius), new Vector3(-width, 0, length), new Vector3(-width, 0, length), new Vector3(-width + radius, 0, length)));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(width, 0, -length + radius), new Vector3(width, 0, -length), new Vector3(width, 0, -length), new Vector3(width - radius, 0, -length)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(-width, 0, -length + radius), new Vector3(-width, 0, -length), new Vector3(-width, 0, -length), new Vector3(-width + radius, 0, -length)));

        //Inside box TODO: Fix inner curve ie. it has none
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - thick, 0, length - thick), new Vector3(width - thick, 0, -length + thick)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + thick, 0, length - thick), new Vector3(-width + thick, 0, -length + thick)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - thick, 0, length - thick), new Vector3(-width + thick, 0, length - thick)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - thick, 0, -length + thick), new Vector3(-width + thick, 0, -length + thick)));
    }

    public static void CreateTopClipAngleLines(LineDrawing drawing, DetailData component, double x = 0.0, bool front = true)
    {
        drawing.lineOffset = 0.01f;

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component) + DrawingMethods.GirderOffset(component);
        var METRIC = (float)ConstNum.METRIC_MULTIPLIER;

        var wclip = component.WinConnect.ShearClipAngle;

        var isLongLegOSL = wclip.OSL == EOSL.LongLeg;
        var isRight = Math.Sign(x) > 0;
        var viewSide = isRight ? CameraViewportType.RIGHT : CameraViewportType.LEFT;
        var oslLeg = isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC;
        var beamLeg = !isLongLegOSL ? wclip.LongLeg * METRIC : wclip.ShortLeg * METRIC;

        var points = new float[7];

        points[0] = (float)DrawingMethods.GetPrimaryWidth() * Math.Sign(x);
        points[1] = (float)oslLeg;
        points[2] = (float)beamLeg;
        points[3] = (float)component.Shape.USShape.tw / 2;
        points[4] = (float)wclip.Thickness;

        //Clip dimensions
        points[5] = points[0] + points[2] * Math.Sign(x);
        points[6] = points[0] + points[4] * Math.Sign(x);


        if (front)
        {
            //Draw the back clip angle
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[3]), new Vector3(points[0], 0, points[3] + points[1]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[3]), new Vector3(points[5], 0, points[3]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[5], 0, points[3]), new Vector3(points[5], 0, points[3] + points[4]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[3] + points[4]), new Vector3(points[5], 0, points[3] + points[4]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[3] + points[4]), new Vector3(points[6], 0, points[1]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[1]), new Vector3(points[6], 0, points[1]), 0.25f, 0.25f));
        }
        else
        {
            points[3] *= -1;
            points[1] *= -1;

            //Draw the front clip angle
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[3]), new Vector3(points[0], 0, points[3] + points[1]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[3]), new Vector3(points[5], 0, points[3]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[5], 0, points[3]), new Vector3(points[5], 0, points[3] - points[4]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[3] - points[4]), new Vector3(points[5], 0, points[3] - points[4]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[3] - points[4]), new Vector3(points[6], 0, points[1]), 0.25f, 0.25f));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[1]), new Vector3(points[6], 0, points[1]), 0.25f, 0.25f));
        }
    }

    public static void CreateRightBeamLines(LineDrawing drawing, DetailData component, double z = 0.0, bool useCope = false)
    {
        drawing.ClearAllLines();
        drawing.lineOffset = 0.01f;

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component);

        if (CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
        {
            yOffset = DrawingMethods.GirderOffset(component) + DrawingMethods.GetTopEl(component);
        }

        //Right beam custom lines
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
        points[0] = (float)(DrawingMethods.GetPrimaryWidth()) + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;// + (extend ? primaryComponent.Shape.USShape.bf / 2 : 0));

        if (extend)
        {
            points[0] = (float)(primaryComponent.Shape.USShape.bf / 2) + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;
        }

        var isDirectlyWelded = component.MomentConnection == EMomentCarriedBy.DirectlyWelded && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane;
        var directlyWeldedOffset = (float)((primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2 + component.WinConnect.MomentDirectWeld.Top.a);

        if (isDirectlyWelded)
            points[0] = (float)(DrawingMethods.GetPrimaryWidth() + (isDirectlyWelded ? directlyWeldedOffset : component.EndSetback)) * Math.Sign(1);

        points[1] = points[0];
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
                DrawingMethods.CreateShapeDottedLineGirder(drawing.GetAllLines(), ViewMask.FRONT, component, component.ShearConnection, points[1], points[2], yOffset);

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawLabelsWithTags("Column rbeam weld");
            }
            else
            {
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

                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(angle) + hlength - R, pivotY + R * Mathf.Sin(angle) + yOffset),
                        new Vector3(points[1] + R * Mathf.Cos(angle2) + hlength - R, pivotY + R * Mathf.Sin(angle2) + yOffset)));
                }

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + hlength - R, pivotY + R + yOffset),
                        new Vector3(points[1] + flatLength, pivotY + R + yOffset)));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + flatLength, points[7] + yOffset),
                        new Vector3(points[1] + flatLength, pivotY + R + yOffset)));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(-90 * Mathf.Deg2Rad) + hlength - R, pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1], points[7] - hheight + yOffset)));

                //Vertical dotted line
                DrawingMethods.CreateShapeDottedLine(drawing.GetAllLines(), ViewMask.FRONT, component, component.ShearConnection, points[1], points[7] - hheight, yOffset);

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(90 * Mathf.Deg2Rad) + hlength - R, -pivotY + R * Mathf.Sin(90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1], -points[7] + hheight + yOffset)));

                for (int i = 0; i < fanCount * 2; ++i)
                {
                    var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                    var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(angle) + hlength - R, -pivotY + R * Mathf.Sin(angle) + yOffset),
                        new Vector3(points[1] + R * Mathf.Cos(angle2) + hlength - R, -pivotY + R * Mathf.Sin(angle2) + yOffset)));
                }

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + R * Mathf.Cos(-90 * Mathf.Deg2Rad) + hlength - R, -pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1] + bevel, -points[7] + yOffset)));
            }

            var copeLength = useCope ?(float)component.WinConnect.Beam.TCopeL : 0;
            var copeDepth = useCope ? (float)component.WinConnect.Beam.TCopeD : 0;
            var botCopeDepth = useCope ? (float)component.WinConnect.Beam.BCopeD : 0;
            var botCopeLength = useCope ? (float)component.WinConnect.Beam.BCopeL : 0;
            var radius = 0.5f;

            //Top and bottom lines
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + bevel + copeLength, points[2] + yOffset, 0), new Vector3(points[4], points[2] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + botCopeLength, -points[2] + yOffset, 0), new Vector3(points[4], -points[2] + yOffset)));

            //Bottom tf/bevel lines
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + bevel + copeLength, points[2] + yOffset, 0), new Vector3(points[1] + copeLength, points[7] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + botCopeLength, -points[2] + yOffset, 0), new Vector3(points[1] + bevel + botCopeLength, -points[7] + yOffset)));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + bevel + botCopeLength, points[5] + yOffset, 0), new Vector3(points[4], points[5] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + copeLength, -points[5] + yOffset, 0), new Vector3(points[4], -points[5] + yOffset)));

            if (useCope)
            {
                if (copeDepth > 0)
                {
                    //Add cope cut
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + copeLength - radius, points[2] + yOffset - copeDepth, 0), new Vector3(points[1], points[2] + yOffset - copeDepth)));

                    //Add little top line to cope
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + copeLength, points[2] + yOffset, 0), new Vector3(points[1] + copeLength, points[2] + yOffset - copeDepth + radius)));

                    //Add the radius cut
                    for (int i = 0; i < 16; ++i)
                    {
                        var angle = (i / 16.0f) * 90.0f - 90;
                        var angle2 = ((i + 1) / 16.0f) * 90.0f - 90;

                        var offset1 = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                        var offset2 = new Vector3(Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius, Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius);

                        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + copeLength - radius, points[2] + yOffset - copeDepth + radius, 0) + offset1,
                            new Vector3(points[1] + copeLength - radius, points[2] + yOffset - copeDepth + radius) + offset2));
                    }
                }

                if (botCopeDepth > 0)
                {
                    //Bottom cope
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + botCopeLength - radius, -points[2] + yOffset + botCopeDepth, 0), new Vector3(points[1], -points[2] + yOffset + botCopeDepth)));

                    //Add little top line to cope
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + botCopeLength, -points[2] + yOffset, 0), new Vector3(points[1] + botCopeLength, -points[2] + yOffset + botCopeDepth - radius)));

                    //Add the radius cut
                    for (int i = 0; i < 16; ++i)
                    {
                        var angle = (i / 16.0f) * 90.0f + 0;
                        var angle2 = ((i + 1) / 16.0f) * 90.0f + 0;

                        var offset1 = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                        var offset2 = new Vector3(Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius, Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius);

                        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + botCopeLength - radius, -points[2] + yOffset + botCopeDepth - radius, 0) + offset1,
                            new Vector3(points[1] + botCopeLength - radius, -points[2] + yOffset + botCopeDepth - radius) + offset2));
                    }
                }
            }

            //Add the bezier curve on the end
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(points[4] - 1, points[2] + extent + yOffset, 0), new Vector3(points[4] + 1, points[2] + 1 + yOffset),
                new Vector3(points[4] - 1, -points[2] - 1 + yOffset, 0), new Vector3(points[4] + 1, -points[2] - extent + yOffset, 0)));

            //Left side view
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[6])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[6])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, points[7] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, points[7] + yOffset, points[8])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[8])));

            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[8]), new Vector3(0, points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, points[2] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(0, -points[2] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

            //Top lines
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[1], 0, points[8]), new Vector3(points[1], 0, -points[8])));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[4], 0, points[8]), new Vector3(points[1], 0, points[8])));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[4], 0, -points[8]), new Vector3(points[1], 0, -points[8])));

            //Dotted lines
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[6]), new Vector3(points[4], 0, points[6]), 1, 1));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[1], 0, -points[6]), new Vector3(points[4], 0, -points[6]), 1, 1));

            drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(points[4] - 1, 0, points[8] + extent), new Vector3(points[4] + 1, 0, points[8] + 1),
                new Vector3(points[4] - 1, 0, -points[8] - 1), new Vector3(points[4] + 1, 0, -points[8] - extent)));
        }
    }

    public static void CreateLeftBeamLines(LineDrawing drawing, DetailData component, double z = 0.0, bool useCope = false)
    {
        drawing.ClearAllLines();
        drawing.lineOffset = 0.01f;

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component);

        if(CommonDataStatic.JointConfig == EJointConfiguration.BeamToGirder)
        {
            yOffset = DrawingMethods.GirderOffset(component) + DrawingMethods.GetTopEl(component);
        }

        //Right beam custom lines
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
        points[0] = (float)(DrawingMethods.GetPrimaryWidth()) + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;// + (extend ? primaryComponent.Shape.USShape.bf / 2 : 0));

        if (extend)
        {
            points[0] = (float)(primaryComponent.Shape.USShape.bf / 2) + (float)component.EndSetback * (float)ConstNum.METRIC_MULTIPLIER;
        }

        var isDirectlyWelded = component.MomentConnection == EMomentCarriedBy.DirectlyWelded && primaryComponent.WebOrientation == EWebOrientation.OutOfPlane;
        var directlyWeldedOffset = (float)((primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2 + component.WinConnect.MomentDirectWeld.Top.a);

        if (isDirectlyWelded)
            points[0] = (float)(DrawingMethods.GetPrimaryWidth() + (isDirectlyWelded ? directlyWeldedOffset : component.EndSetback)) * Math.Sign(1);

        points[1] = -points[0];
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
                DrawingMethods.CreateShapeDottedLineGirder(drawing.GetAllLines(), ViewMask.FRONT, component, component.ShearConnection, points[1], points[2], yOffset);

                MessageQueueTest.instance.GetView(CameraViewportType.FRONT).DestroyDrawLabelsWithTags("Column rbeam weld");
            }
            else
            {
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

                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(angle) - hlength + R, pivotY + R * Mathf.Sin(angle) + yOffset),
                        new Vector3(points[1] - R * Mathf.Cos(angle2) - hlength + R, pivotY + R * Mathf.Sin(angle2) + yOffset)));
                }

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - hlength + R, pivotY + R + yOffset),
                        new Vector3(points[1] - flatLength, pivotY + R + yOffset)));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - flatLength, points[7] + yOffset),
                        new Vector3(points[1] - flatLength, pivotY + R + yOffset)));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(-90 * Mathf.Deg2Rad) - hlength + R, pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1], points[7] - hheight + yOffset)));

                //Vertical dotted line
                DrawingMethods.CreateShapeDottedLine(drawing.GetAllLines(), ViewMask.FRONT, component, component.ShearConnection, points[1], points[7] - hheight, yOffset);

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(90 * Mathf.Deg2Rad) - hlength + R, -pivotY + R * Mathf.Sin(90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1], -points[7] + hheight + yOffset)));

                for (int i = 0; i < fanCount * 2; ++i)
                {
                    var angle = (((float)i / fanCount) * 90 - 90) * Mathf.Deg2Rad;
                    var angle2 = (((float)(i + 1) / fanCount) * 90 - 90) * Mathf.Deg2Rad;

                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(angle) - hlength + R, -pivotY + R * Mathf.Sin(angle) + yOffset),
                        new Vector3(points[1] - R * Mathf.Cos(angle2) - hlength + R, -pivotY + R * Mathf.Sin(angle2) + yOffset)));
                }

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - R * Mathf.Cos(-90 * Mathf.Deg2Rad) - hlength + R, -pivotY + R * Mathf.Sin(-90 * Mathf.Deg2Rad) + yOffset),
                        new Vector3(points[1] - bevel, -points[7] + yOffset)));
            }

            var copeLength = useCope ? (float)component.WinConnect.Beam.TCopeL : 0;
            var copeDepth = useCope ? (float)component.WinConnect.Beam.TCopeD : 0;
            var botCopeDepth = useCope ? (float)component.WinConnect.Beam.BCopeD : 0;
            var botCopeLength = useCope ? (float)component.WinConnect.Beam.BCopeL : 0;
            var radius = 0.5f;

            //Top and bottom lines
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - bevel - copeLength, points[2] + yOffset, 0), new Vector3(points[4], points[2] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - botCopeLength, -points[2] + yOffset, 0), new Vector3(points[4], -points[2] + yOffset)));

            //Bottom tf/bevel lines
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - bevel - copeLength, points[2] + yOffset, 0), new Vector3(points[1] - copeLength, points[7] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - botCopeLength, -points[2] + yOffset, 0), new Vector3(points[1] - bevel - botCopeLength, -points[7] + yOffset)));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - bevel - botCopeLength, points[5] + yOffset, 0), new Vector3(points[4], points[5] + yOffset)));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - copeLength, -points[5] + yOffset, 0), new Vector3(points[4], -points[5] + yOffset)));

            if (useCope)
            {
                if (copeDepth > 0)
                {
                    //Add cope cut
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - copeLength + radius, points[2] + yOffset - copeDepth, 0), new Vector3(points[1], points[2] + yOffset - copeDepth)));

                    //Add little top line to cope
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - copeLength, points[2] + yOffset, 0), new Vector3(points[1] - copeLength, points[2] + yOffset - copeDepth + radius)));

                    //Add the radius cut
                    for (int i = 0; i < 16; ++i)
                    {
                        var angle = (i / 16.0f) * 90.0f - 90;
                        var angle2 = ((i + 1) / 16.0f) * 90.0f - 90;

                        var offset1 = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                        var offset2 = new Vector3(-Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius, Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius);

                        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - copeLength + radius, points[2] + yOffset - copeDepth + radius, 0) + offset1,
                            new Vector3(points[1] - copeLength + radius, points[2] + yOffset - copeDepth + radius) + offset2));
                    }
                }

                if (botCopeDepth > 0)
                {
                    //Bottom cut
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - botCopeLength + radius, -points[2] + yOffset + botCopeDepth, 0), new Vector3(points[1], -points[2] + yOffset + botCopeDepth)));

                    //Add little top line to cope
                    drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - botCopeLength, -points[2] + yOffset, 0), new Vector3(points[1] - botCopeLength, -points[2] + yOffset + botCopeDepth - radius)));

                    //Add the radius cut
                    for (int i = 0; i < 16; ++i)
                    {
                        var angle = (i / 16.0f) * 90.0f + 0;
                        var angle2 = ((i + 1) / 16.0f) * 90.0f + 0;

                        var offset1 = new Vector3(-Mathf.Cos(angle * Mathf.Deg2Rad) * radius, Mathf.Sin(angle * Mathf.Deg2Rad) * radius);
                        var offset2 = new Vector3(-Mathf.Cos(angle2 * Mathf.Deg2Rad) * radius, Mathf.Sin(angle2 * Mathf.Deg2Rad) * radius);

                        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] - botCopeLength + radius, -points[2] + yOffset + botCopeDepth - radius, 0) + offset1,
                            new Vector3(points[1] - botCopeLength + radius, -points[2] + yOffset + botCopeDepth - radius) + offset2));
                    }
                }
            }

            //Add the bezier curve on the end
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(points[4] - 1, points[2] + extent + yOffset, 0), new Vector3(points[4] + 1, points[2] + 1 + yOffset),
                new Vector3(points[4] - 1, -points[2] - 1 + yOffset, 0), new Vector3(points[4] + 1, -points[2] - extent + yOffset, 0)));

            //Right side view
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[6])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[6])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[6]), new Vector3(0, points[7] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[6]), new Vector3(0, points[7] + yOffset, points[8])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[6]), new Vector3(0, -points[7] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[6]), new Vector3(0, -points[7] + yOffset, points[8])));

            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[7] + yOffset, points[8]), new Vector3(0, points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, points[2] + yOffset, -points[8]), new Vector3(0, points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, -points[8])));
            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, -points[7] + yOffset, points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

            drawing.AddLine(ViewMask.LEFT, CustomLine.CreateNormalLine(new Vector3(0, -points[2] + yOffset, -points[8]), new Vector3(0, -points[2] + yOffset, points[8])));

            //Top lines
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[1], 0, points[8]), new Vector3(points[1], 0, -points[8])));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[4], 0, points[8]), new Vector3(points[1], 0, points[8])));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[4], 0, -points[8]), new Vector3(points[1], 0, -points[8])));

            //Dotted lines
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[6]), new Vector3(points[4], 0, points[6]), 1, 1));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[1], 0, -points[6]), new Vector3(points[4], 0, -points[6]), 1, 1));

            drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(points[4] - 1, 0, points[8] + extent), new Vector3(points[4] + 1, 0, points[8] + 1),
                new Vector3(points[4] - 1, 0, -points[8] - 1), new Vector3(points[4] + 1, 0, -points[8] - extent)));
        }
    }

    public static void AddMeshLines(LineDrawing drawing, Mesh sourceMesh, ViewMask[] masks)
    {
        drawing.lineOffset = 0.01f;

        //Create lines based on the vertices
        var verts = sourceMesh.vertices;
        var lines = new List<CustomLine>(512);

        for (int i = 0; i < verts.Length / 6; ++i)
        {
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 0], verts[i * 6 + 1]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 1], verts[i * 6 + 2]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 3], verts[i * 6 + 4]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 4], verts[i * 6 + 0]));
        }

        CustomLine[] array = lines.ToArray();

        foreach (var mask in masks)
        {
            drawing.AddLines(mask, array);
        }
    }

    public static void CreateMeshLines(LineDrawing drawing, Mesh sourceMesh, ViewMask[] masks)
    {
        drawing.ClearAllLines();
        drawing.lineOffset = 0.01f;

        //Create lines based on the vertices
        var verts = sourceMesh.vertices;
        var lines = new List<CustomLine>(512);

        for (int i = 0; i < verts.Length / 6; ++i)
        {
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 0], verts[i * 6 + 1]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 1], verts[i * 6 + 2]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 3], verts[i * 6 + 4]));
            lines.Add(CustomLine.CreateNormalLine(verts[i * 6 + 4], verts[i * 6 + 0]));
        }

        CustomLine[] array = lines.ToArray();

        foreach (var mask in masks)
        {
            drawing.AddLines(mask, array);
        }
    }

    public static void CreateDirectlyWeldedSinglePlateLines(LineDrawing drawing, DetailData component, double x = 0.0)
    {
        drawing.ClearAllLines();
        drawing.lineOffset = 0.01f;

        var plate = component.WinConnect.ShearWebPlate;

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component);

        var t = (float)(plate.Thickness * ConstNum.METRIC_MULTIPLIER);

        var plateHeight = (float)(plate.Height * ConstNum.METRIC_MULTIPLIER);
        var plateWidth = (float)(primaryComponent.Shape.USShape.bf - primaryComponent.Shape.USShape.tw) / 2;
        var plate2Width = (float)(component.WinConnect.Beam.Lh + component.WinConnect.MomentDirectWeld.Top.a + plate.Bolt.EdgeDistTransvDir);
        var plate2Height = (float)plate.Length;

        if (plate.WebPlateStiffener == EWebPlateStiffener.With)
            plate2Width = (float)plate.Width - plateWidth;

        var radius = 0.5f;

        //Places the top of the bigger plate flush with the beam
        var offset = (plateHeight - (float)component.Shape.d) / 2;
        var offset2 = DrawingMethods.GetPositionOffset(component.WinConnect.Beam.DistanceToFirstBoltDisplay, component.Shape.d, plate2Height, plate.Bolt.EdgeDistLongDir, plate.Position);

        if (plate.WebPlateStiffener == EWebPlateStiffener.Without)
        {
            offset = 0;
            offset2 = 0;
        }

        var points = new float[20];
        var leftSide = x < 0;

        points[0] = (float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(x));
        points[1] = points[0] + plateWidth * Math.Sign(x);
        points[2] = plateHeight / 2 + yOffset - offset;
        points[3] = -plateHeight / 2 + yOffset - offset;
        points[4] = ((float)plate2Height / 2) + radius + yOffset + offset2;
        points[5] = -((float)plate2Height / 2) - radius + yOffset + offset2;
        points[6] = points[1] + (float)plate2Width * Math.Sign(x);
        points[7] = -((float)component.Shape.USShape.tw / 2 + t / 2);
        points[8] = ((float)plate2Height / 2) + yOffset + offset2;
        points[9] = ((float)-plate2Height / 2) + yOffset + offset2;

        //Create the custom lines
        if (plate.WebPlateStiffener == EWebPlateStiffener.Without)
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[2]), new Vector3(points[0], points[3])));
        else
        {
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[2] - radius), new Vector3(points[0], points[3] + radius)));

            //Draw the small curves
            for (int i = 0; i < 8; ++i)
            {
                var angle = ((i / 8.0f) * 90.0f - 90 + (!leftSide ? -90 : 0)) * Mathf.Deg2Rad;
                var angle2 = (((float)i + 1) / 8.0f * 90.0f - 90 + (!leftSide ? -90 : 0)) * Mathf.Deg2Rad;

                var offset1 = new Vector3(-radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                var offset11 = new Vector3(-radius * Mathf.Cos(angle2), radius * Mathf.Sin(angle2));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0] + 0 * Math.Sign(x), points[2]) + offset1,
                    new Vector3(points[0] + 0 * Math.Sign(x), points[2]) + offset11));
            }

            //Draw the small curves
            for (int i = 0; i < 8; ++i)
            {
                var angle = ((i / 8.0f) * 90.0f + (!leftSide ? 90 : 0)) * Mathf.Deg2Rad;
                var angle2 = (((float)i + 1) / 8.0f * 90.0f + (!leftSide ? 90 : 0)) * Mathf.Deg2Rad;

                var offset1 = new Vector3(-radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
                var offset11 = new Vector3(-radius * Mathf.Cos(angle2), radius * Mathf.Sin(angle2));

                drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0] + 0 * Math.Sign(x), points[3]) + offset1,
                    new Vector3(points[0] + 0 * Math.Sign(x), points[3]) + offset11));
            }
        }

        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[2]), new Vector3(points[1], points[2])));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[3]), new Vector3(points[1], points[3])));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1], points[3]), new Vector3(points[1], points[5])));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1], points[2]), new Vector3(points[1], points[4])));

        //Draw the small curves
        for (int i = 0; i < 8; ++i)
        {
            var angle = ((i / 8.0f) * 90.0f - 90 + (!leftSide ? -90 : 0)) * Mathf.Deg2Rad;
            var angle2 = (((float)i + 1) / 8.0f * 90.0f - 90 + (!leftSide ? -90 : 0)) * Mathf.Deg2Rad;

            var offset1 = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
            var offset11 = new Vector3(radius * Mathf.Cos(angle2), radius * Mathf.Sin(angle2));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + radius * Math.Sign(x), points[4]) + offset1,
                new Vector3(points[1] + radius * Math.Sign(x), points[4]) + offset11));
        }

        //Draw the small curves
        for (int i = 0; i < 8; ++i)
        {
            var angle = ((i / 8.0f) * 90.0f + (!leftSide ? 90 : 0)) * Mathf.Deg2Rad;
            var angle2 = (((float)i + 1) / 8.0f * 90.0f + (!leftSide ? 90 : 0)) * Mathf.Deg2Rad;

            var offset1 = new Vector3(radius * Mathf.Cos(angle), radius * Mathf.Sin(angle));
            var offset11 = new Vector3(radius * Mathf.Cos(angle2), radius * Mathf.Sin(angle2));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[1] + radius * Math.Sign(x), points[5]) + offset1,
                new Vector3(points[1] + radius * Math.Sign(x), points[5]) + offset11));
        }

        //Draw the small curves
        //drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(points[1], points[4]), new Vector3(points[1], points[4] - radius),
        //    new Vector3(points[1], points[4] - radius), new Vector3(points[1] + radius * Math.Sign(x), points[4] - radius)));

        //drawing.AddLine(ViewMask.FRONT, CustomLine.CreateBezierLine(new Vector3(points[1], points[5]), new Vector3(points[1], points[5] + radius),
        //    new Vector3(points[1], points[5] + radius), new Vector3(points[1] + radius * Math.Sign(x), points[5] + radius)));

        //Draw the lines for the smaller plate
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[6], points[4] - radius), new Vector3(points[1] + radius * Math.Sign(x), points[4] - radius)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[6], points[5] + radius), new Vector3(points[1] + radius * Math.Sign(x), points[5] + radius)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[6], points[5] + radius), new Vector3(points[6], points[4] - radius)));

        //Make the lines in the other viewports
        //Make the side lines
        var viewSide = Math.Sign(x) > 0 ? ViewMask.RIGHT : ViewMask.LEFT;

        //Make the side large plate
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] + t / 2), new Vector3(0, points[2], points[7] - t / 2)));
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] + t / 2), new Vector3(0, points[3], points[7] + t / 2)));
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[2], points[7] - t / 2), new Vector3(0, points[3], points[7] - t / 2)));
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[3], points[7] + t / 2), new Vector3(0, points[3], points[7] - t / 2)));

        //Make the two lines for the small plate
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[8], points[7] + t / 2), new Vector3(0, points[8], points[7] - t / 2)));
        drawing.AddLine(viewSide, CustomLine.CreateNormalLine(new Vector3(0, points[9], points[7] + t / 2), new Vector3(0, points[9], points[7] - t / 2)));

        //Make the top lines
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] + t / 2), new Vector3(points[6], 0, points[7] + t / 2), 0.25f, 0.25f));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] - t / 2), new Vector3(points[0], 0, points[7] + t / 2), 0.25f, 0.25f));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[6], 0, points[7] - t / 2), new Vector3(points[6], 0, points[7] + t / 2), 0.25f, 0.25f));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[0], 0, points[7] - t / 2), new Vector3(points[6], 0, points[7] - t / 2), 0.25f, 0.25f));

        //Create the small plate line division
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(points[1], 0, points[7] - t / 2), new Vector3(points[1], 0, points[7] + t / 2), 0.25f, 0.25f));
    }

    public static void CreateContinuityPlateLines(LineDrawing drawing, DetailData component, double x = 0.0)
    {
        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component);

        var points = new float[20];
        points[0] = (float)(primaryComponent.Shape.USShape.tw / 2 * Math.Sign(x));
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
        points[12] = (float)(primaryComponent.Shape.USShape.bf / 2 * Math.Sign(x));
        points[13] = points[12] + points[4] * Math.Sign(x);

        //Create the custom lines
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[12], 0, -points[11]), new Vector3(points[0] + points[2] * Math.Sign(x), 0, -points[11])));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[12], 0, points[11]), new Vector3(points[0] + points[2] * Math.Sign(x), 0, points[11])));

        //Clip lines
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[0], 0, -points[11] + points[2]), new Vector3(points[0] + points[2] * Math.Sign(x), 0, -points[11])));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[0], 0, points[11] - points[2]), new Vector3(points[0] + points[2] * Math.Sign(x), 0, points[11])));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[12], 0, -points[11]), new Vector3(points[13], 0, -points[5] / 2)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[12], 0, points[11]), new Vector3(points[13], 0, points[5] / 2)));

        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[13], 0, -points[5] / 2), new Vector3(points[13], 0, points[5] / 2)));
        drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(points[0], 0, -points[11] + points[2]), new Vector3(points[0], 0, points[11] - points[2])));

        //Create the side lines
        var viewside = x < 0 ? ViewMask.LEFT : ViewMask.RIGHT;
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[1] + points[7] / 2, -points[11]), new Vector3(0, points[1] + points[7] / 2, points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, -points[11]), new Vector3(0, points[1] - points[7] / 2, points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, -points[11]), new Vector3(0, points[1] + points[7] / 2, -points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[1] - points[7] / 2, points[11]), new Vector3(0, points[1] + points[7] / 2, points[11])));

        //Create front lines
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[1] + points[7] / 2), new Vector3(points[13], points[1] + points[7] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[1] - points[7] / 2), new Vector3(points[13], points[1] - points[7] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[1] - points[7] / 2), new Vector3(points[0], points[1] + points[7] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[13], points[1] - points[7] / 2), new Vector3(points[13], points[1] + points[7] / 2)));

        //Create bottom side lines
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[10] + points[9] / 2, -points[11]), new Vector3(0, points[10] + points[9] / 2, points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, -points[11]), new Vector3(0, points[10] - points[9] / 2, points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, -points[11]), new Vector3(0, points[10] + points[9] / 2, -points[11])));
        drawing.AddLine(viewside, CustomLine.CreateNormalLine(new Vector3(0, points[10] - points[9] / 2, points[11]), new Vector3(0, points[10] + points[9] / 2, points[11])));

        //Create bottom front lines
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[10] + points[9] / 2), new Vector3(points[13], points[10] + points[9] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[10] - points[9] / 2), new Vector3(points[13], points[10] - points[9] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[0], points[10] - points[9] / 2), new Vector3(points[0], points[10] + points[9] / 2)));
        drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(points[13], points[10] - points[9] / 2), new Vector3(points[13], points[10] + points[9] / 2)));
    }
    #endregion

    #region BeamToGirder
    public static void CreateGirderPrimaryBeamLines(LineDrawing drawing, DetailData component, double z = 0.0)
    {
        drawing.ClearAllLines();

        var primaryComponent = CommonDataStatic.DetailDataDict[EMemberType.PrimaryMember];
        var isPrimaryInPlane = primaryComponent.WebOrientation == EWebOrientation.InPlane;
        var yOffset = (float)DrawingMethods.GetTopElAndYOffset(component);

        if (isPrimaryInPlane)
        {
            var width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.d : component.Shape.USShape.bf) / 2);
            var innerWidth = (float)((isPrimaryInPlane == true ? component.Shape.USShape.tf : (component.Shape.USShape.bf - component.Shape.USShape.tw) / 2));
            var extent = width + 4;

            //Right lines //////////////////
            //Outside lines
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0)));

            var outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.2f);
            var outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, -1 + (float)z - yOffset, 0), new Vector3(width, 1 + (float)z - yOffset, 0), new Vector3(-width, -1 + (float)z - yOffset, 0), new Vector3(-extent, 1 + (float)z - yOffset, 0), 0.8f);

            var rbeam = CommonDataStatic.DetailDataDict[EMemberType.RightBeam];
            //Right side
            if (rbeam.IsActive && rbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
            {
                var plate = rbeam.WinConnect.ShearWebPlate;
                var yoff = DrawingMethods.GetTopElAndYOffset(component);

                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateIntersectLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
            }
            else
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(width, outerLeftHeight.y, 0), new Vector3(width, -outerRightHeight.y - 2 * yOffset, 0)));

            var lbeam = CommonDataStatic.DetailDataDict[EMemberType.LeftBeam];
            //Left side
            if (lbeam.IsActive && lbeam.MomentConnection == EMomentCarriedBy.DirectlyWelded)
            {
                var plate = lbeam.WinConnect.ShearWebPlate;
                var yoff = DrawingMethods.GetTopElAndYOffset(component);

                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateIntersectLine(new Vector3(-width, outerLeftHeight.y, 0), new Vector3(-width, yoff + (float)plate.Length / 2 + 0.5f), new Vector3(-width, yoff - (float)plate.Length / 2 - 0.5f), new Vector3(-width, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
            }
            else
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(-width, outerRightHeight.y, 0), new Vector3(-width, -outerLeftHeight.y - 2 * yOffset, 0)));

            //Inner lines
            if (isPrimaryInPlane)
            {
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0)));
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateNormalLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0)));
            }
            else
            {
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(width - innerWidth, outerLeftHeight.y, 0), new Vector3(width - innerWidth, -outerRightHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
                drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, outerRightHeight.y, 0), new Vector3(-width + innerWidth, -outerLeftHeight.y - 2 * yOffset, 0), 1.0f, 1.0f));
            }

            //Bottom Bezier
            drawing.AddLine(ViewMask.RIGHT, CustomLine.CreateBezierLine(new Vector3(extent, -1 - (float)z - yOffset, 0), new Vector3(width, 1 - (float)z - yOffset, 0), new Vector3(-width, -1 - (float)z - yOffset, 0), new Vector3(-extent, 1 - (float)z - yOffset, 0)));

            drawing.RotateLines(ViewMask.RIGHT, new Vector3(0, 90, 90));

            //Duplicate the lines to the left side
            drawing.Duplicate(ViewMask.LEFT, drawing.GetLines(ViewMask.RIGHT).ToArray());

            //Front lines ///////////////////////
            width = (float)(component.Shape.USShape.d / 2);
            var length = (float)(component.Shape.USShape.bf / 2);
            var flangeThick = (float)(component.Shape.USShape.tf);
            var webThick = (float)(component.Shape.USShape.tw / 2);
            var angles = new Vector3(0, 0, 90);

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, webThick), new Vector3(width - flangeThick, webThick), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, -webThick), new Vector3(width - flangeThick, -webThick), angles));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, webThick), new Vector3(-width + flangeThick, length), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, webThick), new Vector3(width - flangeThick, length), angles));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, length), new Vector3(-width, length), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, length), new Vector3(width, length), angles));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width, length), new Vector3(-width, -length), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width, length), new Vector3(width, -length), angles));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, -webThick), new Vector3(-width + flangeThick, -length), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, -webThick), new Vector3(width - flangeThick, -length), angles));

            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(-width + flangeThick, -length), new Vector3(-width, -length), angles));
            drawing.AddLine(ViewMask.FRONT, CustomLine.CreateNormalLine(new Vector3(width - flangeThick, -length), new Vector3(width, -length), angles));

            //Top lines
            width = (float)((isPrimaryInPlane == true ? component.Shape.USShape.bf : component.Shape.USShape.d) / 2);
            innerWidth = (float)((isPrimaryInPlane == true ? ((component.Shape.USShape.bf - component.Shape.USShape.tw) / 2) : component.Shape.USShape.tf));
            extent = width + 4;
            //Top
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(extent, 0, -1 + (float)z - yOffset), new Vector3(width, 0, 1 + (float)z - yOffset), new Vector3(-width, 0, -1 + (float)z - yOffset), new Vector3(-extent, 0, 1 + (float)z - yOffset)));
            //Outside lines
            outerLeftHeight = MeshCreator.GetBezierPoint(new Vector3(extent, 0, -1 + (float)z - yOffset), new Vector3(width, 0, 1 + (float)z - yOffset), new Vector3(-width, 0, -1 + (float)z - yOffset), new Vector3(-extent, 0, 1 + (float)z - yOffset), 0.2f);
            outerRightHeight = MeshCreator.GetBezierPoint(new Vector3(extent, 0, -1 + (float)z - yOffset), new Vector3(width, 0, 1 + (float)z - yOffset), new Vector3(-width, 0, -1 + (float)z - yOffset), new Vector3(-extent, 0, 1 + (float)z - yOffset), 0.8f);

            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width, 0, outerLeftHeight.z), new Vector3(width, 0, -outerRightHeight.z - 2 * yOffset)));
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width, 0, outerRightHeight.z), new Vector3(-width, 0, -outerLeftHeight.z - 2 * yOffset)));

            //Inner lines
            if (isPrimaryInPlane)
            {
                drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(width - innerWidth, 0, outerLeftHeight.z), new Vector3(width - innerWidth, 0, -outerRightHeight.z - 2 * yOffset), 1.0f, 1.0f));
                drawing.AddLine(ViewMask.TOP, CustomLine.CreateDottedLine(new Vector3(-width + innerWidth, 0, outerRightHeight.z), new Vector3(-width + innerWidth, 0, -outerLeftHeight.z - 2 * yOffset), 1.0f, 1.0f));
            }
            else
            {
                drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(width - innerWidth, 0, outerLeftHeight.z), new Vector3(width - innerWidth, 0, -outerRightHeight.z - 2 * yOffset)));
                drawing.AddLine(ViewMask.TOP, CustomLine.CreateNormalLine(new Vector3(-width + innerWidth, 0, outerRightHeight.z), new Vector3(-width + innerWidth, 0, -outerLeftHeight.z - 2 * yOffset)));
            }

            //Bottom Bezier
            drawing.AddLine(ViewMask.TOP, CustomLine.CreateBezierLine(new Vector3(extent, 0, -1 - (float)z - yOffset), new Vector3(width, 0, 1 - (float)z - yOffset), new Vector3(-width, 0, -1 - (float)z - yOffset), new Vector3(-extent, 0, 1 - (float)z - yOffset)));
        }
    }
    #endregion
}
