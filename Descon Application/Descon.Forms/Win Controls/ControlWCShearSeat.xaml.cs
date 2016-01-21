using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class ControlWCShearSeat
	{
		private CommonData _data;

		public ControlWCShearSeat(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
		}

		private void rbConnection_Changed(object sender, RoutedEventArgs e)
		{
			var bitmapImage =
				new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Icons/QuestionMark.png", UriKind.Absolute));
			ctrlWeld.IsEnabled = true;
			btnBolt.IsEnabled = true;
			gbxStiffener.IsEnabled = true;
			ctrlNumberOfRowsOfBolts.IsEnabled = ctrlNumberBoltsOnEachRow.IsEnabled = true;
			ctrlBoltHorizontal.IsEnabled = ctrlBoltVertical.IsEnabled = true;
			ctrlAngle.IsEnabled = canvasShortLeg.IsEnabled = true;
			ctrl2L.IsEnabled = ctrlTee.IsEnabled = true;
			canvasStiffenerPlate.IsEnabled = canvasSeatPlate.IsEnabled = true;
			ctrlBeamSideWeld.IsEnabled = ctrlSupportSideWeld.IsEnabled = true;
			rbStiff2L.IsEnabled = rbStiffPlate.IsEnabled = rbStiffTee.IsEnabled = true;

			switch (_data.SelectedMember.WinConnect.ShearSeat.Connection)
			{
				case ESeatConnection.Bolted:
					bitmapImage = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Seat.Bolted.png", UriKind.Absolute));
					ctrlWeld.IsEnabled = gbxStiffener.IsEnabled = canvasSeatPlate.IsEnabled = false;
					ctrlBeamSideWeld.IsEnabled = ctrlSupportSideWeld.IsEnabled = false;
					rbStiff2L.IsEnabled = rbStiffPlate.IsEnabled = rbStiffTee.IsEnabled = false;
					_data.SelectedMember.WinConnect.ShearSeat.Stiffener = ESeatStiffener.None;
					break;
				case ESeatConnection.BoltedStiffenedPlate:
					bitmapImage = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Seat.BoltedStiffened.png", UriKind.Absolute));
					if (_data.SelectedMember.WinConnect.ShearSeat.Stiffener == ESeatStiffener.L2)
						ctrlAngle.IsEnabled = canvasShortLeg.IsEnabled = canvasStiffenerPlate.IsEnabled = ctrlTee.IsEnabled = false;
					else
						ctrlAngle.IsEnabled = canvasShortLeg.IsEnabled = canvasStiffenerPlate.IsEnabled = ctrl2L.IsEnabled = false;
					rbStiffPlate.IsEnabled = false;
					if (_data.SelectedMember.WinConnect.ShearSeat.Stiffener == ESeatStiffener.Plate)
						_data.SelectedMember.WinConnect.ShearSeat.Stiffener = ESeatStiffener.L2;
					break;
				case ESeatConnection.Welded:
					bitmapImage = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Seat.Welded.png", UriKind.Absolute));
					btnBolt.IsEnabled = false;
					rbStiff2L.IsEnabled = rbStiffPlate.IsEnabled = rbStiffTee.IsEnabled = false;
					_data.SelectedMember.WinConnect.ShearSeat.Stiffener = ESeatStiffener.None;
					ctrlNumberOfRowsOfBolts.IsEnabled = ctrlNumberBoltsOnEachRow.IsEnabled = false;
					ctrlBoltHorizontal.IsEnabled = ctrlBoltVertical.IsEnabled = false;
					canvasSeatPlate.IsEnabled = false;
					break;
				case ESeatConnection.WeldedStiffened:
					bitmapImage = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Seat.WeldedStiffened.png", UriKind.Absolute));
					btnBolt.IsEnabled = false;
					ctrlNumberOfRowsOfBolts.IsEnabled = ctrlNumberBoltsOnEachRow.IsEnabled = false;
					ctrlBoltHorizontal.IsEnabled = ctrlBoltVertical.IsEnabled = false;
					if (_data.SelectedMember.WinConnect.ShearSeat.Stiffener == ESeatStiffener.L2 ||
						_data.SelectedMember.WinConnect.ShearSeat.Stiffener == ESeatStiffener.None)
					{
						_data.SelectedMember.WinConnect.ShearSeat.Stiffener = ESeatStiffener.Tee;
						ctrlAngle.IsEnabled = canvasShortLeg.IsEnabled = canvasStiffenerPlate.IsEnabled = ctrlTee.IsEnabled = false;
					}
					else
						ctrlAngle.IsEnabled = canvasShortLeg.IsEnabled = canvasStiffenerPlate.IsEnabled = ctrl2L.IsEnabled = false;

					rbStiff2L.IsEnabled = false;
					break;
			}
			imageConnectionExample.Source = bitmapImage;
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.ShearSeat.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}