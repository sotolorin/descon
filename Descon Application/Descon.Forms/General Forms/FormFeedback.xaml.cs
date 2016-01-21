using System;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using Descon.Data;
using Descon.UI.DataAccess;
using Descon.WebAccess;
using Application = System.Windows.Application;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;

namespace Descon.Forms
{
	/// <summary>
	/// Interaction logic for FormFeedback.xaml
	/// </summary>
	public partial class FormFeedback
	{
		public FormFeedback(CommonData data)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			DataContext = data;

			tbxDetail.Focus();
		}

		private void btnSendReport_Click(object sender, RoutedEventArgs e)
		{
			string fileName;
			string attachmentFileName;

			if (tbxDescription.Text == string.Empty)
			{
				MessageBox.Show("Please enter a problem description.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}
			if (tbxDetail.Text == string.Empty)
			{
				MessageBox.Show("Please enter the details of your problem.", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
				return;
			}

			Mouse.OverrideCursor = Cursors.Wait;

			try
			{
				var licenseData = new LoadDataFromXML().LoadLicenseFile();

				// Report Format:
				// Line 1: Company Name
				// Line 2: E-mail
				// Line 3: Version
				// Line 4: License Level
				// Line 5: Description
				// Line 6: Details

				File.WriteAllText(ConstString.FILE_BUG_REPORT_TEXT,
					licenseData.CompanyName +
					Environment.NewLine +
					licenseData.UserEmail +
					Environment.NewLine +
					MiscMethods.GetVersionString() +
					Environment.NewLine +
					licenseData.LicenseType +
					Environment.NewLine +
					tbxDescription.Text +
					Environment.NewLine +
					tbxDetail.Text);

				File.Copy(ConstString.FILE_PREFERENCES, ConstString.FILE_BUG_REPORT_PREFERENCES, true);

				new SaveDataToXML().SaveDesconDrawing(ConstString.FILE_BUG_REPORT_SAVE, false);
				new ReportSave().SaveReport(new ReportBuild().BuildReport(), ConstString.FILE_BUG_REPORT_HTML, null, false);

				// This copies the attachment if one exists and includes it in the zip file that is sent
				if (lblFileName.Content != null && File.Exists(lblFileName.Content.ToString()))
				{
					int fileNameIndex = lblFileName.Content.ToString().Split('\\').Length;
					attachmentFileName = lblFileName.Content.ToString().Split('\\')[fileNameIndex - 1];
					File.Copy(lblFileName.Content.ToString(), ConstString.FOLDER_DESCONDATA_BUG_REPORT + attachmentFileName, true);
				}

				fileName = ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA +
							  DateTime.Now.ToString("MM-dd-yy HH.mm.ss") +
							  " (" + licenseData.UserEmail + ") " +
							  ConstString.FILE_FEEDBACK_REPORT_ZIP;

				if(File.Exists(fileName))
					File.Delete(fileName);
				ZipFile.CreateFromDirectory(ConstString.FOLDER_DESCONDATA_BUG_REPORT, fileName);

				string result = new WebServerInteraction().SendFeedbackEmail(fileName);
				if (result != "0")
				{
					MessageBox.Show("Message failed to send. Please send message to SupportDesk@desconplus.com", "Error", MessageBoxButton.OK,
						MessageBoxImage.Error);
					return;
				}

				foreach (var file in Directory.EnumerateFiles(ConstString.FOLDER_DESCONDATA_BUG_REPORT))
					File.Delete(file);

				foreach (var file in Directory.EnumerateFiles(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA))
				{
					if (file.Contains(ConstString.FILE_FEEDBACK_REPORT_ZIP))
						File.Delete(file);
				}

				MessageBox.Show("Feedback successfully submitted. Thank you.", "FEEDBACK SUBMITTED", MessageBoxButton.OK, MessageBoxImage.Information);
				Close();
			}
			catch (Exception ex)
			{
				MessageBox.Show("Message failed to send. Please send message to SupportDesk@desconplus.com", "Error",
					MessageBoxButton.OK, MessageBoxImage.Error);
			}
			finally
			{
				Mouse.OverrideCursor = Cursors.Arrow;
			}
		}

		private void btnAttachFile_Click(object sender, RoutedEventArgs e)
		{
			var openDialog = new OpenFileDialog
			{
				Title = "Attach File"
			};

			if (openDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
			{
				lblFileName.Content = openDialog.FileName;

				if (new FileInfo(openDialog.FileName).Length > 10 * 1024 * 1024) // 10 MB to bytes
				{
					MessageBox.Show("File size must be less than 10MB", "WARNING", MessageBoxButton.OK, MessageBoxImage.Warning);
					lblFileName.Content = string.Empty;
				}
			}
		}

		private void btnCancel_Click(object sender, RoutedEventArgs e)
		{
			Close();
		}
	}
}