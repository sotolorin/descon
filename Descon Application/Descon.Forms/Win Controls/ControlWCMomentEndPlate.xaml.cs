using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCMomentEndPlate
	{
		private CommonData _data;

		public ControlWCMomentEndPlate(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentEndPlate.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}
