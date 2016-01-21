using System.Windows;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlBCClipAngle
	{
		private CommonData _data;

		public ControlBCClipAngle(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void btnBoltColumn_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearClipAngle.BoltOnColumn), "Bolt Selection");
			form.ShowDialog();
		}

		private void btnBoltGusset_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearClipAngle.BoltOnGusset), "Bolt Selection");
			form.ShowDialog();
		}
	}
}
