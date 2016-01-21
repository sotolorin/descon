namespace Descon.Data
{
	// No XML comments here for security
	public sealed class LicensingData
	{
		public LicensingData()
		{
			LicenseType = ELicenseType.Demo_1;
		}

		// Code for the license type
		public ELicenseType LicenseType { get; set; }
		// Full name of the user
		public string CompanyName { get; set; }
		// User E-mail address
		public string UserEmail { get; set; }
		// User Password
		public string Password { get; set; }
		// True if license server required to connect. Requires ServerAddress and Port
		public bool UseLicenseProxy { get; set; }
		public string ServerAddress { get; set; }
		public int Port { get; set; }
	}
}