using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlBCFabricatedTee
	{
		private CommonData _data;

		public ControlBCFabricatedTee(CommonData data)
		{
			InitializeComponent();

			_data = data;

			imageFabricatedTee.ToolTip = ConstString.HELP_FABRICATED_TEE;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.BraceConnect.FabricatedTee.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}