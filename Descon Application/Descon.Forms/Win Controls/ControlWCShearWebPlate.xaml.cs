using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCShearWebPlate
	{
		private CommonData _data;

		public ControlWCShearWebPlate(ref CommonData data)
		{
			InitializeComponent();

			DataContext = data;
			_data = data;
		}

		private void btnBolts_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearWebPlate.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}