using System.Collections.Generic;
using Descon.Data;

namespace Utilities
{
	public class CommonData
	{
		public Dictionary<ELicenseType, string> LicenseTypes
		{
			get { return CommonDataStatic.CommonLists.LicenseTypes; }
		}
	}
}