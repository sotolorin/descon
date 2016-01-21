using System.Net.Http;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
	public class DownloadDesconSetupController : ApiController
	{
		//GET: http://access.desconplus.com/api/DownloadDesconSetup
		/// <summary>
		/// Downloads the latest version of the installer
		/// </summary>
		public HttpResponseMessage Get()
		{
			return new CheckForUpdateController().DownloadLatestVersion(true);
		}
	}

	public class DownloadInternalDesconSetupController : ApiController
	{
		//GET: http://access.desconplus.com/api/DownloadInternalDesconSetup
		/// <summary>
		/// Downloads the latest version of the installer
		/// </summary>
		public HttpResponseMessage Get()
		{
			return new CheckForUpdateController().DownloadLatestVersion(false);
		}
	}
}