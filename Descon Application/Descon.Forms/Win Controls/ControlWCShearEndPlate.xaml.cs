using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCShearEndPlate
	{
		private CommonData _data;

		public ControlWCShearEndPlate(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
			DataContext = _data;

			if (MiscMethods.IsBrace(CommonDataStatic.SelectedMember.MemberType))
				gbxPosition.Visibility = Visibility.Collapsed;
			else
				ctrlConnectionPlateThickness.Visibility = ctrlTOBToFirstBolt.Visibility = Visibility.Collapsed;
		}

		private void btnBolts_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearEndPlate.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}
