using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Serialization;
using Descon.Data;

namespace Descon.UI.DataAccess
{
	public partial class CommonData
	{
		/// <summary>
		/// Resets the UI by clearing the data for each component. Used when creating a new file or before one is open.
		/// </summary>
		public void SetDefaultUIData()
		{
			CurrentBolt = new Bolt();

			MiscMethods.SetDefaultData();
			FormControlShellPositionLeft = FormControlShellPositionTop = 0;

			Preferences.ReportSettings.ShowCalculations = true;

			DetailDataDict = CommonDataStatic.DetailDataDict;

			MemberType = (EMemberType)CommonLists.MemberList.GetKey(0);

			MiscMethods.ResetReportLists();
		}

		/// <summary>
		/// Loads the user shapes file
		/// </summary>
		private ObservableCollection<Shape> LoadUserShapes()
		{
			var reader = new XmlSerializer(typeof(List<Shape>));
			var shapeList = new List<Shape>();
			var shapes = new ObservableCollection<Shape>();

			if(File.Exists(ConstString.FILE_USER_SHAPES))
			{
				using (var fileStream = File.OpenRead(ConstString.FILE_USER_SHAPES))
				{
					shapeList.AddRange((List<Shape>)reader.Deserialize(fileStream));
				}

				foreach (var shape in shapeList)
					shapes.Add(shape);
			}

			return shapes;
		}
	}
}