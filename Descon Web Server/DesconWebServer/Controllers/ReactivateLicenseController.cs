using System;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Descon.WebAccess;

namespace DesconWebServer.Controllers
{
    public class ReactivateLicenseController : ApiController
    {
		/// <summary>
		/// Called by http://access.desconplus.com/api/ReactivateLicense
		/// </summary>
		public HttpResponseMessage Post()
		{
			var _sqlAddUpdate = new SQLAddUpdate();
			HttpResponseMessage result;
			string[] stringSeparator = { "|||" };

			try
			{
				var str = HttpContext.Current.Request.Form;
				var licenseInfo = str.Get("userInfo").Split(stringSeparator, StringSplitOptions.None);

				string companyName = licenseInfo[0];
				string userEmail = licenseInfo[1];
				string computerID = licenseInfo[2];

				result = Request.CreateResponse(_sqlAddUpdate.ReactivateLicense(userEmail, companyName, computerID));
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