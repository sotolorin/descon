using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//New hierachy shift, all things that can be draw in descon now must be drawabble
//This class manages the creation and destruction of line objects needed by the viewport controllers to draw lines for these objects
public class Drawable : System.Object
{
    public GameObject lineObject;
    private Mesh lineObjectMesh;

    public Drawable()
    {
        lineObject = MeshCreator.CreateLineObject("Line Object");
        lineObject.hideFlags = HideFlags.HideAndDontSave;
        //Cache the mesh for easier retrieveal
        lineObjectMesh = lineObject.GetComponent<MeshFilter>().sharedMesh;
        lineObjectMesh.MarkDynamic();
    }

    public Color LineColor
    {
        get
        {
            return lineObject.renderer.material.color;
        }

        set
        {
            lineObject.renderer.material.color = value;
        }
    }

    public Mesh LineMesh
    {
        get
        {
            return lineObjectMesh;
        }

        set
        {
            lineObjectMesh = value;
        }
    }

    public void CreateLines(List<CustomLine> lines, Camera cam, float lineSize, float zOffset = 0.0f)
    {
        MeshCreator.Reset();

        //Draw the lines from the positions
        foreach (var line in lines)
        {
            switch (line.lineType)
            {
                case LineType.NORMAL:
                    MeshCreator.DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    break;

                case LineType.DOTTED:
                    MeshCreator.DrawDottedEdgeWorldSpace(cam, line.p0, line.p1, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    break;

                case LineType.BEZIER:
                    MeshCreator.DrawBezierCurveWorldSpace(cam, line.p0, line.p1, line.p2, line.p3, 32, lineSize, zOffset);
                    break;

                case LineType.INTERSECT:
                    MeshCreator.DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    MeshCreator.DrawDottedEdgeWorldSpace(cam, line.p1, line.p2, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    MeshCreator.DrawEdgeWorldSpace(cam, line.p2, line.p3, lineSize, zOffset);
                    break;
            }
        }

        MeshCreator.Create(LineMesh);
    }

    public void Destroy()
    {
        lineObjectMesh.Clear();
        lineObjectMesh = null;
        if (lineObjectMesh != null) GameObject.Destroy(lineObjectMesh);
        if (lineObject != null) GameObject.Destroy(lineObject);
    }
}
