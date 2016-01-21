using System;
using System.Windows;
using System.Windows.Media.Imaging;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	// This form is used for the Directly Welded and Flange Plate connections when the Joint Configuration is set to Beam to Column Web.
	// Otherwise, Flange Plate connection uses the actual Flange plate form ControlWCMomentFlangePlate
	public partial class ControlWCMomentDirectlyWelded
	{
		private CommonData _data;

		public ControlWCMomentDirectlyWelded(ref CommonData data)
		{
			InitializeComponent();

			_data = data;
			DataContext = _data;

			if (_data.MemberType == EMemberType.LeftBeam && _data.DetailDataDict[EMemberType.LeftBeam].MomentConnection == EMomentCarriedBy.DirectlyWelded)
			{
				rbType6.Visibility = imageType6.Visibility = Visibility.Hidden;
				imageType5.Width = 260; // Width of larger image
				imageType1.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Web.DW1.png", UriKind.Absolute));
				imageType2.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Web.DW2.png", UriKind.Absolute));
				imageType3.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Web.DW3.png", UriKind.Absolute));
				imageType4.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Web.DW4.png", UriKind.Absolute));
				imageType5.Source = new BitmapImage(new Uri("pack://application:,,,/Descon.Resources;component/Images/Drawing_Examples/Attachments.Web.DW5.png", UriKind.Absolute));
			}
		}

		private void btnBolt_Click(object sender, RoutedEventArgs e)
		{
			var form = new FormControlShell(_data, new ControlBoltSelection(ref _data, _data.SelectedMember.WinConnect.MomentDirectWeld.Bolt), "Bolt Selection");
			form.ShowDialog();
		}
	}
}