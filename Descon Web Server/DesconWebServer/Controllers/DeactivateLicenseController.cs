using System;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Descon.WebAccess;

namespace DesconWebServer.Controllers
{
    public class DeactivateLicenseController : ApiController
    {
		/// <summary>
		/// Called by http://access.desconplus.com/api/DeactivateLicense
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

				result = Request.CreateResponse(_sqlAddUpdate.DeactivateComputerSlot(userEmail, companyName));
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
