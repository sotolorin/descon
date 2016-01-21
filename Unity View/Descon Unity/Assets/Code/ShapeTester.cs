using UnityEngine;
using System.IO;
using System.Xml.Serialization;
using Descon.Data;
using System.Collections.Generic;

public class ShapeTester : MonoBehaviour
{
    public string shapeName;
    public float y = 0f;
    public float z = 12f;

	// Use this for initialization
	void Start ()
    {
        LoadShapes();

        //GetComponent<MeshFilter>().mesh = DrawingMethods.CreateMemberMesh(shapeName, y, z);
	}

    public void LoadShapes()
    {
        //var shapes = new List<Shape>();

        //if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_SHAPES))
        //{
        //    var reader = new XmlSerializer(typeof(List<Shape>));

        //    using (var fileStream = File.OpenRead(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_SHAPES))
        //    {
        //        CommonDataStatic.AllShapes = new Dictionary<string, Shape>();
        //        var allShapes = (List<Shape>)reader.Deserialize(fileStream);
        //        shapes.AddRange(allShapes);
        //        foreach (var shape in shapes)
        //        {
        //            switch (shape.type)
        //            {
        //                case "":
        //                    shape.TypeEnum = EShapeType.None;
        //                    break;
        //                case "W":
        //                case "S":
        //                case "HP":
        //                    shape.TypeEnum = EShapeType.WideFlange;
        //                    break;
        //                case "WT":
        //                case "MT":
        //                case "ST":
        //                    shape.TypeEnum = EShapeType.WTSection;
        //                    break;
        //                case "L":
        //                    shape.TypeEnum = EShapeType.SingleAngle;
        //                    break;
        //                case "2L":
        //                    shape.TypeEnum = EShapeType.DoubleAngle;
        //                    break;
        //                case "HSS":
        //                case "PIPE":
        //                    shape.TypeEnum = EShapeType.HollowSteelSection;
        //                    break;
        //                case "C":
        //                case "MC":
        //                    shape.TypeEnum = EShapeType.SingleChannel;
        //                    break;
        //                case "2C":
        //                case "2MC":
        //                    shape.TypeEnum = EShapeType.DoubleChannel;
        //                    break;
        //            }

        //            shape.User = false;
        //            // Don't add shapes with more than 3 parts (1X1X1X1 wouldn't show up)
        //            if (shape.Name.Split('X').Length <= 3)
        //                CommonDataStatic.AllShapes.Add(shape.Name, shape);
        //        }
        //    }
        //}
    }
}
