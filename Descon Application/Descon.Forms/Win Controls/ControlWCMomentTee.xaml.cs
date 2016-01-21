using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCMomentTee
	{
		private CommonData _data;

		public ControlWCMomentTee(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void rbBeamSide_Changed(object sender, RoutedEventArgs e)
		{
			gbxTopTee.Header = _data.SelectedMember.WinConnect.MomentTee.TeeConnectionStyle == EConnectionStyle.Welded ? "Top Tee" : "Top and Bottom Tee's";
		}

		private void btnColumnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentTee.BoltColumnFlange), "Bolt Selection");
			form.ShowDialog();
		}

		private void btnSideBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentTee.BoltBeamStem), "Bolt Selection");
			form.ShowDialog();
		}
	}
}