using System.Windows;
using Descon.Calculations;
using Descon.Data;
using Descon.UI.DataAccess;
using Fema = Descon.Calculations.Fema;

namespace Descon.Forms
{
	/*
	How to access in Descon 7:
	
	DesconBrace: Once you have the software open, choose Seismic and R > 3. Then push close. Choose any Wide Flange column,
	push apply, then next. Choose any Wide flange beam, then choose apply and next. Skip to upper right brace. Choose any hollow
	steel section, type in a lateral unbraced length of 10, for example, then push apply. It should pop up the window you are talking about

	DesconWin: When it opens, choose seismic, then choose any column and right side beam, for example. Push close on the seismic panel,
	and the purple screen should pop up
	 */
	/// <summary>
	/// Seismic setting form
	/// </summary>
	public partial class FormSeismicSettings
	{
		private CommonData _data;

		public FormSeismicSettings(CommonData data)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_data = data;
			DataContext = _data;

			imageClearSpan.ToolTip = ConstString.HELP_SEISMIC;
		}

		private void btnEndPlateBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SeismicSettings.EndPlateBolt), "Bolt Selection");
			form.ShowDialog();
			_data.SeismicSettings.EndPlateBolt = _data.CurrentBolt;
		}

		private void btnBeamWebBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SeismicSettings.BeamWebBolt), "Bolt Selection");
			form.ShowDialog();
			_data.SeismicSettings.EndPlateBolt = _data.CurrentBolt;
		}

		private void btnFemaSettings_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlFema(_data), "FEMA Settings");
			form.ShowDialog();
			Fema.SetFemaVariables(EMemberType.PrimaryMember);
		}

		private void btnApply_Click(object sender, RoutedEventArgs e)
		{
			//if (SeismicCalc.CheckSeismicSelections())
				Close();
		}

		private void btnCheck_Click(object sender, RoutedEventArgs e)
		{
			SeismicCalc.CheckSeismicSelections();
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}