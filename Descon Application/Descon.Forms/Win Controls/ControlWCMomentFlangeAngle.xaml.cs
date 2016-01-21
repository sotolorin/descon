using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCMomentFlangeAngle
	{
		private CommonData _data;

		public ControlWCMomentFlangeAngle(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void btnBeamBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentFlangeAngle.BeamBolt), "Bolt Selection");
			form.ShowDialog();
		}

		private void btnColumnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentFlangeAngle.ColumnBolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}