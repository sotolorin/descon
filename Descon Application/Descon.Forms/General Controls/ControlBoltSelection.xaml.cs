using System.Linq;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Bolt selection and setup form
	/// </summary>
	public partial class ControlBoltSelection
	{
		private readonly CommonData _data;

		public ControlBoltSelection(ref CommonData data, Bolt bolt)
		{
			InitializeComponent();

			_data = data;
			_data.CurrentBolt = bolt;
			DataContext = _data;
		}

		private void btnAddASTM_Click(object sender, RoutedEventArgs e)
		{
			int fuValue;
			string nameValue = ctrlASTMName.Text;
			bool valueUpdated = false;

			if (string.IsNullOrEmpty(nameValue))
				MessageBox.Show("Please enter a Name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
			else
			{
				if (!int.TryParse(ctrlASTMFu.DSCTextBoxValue.ToString(), out fuValue))
					MessageBox.Show("Please enter a whole number value for Fu", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				else
				{
					if (_data.NonASTMValues.Any(a => a.Name == nameValue))
					{
						_data.NonASTMValues.RemoveAll(a => a.Name == nameValue);
						valueUpdated = true;
					}
					
					_data.NonASTMValues.Add(new BoltUserASTM {Name = nameValue, Fu = fuValue});

					if (valueUpdated)
						MessageBox.Show("Non ASTM Fu Value Updated: \"" + nameValue + "\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
					else
						MessageBox.Show("Non ASTM Added: \"" + nameValue + "\"", "Success", MessageBoxButton.OK, MessageBoxImage.Information);

					ctrlASTMFu.DSCTextBoxValue = ctrlASTMName.Text = string.Empty;
				}
			}

			cbxSelectedASTM.Items.Refresh();
		}

		private void btnDeleteASTM_Click(object sender, RoutedEventArgs e)
		{
			_data.NonASTMValues.Remove((BoltUserASTM)cbxSelectedASTM.SelectedItem);
			cbxSelectedASTM.Items.Refresh();
		}
	}
}