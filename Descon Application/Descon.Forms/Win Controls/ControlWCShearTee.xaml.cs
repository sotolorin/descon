using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCShearTee
	{
		private CommonData _data;

		public ControlWCShearTee(ref CommonData data)
		{
			InitializeComponent();

			DataContext = data;
			_data = data;

			imageBoltStagger.ToolTip = ConstString.HELP_BOLT_STAGGER;
		}

		private void btnBoltsOnOSL_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearWebTee.BoltOslOnFlange), "Bolt Selection");
			form.ShowDialog();
		}

		private void btnBoltsOnWeb_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearWebTee.BoltWebOnStem), "Bolt Selection");
			form.ShowDialog();
		}

		/// <summary>
		/// Disables and collapses specific parts of the form depending on whether or not each side is bolted or welded.
		/// Because there are so many parts, it is a little clear to do this in code rather than with bindings.
		/// </summary>
		private void BeamOSL_CheckChanged(object sender, RoutedEventArgs e)
		{
			// This first section resets each control so the logic can just disable or collapse
			ctrlWeld.Visibility = Visibility.Visible;
			ctrlWeldSizeFlange.Visibility = Visibility.Visible;
			ctrlWeldSizeStem.Visibility = Visibility.Visible;
			ctrlBoltGage.Visibility = Visibility.Visible;
			ctrlTOBtoOSL.Visibility = Visibility.Visible;
			canvasFlangeControls.Visibility = Visibility.Visible;
			canvasStemControls.Visibility = Visibility.Visible;

			gbxBoltStagger.IsEnabled = true;
			ctrlEdgeDistTransFlange.IsEnabled = true;
			ctrlEdgeDistTransStem.IsEnabled = true;

			if (_data.SelectedMember.WinConnect.ShearWebTee.OSLConnection == EConnectionStyle.Bolted &&
			    _data.SelectedMember.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				ctrlWeld.Visibility = Visibility.Collapsed;
				ctrlWeldSizeFlange.Visibility = Visibility.Collapsed;
				ctrlWeldSizeStem.Visibility = Visibility.Collapsed;
				ctrlEdgeDistTransFlange.IsEnabled = false;
				ctrlEdgeDistTransStem.IsEnabled = false;
			}
			else if (_data.SelectedMember.WinConnect.ShearWebTee.OSLConnection == EConnectionStyle.Welded &&
			         _data.SelectedMember.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Welded)
			{
				canvasFlangeControls.Visibility = Visibility.Collapsed;
				canvasStemControls.Visibility = Visibility.Collapsed;
				ctrlBoltGage.Visibility = Visibility.Collapsed;
				ctrlTOBtoOSL.Visibility = Visibility.Collapsed;
				gbxBoltStagger.IsEnabled = false;
			}
			else if (_data.SelectedMember.WinConnect.ShearWebTee.OSLConnection == EConnectionStyle.Bolted &&
			         _data.SelectedMember.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Welded)
			{
				canvasStemControls.Visibility = Visibility.Collapsed;
				ctrlWeldSizeFlange.Visibility = Visibility.Collapsed;
				gbxBoltStagger.IsEnabled = false;
				ctrlEdgeDistTransFlange.IsEnabled = false;
				ctrlEdgeDistTransStem.IsEnabled = false;
			}
			else if (_data.SelectedMember.WinConnect.ShearWebTee.OSLConnection == EConnectionStyle.Welded &&
			         _data.SelectedMember.WinConnect.ShearWebTee.BeamSideConnection == EConnectionStyle.Bolted)
			{
				canvasFlangeControls.Visibility = Visibility.Collapsed;
				ctrlWeldSizeStem.Visibility = Visibility.Collapsed;
				ctrlBoltGage.Visibility = Visibility.Collapsed;
				gbxBoltStagger.IsEnabled = false;
			}
		}
	}
}