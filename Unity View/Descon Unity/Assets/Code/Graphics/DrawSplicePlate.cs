using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;
using System;

public partial class DrawingMethods
{
    public void DrawSplicePlate(DetailData component, double x = 0, double y = 0, bool conBool = false)
    {
        if (component.BraceConnect.SplicePlates != null)
        {
            mesh = CreateSplicePlateMesh(component.BraceConnect.SplicePlates, x, y);
            connectionMeshes.Add(mesh);
            connectionMats.Add(MeshCreator.GetConnectionMaterial());
        }
    }
}