using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Generic selection form that can be used for all different kinds of data
	/// </summary>
	public partial class FormUserShapes
	{
		private readonly CommonData _data;

		public FormUserShapes(CommonData data)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_data = data;
			DataContext = data;
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			AutoFitGrid();
		}

		/// <summary>
		/// Fit the columns to the data
		/// </summary>
		private void AutoFitGrid()
		{
			// Set the last column to fill the remainder of the grid
			var lastColumn = gridData.Columns.LastOrDefault();
			if (lastColumn != null)
				lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

			gridData.UpdateLayout();

			// Autofit all other columns
			foreach (var column in gridData.Columns)
			{
				if (Equals(column, lastColumn))		// Skip the last column
					continue;

				column.Width = new DataGridLength(1, DataGridLengthUnitType.Auto);
			}

			gridData.UpdateLayout();
		}

		private void btnAddShape_Click(object sender, RoutedEventArgs e)
		{
			string name = "UserShape1";
			int currentNumber = 1;

			while(_data.ShapesUser.Any(s => s.Name == name))
				name = "UserShape" + ++currentNumber;

			var shape = CommonDataStatic.AllShapes.First(s => s.Value.TypeEnum == EShapeType.WideFlange &&
			                                                  s.Value.UnitSystem == CommonDataStatic.Units).Value.ShallowCopy();

			shape.User = true;
			shape.Name = name;

			_data.ShapesUser.Add(shape);
			CommonDataStatic.AllShapes.Add(shape.Name, shape);

			_data.OnPropertyChanged("ShapesUser");

			AutoFitGrid();
		}

		private void btnRemoveShape_Click(object sender, RoutedEventArgs e)
		{
			if (gridData.SelectedValue == null)
				return;

			var shape = (Shape)gridData.SelectedItem;

			CommonDataStatic.AllShapes.Remove(shape.Name);
			_data.ShapesUser.Remove(shape);

			_data.OnPropertyChanged("ShapesUser");

			AutoFitGrid();
		}

		private void btnSaveAndClose_Click(object sender, RoutedEventArgs e)
		{
			// This updates the shape data if the user edited the values
			foreach (var shape in _data.ShapesUser)
			{
				if (CommonDataStatic.AllShapes.ContainsKey(shape.Name))
					CommonDataStatic.AllShapes[shape.Name] = shape;
			}

			new SaveDataToXML().SaveUserShapes();

			_data.OnPropertyChanged("ShapesFiltered");

			Close();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}