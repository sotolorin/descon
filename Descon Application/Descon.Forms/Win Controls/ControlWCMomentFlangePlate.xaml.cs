using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	// This form is not used when the Joint Configuration is set to Beam to Column Web. In that case ControlWCMomentDirectlyWelded is used.
	public partial class ControlWCMomentFlangePlate
	{
		private CommonData _data;

		public ControlWCMomentFlangePlate(ref CommonData data)
		{
			InitializeComponent();

			_data = data;

			if (_data.DetailDataDict[EMemberType.PrimaryMember].ShapeType == EShapeType.HollowSteelSection)
				canvasWidthLength.IsEnabled = false;
			else
				gbxDiaphragm.IsEnabled = false;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentFlangePlate.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}