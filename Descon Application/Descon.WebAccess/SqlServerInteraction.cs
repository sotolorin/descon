using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using Descon.Data;

namespace Descon.WebAccess
{
	public class SQLServerIntraction
	{
		private readonly SQLAddUpdate _sqlAddUpdate = new SQLAddUpdate();
		private readonly SQLRead _sqlRead = new SQLRead();

		/// <summary>
		/// Creates a database connection and returns an error string if something goes wrong. Returns string.Empty if connection is good.
		/// </summary>
		public string TestDatabaseConnection()
		{
			if (_sqlRead.TestDatabaseConnection() == string.Empty)
				return string.Empty;
			else // Tries a second time to make sure there wasn't just a minor hiccup
				return _sqlRead.TestDatabaseConnection();
		}

		#region SQL Adds

		/// <summary>
		/// Adds a new company to the database
		/// </summary>
		public string AddNewCompany(string companyName, string licenseType, int numberOfUsers, DateTime licenseEndDate)
		{
			return _sqlAddUpdate.AddNewCompany(companyName, licenseType, numberOfUsers, licenseEndDate);
		}

		/// <summary>
		/// Updates an existing company
		/// </summary>
		public string UpdateCompany(string companyName, string licenseType, int numberOfUsers, DateTime licenseEndDate)
		{
			return _sqlAddUpdate.UpdateCompany(companyName, licenseType, numberOfUsers, licenseEndDate);
		}

		/// <summary>
		/// Updates an existing company name
		/// </summary>
		public string UpdateCompanyName(string oldCompanyName, string newCompanyName)
		{
			return _sqlAddUpdate.UpdateCompanyName(oldCompanyName, newCompanyName);
		}

		/// <summary>
		/// Adds a new user to the database
		/// </summary>
		public string AddNewUser(string companyName, string userEmail, string userSupportID)
		{
			return _sqlAddUpdate.AddNewUser(companyName, userEmail, userSupportID);
		}

		/// <summary>
		/// Adds a new user to the database
		/// </summary>
		public string UpdateUserEmail(string oldEmail, string newEmail)
		{
			return _sqlAddUpdate.UpdateUserEmail(oldEmail, newEmail);
		}

		/// <summary>
		/// Resets any users password
		/// </summary>
		public string ResetUserPassword(string companyName, string userEmail)
		{
			return _sqlAddUpdate.ResetUserPassword(companyName, userEmail);
		}

		/// <summary>
		/// Resets any users computers
		/// </summary>
		public string ResetUserComputers(string companyName, string userEmail)
		{
			return _sqlAddUpdate.ResetUserComputers(companyName, userEmail);
		}

		/// <summary>
		/// Checks and then reactivates the license after a certain timer
		/// </summary>
		public string ReactivateLicense(LicensingData licenseData)
		{
			string computerID = ComputerID.GetComputerID();

			if (licenseData.UseLicenseProxy)
			{
				return new LicenseProxyClient().Connect(
					licenseData.ServerAddress,
					licenseData.Port,
					2 + "|" +
					licenseData.UserEmail + "|" +
					computerID);
			}
			else
			{
				var myWebClient = new WebClient();
				var values = new NameValueCollection();

				values.Add("userInfo", licenseData.CompanyName + "|||" + licenseData.UserEmail + "|||" + computerID);

				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				var responseArray = myWebClient.UploadValues("http://access.desconplus.com/api/ReactivateLicense", values);

				return MiscMethods.ConvertHTMLReponseToString(responseArray);
			}
		}

		/// <summary>
		/// Deactivates the license on exit
		/// </summary>
		public string DeactivateLicense(LicensingData licenseData)
		{
			if (licenseData.UseLicenseProxy)
			{
				return new LicenseProxyClient().Connect(
					licenseData.ServerAddress,
					licenseData.Port,
					2 + "|" +
					licenseData.UserEmail);
			}
			else
			{
				var myWebClient = new WebClient();
				var values = new NameValueCollection();

				values.Add("userInfo", licenseData.CompanyName + "|||" + licenseData.UserEmail);

				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				var responseArray = myWebClient.UploadValues("http://access.desconplus.com/api/DeactivateLicense", values);

				return MiscMethods.ConvertHTMLReponseToString(responseArray);
			}
		}

		#endregion

		#region SQL Reads

		public string CheckAndUpdateComputerID(LicensingData licenseData)
		{
			string computerID = ComputerID.GetComputerID();

			if (licenseData.UseLicenseProxy)
			{
				return new LicenseProxyClient().Connect(
					licenseData.ServerAddress,
					licenseData.Port,
					2 + "|" +
					licenseData.UserEmail + "|" +
					computerID);
			}
			else
			{
				//return new SQLRead().CheckAndUpdateComputerID(licenseData.CompanyName, licenseData.UserEmail, licenseData.Password, computerID);

				var myWebClient = new WebClient();
				var values = new NameValueCollection();

				values.Add("userInfo", licenseData.CompanyName + "|||" + licenseData.UserEmail + "|||" + licenseData.Password + "|||" + computerID);

				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				var responseArray = myWebClient.UploadValues("http://access.desconplus.com/api/CheckAndUpdateComputerID", values);

				return MiscMethods.ConvertHTMLReponseToString(responseArray);
			}
		}

		/// <summary>
		/// Communicates through the proxy client if neccessary
		/// </summary>
		public void GetLicenseType(LicensingData licenseData)
		{
			if (licenseData.UseLicenseProxy)
			{
				string licenseType = new LicenseProxyClient().Connect(
					licenseData.ServerAddress,
					licenseData.Port,
					1 + "|" +
					licenseData.UserEmail);
				licenseData.LicenseType = MiscMethods.ConvertFromStringToLicenseType(licenseType);
			}
			else
			{
				var myWebClient = new WebClient();
				var values = new NameValueCollection();

				values.Add("userInfo", licenseData.CompanyName + "|||" + licenseData.UserEmail);

				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				var responseArray = myWebClient.UploadValues("http://access.desconplus.com/api/GetLicenseType", values);

				licenseData.LicenseType = MiscMethods.ConvertFromStringToLicenseType(MiscMethods.ConvertHTMLReponseToString(responseArray));
			}
		}

		#endregion
	}
}