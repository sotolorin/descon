using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlBCBrace
	{
		private CommonData _data;

		public ControlBCBrace(ref CommonData data)
		{
			InitializeComponent();

			_data = data;

			// Member is HSS and Bolted
			if (_data.SelectedMember.ShapeType == EShapeType.HollowSteelSection && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
				ctrlWeldLength.Visibility = Visibility.Visible;
			else
				ctrlWeldLength.Visibility = Visibility.Collapsed;

			// Member is WT and Bolted
			if (_data.SelectedMember.ShapeType == EShapeType.WTSection && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted)
				ctrlTransSpacing.Visibility = ctrlWeldSize.Visibility = Visibility.Collapsed;

			// Member is Angle and Bolted
			if ((_data.SelectedMember.ShapeType == EShapeType.SingleAngle && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted) ||
				(_data.SelectedMember.ShapeType == EShapeType.DoubleAngle && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
			{
				ctrlWeldSize.Visibility = Visibility.Collapsed;
				ctrlGageOnFlange.DSCLabel = "Gage on Angle";
			}

			// Member is Channel and Bolted
			if ((_data.SelectedMember.ShapeType == EShapeType.SingleChannel && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted) ||
			    (_data.SelectedMember.ShapeType == EShapeType.DoubleChannel && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Bolted))
				ctrlGageOnFlange.Visibility = ctrlBoltDistanceToLongitEdge.Visibility = ctrlWeldSize.Visibility = Visibility.Collapsed;

			// Member is HSS Bolted or Welded, WT and Welded, Angle and Welded, Channel and Welded
			if (_data.SelectedMember.ShapeType == EShapeType.HollowSteelSection ||
			    (_data.SelectedMember.ShapeType == EShapeType.WTSection && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded) ||
			    (_data.SelectedMember.ShapeType == EShapeType.SingleAngle && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded) ||
				(_data.SelectedMember.ShapeType == EShapeType.DoubleAngle && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded) ||
				(_data.SelectedMember.ShapeType == EShapeType.SingleChannel && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded) ||
				(_data.SelectedMember.ShapeType == EShapeType.DoubleChannel && _data.SelectedMember.BraceToGussetWeldedOrBolted == EConnectionStyle.Welded))
				ctrlGageOnFlange.Visibility = ctrlBolt.Visibility =
					ctrlNumberOfBolts.Visibility = ctrlNumberOfRowsOfBolts.Visibility =
					ctrlTransSpacing.Visibility = ctrlLongitSpacing.Visibility =
					ctrlBoltDistanceToLongitEdge.Visibility = ctrlBoltDistanceToTransvEdge.Visibility =
					ctrlWeldLength.Visibility = Visibility.Collapsed;

			// Member is Wide Flange
			if (_data.SelectedMember.ShapeType == EShapeType.WideFlange)
			{
				ctrlBolt.IsEnabled =
					ctrlNumberOfBolts.IsEnabled = ctrlNumberOfRowsOfBolts.IsEnabled =
					ctrlTransSpacing.IsEnabled = ctrlLongitSpacing.IsEnabled =
					ctrlWeldSize.IsEnabled = false;
				ctrlGageOnFlange.DSCLabel = "Gage on Brace";
			}
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.BoltBrace), "Bolt Selection");
			form.ShowDialog();
		}
	}
}
