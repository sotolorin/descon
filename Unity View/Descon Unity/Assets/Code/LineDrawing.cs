using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Descon.Data;

//This data structure holds all the information needed to create and hold the lines used for drawing
public class LineDrawing : MonoBehaviour
{
    private Dictionary<ViewMask, Drawable> drawables; //Represents the actual mesh object
    private Dictionary<ViewMask, List<CustomLine>> drawingLines; //A list of line structures to create the line meshes

    public float lineOffset = 0.0f;

    private void Awake()
    {
        drawingLines = new Dictionary<ViewMask, List<CustomLine>>();

        drawingLines.Add(ViewMask.D3, new List<CustomLine>());
        drawingLines.Add(ViewMask.FRONT, new List<CustomLine>());
        drawingLines.Add(ViewMask.LEFT, new List<CustomLine>());
        drawingLines.Add(ViewMask.RIGHT, new List<CustomLine>());
        drawingLines.Add(ViewMask.TOP, new List<CustomLine>());

        drawables = new Dictionary<ViewMask, Drawable>();
        drawables.Add(ViewMask.D3, new Drawable());
        drawables.Add(ViewMask.FRONT, new Drawable());
        drawables.Add(ViewMask.LEFT, new Drawable());
        drawables.Add(ViewMask.RIGHT, new Drawable());
        drawables.Add(ViewMask.TOP, new Drawable());

        //Starting color
        foreach(var item in drawables)
        {
            item.Value.LineColor = Color.magenta;
        }
    }

    public void SetVisible(bool visible)
    {
        foreach(var draw in drawables)
        {
            draw.Value.lineObject.SetActive(visible);
        }
    }

    public Dictionary<ViewMask, List<CustomLine>> GetAllLines()
    {
        return drawingLines;
    }

    public List<CustomLine> GetLines(ViewMask mask)
    {
        return drawingLines[mask];
    }

    public Color Color
    {
        set
        {
            var masks = (ViewMask[])System.Enum.GetValues(typeof(ViewMask));

            foreach(var mask in masks)
            {
                drawables[mask].LineColor = value;
            }
        }
    }

    public void SetLineColor(ViewMask mask, Color color)
    {
        drawables[mask].LineColor = color;
    }

    public void ClearLines(ViewMask mask)
    {
        drawingLines[mask].Clear();
    }

    public void ClearAllLines()
    {
        foreach(var item in drawingLines)
        {
            item.Value.Clear();
        }
    }

    public Mesh GetLineMesh(ViewMask mask)
    {
        return drawables[mask].LineMesh;
    }

    public void UpdateLineMesh(Camera cam, ViewMask mask)
    {
        drawables[mask].CreateLines(drawingLines[mask], cam, MessageQueueTest.GetLineSize(), lineOffset);
    }

    public void AddLine(ViewMask mask, CustomLine line)
    {
        drawingLines[mask].Add(line);
    }

    public void AddLines(ViewMask mask, CustomLine[] lines)
    {
        drawingLines[mask].AddRange(lines);
    }

    public void RotateLines(ViewMask mask, Vector3 angles)
    {
        //Rotate the lines of a view
        var rlines = drawingLines[mask];

        foreach(var line in rlines)
        {
            line.Rotate(angles);
        }
    }

    public void Duplicate(ViewMask mask, CustomLine[] lines)
    {
        foreach(var line in lines)
        {
            drawingLines[mask].Add(new CustomLine(line));
        }
    }

    private void OnDestroy()
    {
        foreach(var draw in drawables)
        {
            draw.Value.Destroy();
        }

        //Clear the dictionary
        drawables.Clear();
        drawables = null;

        foreach(var item in drawingLines)
        {
            item.Value.Clear();
        }

        //Clear the dictionary
        drawingLines.Clear();
        drawingLines = null;
    }
}
