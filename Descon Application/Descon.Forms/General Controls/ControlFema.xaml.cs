using System.Windows;
using System.Windows.Controls;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// FEMA data form
	/// </summary>
	public partial class ControlFema
	{
		private CommonData _data;

		public ControlFema(CommonData data)
		{
			InitializeComponent();

			_data = data;

			imageFemaConnectionDetail.ToolTip = ConstString.HELP_FEMA_CONNECTION_DETAIL;
		}

		/// <summary>
		/// Triggered when the connection type changes. Determines if certain fields are enabled or not
		/// </summary>
		private void cbxConnectionType_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (cbxConnectionType.SelectedValue == null)
				return;

			btnFlangeBolt.IsEnabled = btnWebBolt.IsEnabled = rbSMF.IsEnabled = true;

			switch ((EFemaConnectionType)cbxConnectionType.SelectedValue)
			{
				case EFemaConnectionType.WUFB:
					btnFlangeBolt.IsEnabled = rbSMF.IsEnabled = false;
					break;
				case EFemaConnectionType.WUFW:
				case EFemaConnectionType.FF:
				case EFemaConnectionType.WFP:
					btnFlangeBolt.IsEnabled = btnWebBolt.IsEnabled = false;
					break;
				case EFemaConnectionType.RBS:
					btnFlangeBolt.IsEnabled = false;
					break;
				case EFemaConnectionType.BUEP:
				case EFemaConnectionType.BSEP:
					btnWebBolt.IsEnabled = false;
					break;
				case EFemaConnectionType.DST:
					rbSMF.IsEnabled = false;
					break;
			}
		}

		private void btnFlangeBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.Fema.FlangeBolt), "Bolt Selection");
			form.ShowDialog();
			_data.SelectedMember.WinConnect.Fema.FlangeBolt = _data.CurrentBolt;
		}

		private void btnWebBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.Fema.WebBolt), "Bolt Selection");
			form.ShowDialog();
			_data.SelectedMember.WinConnect.Fema.WebBolt = _data.CurrentBolt;
		}
	}
}