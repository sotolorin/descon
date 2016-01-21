using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class that handles the creation of meshes. This script must be attached to a gameObject in the scene to work.
/// </summary>
public class MeshCreator : MonoBehaviour
{
    private static MeshCreator instance;
    private List<Vector3> vertices;
    private List<Vector2> uvs;
    private List<Vector3> normals;
    private List<int> triangles;
    private int quadCount = 0;

    public Material lineMaterial;
    public Material weldMaterial;
    public Material boltMaterial;
    public Material beamMaterial;
    public Material connectionMaterial;

    private static float planeZ = 1f;
    public static float textOffset = 0.5f;

    public static float GetZPlaneOffset()
    {
        return planeZ;
    }

    public static void SetLineColor(Color c)
    {
        instance.lineMaterial.SetColor("_Color", c);
    }

    public static Color GetLineColor()
    {
        return instance.lineMaterial.GetColor("_Color");
    }

    public static Material GetWeldMaterial()
    {
        return instance.weldMaterial;
    }

    public static Material GetBoltMaterial()
    {
        return instance.boltMaterial;
    }

    public static Material GetBeamMaterial()
    {
        return instance.beamMaterial;
    }

    public static Material GetConnectionMaterial()
    {
        return instance.connectionMaterial;
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

    /// <summary>
    /// Clears the lists and resets the quadCount.
    /// </summary>
    public static void Reset()
    {
        instance.vertices.Clear();
        instance.uvs.Clear();
        instance.normals.Clear();
        instance.triangles.Clear();

        instance.quadCount = 0;
    }

    /// <summary>
    /// Uses the data received so far to create a mesh and returns it.
    /// </summary>
    /// <returns></returns>
    public static Mesh Create()
    {
        var mesh = new Mesh();
        mesh.name = "Created Mesh";

        if (instance.vertices.Count >= 65000)
        {
            Debug.Log("Vertices over 65000");
        }

        mesh.vertices = instance.vertices.ToArray();
        //mesh.normals = instance.normals.ToArray();
        mesh.uv = instance.uvs.ToArray();
        mesh.triangles = instance.triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        //TODO: This methods memory leaks because the previous mesh was released!
        //Change code to use Create(Mesh mesh) instead.

        return mesh;
    }

    public static Mesh Combine(Mesh[] meshes)
    {
        for(int i = 0; i < meshes.Length; ++i)
        {
            Add(meshes[i]);
        }

        return Create();
    }

    public static void Create(Mesh mesh)
    {
        //Huge memory leak, the assign arrays were not being released.
        //Must release in reverse order or Unity will complain at the vertices size
        mesh.uv = null;
        mesh.triangles = null;
        mesh.vertices = null;

        mesh.vertices = instance.vertices.ToArray();
        //mesh.normals = instance.normals.ToArray();
        mesh.uv = instance.uvs.ToArray();
        mesh.triangles = instance.triangles.ToArray();

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }

    public static void Add(Mesh mesh)
    {
        if (mesh != null)
        {
            MeshCreator.AddVertices(mesh);
            MeshCreator.AddUvs(mesh);
            MeshCreator.AddTriangles(mesh);

            instance.quadCount += (mesh.vertices.Length / 6);
        }
    }

    public static void Add(Mesh[] meshes)
    {
        foreach (var mesh in meshes)
        {
            Add(mesh);
        }
    }

    private static void AddVertices(Mesh mesh)
    {
        MeshCreator.instance.vertices.AddRange(mesh.vertices);
    }

    private static void AddUvs(Mesh mesh)
    {
        MeshCreator.instance.uvs.AddRange(mesh.uv);
    }

    private static void AddTriangles(Mesh mesh)
    {
        var tris = mesh.triangles;
        var len = tris.Length;

        for (int i = 0; i < tris.Length; ++i)
        {
            tris[i] += instance.quadCount * 6;
        }
        
        MeshCreator.instance.triangles.AddRange(tris);
    }

    public static GameObject CreateLineObject(string name)
    {
        var obj = new GameObject(name);
        obj.hideFlags = HideFlags.HideAndDontSave;
        obj.layer = LayerMask.NameToLayer("Lines");
        var filter = obj.AddComponent<MeshFilter>();
        filter.sharedMesh = new Mesh();
        var render = obj.AddComponent<MeshRenderer>();
        render.material = instance.lineMaterial;

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
        instance.vertices.Add(p0);
        instance.vertices.Add(p1);
        instance.vertices.Add(p2);

        instance.vertices.Add(p2);
        instance.vertices.Add(p3);
        instance.vertices.Add(p0);

        //Add the uvs
        instance.uvs.Add(new Vector2(1f, 0f));
        instance.uvs.Add(new Vector2(0f, 0f));
        instance.uvs.Add(new Vector2(0f, 1f));

        instance.uvs.Add(new Vector2(0f, 1f));
        instance.uvs.Add(new Vector2(1f, 1f));
        instance.uvs.Add(new Vector2(1f, 0f));

        //Add the normals
        var A = p0 - p1;
        var B = p2 - p1;
        var n = Vector3.Cross(B, A);

        instance.normals.Add(n);
        instance.normals.Add(n);
        instance.normals.Add(n);
        instance.normals.Add(n);
        instance.normals.Add(n);
        instance.normals.Add(n);

        //Add the triangles
        instance.triangles.Add(0 + instance.quadCount * 6);
        instance.triangles.Add(1 + instance.quadCount * 6);
        instance.triangles.Add(2 + instance.quadCount * 6);

        instance.triangles.Add(3 + instance.quadCount * 6);
        instance.triangles.Add(4 + instance.quadCount * 6);
        instance.triangles.Add(5 + instance.quadCount * 6);

        instance.quadCount++;
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

    public static Mesh CreateInvertedCorner(Vector3 position, Vector3 rotation, int numSegments, float radius, float thickness)
    {
        MeshCreator.Reset();

        //Create a semi circle shape, starting in the bottom left
        for (int i = 0; i < numSegments; ++i)
        {
            float t1 = ((float)i / numSegments) * 90.0f * Mathf.Deg2Rad;
            float t2 = ((float)(i + 1) / numSegments) * 90.0f * Mathf.Deg2Rad;

            MeshCreator.AddQuad(new Vector3(0, 0, -thickness / 2), new Vector3(0, 0, -thickness / 2), new Vector3(radius - Mathf.Cos(t1) * radius, radius - Mathf.Sin(t1) * radius, -thickness / 2),
                new Vector3(radius - Mathf.Cos(t2) * radius, radius - Mathf.Sin(t2) * radius, -thickness / 2));

            MeshCreator.AddQuad(new Vector3(0, 0, thickness / 2), new Vector3(0, 0, thickness / 2), new Vector3(radius - Mathf.Cos(t1) * radius, radius - Mathf.Sin(t1) * radius, thickness / 2),
                new Vector3(radius - Mathf.Cos(t2) * radius, radius - Mathf.Sin(t2) * radius, thickness / 2), true);

            //Bridge the gap
            MeshCreator.AddQuad(new Vector3(radius - Mathf.Cos(t1) * radius, radius - Mathf.Sin(t1) * radius, -thickness / 2), new Vector3(radius - Mathf.Cos(t1) * radius, radius - Mathf.Sin(t1) * radius, thickness / 2),
                new Vector3(radius - Mathf.Cos(t2) * radius, radius - Mathf.Sin(t2) * radius, thickness / 2), new Vector3(radius - Mathf.Cos(t2) * radius, radius - Mathf.Sin(t2) * radius, -thickness / 2));
        }

        MeshCreator.Rotate(rotation);
        MeshCreator.Translate(position);

        return MeshCreator.Create();
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
        var startIndex = instance.vertices.Count;

        AddBoltPosition(headHeight, headRadius / 0.866025f, new Vector3(0, 0, 0));
        AddBoltPosition(shankLength, shankRadius / 0.866025f, new Vector3(0, -shankLength / 2 - headHeight / 2, 0));
        AddBoltPosition(headHeight, headRadius / 0.866025f, new Vector3(0, -shankLength - headHeight));

        var endIndex = instance.vertices.Count;

        var rot = Quaternion.Euler(angles);

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            instance.vertices[i] = rot * instance.vertices[i];
            instance.vertices[i] += translation;
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
        var startIndex = instance.vertices.Count;

        AddBolt(h, radius);

        var endIndex = instance.vertices.Count;

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            instance.vertices[i] += translation;
        }
    }

    public static void AddBoltPositionAndRotation(float h, float radius, Vector3 translation, Vector3 angles)
    {
        var startIndex = instance.vertices.Count;

        AddBolt(h, radius);

        var endIndex = instance.vertices.Count;

        var rot = Quaternion.Euler(angles);

        //Translate specific set
        for (int i = startIndex; i < endIndex; ++i)
        {
            instance.vertices[i] = rot * instance.vertices[i];
            instance.vertices[i] += translation;
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
        for (int i = 0; i < instance.vertices.Count; ++i)
        {
            instance.vertices[i] += translation;
        }
    }

    /// <summary>
    /// Rotates the vertices around the origin. Note: Rotate before moving.
    /// </summary>
    /// <param name="angles"></param>
    public static void Rotate(Vector3 angles)
    {
        var rot = Quaternion.Euler(angles);

        for(int i = 0; i < instance.vertices.Count; ++i)
        {
            instance.vertices[i] = rot * instance.vertices[i];
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
        for (int i = 0; i < instance.vertices.Count; ++i)
        {
            var vert = instance.vertices[i];

            vert = new Vector3(vert.x * scale.x, vert.y * scale.y, vert.z * scale.z);

            instance.vertices[i] = vert;
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

    public static void DrawLineObject(Mesh lineMesh, Mesh sourceMesh, Vector3 position, Quaternion rotation, Camera cam, float lineSize, float zOffset = 0.0f)
    {
        MeshCreator.Reset();

        var verts = sourceMesh.vertices;

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

        MeshCreator.Create(lineMesh);
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

    public static void DrawCustomLineObject(Mesh mesh, MemberControl memberControl, Vector3 position, Quaternion rotation, Camera cam, float lineSize, ViewMask mask, float zOffset = 0.0f)
    {
        MeshCreator.Reset();

        //Draw the lines from the positions
        foreach(var line in memberControl.customLines[mask])
        {
            switch (line.lineType)
            {
                case LineType.NORMAL:
                    DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    break;

                case LineType.DOTTED:
                    DrawDottedEdgeWorldSpace(cam, line.p0, line.p1, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    break;

                case LineType.BEZIER:
                    DrawBezierCurveWorldSpace(cam, line.p0, line.p1, line.p2, line.p3, 32, lineSize, zOffset);
                    break;

                case LineType.INTERSECT:
                    DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    DrawDottedEdgeWorldSpace(cam, line.p1, line.p2, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    DrawEdgeWorldSpace(cam, line.p2, line.p3, lineSize, zOffset);
                    break;
            }
        }

        MeshCreator.Create(mesh);
    }

    public static void DrawCustomLineObject(Mesh mesh, ConnectionControl connControl, Vector3 position, Quaternion rotation, Camera cam, float lineSize, ViewMask mask, float zOffset = 0.0f)
    {
        MeshCreator.Reset();

        //Draw the lines from the positions
        foreach (var line in connControl.customLines[mask])
        {
            switch (line.lineType)
            {
                case LineType.NORMAL:
                    DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    break;

                case LineType.DOTTED:
                    DrawDottedEdgeWorldSpace(cam, line.p0, line.p1, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    break;

                case LineType.BEZIER:
                    DrawBezierCurveWorldSpace(cam, line.p0, line.p1, line.p2, line.p3, 32, lineSize, zOffset);
                    break;

                case LineType.INTERSECT:
                    DrawEdgeWorldSpace(cam, line.p0, line.p1, lineSize, zOffset);
                    DrawDottedEdgeWorldSpace(cam, line.p1, line.p2, lineSize, line.segmentSize, line.spacingSize, zOffset);
                    DrawEdgeWorldSpace(cam, line.p2, line.p3, lineSize, zOffset);
                    break;
            }
        }

        MeshCreator.Create(mesh);
    }

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

    public static void DrawArrowWorldSpace(Camera cam, Vector3 pointA, Vector3 pointB, float size)
    {
        MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB), size);
    }

    public static void DrawArrowithLine(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize, float lineLength)
    {
        MeshCreator.DrawArrow(cam, pointA, pointB + cam.transform.right * lineLength, lineSize);
        MeshCreator.DrawEdge(cam, pointB, pointB + cam.transform.right * lineLength, lineSize);
    }

    public static void DrawArrowLineWorld(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize, float textLength, bool leftSize, float verticalTextOffset)
    {
        var lineOffset = new Vector3(0, -verticalTextOffset, 0);
        var textOffset = Vector3.right * (textLength * MessageQueueTest.GetFontSize()) / 2.0f;
        var lineLength = Vector3.right * (textLength);

        if (leftSize)
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + lineOffset, lineSize);
            lineLength *= -1;
        }
        else
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + lineOffset, lineSize);
        }

        MeshCreator.DrawEdge(cam, cam.WorldToScreenPoint(pointB) + lineOffset, cam.WorldToScreenPoint(pointB) + lineLength + lineOffset, lineSize);
    }

    public static void DrawLineWorld(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize, float textLength, bool leftSize, float verticalTextOffset)
    {
        var lineOffset = new Vector3(0, -verticalTextOffset, 0);
        var textOffset = Vector3.right * (textLength * MessageQueueTest.GetFontSize()) / 2.0f;
        var lineLength = Vector3.right * (textLength);

        if (leftSize)
            lineLength *= -1;

        MeshCreator.DrawEdge(cam, cam.WorldToScreenPoint(pointB) + lineOffset, cam.WorldToScreenPoint(pointB) + lineLength + lineOffset, lineSize);
    }

    public static void DrawWeldArrowLineWorld(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize, float textLength, bool leftSize, float verticalTextOffset, int weldType = 0)
    {
        var lineOffset = new Vector3(0, verticalTextOffset * 0.1f, 0);
        var lineLength = Vector3.right * (textLength);
        var segmentLengthLeft = Vector3.left * 20;
        var segmentLengthRight = Vector3.right * 70;

        if (leftSize)
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + segmentLengthRight, lineSize);
        }
        else
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + segmentLengthLeft, lineSize);
        }
    }

    public static void DrawWeldLineWorld(Camera cam, Vector3 pointA, Vector3 pointB, float lineSize, float textLength, bool leftSize, float verticalTextOffset, int weldType = 0)
    {
        var lineOffset = new Vector3(0, verticalTextOffset * 0.1f, 0);
        var lineLength = Vector3.right * (textLength);
        var segmentLengthLeft = Vector3.left * 20;
        var segmentLengthRight = Vector3.right * 70;

        if (leftSize)
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + segmentLengthRight, lineSize);
        }
        else
        {
            MeshCreator.DrawArrow(cam, cam.WorldToScreenPoint(pointA), cam.WorldToScreenPoint(pointB) + segmentLengthLeft, lineSize);
        }

        MeshCreator.DrawEdge(cam, cam.WorldToScreenPoint(pointB) + segmentLengthLeft, cam.WorldToScreenPoint(pointB) + segmentLengthRight, lineSize);

        var leftPoint = cam.WorldToScreenPoint(pointB) + segmentLengthLeft;
        var rightPoint = cam.WorldToScreenPoint(pointB) + segmentLengthRight;

        var triangleSize = 20;
        //Draw a triangle

        var trianglePoint = cam.WorldToScreenPoint(pointB) + Vector3.right * 25;

        switch(weldType)
        {
            case 0:
                //Top Triangle
                MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, triangleSize, 0), lineSize);

                //Bottom Triangle
                MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            break;

            case 4:
                //Bottom Triangle
                MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);

                if (!leftSize)
                {
                    MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                    MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);
                }
                else
                {
                    MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                    MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);
                }
            break;

            case 5:
                //Top Triangle
                MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, triangleSize, 0), lineSize);

                //Bottom Triangle
                MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);

                //Extra lines
                if (!leftSize)
                {
                    MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                    MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);
                }
                else
                {
                    MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                    MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);
                }

            break;

            case 6:
            //Top Square
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(-triangleSize / 2, 0, 0), trianglePoint + new Vector3(-triangleSize / 2, triangleSize / 4, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(-triangleSize / 2, triangleSize / 4, 0), trianglePoint + new Vector3(triangleSize / 2, triangleSize / 4, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize / 2, 0, 0), trianglePoint + new Vector3(triangleSize / 2, triangleSize / 4, 0), lineSize);

            //Bottom Triangle
            MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(0, 0, 0), trianglePoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);

            //Extra lines
            if (!leftSize)
            {
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);

                //Flag
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint + new Vector3(0, triangleSize * 2, 0), lineSize);

                MeshCreator.DrawEdge(cam, leftPoint + new Vector3(triangleSize, triangleSize * 1.5f), leftPoint + new Vector3(0, triangleSize * 2, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint + new Vector3(triangleSize, triangleSize * 1.5f), leftPoint + new Vector3(0, triangleSize, 0), lineSize);
            }
            else
            {
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);

                //Flag
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(0, triangleSize * 2, 0), lineSize);

                MeshCreator.DrawEdge(cam, rightPoint + new Vector3(-triangleSize, triangleSize * 1.5f), rightPoint + new Vector3(0, triangleSize * 2, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint + new Vector3(-triangleSize, triangleSize * 1.5f), rightPoint + new Vector3(0, triangleSize, 0), lineSize);
            }

            break;

            case 7:

            //Top Square
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(-triangleSize / 2, 0, 0), trianglePoint + new Vector3(-triangleSize / 2, triangleSize / 4, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(-triangleSize / 2, triangleSize / 4, 0), trianglePoint + new Vector3(triangleSize / 2, triangleSize / 4, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize / 2, 0, 0), trianglePoint + new Vector3(triangleSize / 2, triangleSize / 4, 0), lineSize);

            //Bottom Triangle
            MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(0, 0, 0), trianglePoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);

            //Extra lines
            if (!leftSize)
            {
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);
            }
            else
            {
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);
            }


            break;

            case 8:
            //Bottom Triangle
            MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);

            //Extra lines
            if (!leftSize)
            {
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);

                //Flag
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint + new Vector3(0, triangleSize * 2, 0), lineSize);

                MeshCreator.DrawEdge(cam, leftPoint + new Vector3(triangleSize, triangleSize * 1.5f), leftPoint + new Vector3(0, triangleSize * 2, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint + new Vector3(triangleSize, triangleSize * 1.5f), leftPoint + new Vector3(0, triangleSize, 0), lineSize);
            }
            else
            {
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);

                //Flag
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(0, triangleSize * 2, 0), lineSize);

                MeshCreator.DrawEdge(cam, rightPoint + new Vector3(-triangleSize, triangleSize * 1.5f), rightPoint + new Vector3(0, triangleSize * 2, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint + new Vector3(-triangleSize, triangleSize * 1.5f), rightPoint + new Vector3(0, triangleSize, 0), lineSize);
            }
            break;

            case 9:
            //Triangle on left
            //Semi circle shape for HSS

            //Bottom Triangle
            MeshCreator.DrawEdge(cam, trianglePoint, trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            //MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize, 0, 0), trianglePoint + new Vector3(0, -triangleSize, 0), lineSize);
            for (int i = 0; i < 8; ++i)
            {
                float a1 = i / 8.0f * 90.0f * Mathf.Deg2Rad;
                float a2 = (i + 1) / 8.0f * 90.0f * Mathf.Deg2Rad;

                MeshCreator.DrawEdge(cam, trianglePoint + new Vector3(triangleSize + 4 - triangleSize * Mathf.Cos(a1), -triangleSize * Mathf.Sin(a1), 0), trianglePoint + 
                    new Vector3(triangleSize + 4 - triangleSize * Mathf.Cos(a2), -triangleSize * Mathf.Sin(a2), 0), lineSize);
            }

            //Extended text lines
            if (!leftSize)
            {
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, rightPoint, rightPoint + new Vector3(triangleSize, -triangleSize, 0), lineSize);
            }
            else
            {
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, triangleSize, 0), lineSize);
                MeshCreator.DrawEdge(cam, leftPoint, leftPoint - new Vector3(triangleSize, -triangleSize, 0), lineSize);
            }

            break;
        }
    }

    /// <summary>
    /// Create a box mesh that can be used for simple shapes.
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="dim"></param>
    /// <param name="angles"></param>
    /// <param name="additionalTranslation"></param>
    /// <returns></returns>
    public static Mesh CreateBoxMesh(Vector3 translation, Vector3 dim, Vector3 angles, Vector3? additionalTranslation = null)
    {
        dim /= 2.0f;

        MeshCreator.Reset();

        MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(dim.x, -dim.y, -dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, dim.z), new Vector3(-dim.x, -dim.y, -dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, -dim.y, dim.z), new Vector3(dim.x, -dim.y, dim.z), new Vector3(dim.x, dim.y, dim.z), new Vector3(-dim.x, dim.y, dim.z));
        MeshCreator.AddQuad(new Vector3(-dim.x, dim.y, -dim.z), new Vector3(dim.x, dim.y, -dim.z), new Vector3(dim.x, -dim.y, -dim.z), new Vector3(-dim.x, -dim.y, -dim.z));

        MeshCreator.Rotate(angles);
        MeshCreator.Translate(translation);
        if (additionalTranslation != null) MeshCreator.Translate((Vector3)additionalTranslation);
        return MeshCreator.Create();
    }

    public static Mesh CreateWedgePlate(Vector3 translation, float clip, float smallLength, float slant, float widthA, float width, float thickness, Vector3 angles)
    {
        MeshCreator.Reset();

        //     |---length--|
        //       _slength_
        // slant/        \ clip
        //     /          |
        //     |          |
        //widthA          | smallWidth
        //     |          |
        //     \          |
        //      \________/

        var smallWidth = width - clip * 2;

        //First create the top back part
        MeshCreator.AddQuad(new Vector3(-clip, thickness / 2, width / 2), new Vector3(0, thickness / 2, smallWidth / 2), new Vector3(0, thickness / 2, -smallWidth / 2), new Vector3(-clip, thickness / 2, -width / 2));
        //Create top middle selection
        MeshCreator.AddQuad(new Vector3(-smallLength, thickness / 2, width / 2), new Vector3(-clip, thickness / 2, width / 2), new Vector3(-clip, thickness / 2, -width / 2), new Vector3(-smallLength, thickness / 2, -width / 2));
        //Create top last piece
        MeshCreator.AddQuad(new Vector3(-smallLength - slant, thickness / 2, widthA / 2), new Vector3(-smallLength, thickness / 2, width / 2), new Vector3(-smallLength, thickness / 2, -width / 2), new Vector3(-smallLength - slant, thickness / 2, -widthA / 2));

        //Flip the pieces upside down
        MeshCreator.AddQuad(new Vector3(-clip, -thickness / 2, width / 2), new Vector3(0, -thickness / 2, smallWidth / 2), new Vector3(0, -thickness / 2, -smallWidth / 2), new Vector3(-clip, -thickness / 2, -width / 2), true);
        MeshCreator.AddQuad(new Vector3(-smallLength, -thickness / 2, width / 2), new Vector3(-clip, -thickness / 2, width / 2), new Vector3(-clip, -thickness / 2, -width / 2), new Vector3(-smallLength, -thickness / 2, -width / 2), true);
        MeshCreator.AddQuad(new Vector3(-smallLength - slant, -thickness / 2, widthA / 2), new Vector3(-smallLength, -thickness / 2, width / 2), new Vector3(-smallLength, -thickness / 2, -width / 2), new Vector3(-smallLength - slant, -thickness / 2, -widthA / 2), true);

        //Bridge the gap
        MeshCreator.AddQuad(new Vector3(-smallLength, thickness / 2, width / 2), new Vector3(-clip, thickness / 2, width / 2), new Vector3(-clip, -thickness / 2, width / 2), new Vector3(-smallLength, -thickness / 2, width / 2), true);
        MeshCreator.AddQuad(new Vector3(-smallLength, thickness / 2, -width / 2), new Vector3(-clip, thickness / 2, -width / 2), new Vector3(-clip, -thickness / 2, -width / 2), new Vector3(-smallLength, -thickness / 2, -width / 2));

        //Bridge slant gaps
        MeshCreator.AddQuad(new Vector3(-smallLength - slant, -thickness / 2, -widthA / 2), new Vector3(-smallLength - slant, thickness / 2, -widthA / 2), new Vector3(-smallLength, thickness / 2, -width / 2), new Vector3(-smallLength, -thickness / 2, -width / 2));
        MeshCreator.AddQuad(new Vector3(-smallLength - slant, -thickness / 2, widthA / 2), new Vector3(-smallLength - slant, thickness / 2, widthA / 2), new Vector3(-smallLength, thickness / 2, width / 2), new Vector3(-smallLength, -thickness / 2, width / 2), true);

        //Briddge widthA gap
        MeshCreator.AddQuad(new Vector3(-smallLength - slant, -thickness / 2, -widthA / 2), new Vector3(-smallLength - slant, -thickness / 2, widthA / 2), new Vector3(-smallLength - slant, thickness / 2, widthA / 2), new Vector3(-smallLength - slant, thickness / 2, -widthA / 2));

        //Bridge clip gap
        MeshCreator.AddQuad(new Vector3(-clip, thickness / 2, width / 2), new Vector3(-clip, -thickness / 2, width / 2), new Vector3(0, -thickness / 2, smallWidth / 2), new Vector3(0, thickness / 2, smallWidth / 2));
        MeshCreator.AddQuad(new Vector3(-clip, thickness / 2, -width / 2), new Vector3(-clip, -thickness / 2, -width / 2), new Vector3(0, -thickness / 2, -smallWidth / 2), new Vector3(0, thickness / 2, -smallWidth / 2), true);

        //Bridge smallWidth gap
        MeshCreator.AddQuad(new Vector3(0, thickness / 2, smallWidth / 2), new Vector3(0, -thickness / 2, smallWidth / 2), new Vector3(0, -thickness / 2, -smallWidth / 2), new Vector3(0, thickness / 2, -smallWidth / 2));


        MeshCreator.Rotate(angles);
        MeshCreator.Translate(translation);

        return MeshCreator.Create();
    }

    /// <summary>
    /// Create a skewed box between the two points
    /// </summary>
    /// <param name="translation"></param>
    /// <param name="leftPoint"></param>
    /// <param name="rightPoint"></param>
    /// <param name="thickness"></param>
    /// <param name="depth"></param>
    /// <param name="angles"></param>
    /// <param name="additionalTranslation"></param>
    /// <returns></returns>
    public static Mesh CreateSkewedBoxMesh(Vector3 translation, Vector3 leftPoint, Vector3 rightPoint, float thickness, float depth, Vector3 angles, Vector3? additionalTranslation = null)
    {
        thickness /= 2.0f;
        depth /= 2.0f;

        if (leftPoint.x > rightPoint.x)
        {
            var temp = leftPoint;
            leftPoint = rightPoint;
            rightPoint = temp;
        }

        MeshCreator.Reset();

        //Top/bottom
        MeshCreator.AddQuad(leftPoint + new Vector3(0, thickness, -depth), leftPoint + new Vector3(0, thickness, depth), rightPoint + new Vector3(0, thickness, depth), rightPoint + new Vector3(0, thickness, -depth));
        MeshCreator.AddQuad(leftPoint + new Vector3(0, -thickness, -depth), leftPoint + new Vector3(0, -thickness, depth), rightPoint + new Vector3(0, -thickness, depth), rightPoint + new Vector3(0, -thickness, -depth), true);

        //Left/Right
        MeshCreator.AddQuad(leftPoint + new Vector3(0, -thickness, -depth), leftPoint + new Vector3(0, -thickness, depth), leftPoint + new Vector3(0, thickness, depth), leftPoint + new Vector3(0, thickness, -depth));
        MeshCreator.AddQuad(rightPoint + new Vector3(0, -thickness, -depth), rightPoint + new Vector3(0, -thickness, depth), rightPoint + new Vector3(0, thickness, depth), rightPoint + new Vector3(0, thickness, -depth), true);

        //Front/back
        MeshCreator.AddQuad(leftPoint + new Vector3(0, -thickness, -depth), leftPoint + new Vector3(0, thickness, -depth), rightPoint + new Vector3(0, thickness, -depth), rightPoint + new Vector3(0, -thickness, -depth));
        MeshCreator.AddQuad(leftPoint + new Vector3(0, -thickness, depth), leftPoint + new Vector3(0, thickness, depth), rightPoint + new Vector3(0, thickness, depth), rightPoint + new Vector3(0, -thickness, depth), true);

        MeshCreator.Rotate(angles);
        MeshCreator.Translate(translation);
        if (additionalTranslation != null) MeshCreator.Translate((Vector3)additionalTranslation);
        return MeshCreator.Create();
    }
}
