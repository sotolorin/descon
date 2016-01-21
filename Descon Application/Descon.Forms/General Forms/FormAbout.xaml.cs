using System.Diagnostics;
using System.IO;
using System.Windows;
using Descon.Data;
using Descon.UI.DataAccess;

namespace Descon.Forms
{
	public partial class FormAbout
	{
		public FormAbout()
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;
		}

		private void btnEULA_Click(object sender, RoutedEventArgs e)
		{
			if (File.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_EULA))
				Process.Start(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_EULA);
			else
			{
				new FileManipulatation().WriteResourceToFile(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_EULA);
				Process.Start(ConstString.FOLDER_MYDOCUMENTS_DESCON + ConstString.FILE_EULA);
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}
