using System;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using Descon.WebAccess;

namespace DesconWebServer.Controllers
{
    public class GetLicenseTypeController : ApiController
    {
		/// <summary>
		/// Called by http://preview.desconplus.com/api/GetLicenseType
		/// </summary>
		public HttpResponseMessage Post()
		{
			SQLRead _sqlRead = new SQLRead();
			HttpResponseMessage result;
			string[] stringSeparator = { "|||" };

			try
			{
				var str = HttpContext.Current.Request.Form;
				var licenseInfo = str.Get("userInfo").Split(stringSeparator, StringSplitOptions.None);

				string companyName = licenseInfo[0];
				string userEmail = licenseInfo[1];

				result = Request.CreateResponse(_sqlRead.GetLicenseType(userEmail, companyName));
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