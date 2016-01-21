using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlBCSplicePlates
	{
		private CommonData _data;

		public ControlBCSplicePlates(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
			if (_data.SelectedMember.ShapeType == Data.EShapeType.HollowSteelSection)
				ctrlLBoltSpacing.IsEnabled =
					ctrlTBoltSpacing.IsEnabled =
					ctrlTEdgeDistance.IsEnabled =
					ctrlLongBoltLines.IsEnabled =
					ctrlRowsOfBolts.IsEnabled = true;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.BraceConnect.SplicePlates.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}