using System.Collections.Generic;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Form for editing and viewing the Materials and Welds. 
	/// </summary>
	public partial class ControlMaterialWeldEdit
	{
		private readonly CommonData _data;

		public ControlMaterialWeldEdit(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
			DataContext = _data;
		}

		private void btnAddNewMaterial_Click(object sender, RoutedEventArgs e)
		{
			if (CommonDataStatic.MaterialDict.ContainsKey(tbxMaterialName.Text.Trim()))
				MessageBox.Show("Duplicate Material Cannot be Added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			else if (string.IsNullOrEmpty(tbxMaterialName.Text.Trim()) ||
			         string.IsNullOrEmpty(ctrlFu.DSCTextBoxValue.ToString().Trim()) || string.IsNullOrEmpty(ctrlFy.DSCTextBoxValue.ToString().Trim()) ||
			         string.IsNullOrEmpty(ctrlRy.DSCTextBoxValue.ToString().Trim()) || string.IsNullOrEmpty(ctrlRt.DSCTextBoxValue.ToString().Trim()))
				MessageBox.Show("Please enter values for:\n\nName\nFu\nFy\nRy\nRt", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			else
			{
				var newMaterial = new Material
				{
					Name = tbxMaterialName.Text.Trim(),
					Fu = double.Parse(ctrlFu.DSCTextBoxValue.ToString().Trim()),
					Fy = double.Parse(ctrlFy.DSCTextBoxValue.ToString().Trim()),
					Ry = double.Parse(ctrlRy.DSCTextBoxValue.ToString().Trim()),
					Rt = double.Parse(ctrlRt.DSCTextBoxValue.ToString().Trim()),
					UserDefined = true
				};

				CommonDataStatic.MaterialDict.Add(newMaterial.Name, newMaterial);
				_data.OnPropertyChanged("MaterialDict");
				tbxMaterialName.Text = string.Empty;
				ctrlFu.DSCTextBoxValue = ctrlFy.DSCTextBoxValue = ctrlRt.DSCTextBoxValue = ctrlRy.DSCTextBoxValue = "0";
			}
		}

		private void btnDeleteMaterial_Click(object sender, RoutedEventArgs e)
		{
			if (dataGridMaterials.SelectedItem == null)
			{
				MessageBox.Show("Please select a material in the grid");
				return;
			}
			
			var selectedItem = (KeyValuePair<string, Material>)dataGridMaterials.SelectedItem;
			if (!selectedItem.Value.UserDefined)
			{
				MessageBox.Show("Only User Defined Materials can be deleted");
				return;
			}
			CommonDataStatic.MaterialDict.Remove(selectedItem.Key);
			_data.OnPropertyChanged("MaterialDict");

			if (CommonDataStatic.Preferences.DefaultMaterials.WShape.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.WShape = CommonDataStatic.MaterialDict["A992"];
			if (CommonDataStatic.Preferences.DefaultMaterials.WTShape.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.WTShape = CommonDataStatic.MaterialDict["A992"];
			if (CommonDataStatic.Preferences.DefaultMaterials.HSSShape.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.HSSShape = CommonDataStatic.MaterialDict["A500-B-46"];
			if (CommonDataStatic.Preferences.DefaultMaterials.Angle.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.Angle = CommonDataStatic.MaterialDict["A36"];
			if (CommonDataStatic.Preferences.DefaultMaterials.Channel.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.Channel = CommonDataStatic.MaterialDict["A36"];
			if (CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.ConnectionPlate = CommonDataStatic.MaterialDict["A36"];
			if (CommonDataStatic.Preferences.DefaultMaterials.GussetPlate.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.GussetPlate = CommonDataStatic.MaterialDict["A36"];
			if (CommonDataStatic.Preferences.DefaultMaterials.StiffenerPlate.Name == selectedItem.Value.Name)
				CommonDataStatic.Preferences.DefaultMaterials.StiffenerPlate = CommonDataStatic.MaterialDict["A36"];
		}

		private void btnAddNewWeld_Click(object sender, RoutedEventArgs e)
		{

			if (CommonDataStatic.WeldDict.ContainsKey(tbxWeldName.Text.Trim()))
				MessageBox.Show("Duplicate Weld cannot be added.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			else if (string.IsNullOrEmpty(tbxWeldName.Text.Trim()) || string.IsNullOrEmpty(ctrlFexx.DSCTextBoxValue.ToString().Trim()))
				MessageBox.Show(@"Please enter values for:\n\nName\nFexx", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			else
			{
				var newWeld = new Weld
				{
					Name = tbxWeldName.Text.Trim(),
					Fexx = float.Parse(ctrlFexx.DSCTextBoxValue.ToString().Trim()),
					Metric = CommonDataStatic.Units == EUnit.Metric,
					UserDefined = true
				};

				CommonDataStatic.WeldDict.Add(newWeld.Name, newWeld);
				_data.OnPropertyChanged("WeldDict");
				tbxWeldName.Text = string.Empty;
				ctrlFexx.DSCTextBoxValue = string.Empty;
			}
		}

		private void btnDeleteWeld_Click(object sender, RoutedEventArgs e)
		{
			if (dataGridWelds.SelectedItem == null)
			{
				MessageBox.Show("Please select a weld in the grid");
				return;
			}

			var selectedItem = (KeyValuePair<string, Weld>)dataGridWelds.SelectedItem;
			if (!selectedItem.Value.UserDefined)
			{
				MessageBox.Show("Only User Defined Welds can be deleted");
				return;
			}
			CommonDataStatic.WeldDict.Remove(selectedItem.Key);
			_data.OnPropertyChanged("WeldDict");

			if (CommonDataStatic.Preferences.DefaultElectrode.Name == selectedItem.Value.Name)
			{
				if (CommonDataStatic.Preferences.Units == EUnit.US)
					CommonDataStatic.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E70XX"].ShallowCopy();
				else
					CommonDataStatic.Preferences.DefaultElectrode = CommonDataStatic.WeldDict["E48XX"].ShallowCopy();
			}
		}
	}
}