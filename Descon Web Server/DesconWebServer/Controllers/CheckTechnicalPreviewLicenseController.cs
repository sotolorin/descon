using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Web;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
	public class CheckTechnicalPreviewLicenseController : ApiController
	{
		private const string FileApprovalListLocationKey = "FileApprovalListLocation";

		/* Response 1 = Valid license information with exact match
		 * Response 0 = Invalid license information
		 * Response 2 = MAC address is invalid for current user meaning they tried to register on more than one computer
		 * Response 3 = MAC address already in use
		 */

		/// <summary>
		/// Called by http://preview.desconplus.com/api/CheckTechnicalPreviewLicense
		/// </summary>
		public HttpResponseMessage Post()
		{
			HttpResponseMessage result = null;

			try
			{
				var str = HttpContext.Current.Request.Form;
				string licenseInfo = str.Get("userInfo");

				if (File.Exists(ConfigurationManager.AppSettings[FileApprovalListLocationKey]))
				{
					var lines = File.ReadAllLines(ConfigurationManager.AppSettings[FileApprovalListLocationKey], Encoding.UTF7);
					
					if (lines.Any(l => l == licenseInfo))
						result = Request.CreateResponse(1);
					else
					{
						var splitLicenseInfo = licenseInfo.Split('|');
						result = Request.CreateResponse(0);

						// Test to see if any of the MAC's are already in use
						foreach (var line in lines)
						{
							var splitLine = line.Split('|');

							if (splitLine[3] == splitLicenseInfo[3])
								result = Request.CreateResponse(3);
						}

						// Check to see if only the MAC address doesn't match. Can override above
						foreach (var line in lines)
						{
							var splitLine = line.Split('|');

							if (splitLine[0] == splitLicenseInfo[0] && 
								splitLine[1] == splitLicenseInfo[1] && 
								splitLine[2] == splitLicenseInfo[2] &&
							    splitLine[3] != splitLicenseInfo[3])
								result = Request.CreateResponse(2);
						}
					}
				}
				else
					result = Request.CreateResponse(0);
			}
			catch (HttpException ex)
			{
				result = Request.CreateResponse(ex.ErrorCode + " - " + ex.Message);
			}
			catch (Exception ex)
			{
				result = Request.CreateResponse(ex.Message);
			}

			return result;
		}
	}
}