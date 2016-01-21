using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawGussetPlate(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        if (gusset != null)
        {
            mesh = CreateGussetPlateMesh(gusset);
            var gurX = brace0.Shape.USShape.d / 2; //UR
            var glrX = brace0.Shape.USShape.d / 2; //LR
            var gurY = brace1.Shape.USShape.d / 2; //UR
            var glrY = -brace1.Shape.USShape.d / 2; //LR
            var gulX = -brace0.Shape.USShape.d / 2; //UL
            var glLX = -brace0.Shape.USShape.d / 2; //lL
            var gulY = brace2.Shape.USShape.d / 2; //UL
            var glLY = -brace2.Shape.USShape.d / 2; //lL
            if (uR) translation = new Vector3((float)gurX, (float)gurY, 0);
            if (uL) translation = new Vector3((float)gulX, (float)gulY, 0);
            if (lR) translation = new Vector3((float)glrX, (float)glrY, 0);
            if (lL) translation = new Vector3((float)glLX, (float)glLY, 0);

            MeshCreator.Rotate((lL || lR) ? -180 : 0, (uL || lL) ? -180 : 0, 0);
            MeshCreator.Translate(translation);
            MeshCreator.Rotate(0, 180, 0);

            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());
            MeshCreator.Create(mesh);
        }
    }
}
