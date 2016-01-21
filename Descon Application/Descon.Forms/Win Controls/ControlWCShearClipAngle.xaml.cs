using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCShearClipAngle
	{
		private CommonData _data;

		public ControlWCShearClipAngle(ref CommonData data)
		{
			InitializeComponent();

			DataContext = data;
			_data = data;

			imageBoltStagger.ToolTip = ConstString.HELP_BOLT_STAGGER;
		}

		private void btnBoltsOnOSL_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearClipAngle.BoltOslOnSupport), "Bolt Selection");
			form.ShowDialog();
		}

		private void btnBoltsOnWeb_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearClipAngle.BoltWebOnBeam), "Bolt Selection");
			form.ShowDialog();
		}

		private void BeamSideOrOSL_Checked(object sender, RoutedEventArgs e)
		{
			ctrlWeld.Visibility = Visibility.Visible;
			ctrlWeldSizeSupport.Visibility = Visibility.Visible;
			ctrlWeldSizeBeam.Visibility = Visibility.Visible;
			ctrlBoltGage.Visibility = Visibility.Visible;
			ctrlTOBtoOSL.Visibility = Visibility.Visible;
			canvasSupport.Visibility = Visibility.Visible;
			canvasBeam.Visibility = Visibility.Visible;

			gbxBoltStagger.IsEnabled = true;
			ctrlEdgeDistTransSupport.IsEnabled = true;
			ctrlEdgeDistTransBeam.IsEnabled = true;

			if (_data.SelectedMember.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Bolted &&
			    _data.SelectedMember.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				ctrlWeld.Visibility = Visibility.Collapsed;
				ctrlWeldSizeSupport.Visibility = Visibility.Collapsed;
				ctrlWeldSizeBeam.Visibility = Visibility.Collapsed;
				ctrlEdgeDistTransSupport.IsEnabled = false;
				ctrlEdgeDistTransBeam.IsEnabled = false;
			}
			else if (_data.SelectedMember.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Welded &&
			         _data.SelectedMember.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
			{
				canvasSupport.Visibility = Visibility.Collapsed;
				canvasBeam.Visibility = Visibility.Collapsed;
				ctrlBoltGage.Visibility = Visibility.Collapsed;
				ctrlTOBtoOSL.Visibility = Visibility.Collapsed;
				gbxBoltStagger.IsEnabled = false;
				_data.SelectedMember.WinConnect.ShearClipAngle.BoltStagger = EBoltStagger.None;
			}
			else if (_data.SelectedMember.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Bolted &&
			         _data.SelectedMember.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Welded)
			{
				canvasBeam.Visibility = Visibility.Collapsed;
				ctrlWeldSizeSupport.Visibility = Visibility.Collapsed;
				ctrlEdgeDistTransSupport.IsEnabled = false;
				ctrlEdgeDistTransBeam.IsEnabled = false;
				gbxBoltStagger.IsEnabled = false;
				_data.SelectedMember.WinConnect.ShearClipAngle.BoltStagger = EBoltStagger.None;
			}
			else if (_data.SelectedMember.WinConnect.ShearClipAngle.SupportSideConnection == EConnectionStyle.Welded &&
			         _data.SelectedMember.WinConnect.ShearClipAngle.BeamSideConnection == EConnectionStyle.Bolted)
			{
				canvasSupport.Visibility = Visibility.Collapsed;
				ctrlWeldSizeBeam.Visibility = Visibility.Collapsed;
				ctrlBoltGage.Visibility = Visibility.Collapsed;
				gbxBoltStagger.IsEnabled = false;
				_data.SelectedMember.WinConnect.ShearClipAngle.BoltStagger = EBoltStagger.None;
			}
		}
	}
}