using System;
using System.IO;
using System.Windows.Forms;
using System.Windows.Media.Imaging;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	/// <summary>
	/// Report settings. Allows the user to select a header image and change the default fields that appear on the report
	/// </summary>
	public partial class ControlReportSettings
	{
		public ControlReportSettings(bool FromProjectManager)
		{
			InitializeComponent();

			LoadImage();
		}

		/// <summary>
		/// Allows the user to choose a new image to use as the header in the report
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void btnSelectHeaderImage_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			imageHeader.Source = null;

			var openDialog = new OpenFileDialog
			{
				AddExtension = true,
				DefaultExt = ".png",
				Filter = "PNG Format|*.PNG",
				Title = "Open Picture File"
			};
			if (openDialog.ShowDialog() == DialogResult.OK)
				File.Copy(openDialog.FileName, ConstString.FILE_LOGO, true);
			
			LoadImage();
		}

		private void btnRestoreImage_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			new FileManipulatation().WriteResourceToFile(ConstString.FILE_LOGO);

			LoadImage();
		}

		private void btnRestoreHeaderText_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			CommonDataStatic.Preferences.ReportSettings = new DefaultPreferences().SetReportDefaults();
		}

		/// <summary>
		/// Loads the image from the HD and displays in on the form
		/// </summary>
		private void LoadImage()
		{
			var bitmapImage = new BitmapImage();

			bitmapImage.BeginInit();
			bitmapImage.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
			bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
			bitmapImage.UriSource = new Uri(ConstString.FILE_LOGO);
			bitmapImage.EndInit();

			imageHeader.Source = bitmapImage;
		}
	}
}