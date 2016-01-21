using UnityEngine;
using System.Collections;

public enum LineType { NORMAL, DOTTED, BEZIER, INTERSECT } //Intersect line is a normal line with a dotted section inside

public class CustomLine : System.Object
{
    public LineType lineType;
    public Vector3 p0;
    public Vector3 p1;
    public Vector3 p2;
    public Vector3 p3;
    public float segmentSize;
    public float spacingSize;

    public void Rotate(Vector3 angles)
    {
        var rot = Quaternion.Euler(angles);

        p0 = rot * p0;
        p1 = rot * p1;
        p2 = rot * p2;
        p3 = rot * p3;
    }

    private CustomLine() { }

    public CustomLine(CustomLine line)
    {
        this.lineType = line.lineType;
        this.p0 = line.p0;
        this.p1 = line.p1;
        this.p2 = line.p2;
        this.p3 = line.p3;
    }

    public static CustomLine CreateNormalLine(Vector3 p0, Vector3 p1)
    {
        var line = new CustomLine();
        line.p0 = p0;
        line.p1 = p1;
        line.lineType = LineType.NORMAL;

        return line;
    }

    public static CustomLine CreateNormalLine(Vector3 p0, Vector3 p1, Vector3 angles)
    {
        var line = new CustomLine();
        line.p0 = p0;
        line.p1 = p1;
        line.lineType = LineType.NORMAL;
        line.Rotate(angles);

        return line;
    }

    public static CustomLine CreateDottedLine(Vector3 p0, Vector3 p1, float segmentSize, float spacingSize)
    {
        var line = new CustomLine();
        line.p0 = p0;
        line.p1 = p1;
        line.lineType = LineType.DOTTED;
        line.segmentSize = segmentSize;
        line.spacingSize = spacingSize;

        return line;
    }

    /// <summary>
    /// p0 - Normal line start
    /// p1 - Dotted line start
    /// p2 - Dotten line end
    /// p3 - Normal line end
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    /// <param name="segmentSize"></param>
    /// <param name="spacingSize"></param>
    /// <returns></returns>
    public static CustomLine CreateIntersectLine(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float segmentSize, float spacingSize)
    {
        var line = new CustomLine();
        line.p0 = p0;
        line.p1 = p1;
        line.p2 = p2;
        line.p3 = p3;
        line.lineType = LineType.INTERSECT;
        line.segmentSize = segmentSize;
        line.spacingSize = spacingSize;

        return line;
    }

    public static CustomLine CreateBezierLine(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        var line = new CustomLine();
        line.p0 = p0;
        line.p1 = p1;
        line.p2 = p2;
        line.p3 = p3;
        line.lineType = LineType.BEZIER;

        return line;
    }
}