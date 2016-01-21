using System.Linq;
using System.Runtime.Remoting.Messaging;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public partial class LineRendererTest
{
    public static Mesh CreateWideFlangeMesh(float length)
    {
        //     bf
        //  |-------| Tf     -
        //     | |            |
        //     | |            |
        //     |Tw|           D
        //     | |            |
        //     | |            |
        //  |-------|        -

        //TODO: Hardcoded for W44X335
        var bf = 15.9f;
        var tw = 1.03f;
        var d = 44f;
        var tf = 1.77f;
        var width = bf/2;
        var iWidth = tw/2;
        var height = d/2;
        var iHeight = height - tf;
        length = Math.Abs(length);

        MeshCreator.Initialize();
        MeshCreator.Reset();

        //Top////////////////
        MeshCreator.AddQuad(new Vector3(-width, height, -length), new Vector3(-width, height, length), new Vector3(width, height, length), new Vector3(width, height, -length));
        //Top back
        MeshCreator.AddQuad(new Vector3(-width, iHeight, -length), new Vector3(-width, height, -length), new Vector3(width, height, -length), new Vector3(width, iHeight, -length));
        //Top front
        MeshCreator.AddQuad(new Vector3(width, iHeight, length), new Vector3(width, height, length), new Vector3(-width, height, length), new Vector3(-width, iHeight, length));
        //Left
        MeshCreator.AddQuad(new Vector3(-width, iHeight, length), new Vector3(-width, height, length), new Vector3(-width, height, -length), new Vector3(-width, iHeight, -length));
        //Right
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(width, height, -length), new Vector3(width, height, length), new Vector3(width, iHeight, length));
        //Underside Left
        MeshCreator.AddQuad(new Vector3(-iWidth, iHeight, -length), new Vector3(-iWidth, iHeight, length), new Vector3(-width, iHeight, length), new Vector3(-width, iHeight, -length));
        //Underside Right
        MeshCreator.AddQuad(new Vector3(width, iHeight, -length), new Vector3(width, iHeight, length), new Vector3(iWidth, iHeight, length), new Vector3(iWidth, iHeight, -length));

        //Bottom////////////////
        MeshCreator.AddQuad(new Vector3(width, -height, -length), new Vector3(width, -height, length), new Vector3(-width, -height, length), new Vector3(-width, -height, -length));
        //Top back
        MeshCreator.AddQuad(new Vector3(width, -iHeight, -length), new Vector3(width, -height, -length), new Vector3(-width, -height, -length), new Vector3(-width, -iHeight, -length));
        //Top front
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, length), new Vector3(-width, -height, length), new Vector3(width, -height, length), new Vector3(width, -iHeight, length));
        //Left
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, -length), new Vector3(-width, -height, -length), new Vector3(-width, -height, length), new Vector3(-width, -iHeight, length));
        //Right
        MeshCreator.AddQuad(new Vector3(width, -iHeight, length), new Vector3(width, -height, length), new Vector3(width, -height, -length), new Vector3(width, -iHeight, -length));
        //Underside Left
        MeshCreator.AddQuad(new Vector3(-width, -iHeight, -length), new Vector3(-width, -iHeight, length), new Vector3(-iWidth, -iHeight, length), new Vector3(-iWidth, -iHeight, -length));
        //Underside Right
        MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, -iHeight, length), new Vector3(width, -iHeight, length), new Vector3(width, -iHeight, -length));

        //Column
        //Back
        MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, -length), new Vector3(-iWidth, iHeight, -length), new Vector3(iWidth, iHeight, -length), new Vector3(iWidth, -iHeight, -length));
        //Front
        MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, length), new Vector3(iWidth, iHeight, length), new Vector3(-iWidth, iHeight, length), new Vector3(-iWidth, -iHeight, length));
        //Right
        MeshCreator.AddQuad(new Vector3(iWidth, -iHeight, -length), new Vector3(iWidth, iHeight, -length), new Vector3(iWidth, iHeight, length), new Vector3(iWidth, -iHeight, length));
        //Left
        MeshCreator.AddQuad(new Vector3(-iWidth, -iHeight, length), new Vector3(-iWidth, iHeight, length), new Vector3(-iWidth, iHeight, -length), new Vector3(-iWidth, -iHeight, -length));

        MeshCreator.Rotate(new Vector3(90, 90, 0));
        if (clickPosition != null)
        {
            var vct = new Vector3(((Vector3)clickPosition).x, ((Vector3)clickPosition).y, 0f);
            MeshCreator.Translate(vct);
        }
        originalVerts = MeshCreator.Create().vertices.ToList();

        MeshCreator.Rotate(originalRotation);
        return MeshCreator.Create();
    }

    private static Mesh CreateSinglePlateMesh(float x, float y)
    {      
        return MeshCreator.CreateBoxMesh(Vector3.zero, new Vector3(x, y, 0.01f), new Vector3(0,0,0), new Vector3(0,0,-55));
    }

    private static List<Mesh> CreateGridPlateMesh(float x, float y)
    {
        var response = new List<Mesh>();
        for (int r = 0; r < row; r++)
        {
            for (int c = 0; c < col; c++)
            {
                response.Add(MeshCreator.CreateBoxMesh(new Vector3(row * x,col *x,0), new Vector3(x, y, 0.01f), new Vector3(0, 0, 0)));
            }
        }
        return response;
    }
}