using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows;
using Descon.Data;
using Descon.Forms;
using Microsoft.Win32;

namespace Descon.Main
{
	/*	The following are the different available licensing tiers. The abbreviations and property names are crazy to obscure
		the data. The date and user name will be psuedo encrypted using Base64. 
		
		See Documentation\Licensing Information.rtf for more info
		 
		FL	(License Type)
		KD	(First run date)
		WC	(User Email)
		MV	(User Name)
		HP	(Company Name)
		TC	(Installed correctly flag)

		Application installed properly: TC = "GFVDBD"
		That string will be set by the installer so we know someone didn't just copy the program onto their PC
	*/

	public class Licensing
	{
		// Not XML comment for security. Loads each registry key and adds missing ones. 
		internal bool LoadLicensing()
		{
			string licenseType = string.Empty;
			DateTime dateOfLaunch = DateTime.Today;
			string installedProperly = string.Empty;
			string date = string.Empty;
			string version = string.Empty;

			try
			{
				using(var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\\Descon Plus", true))
				{
					if (key != null)
					{
						// First check and make sure Descon was installed properly. This key is added by the installer itself.
						installedProperly = (string)key.GetValue("TC");
						if (installedProperly != "GFVDBD")
						{
							MessageBox.Show("Descon not installed properly. Please reinstall or contact Descon support",
								"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
							return false;
						}

						// Next we try to get the license. If it is missing, set it to Technical Preview (TP) for now.
						// Once the technical preview is over, we will set it to DEMO (DP)
						if ((string)key.GetValue("FL") != null)
							licenseType = (string)key.GetValue("FL");
						else
						{
							key.SetValue("FL", "TP");
							licenseType = (string)key.GetValue("FL");
						}

						// Next check the launch date and if this one is missing, add today's date as the date of first launch.
						// This is used to keep track of the length of the demo.
						if ((string)key.GetValue("KD") != null)
							date = Encoding.ASCII.GetString(Convert.FromBase64String((string)key.GetValue("KD")));
						else
						{
							key.SetValue("KD", Convert.ToBase64String(Encoding.ASCII.GetBytes(DateTime.Today.ToLongDateString())));
							date = Encoding.ASCII.GetString(Convert.FromBase64String((string)key.GetValue("KD")));
						}

						// Now check the user email
						if ((string)key.GetValue("WC") != null)
							CommonDataStatic.UserEmail = Encoding.UTF7.GetString(Convert.FromBase64String((string)key.GetValue("WC")));

						// Next check the user name
						if ((string)key.GetValue("MV") != null)
							CommonDataStatic.UserName = Encoding.UTF7.GetString(Convert.FromBase64String((string)key.GetValue("MV")));

						// Next check the company name
						if ((string)key.GetValue("HP") != null)
							CommonDataStatic.CompanyName = Encoding.UTF7.GetString(Convert.FromBase64String((string)key.GetValue("HP")));

						if (!string.IsNullOrEmpty(date))
							dateOfLaunch = DateTime.Parse(date);
					}
				}
			}
			catch (Exception)
			{
				MessageBox.Show("Error Reading Registry. Please contact Descon support.",
					"ERROR", MessageBoxButton.OK, MessageBoxImage.Error);
				return false;
			}

			// Gets the version number from the exe so we don't have to worry about setting it in two places
			version = MiscMethods.GetVersionString();

			switch (licenseType)
			{
				case "EO":
					CommonDataStatic.LicenseType = ELicenseType.Entry_1;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Entry License)";
					break;
				case "BI":
					CommonDataStatic.LicenseType = ELicenseType.Basic_2;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Basic License)";
					break;
				case "SU":
					CommonDataStatic.LicenseType = ELicenseType.Standard_3;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Standard License)";
					break;
				case "FY":
					CommonDataStatic.LicenseType = ELicenseType.Full_4;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Full License)";
					break;
				case "PT":
					CommonDataStatic.LicenseType = ELicenseType.Premium_5;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Premium License)";
					break;
				case "TP":	// Temporary Technical Preview License
					CommonDataStatic.LicenseType = ELicenseType.Technical_Preview_6;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (Technical Preview)";
					break;
				default:
					CommonDataStatic.LicenseType = ELicenseType.Demo_0;
					CommonDataStatic.LicenseTypeDisplay = "Descon " + version + " (DEMO MODE)";
					break;
			}

			if (CommonDataStatic.LicenseType == ELicenseType.Demo_0)
			{
				var internetTime = GetInternetTime();

				if (internetTime == DateTime.MinValue)
					return false;

				int numberOfDaysLeftInTrial = 10 - (int)(internetTime - dateOfLaunch).TotalDays;
				return new FormLicensing(numberOfDaysLeftInTrial, false).ShowDialog() ?? false;
			}
			else if (CommonDataStatic.LicenseType == ELicenseType.Technical_Preview_6)
			{
				var endDate = new DateTime(2015, 5, 31);

				if (GetInternetTime() > endDate)
				{
					MessageBox.Show("This release of the technical preview is no longer valid as of " + endDate.ToShortDateString());
					return false;
				}
				else
					return new FormLicensing(0, false).ShowDialog() ?? false;
			}

			return true;
		}

		private DateTime GetInternetTime()
		{
			try
			{
				Thread.Sleep(2000);		// The server can not be read more than once every 4 seconds. This adds little buffer
				var client = new TcpClient("time.nist.gov", 13);
				using (var streamReader = new StreamReader(client.GetStream()))
				{
					var response = streamReader.ReadToEnd();
					var utcDateTimeString = response.Substring(7, 8);
					return DateTime.ParseExact(utcDateTimeString, "yy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
				}
			}
			catch(Exception)
			{
				//MessageBox.Show("Internet connection required in Preview mode.");
				return DateTime.MinValue;
			}
		}
	}
}