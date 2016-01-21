using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using Descon.Data;
using Descon.UI.DataAccess;
using Microsoft.Win32;

namespace Descon.Forms
{
	/// <summary>
	/// Licensing form used to register the software
	/// </summary>
	public partial class FormLicensing
	{
		private readonly bool _launchedFromDescon;

		public FormLicensing(int trialDaysLeft, bool launchedFromDescon)
		{
			InitializeComponent();

			Owner = Application.Current.MainWindow;

			_launchedFromDescon = launchedFromDescon;

			if (_launchedFromDescon)
			{
				btnLaunch.Content = "OK";
				btnClose.Visibility = Visibility.Collapsed;
				gbxUserInformation.IsEnabled = false;
			}

			tbxEmail.Text = CommonDataStatic.UserEmail;
			tbxName.Text = CommonDataStatic.UserName;
			tbxCompanyName.Text = CommonDataStatic.CompanyName;

			if (CommonDataStatic.LicenseType != ELicenseType.Demo_0)
			{
				lblLicenseInformation.Text = ConstString.LICENSE_CURRENT + " " +
				                             CommonDataStatic.CommonLists.LicenseList.First(l => l.Key == CommonDataStatic.LicenseType).Value;
				rbEnterLicenseKey.IsChecked = true;
			}
			else if (trialDaysLeft > 0)	// Demo and still have time left
			{
				lblLicenseInformation.Text = ConstString.LICENSE_TRIAL + " " + trialDaysLeft;
				rbFreeTrial.IsChecked = true;
			}
			else // Demo and no trial time left
			{
				lblLicenseInformation.Text = ConstString.LICENSE_TRIAL_ENDED;
				rbEnterLicenseKey.IsChecked = true;
			}
		}

		// Attempts to check for a valid registration and then launch the software.
		private bool TryToLaunch()
		{
			WebServerInteraction webServer = new WebServerInteraction();

			if (!ValidateFields())
				return false;
			try
			{
				Mouse.OverrideCursor = Cursors.Wait;
				var webResult = webServer.CheckForTechPreviewLicense(tbxEmail.Text, tbxName.Text, tbxCompanyName.Text, MiscFormMethods.GetMACAddress());
				Mouse.OverrideCursor = Cursors.Arrow;

				switch (webResult)
				{
					case ELicenseCheckResult.Invalid:
					case ELicenseCheckResult.DuplicateMacAddress:
						MessageBox.Show("Credentials not valid for technical preview.\r\r" +
						                "Please register your software or contact Descon support at: support@desconplus.com.",
							"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						return false;
					case ELicenseCheckResult.MacAddressInvalid:
						MessageBox.Show("User registered on a different computer.\r\r" +
						                "Please re-enter your information or contact Descon support at: support@desconplus.com.",
							"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
						return false;
				}

				return true;
			}
			catch (Exception ex)
			{
				MessageBox.Show("Problem contacting server, please try again in a few moments: " + ex.Message, "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}
		}

		// Sends registration e-mail if the user is not already registered AND they haven't already registered at a different computer
		private void TryToSendRegistrationEmail()
		{
			bool validRegistration = true;
			WebServerInteraction webServer = new WebServerInteraction();

			if (ValidateFields())
			{
				var result = MessageBox.Show("Your user information will be sent to Descon sales (sales@desconplus.com) for approval.\r\r" + "You should be approved within 1 workday. Proceed?",
					"Warning", MessageBoxButton.OKCancel, MessageBoxImage.Warning);
				if (result == MessageBoxResult.OK)
				{
					Mouse.OverrideCursor = Cursors.Wait;
					var webResult = webServer.CheckForTechPreviewLicense(tbxEmail.Text, tbxName.Text, tbxCompanyName.Text, MiscFormMethods.GetMACAddress());
					Mouse.OverrideCursor = Cursors.Arrow;

					switch (webResult)
					{
						case ELicenseCheckResult.Valid:
							MessageBox.Show("User registration has been completed. You can now launch Descon 8.",
								"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
							validRegistration = false;
							break;
						case ELicenseCheckResult.MacAddressInvalid:
							MessageBox.Show("User already registered on a different computer.\r\r" +
							                "Please re-enter your information or contact Descon support at: support@desconplus.com.",
								"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
							validRegistration = false;
							break;
						case ELicenseCheckResult.DuplicateMacAddress:
							MessageBox.Show("Another user is already registered on this computer.\r\r" +
											"Please enter that user's information or contact Descon support at: support@desconplus.com.",
								"Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
							validRegistration = false;
							break;
					}

					if (validRegistration)
					{
						Mouse.OverrideCursor = Cursors.Wait;
						webServer.SendRegistrationEmail(tbxEmail.Text, tbxName.Text, tbxCompanyName.Text, MiscFormMethods.GetMACAddress());
						Mouse.OverrideCursor = Cursors.Arrow;
						WriteValuesToRegistry();
					}
				}
			}
		}

		private bool ValidateFields()
		{
			if (MiscFormMethods.CheckForValidEmailAddress(tbxEmail.Text))
				CommonDataStatic.UserEmail = tbxEmail.Text;
			else
			{
				MessageBox.Show("Please enter a valid e-mail address.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}

			if (string.IsNullOrEmpty(tbxName.Text))
			{
				MessageBox.Show("Please enter a valid Name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}
			else
				CommonDataStatic.UserName = tbxName.Text;

			if (string.IsNullOrEmpty(tbxCompanyName.Text))
			{
				MessageBox.Show("Please enter a valid Company Name.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
				return false;
			}
			else
				CommonDataStatic.CompanyName = tbxCompanyName.Text;

			return true;
		}

		private void WriteValuesToRegistry()
		{
			var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true);
			if (key != null)
			{
				key.SetValue("WC", Convert.ToBase64String(Encoding.UTF7.GetBytes(CommonDataStatic.UserEmail)));
				key.SetValue("MV", Convert.ToBase64String(Encoding.UTF7.GetBytes(CommonDataStatic.UserName)));
				key.SetValue("HP", Convert.ToBase64String(Encoding.UTF7.GetBytes(CommonDataStatic.CompanyName)));
			}
		}

		private void btnRegister_Click(object sender, RoutedEventArgs e)
		{
			TryToSendRegistrationEmail();
		}

		private void btnLaunch_Click(object sender, RoutedEventArgs e)
		{
			if (_launchedFromDescon)
				Close();
			else if (TryToLaunch())
			{
				WriteValuesToRegistry();
				DialogResult = true;
				Close();
			}
		}

		private void btnClose_Click(object sender, RoutedEventArgs e)
		{
			DialogResult = false;
			Close();
		}

		private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
		{
			Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
			e.Handled = true;
		}
	}
}