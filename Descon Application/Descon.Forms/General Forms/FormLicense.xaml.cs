using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Descon.Data;
using Descon.UI.DataAccess;
using Descon.WebAccess;
using Application = System.Windows.Application;

namespace Descon.Forms
{
	public partial class FormLicense
	{
		private readonly LicensingData _licenseData;
		private readonly SQLServerIntraction _sqlServerIntraction = new SQLServerIntraction();
		private readonly WebServerInteraction _webServerInteraction = new WebServerInteraction();

		public FormLicense(string title, int numberOfDaysLeftInTrial, bool openedThroughApplication)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			if (!openedThroughApplication)
				lblTitle.Content += title + " (" + MiscMethods.GetVersionString() + ")";

			Title = title;

			_licenseData = new LoadDataFromXML().LoadLicenseFile();
			DataContext = _licenseData;
			if (_licenseData.Port == 0)
				_licenseData.Port = 43000;

			lblInformation.Text = string.Empty;

			if (_licenseData.LicenseType == ELicenseType.Demo_1 && numberOfDaysLeftInTrial < 1)
			{
				btnDemo.IsEnabled = false;
				lblInformation.Text = "Demo expired or server unavailable. Please log in to your account.";
			}

			if (openedThroughApplication)
			{
				gbxTrial.Visibility = Visibility.Collapsed;
				gbxLogin.IsEnabled = false;
			}
		}

		private void formLicense_Loaded(object sender, RoutedEventArgs e)
		{
			if (_licenseData == null)
				return;

			tbxPassword1.Password = tbxPassword2.Password = _licenseData.Password;

			tbxCompanyName.SelectAll();
			tbxCompanyName.Focus();
		}

		private bool LoginUser()
		{
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;

				if (!_webServerInteraction.CheckServer())
				{
					lblInformation.Text = "Could not contact license server.";
					return false;
				}

				_licenseData.Password = tbxPassword1.Password;
				new SaveDataToXML().SaveLicenseFile(_licenseData);
				bool success = false;

				lblInformation.Text = string.Empty;

				if (_licenseData.CompanyName == string.Empty || _licenseData.UserEmail == string.Empty ||
				    tbxPassword1.Password == string.Empty || tbxPassword2.Password == string.Empty)
				{
					lblInformation.Text = "All fields are required to continue.";
					return false;
				}

				if (tbxPassword1.Password != tbxPassword2.Password)
				{
					lblInformation.Text = "Passwords do not match.";
					return false;
				}

				if (!CheckForValidEmailAddress())
				{
					lblInformation.Text = "Please enter a valid e-mail address";
					return false;
				}

				string message = _sqlServerIntraction.CheckAndUpdateComputerID(_licenseData);
				_sqlServerIntraction.GetLicenseType(_licenseData);
				new SaveDataToXML().SaveLicenseFile(_licenseData);

				if (message == "0")
					success = true;
				else if (message == "1")
					lblInformation.Text = "User logged into a another computer.";
				else if (message == "2")
					lblInformation.Text = "No more seats available for this company.";
				else if (message == "3")
					lblInformation.Text = "Incorrect e-mail or password.";
				else if (message == "4")
					lblInformation.Text = "Incorrect company name. Name is case sensitive.";
				else if (message == "5")
					lblInformation.Text = "Company license expired.";
				else if (message == "6")
					lblInformation.Text = "Maximum number of computers activated for this user.";
				else
					lblInformation.Text = "License server error. Try again in a few minutes or contact Descon Support.";

				return success;
			}
			finally
			{
				Mouse.OverrideCursor = Cursors.Arrow;
			}
		}

		/// <summary>
		/// Does a basic check for a valid e-mail address. Returns true if valid, false if not
		/// </summary>
		private bool CheckForValidEmailAddress()
		{
			if (String.IsNullOrEmpty(_licenseData.UserEmail) || !(new Regex(@".+@{1}.+\.{1}.+").IsMatch(_licenseData.UserEmail)))
				return false;
			else
				return true;
		}

		private void btnDemo_Click(object sender, RoutedEventArgs e)
		{
			_licenseData.LicenseType = ELicenseType.Demo_1;
			new SaveDataToXML().SaveLicenseFile(_licenseData);
			DialogResult = true;
			Close();
		}

		private void btnLogin_MouseDown(object sender, MouseButtonEventArgs e)
		{
			lblInformation.Text = string.Empty;
		}

		private void btnLogin_Click(object sender, RoutedEventArgs e)
		{
			if (LoginUser())
			{
				DialogResult = true;
				Close();
			}
		}

		private void btnEULA_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (File.Exists(ConstString.FILE_EULA))
					Process.Start(ConstString.FILE_EULA);
				else
				{
					new FileManipulatation().WriteResourceToFile(ConstString.FILE_EULA);
					Process.Start(ConstString.FILE_EULA);
				}
			}
			catch
			{
				// Do nothing. File is probably already open.
			}
		}
	}
}