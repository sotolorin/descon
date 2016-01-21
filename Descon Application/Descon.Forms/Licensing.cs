using System;
using System.IO;
using System.Text;
using System.Windows;
using Descon.Data;
using Descon.WebAccess;
using Microsoft.Win32;

namespace Descon.Forms
{
	public class Licensing
	{
		// No XML comment for security.
		public bool LoadLicensingData()
		{
			string version;
			string licenseFormTitle = string.Empty;
			var sqlServerInteraction = new SQLServerIntraction();
			var webServerInteraction = new WebServerInteraction();

			DateTime firstLaunchDate;
			int numberOfDaysLeftInTrial;
			bool success;

			if (!Directory.Exists(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA))
				Directory.CreateDirectory(ConstString.FOLDER_MYDOCUMENTS_DESCON_PROGRAMDATA);
			if (!File.Exists(ConstString.FILE_LICENSE))
				new SaveDataToXML().SaveLicenseFile(new LicensingData());

			var licenseData = new LoadDataFromXML().LoadLicenseFile();

			// First we just check the registry information to make sure the program has been installed correctly
			try
			{
				using (var key = Registry.LocalMachine.OpenSubKey("SOFTWARE\\WOW6432Node\\REXWW"))
				{
					string keyValue;

					if (key != null)
					{
						// First check and make sure Descon was installed properly (TC). This key is added by the installer.
						keyValue = (string)key.GetValue("RW"); // Unencoded string: lpzpdaatzxenbaoffbveplnmfrzmgr
						if (keyValue != Encoding.UTF8.GetString(Convert.FromBase64String("bHB6cGRhYXR6eGVuYmFvZmZidmVwbG5tZnJ6bWdy"))) // Base64 encoded string
						{
							MessageBox.Show("Descon not installed properly. Please reinstall or contact Descon support.",
								"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
							return false;
						}
					}
					else
					{
						MessageBox.Show("Descon not installed properly. Please reinstall or contact Descon support.",
								"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
						return false;
					}
				}

				using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Descon Plus", true))
				{
					string keyValue;

					if (key != null)
					{
						// Then check the First Launch Date (HR) value
						keyValue = (string)key.GetValue("HR");
						// If date is not null, then grab it
						if (keyValue != null)
							keyValue = Encoding.UTF8.GetString(Convert.FromBase64String((string)key.GetValue("HR")));

						// If the date is null or doesn't parse properly, try to set it to the internet time
						if (keyValue == null || !DateTime.TryParse(keyValue, out firstLaunchDate)) // Base64 encoded string
						{
							string internetTime = DateTime.MaxValue.ToShortDateString();

							if (webServerInteraction.CheckServer())
								internetTime = webServerInteraction.GetTime().ToShortDateString();

							byte[] dateBytes = Encoding.UTF8.GetBytes(internetTime);
							key.SetValue("HR", Convert.ToBase64String(dateBytes));
							firstLaunchDate = DateTime.Today;
						}
						else // Date is ok. If it is the max value, try to set it to internet time
						{
							DateTime.TryParse(keyValue, out firstLaunchDate);
							if (firstLaunchDate == DateTime.MaxValue)
							{
								if (webServerInteraction.CheckServer())
									firstLaunchDate = webServerInteraction.GetTime();
							}
						}

						// Then check if we have developer license installed properly setup and if not then set back to demo mode
						if (licenseData.LicenseType == ELicenseType.Developer_0)
						{
							keyValue = (string)key.GetValue("WS");
							if (keyValue != Encoding.UTF8.GetString(Convert.FromBase64String("WkVCUU1M"))) // Base64 encoded string
								licenseData.LicenseType = ELicenseType.Demo_1;
						}
					}
					else
					{
						MessageBox.Show("Descon not installed properly. Please reinstall or contact Descon support.",
							"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
						return false;
					}
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Error Reading Registry. Please contact Descon support.", "ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// Gets the version number from the exe
			version = MiscMethods.GetVersionString();

			// Developer mode
			if (licenseData.LicenseType == ELicenseType.Developer_0)
					success = true;
			else if (!webServerInteraction.CheckServer()) // No internet access needs to force demo
			{
				int numberOfDays = 7 - (int)(DateTime.Today - firstLaunchDate).TotalDays;

				if (numberOfDays > 0 && numberOfDays <= 7)
					success = true;
				else
					success = new FormLicense(" (0 days left in trial)", 0, false).ShowDialog() ?? false;
			}
			else // Internet is available
			{
				// Demo mode
				if (licenseData.LicenseType == ELicenseType.Demo_1)
				{
					var internetTime = webServerInteraction.GetTime();
					if (internetTime != DateTime.MaxValue)
						numberOfDaysLeftInTrial = 7 - (int)(internetTime - firstLaunchDate).TotalDays;
					else // Time server check failed
						numberOfDaysLeftInTrial = 0;

					if (numberOfDaysLeftInTrial < 0)
						numberOfDaysLeftInTrial = 0;

					licenseFormTitle = " (" + numberOfDaysLeftInTrial + " days left in trial)";
					success = new FormLicense(licenseFormTitle, numberOfDaysLeftInTrial, false).ShowDialog() ?? false;
					licenseData = new LoadDataFromXML().LoadLicenseFile();
				}
				// Normal license
				else
				{
					sqlServerInteraction.GetLicenseType(licenseData);

					// This checks for a valid license and double checks the license type on the server. If we have one, we can
					// change the first launch date to today. Otherwise, we check to make sure there is still offline time left 
					// and if not, we set Descon to Demo mode.
					if (licenseData.LicenseType != ELicenseType.NoMatch)
					{
						string result = sqlServerInteraction.CheckAndUpdateComputerID(licenseData);

						if (result == "0")
							success = true;
						else
							success = new FormLicense(licenseFormTitle, 0, false).ShowDialog() ?? false;

						using (var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true))
						{
							if (key != null)
							{
								byte[] dateBytes = Encoding.UTF8.GetBytes(DateTime.Today.ToShortDateString());
								key.SetValue("HR", Convert.ToBase64String(dateBytes));
							}
						}
					}
					else
						success = new FormLicense(licenseFormTitle, 0, false).ShowDialog() ?? false;

					licenseData = new LoadDataFromXML().LoadLicenseFile();
					sqlServerInteraction.GetLicenseType(licenseData);
				}
			}

			switch (licenseData.LicenseType)
			{
				case ELicenseType.Developer_0:
					CommonDataStatic.LicenseType = ELicenseType.Developer_0;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (DEVELOPER)";
					break;
				case ELicenseType.Demo_1:
					CommonDataStatic.LicenseType = ELicenseType.Demo_1;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (DEMO MODE)";
					break;
				case ELicenseType.Open_2:
					CommonDataStatic.LicenseType = ELicenseType.Open_2;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Open License)";
					break;
				case ELicenseType.Basic_3:
					CommonDataStatic.LicenseType = ELicenseType.Basic_3;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Basic License)";
					break;
				case ELicenseType.Standard_4:
					CommonDataStatic.LicenseType = ELicenseType.Standard_4;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Standard License)";
					break;
				case ELicenseType.Next_5:
					CommonDataStatic.LicenseType = ELicenseType.Next_5;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Next License)";
					break;
				default:
					CommonDataStatic.LicenseType = ELicenseType.Demo_1;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (DEMO MODE)";
					break;
			}

			if (CommonDataStatic.LicenseType != ELicenseType.Open_2 && CommonDataStatic.LicenseType != ELicenseType.Demo_1)
			{
				string internetTime;

				using (var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Descon Plus", true))
				{
					if (key != null && webServerInteraction.CheckServer())
					{
						internetTime = webServerInteraction.GetTime().ToShortDateString();

						byte[] dateBytes = Encoding.UTF8.GetBytes(internetTime);
						key.SetValue("HR", Convert.ToBase64String(dateBytes));
					}
				}
			}

			new SaveDataToXML().SaveLicenseFile(licenseData);

			CommonDataStatic.CompanyName = licenseData.CompanyName;

			return success;
		}
	}
}