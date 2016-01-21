using System.Collections.Generic;
using System.IO;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	/// <summary>
	/// Converts the old Descon 7 formats into the new software.
	/// </summary>
	internal class LegacyConversion
	{
		/// <summary>
		/// Loads the user shapes file called User I-Shapes.txt automatically
		/// </summary>
		internal void LoadUserIShapes()
		{
			var shapes = new List<Shape>();

			shapes.AddRange(LoadUserIShapes("User I-Shapes.txt"));
			new SaveDataToXML().SaveUserShapes();
		}

		/// <summary>
		/// Loads user shapes file passing in the path if the file name is not standard
		/// </summary>
		private List<Shape> LoadUserIShapes(string filePath)
		{
			// Example Line: "PACO1200I350P1420",4.132,12,.21,3.5,.245,.37,.37,.23,11.26,2,14.323,17.03

			var shapes = new List<Shape>();

			if (!File.Exists(filePath))
				return shapes;

			var userShapes = File.ReadAllLines(filePath);
			foreach (string shape in userShapes)
			{
				// User I-Shapes.txt can have a variable number of data elements, so we are setting empty values to 0
				var shapeData = new string[15];
				var tempShapes = shape.Split(',');

				for (int j = 0; j < tempShapes.Length; j++)
					shapeData[j] = tempShapes[j];

				shapes.Add(new Shape
				{
					User = true,
					Name = shapeData[0].Trim().Trim('\"'),
					//GR = float.Parse(shapeData[1] ?? "0"),
					a = float.Parse(shapeData[2] ?? "0"),
					d = float.Parse(shapeData[3] ?? "0"),
					tw = float.Parse(shapeData[4] ?? "0"),
					bf = float.Parse(shapeData[5] ?? "0"),
					tf = float.Parse(shapeData[6] ?? "0"),
					kdes = float.Parse(shapeData[7] ?? "0"),
					kdet = float.Parse(shapeData[8] ?? "0"),
					k1 = float.Parse(shapeData[9] ?? "0"),
					t = float.Parse(shapeData[10] ?? "0"),
					//Gage = float.Parse(shapeData[11] ?? "0"),
					//G2 = float.Parse(shapeData[12] ?? "0"),
					sx = float.Parse(shapeData[13] ?? "0"),
					zx = float.Parse(shapeData[14] ?? "0")
				});
			}

			File.Delete(filePath);

			return shapes;
		}
	}
}