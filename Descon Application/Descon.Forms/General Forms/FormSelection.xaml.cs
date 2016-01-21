using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Generic selection form that can be used for all different kinds of data
	/// </summary>
	public partial class FormSelection
	{
		private readonly CommonData _data;
		private readonly EFormSelectionDataType _dataType;
		// Value selected by the user to be returned
		public string SelectedValue { get; private set; }

		/// <summary>
		/// A generic selection form that has different modes for different kinds of data.
		/// </summary>
		/// <param name="data">Any kind of data that lends itself to being dislpayed in a grid</param>
		/// <param name="dataType">DataType, choose Generic if you don't need one of the custom ones</param>
		public FormSelection(CommonData data, EFormSelectionDataType dataType)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_dataType = dataType;
			_data = data;

			SetUpForm();
		}

		// Widths are arbitrary and set to make the grid width reasonable for whichever data is being shown
		private void SetUpForm()
		{
			switch (_dataType)
			{
				case EFormSelectionDataType.Materials:
					gridData.AutoGenerateColumns = false;
					gridData.Columns.Add(new DataGridTextColumn {Header = "Material Name", Binding = new Binding("Key")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Fu", Binding = new Binding("Value.Fu")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Fy", Binding = new Binding("Value.Fy")});
					gridData.ItemsSource = cbxSelection.ItemsSource = CommonDataStatic.MaterialDict;
					Title = "Select Material";
					lblSelect.Content = "Select Material";
					Width = 300;
					break;
				case EFormSelectionDataType.Welds:
					gridData.AutoGenerateColumns = false;
					gridData.Columns.Add(new DataGridTextColumn {Header = "Weld Name", Binding = new Binding("Key")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Fexx", Binding = new Binding("Value.Fexx")});
					gridData.ItemsSource = cbxSelection.ItemsSource = CommonDataStatic.WeldDict;
					Title = "Select Weld";
					lblSelect.Content = "Select Weld";
					Width = 300;
					break;
				case EFormSelectionDataType.Shapes:
					gridData.AutoGenerateColumns = false;
					gridData.Columns.Add(new DataGridTextColumn {Header = "Shape Name", Binding = new Binding("Key")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Area", Binding = new Binding("Value.a")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Depth", Binding = new Binding("Value.d")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Web Thickness", Binding = new Binding("Value.tw")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Flange Width", Binding = new Binding("Value.tf")});
					gridData.Columns.Add(new DataGridTextColumn {Header = "Flange Thickness", Binding = new Binding("Value.bf")});
					gridData.ItemsSource = cbxSelection.ItemsSource = _data.ShapesFiltered.Where(s => s.Key != ConstString.NONE);
					Title = "Select Shape";
					lblSelect.Content = "Select Shape";
					Width = 550;
					break;
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			var lastColumn = gridData.Columns.LastOrDefault();
			if (lastColumn != null)
				lastColumn.Width = new DataGridLength(1, DataGridLengthUnitType.Star);

			FocusItem(0);
			cbxSelection.Focus();
		}

		private void FocusItem(int index)
		{
			gridData.SelectedItem = gridData.Items[index];
			gridData.ScrollIntoView(gridData.Items[index]);
			var row = (DataGridRow)gridData.ItemContainerGenerator.ContainerFromItem(gridData.Items[index]);
			row.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
		}

		private void gridData_MouseDoubleClick(object sender, MouseButtonEventArgs e)
		{
			btnReturn_Click(null, new RoutedEventArgs());
		}

		private void btnReturn_Click(object sender, RoutedEventArgs e)
		{
			if (gridData.SelectedItems.Count > 0)
			{
				switch (_dataType)
				{
					case EFormSelectionDataType.Shapes:
						SelectedValue = ((KeyValuePair<string, Shape>) gridData.SelectedItem).Key;
						break;
					case EFormSelectionDataType.Materials:
						SelectedValue = ((KeyValuePair<string, Material>)gridData.SelectedItem).Key;
						break;
					case EFormSelectionDataType.Welds:
						SelectedValue = ((KeyValuePair<string, Weld>)gridData.SelectedItem).Key;
						break;
				}
				DialogResult = true;
				Close();
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void gridData_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (gridData.SelectedItem != null)
				gridData.ScrollIntoView(gridData.SelectedItem);
		}
	}
}