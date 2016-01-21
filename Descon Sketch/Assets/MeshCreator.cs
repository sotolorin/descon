using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class that handles the creation of meshes. This script must be attached to a gameObject in the scene to work.
/// </summary>
public class MeshCreator : MonoBehaviour
{
    private static MeshCreator instance;
    private static List<Vector3> vertices;
    private static List<Vector2> uvs;
    private static List<Vector3> normals;
    private static List<int> triangles;
    private static int quadCount = 0;

    public Material lineMaterial;
    public Material weldMaterial;
    public Material boltMaterial;
    public Material beamMaterial;
    public Material connectionMaterial;

    private static float planeZ = 1f;
    public static float textOffset = 0.5f;

    public static Mesh CreateBoxMesh(Vector3 translation, Vector3 dim, Vector3 angles, Vector3? additionalTranslation = null)
    {
        dim /= 2.0f;

        //MeshCreator.Reset();

        AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, dim.y, -dim.z));
        AddQuad(new Vector3(-dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, -dim.z));
        AddQuad(new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, -dim.z));
        AddQuad(new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
        AddQuad(new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z));
        AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, -dim.z));

        Rotate(angles);
        Translate(translation);
        if (additionalTranslation != null) Translate((Vector3)additionalTranslation);
        return Create();
    }

    public static void SetLineColor(Color c)
    {
        //lineMaterial.SetColor("_Color", c);
    }

    public static Color? GetLineColor()
    {
        return null;
        //return lineMaterial.GetColor("_Color");
    }

    public static Material GetWeldMaterial()
    {
        return null;
        //return weldMaterial;
    }

    public static Material GetBoltMaterial()
    {
        return null;
        //return boltMaterial;
    }

    public static Material GetBeamMaterial()
    {
        return null;
        //return beamMaterial;
    }

    public static Material GetConnectionMaterial()
    {
        return null;
        //return connectionMaterial;
    }

    /// <summary>
    /// Initialize the lists and variables.
    /// </summary>
    void Awake()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        normals = new List<Vector3>();
        triangles = new List<int>();

        instance = this;
    }

    public static void Initialize()
    {
        vertices = new List<Vector3>();
        uvs = new List<Vector2>();
        normals = new List<Vector3>();
        triangles = new List<int>();

        //instance = this;
    }

    /// <summary>
    /// Clears the lists and resets the quadCount.
    /// </summary>
    public static void Reset()
    {
        vertices.Clear();
        uvs.Clear();
        normals.Clear();
        triangles.Clear();

        quadCount = 0;
    }

    /// <summary>
    /// Uses the data received so far to create a mesh and returns it.
    /// </summary>
    /// <returns></returns>
    public static Mesh Create()
    {
        var mesh = new Mesh();
        mesh.name = "Created Mesh";

        if (vertices.Count >= 65000)
        {
            Debug.Log("Vertices over 65000");
        }

        mesh.vertices = vertices.ToArray();
        //mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //TODO: This methods memory leaks because the previous mesh was released!
        //Change code to use Create(Mesh mesh) instead.

        return mesh;
    }

    public static void Create(Mesh mesh)
    {
        //Huge memory leak, the assign arrays were not being released.
        //Must release in reverse order or Unity will complain at the vertices size
        mesh.uv = null;
        mesh.triangles = null;
        mesh.vertices = null;

        mesh.vertices = vertices.ToArray();
        //mesh.normals = normals.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    /// <summary>
    /// Combines the meshes together but does not edit the originals. Resets the Mesh Creator's internals
    /// </summary>
    /// <param name="meshA"></param>
    /// <param name="meshB"></param>
    /// <returns></returns>
    public static void Combine(Mesh meshA, Mesh meshB)
    {
        MeshCreator.AddVertices(meshA);
        MeshCreator.AddUvs(meshA);
        MeshCreator.AddTriangles(meshA);

        MeshCreator.AddVertices(meshB);
        MeshCreator.AddUvs(meshB);
        MeshCreator.AddTriangles(meshB);

        quadCount += (meshA.vertices.Length / 6) + (meshB.vertices.Length / 6);
    }

    private static void AddVertices(Mesh mesh)
    {
        MeshCreator.vertices.AddRange(mesh.vertices);
    }

    private static void AddUvs(Mesh mesh)
    {
        MeshCreator.uvs.AddRange(mesh.uv);
    }

    private static void AddTriangles(Mesh mesh)
    {
        MeshCreator.triangles.AddRange(mesh.triangles);
    }

    public static GameObject CreateLineObject(string name)
    {
        var obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("Lines");
        var filter = obj.AddComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();
        var render = obj.AddComponent<MeshRenderer>();
        //render.material = lineMaterial;

        return obj;
    }

    public static GameObject CreateLineObject(string name, Material material)
    {
        var obj = new GameObject(name);
        obj.layer = LayerMask.NameToLayer("Lines");
        var filter = obj.AddComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();
        var renderer = obj.AddComponent<MeshRenderer>();
        renderer.material = material;

        return obj;
    }

    /// <summary>
    /// Adds a quad. The quad MUST be defined in clockwise order!
    /// </summary>
    /// <param name="p0"></param>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <param name="p3"></param>
    public static void AddQuad(Vector3 p0, Vector3 p1, Vector3 p2, Vector4 p3, bool flipped = false)
    {
        // Clockwise
        // 2---3
        // |   |
        // 1---0

        if (flipped)
        {
            var points = new Vector3[4]{ p0, p1, p2, p3 };
            p0 = points[3];
            p1 = points[2];
            p2 = points[1];
            p3 = points[0];
        }

        //Create two triangles
        vertices.Add(p0);
        vertices.Add(p1);
        vertices.Add(p2);

        vertices.Add(p2);
        vertices.Add(p3);
        vertices.Add(p0);

        //Add the uvs
        uvs.Add(new Vector2(1f, 0f));
        uvs.Add(new Vector2(0f, 0f));
        uvs.Add(new Vector2(0f, 1f));

        uvs.Add(new Vector2(0f, 1f));
        uvs.Add(new Vector2(1f, 1f));
        uvs.Add(new Vector2(1f, 0f));

        //Add the normals
        var A = p0 - p1;
        var B = p2 - p1;
        var n = Vector3.Cross(B, A);

        normals.Add(n);
        normals.Add(n);
        normals.Add(n);
        normals.Add(n);
        normals.Add(n);
        normals.Add(n);

        //Add the triangles
        triangles.Add(0 + quadCount * 6);
        triangles.Add(1 + quadCount * 6);
        triangles.Add(2 + quadCount * 6);

        triangles.Add(3 + quadCount * 6);
        triangles.Add(4 + quadCount * 6);
        triangles.Add(5 + quadCount * 6);

        quadCount++;
    }

    /// <summary>
    /// Adds a curved shape to the mesh.
    /// </summary>
    /// <param name="bl"></param>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="br"></param>
    /// <param name="bottomPlane"></param>
    /// <param name="topPlane"></param>
    /// <param name="numSegments"></param>
    public static void AddBevel(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bottomPlane, Vector3 topPlane, int numSegments)
    {
        bl -= bottomPlane;
        br -= bottomPlane;
        tl -= topPlane;
        tr -= topPlane;

        for(int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            MeshCreator.AddQuad(Vector3.Slerp(bl, br, ti) + bottomPlane, Vector3.Slerp(tl, tr, ti) + topPlane, Vector3.Slerp(tl, tr, t) + topPlane, Vector3.Slerp(bl, br, t) + bottomPlane);
        }
    }

    /// <summary>
    /// Adds a curved shape with caps to the mesh. 
    /// </summary>
    /// <param name="bl"></param>
    /// <param name="tl"></param>
    /// <param name="tr"></param>
    /// <param name="br"></param>
    /// <param name="bottomPlane"></param>
    /// <param name="topPlane"></param>
    /// <param name="numSegments"></param>
    /// <param name="thickness"></param>
    public static void AddBevelExt(Vector3 bl, Vector3 tl, Vector3 tr, Vector3 br, Vector3 bottomPlane, Vector3 topPlane, int numSegments, float thickness)
    {
        //Outside
        AddBevel(bl, tl, tr, br, bottomPlane, topPlane, numSegments);

        //Prep for inside
        var bl2 = bl + (bottomPlane - bl).normalized * thickness;
        var tl2 = tl + (topPlane - tl).normalized * thickness;
        var br2 = br + (bottomPlane - br).normalized * thickness;
        var tr2 = tr + (topPlane - tr).normalized * thickness;

        //Inside
        AddBevel(br2, tr2, tl2, bl2, bottomPlane, topPlane, numSegments);

        bl -= bottomPlane;
        br -= bottomPlane;
        tl -= topPlane;
        tr -= topPlane;

        bl2 -= bottomPlane;
        br2 -= bottomPlane;
        tl2 -= topPlane;
        tr2 -= topPlane;

        //Add connections in between - Front
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            MeshCreator.AddQuad(Vector3.Slerp(bl2, br2, ti) + bottomPlane, Vector3.Slerp(bl, br, ti) + bottomPlane, Vector3.Slerp(bl, br, t) + bottomPlane, Vector3.Slerp(bl2, br2, t) + bottomPlane);
        }

        //Add connections in between - Back
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            MeshCreator.AddQuad(Vector3.Slerp(bl2, br2, t) + topPlane, Vector3.Slerp(bl, br, t) + topPlane, Vector3.Slerp(bl, br, ti) + topPlane, Vector3.Slerp(bl2, br2, ti) + topPlane);
        }
    }

    /// <summary>
    /// Adds a hollow ring to the mesh. Can be used for rings and cylinders.
    /// </summary>
    /// <param name="h"></param>
    /// <param name="radius"></param>
    /// <param name="numSegments"></param>
    /// <param name="thick"></param>
    public static void AddHollowRing(float h, float radius, int numSegments, float thick)
    {
        //Assume the cylinder is vertical

        //Outer cylinder
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleA) * radius, -h / 2, Mathf.Sin(angleA) * radius), new Vector3(Mathf.Cos(angleA) * radius, h / 2, Mathf.Sin(angleA) * radius),
                new Vector3(Mathf.Cos(angleB) * radius, h / 2, Mathf.Sin(angleB) * radius), new Vector3(Mathf.Cos(angleB) * radius, -h / 2, Mathf.Sin(angleB) * radius));
        }

        //Inner cylinder
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleB) * (radius - thick), -h / 2, Mathf.Sin(angleB) * (radius - thick)), new Vector3(Mathf.Cos(angleB) * (radius - thick), h / 2, Mathf.Sin(angleB) * (radius - thick)), 
                new Vector3(Mathf.Cos(angleA) * (radius - thick), h / 2, Mathf.Sin(angleA) * (radius - thick)), new Vector3(Mathf.Cos(angleA) * (radius - thick), -h / 2, Mathf.Sin(angleA) * (radius - thick)));
        }

        //Top
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleB) * (radius - thick), h / 2, Mathf.Sin(angleB) * (radius - thick)), new Vector3(Mathf.Cos(angleB) * radius, h / 2, Mathf.Sin(angleB) * radius),
                new Vector3(Mathf.Cos(angleA) * radius, h / 2, Mathf.Sin(angleA) * radius), new Vector3(Mathf.Cos(angleA) * (radius - thick), h / 2, Mathf.Sin(angleA) * (radius - thick)));
        }

        //Bottom
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleA) * (radius - thick), -h / 2, Mathf.Sin(angleA) * (radius - thick)), new Vector3(Mathf.Cos(angleA) * radius, -h / 2, Mathf.Sin(angleA) * radius), 
                new Vector3(Mathf.Cos(angleB) * radius, -h / 2, Mathf.Sin(angleB) * radius),new Vector3(Mathf.Cos(angleB) * (radius - thick), -h / 2, Mathf.Sin(angleB) * (radius - thick)));
        }
    }

    public static void AddBolt(float h, float radius, int numSegments = 6)
    {
        //Assume the cylinder is vertical
        var thick = radius;

        //Outer cylinder
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleA) * radius, -h / 2, Mathf.Sin(angleA) * radius), new Vector3(Mathf.Cos(angleA) * radius, h / 2, Mathf.Sin(angleA) * radius),
                new Vector3(Mathf.Cos(angleB) * radius, h / 2, Mathf.Sin(angleB) * radius), new Vector3(Mathf.Cos(angleB) * radius, -h / 2, Mathf.Sin(angleB) * radius));
        }

        //Top
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleB) * (radius - thick), h / 2, Mathf.Sin(angleB) * (radius - thick)), new Vector3(Mathf.Cos(angleB) * radius, h / 2, Mathf.Sin(angleB) * radius),
                new Vector3(Mathf.Cos(angleA) * radius, h / 2, Mathf.Sin(angleA) * radius), new Vector3(Mathf.Cos(angleA) * (radius - thick), h / 2, Mathf.Sin(angleA) * (radius - thick)));
        }

        //Bottom
        for (int i = 0; i < numSegments; ++i)
        {
            float ti = ((float)i / numSegments);
            float t = ((float)(i + 1) / numSegments);

            float angleA = ti * Mathf.PI * 2;
            float angleB = t * Mathf.PI * 2;

            MeshCreator.AddQuad(new Vector3(Mathf.Cos(angleA) * (radius - thick), -h / 2, Mathf.Sin(angleA) * (radius - thick)), new Vector3(Mathf.Cos(angleA) * radius, -h / 2, Mathf.Sin(angleA) * radius),
                new Vector3(Mathf.Cos(angleB) * radius, -h / 2, Mathf.Sin(angleB) * radius), new Vector3(Mathf.Cos(angleB) * (radius - thick), -h / 2, Mathf.Sin(angleB) * (radius - thick)));
        }
    }

    public static void AddBoltAndShank(float headRadius, float headHeight, float shankRadius, float shankLength)
    {
        AddBoltPosition(headHeight, headRadius, new Vector3(0,shankLength/2 + headHeight/2));
        AddBolt(shankLength, shankRadius, 12);
        AddBoltPosition(headHeight, headRadius, new Vector3(0,-shankLength/2 + headHeight/2));
    }

    public static void AddBoltAndShank(float headRadius, float headHeight, float shankRadius, float shankLength, Vector3 translation, Vector3 angles)
    {
        var startIndex = vertices.Count;

        AddBoltPosition(headHeight, headRadius / 0.866025f, new Vector3(0, 0, 0));
        //AddBolt(shankLength, shankRadius, 12);
        AddBoltPosition(shankLength, shankRadius, new Vector3(0, -shankLength / 2 - headHeight / 2, 0));
        AddBoltPosition(headHeight, headRadius / 0.866025f, new Vector3(0, -shankLength - headHeight));

        var endIndex = vertices.Count;

        var rot = Quaternion.Euler(angles);

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            vertices[i] = rot * vertices[i];
            vertices[i] += translation;
        }
    }

    /// <summary>
    /// Add a bolt at that position without translating the other vertices
    /// </summary>
    /// <param name="h"></param>
    /// <param name="radius"></param>
    /// <param name="position"></param>
    public static void AddBoltPosition(float h, float radius, Vector3 translation)
    {
        var startIndex = vertices.Count;

        AddBolt(h, radius);

        var endIndex = vertices.Count;

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            vertices[i] += translation;
        }
    }

    public static void AddBoltPositionAndRotation(float h, float radius, Vector3 translation, Vector3 angles)
    {
        var startIndex = vertices.Count;

        AddBolt(h, radius);

        var endIndex = vertices.Count;

        var rot = Quaternion.Euler(angles);

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            vertices[i] = rot * vertices[i];
            vertices[i] += translation;
        }
    }

    public static void AddLine(Vector3 start, Vector3 end, Vector3 up, float thickness)
    {
        var right = Vector3.Cross(up, start - end).normalized * thickness;

        MeshCreator.AddQuad(start - right, end - right, end + right, start + right);
    }

    public static void AddBezierCurve(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 up, int numSegments, float thickness)
    {
        for(int i = 0; i < numSegments; ++i)
        {
            var start = MeshCreator.GetBezierPoint(p0, p1, p2, p3, ((float)i) /numSegments);
            var end = MeshCreator.GetBezierPoint(p0, p1, p2, p3, ((float)i + 1) / numSegments);

            MeshCreator.AddLine(start, end, up, thickness);
        }
    }

    public static Vector3 GetBezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        return (Mathf.Pow(1 - t, 3) * p0 + 3 * Mathf.Pow(1 - t, 2) * t * p1 + 3 * (1 - t) * Mathf.Pow(t, 2) * p2 + Mathf.Pow(t, 3) * p3);
    }

    public static void DrawBezierCurve(Camera cam, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int numSegments, float size, float zOffset = 0.0f)
    {
        p0.z = cam.nearClipPlane + planeZ + zOffset;
        p1.z = cam.nearClipPlane + planeZ + zOffset;
        p2.z = cam.nearClipPlane + planeZ + zOffset;
        p3.z = cam.nearClipPlane + planeZ + zOffset;

        p0 = cam.ScreenToWorldPoint(p0);
        p1 = cam.ScreenToWorldPoint(p1);
        p2 = cam.ScreenToWorldPoint(p2);
        p3 = cam.ScreenToWorldPoint(p3);

        float thick = cam.orthographicSize / 2.0f / cam.pixelWidth * cam.aspect * size;
        MeshCreator.AddBezierCurve(p0, p1, p2, p3, cam.transform.forward, numSegments, thick);
    }

    public static void DrawBezierCurveWorldSpace(Camera cam, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int numSegments, float size, float zOffset = 0.0f)
    {
        MeshCreator.DrawBezierCurve(cam, cam.WorldToScreenPoint(p0), cam.WorldToScreenPoint(p1), cam.WorldToScreenPoint(p2), cam.WorldToScreenPoint(p3), numSegments, size, zOffset);
    }

    public static void AddDottedLine(Vector3 start, Vector3 end, Vector3 up, float thickness, float segmentLength, float spacing)
    {
        var right = Vector3.Cross(up, start - end).normalized * thickness;
        var lineLength = Vector3.Distance(start, end);
        var segmentDir = (end - start).normalized;
        var totalLength = 0.0f;

        if (segmentLength > 0.0f)
        {
            var i = 0;
            while (totalLength < lineLength)
            {
                if (totalLength + segmentLength <= lineLength)
                {
                    //var s1 = Vector3.Lerp(start, end, (float)i / numSegments);
                    var s1 = start + segmentDir * (segmentLength + spacing) * i;
                    //var e1 = Vector3.Lerp(start, end, (float)(i + 1) / numSegments);
                    var e1 = start + segmentDir * (segmentLength * (i + 1) + (spacing * i));

                    MeshCreator.AddQuad(s1 - right, e1 - right, e1 + right, s1 + right);
                }
                else
                {
                    var cutLength = lineLength - totalLength;
                    //Have to cutof the last bit
                    //var s1 = Vector3.Lerp(start, end, (float)i / numSegments);
                    var s1 = start + segmentDir * (segmentLength + spacing) * i;
                    //var e1 = Vector3.Lerp(start, end, (float)(i + 1) / numSegments);
                    var e1 = start + segmentDir * (((segmentLength + spacing) * i) + cutLength);

                    MeshCreator.AddQuad(s1 - right, e1 - right, e1 + right, s1 + right);
                    break;
                }

                totalLength += (segmentLength + spacing);
                i++;
            }
        }
        

        //for (int i = 0; i < numSegments - 1; ++i)
        //{
        //    //var s1 = Vector3.Lerp(start, end, (float)i / numSegments);
        //    var s1 = start + segmentDir * (segmentLength + spacing) * i;
        //    //var e1 = Vector3.Lerp(start, end, (float)(i + 1) / numSegments);
        //    var e1 = start + segmentDir * (segmentLength * (i + 1) + (spacing * i));

        //    MeshCreator.AddQuad(s1 - right, e1 - right, e1 + right, s1 + right);
        //}
    }

    /// <summary>
    /// Moves all the vertices a set amount
    /// </summary>
    /// <param name="translation"></param>
    public static void Translate(Vector3 translation)
    {
        for (int i = 0; i < vertices.Count; ++i)
        {
            vertices[i] += translation;
        }
    }

    /// <summary>
    /// Rotates the vertices around the origin. Note: Rotate before moving.
    /// </summary>
    /// <param name="angles"></param>
    public static void Rotate(Vector3 angles)
    {
        var rot = Quaternion.Euler(angles);

        for(int i = 0; i < vertices.Count; ++i)
        {
            vertices[i] = rot * vertices[i];
        }
    }

    /// <summary>
    /// Rotates the vertices around the origin. Note: Rotate before moving.
    /// </summary>
    /// <param name="pitch"></param>
    /// <param name="yaw"></param>
    /// <param name="roll"></param>
    public static void Rotate(float pitch, float yaw, float roll)
    {
        MeshCreator.Rotate(new Vector3(pitch, yaw, roll));
    }

    /// <summary>
    /// Scales the vertices by the vector3 around the origin.
    /// </summary>
    /// <param name="scale"></param>
    public static void Scale(Vector3 scale)
    {
        for (int i = 0; i < vertices.Count; ++i)
        {
            var vert = vertices[i];

            vert = new Vector3(vert.x * scale.x, vert.y * scale.y, vert.z * scale.z);

            vertices[i] = vert;
        }
    }

    public static void Scale(float scale)
    {
        MeshCreator.Scale(new Vector3(scale, scale, scale));
    }

    public static void Translate(Mesh mesh, Vector3 translation)
    {
        var vertices = mesh.vertices;

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] += translation;
        }

        //Assign it back
        mesh.vertices = vertices;
    }

    public static void Rotate(Mesh mesh, Vector3 angles)
    {
        var vertices = mesh.vertices;
        var rot = Quaternion.Euler(angles);

        for (int i = 0; i < vertices.Length; ++i)
        {
            vertices[i] = rot * vertices[i];
        }

        //Assign it back
        mesh.vertices = vertices;
    }

    /// <summary>
    /// Rotates the vertices around the origin. Note: Rotate before moving.
    /// </summary>
    /// <param name="pitch"></param>
    /// <param name="yaw"></param>
    /// <param name="roll"></param>
    public static void Rotate(double pitch, double yaw, double roll)
    {
        MeshCreator.Rotate((float)pitch, (float)yaw, (float)roll);
    }

    public static void DrawLineObject(Mesh mesh, Vector3[] verts, Vector3 position, Quaternion rotation, Camera cam, float lineSize, float zOffset = 0.0f)
    {
        MeshCreator.Reset();

        //Rotate the verts by the object
        for (int i = 0; i < verts.Length; ++i)
        {
            verts[i] = (rotation * verts[i]) + position;
        }

        for (int i = 0; i < verts.Length / 6; ++i)
        {
            DrawEdge(cam, cam.WorldToScreenPoint(verts[i * 6 + 0]), cam.WorldToScreenPoint(verts[i * 6 + 1]), lineSize, zOffset);
            DrawEdge(cam, cam.WorldToScreenPoint(verts[i * 6 + 1]), cam.WorldToScreenPoint(verts[i * 6 + 2]), lineSize, zOffset);
            DrawEdge(cam, cam.WorldToScreenPoint(verts[i * 6 + 3]), cam.WorldToScreenPoint(verts[i * 6 + 4]), lineSize, zOffset);
            DrawEdge(cam, cam.WorldToScreenPoint(verts[i * 6 + 4]), cam.WorldToScreenPoint(verts[i * 6 + 0]), lineSize, zOffset);
        }

        MeshCreator.Create(mesh);
    }

    //public static void DrawCustomLineObject(Mesh mesh, MemberControl memberControl, Vector3 position, Quaternion rotation, Camera cam, float lineSize, ViewMask mask, float zOffset = 0.0f)
    //{
    //    MeshCreator.Reset();

    //    //Draw the lines from the positions
    //    foreach(var line in memberControl.customLines[mask])
    //    {
    //        switch (line.lineType)
    //        {
    //            case LineType.NORMAL:
    //                DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
    //                break;

    //            case LineType.DOTTED:
    //                DrawDottedEdgeWorldSpace(cam, line.p0, line.p1, lineSize, line.segmentSize, line.spacingSize, zOffset);
    //                break;

    //            case LineType.BEZIER:
    //                DrawBezierCurveWorldSpace(cam, line.p0, line.p1, line.p2, line.p3, 32, lineSize, zOffset);
    //                break;

    //            case LineType.INTERSECT:
    //                DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
    //                DrawDottedEdgeWorldSpace(cam, line.p1, line.p2, lineSize, line.segmentSize, line.spacingSize, zOffset);
    //                DrawEdgeWorldSpace(cam, line.p2, line.p3, lineSize, zOffset);
    //                break;
    //        }
    //    }

    //    MeshCreator.Create(mesh);
    //}

    /// <summary>
    /// Takes in a world to screen point vectors
    /// </summary>
    /// <param name="cam"></param>
    /// <param name="pointA"></param>
    /// <param name="pointB"></param>
    /// <param name="size"></param>
    public static void DrawEdge(Camera cam, Vector3 pointA, Vector3 pointB, float size, float zOffset = 0.0f)
    {
        //pointA.y = Screen.height - pointA.y;
        //pointB.y = Screen.height - pointB.y;

        pointA.z = cam.nearClipPlane + planeZ + zOffset;
        pointB.z = cam.nearClipPlane + planeZ + zOffset;

        pointA = cam.ScreenToWorldPoint(pointA);
        pointB = cam.ScreenToWorldPoint(pointB);

        //Drawing.DrawLine(pointA, pointB, Color.green, 0.1f, false);
        //float thick = cam.pixelWidth / (cam.aspect * 2 * cam.orthographicSize) * size;
        //float thick = size;// Screen.height / (2) * size;
        float thick = cam.orthographicSize / 2.0f / cam.pixelWidth * cam.aspect * size;
        MeshCreator.AddLine(pointA, pointB, cam.transform.forward, thick);
    }

    public static void DrawEdgeWorldSpace(Camera cam, Vector3 pointA, Vector3 pointB, float size, float zOffset = 0.0f)
    {
        MeshCreator.DrawEdge(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB), size, zOffset);
    }

    public static void DrawDottedEdge(Camera cam, Vector3 pointA, Vector3 pointB, float size, float segmentLength, float spacing, float zOffset = 0.0f)
    {
        pointA.z = cam.nearClipPlane + planeZ + zOffset;
        pointB.z = cam.nearClipPlane + planeZ + zOffset;

        pointA = cam.ScreenToWorldPoint(pointA);
        pointB = cam.ScreenToWorldPoint(pointB);

        float thick = cam.orthographicSize / 2.0f / cam.pixelWidth * cam.aspect * size;
        MeshCreator.AddDottedLine(pointA, pointB, cam.transform.forward, thick, segmentLength, spacing);
    }

    public static void DrawDottedEdgeWorldSpace(Camera cam, Vector3 pointA, Vector3 pointB, float size, float segmentLength, float spacing, float zOffset = 0.0f)
    {
        MeshCreator.DrawDottedEdge(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB), size, segmentLength, spacing, zOffset);
    }

    public static void DrawArrow(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize)
    {
        //Fixes the arrows getting smaller
        pointA.z = 0;
        pointB.z = 0;

        var arrowLength = 10f;
        var arrowAngle = 15.0f;
        var direction = (pointB - pointA).normalized;

        var lengthA = Quaternion.Euler(new Vector3(0, 0, arrowAngle)) * direction;
        var lengthB = Quaternion.Euler(new Vector3(0, 0, -arrowAngle)) * direction;

        lengthA *= arrowLength;
        lengthB *= arrowLength;

        MeshCreator.DrawEdge(cam, pointA, pointB, lineSize);
        //Arrow part
        MeshCreator.DrawEdge(cam, pointA + lengthA, pointA, lineSize);
        MeshCreator.DrawEdge(cam, pointA + lengthB, pointA, lineSize);
    }



}
