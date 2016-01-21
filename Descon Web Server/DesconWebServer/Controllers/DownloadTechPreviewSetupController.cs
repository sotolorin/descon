using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
	public class DownloadTechPreviewSetupController : ApiController
	{
		private const string SetupFileLocation = "SetupFileLocation";

		//GET: http://preview.desconplus.com/api/DownloadTechPreviewSetup
		/// <summary>
		/// Either returns the latest version or downloads the latest version depending on the parameter
		/// </summary>
		public HttpResponseMessage Get()
		{
			string filePath = ConfigurationManager.AppSettings[SetupFileLocation];

			if (!File.Exists(filePath))
				return Request.CreateResponse(HttpStatusCode.NotFound, "Error: File Not Found");
			else
			{
				try
				{
					FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read);
					var response = new HttpResponseMessage();
					response.Content = new StreamContent(fileStream);
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					response.Content.Headers.ContentDisposition.FileName = "DesconTechPreviewSetup.exe";
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					response.Content.Headers.ContentLength = new FileInfo(filePath).Length;
					return response;
				}
				catch
				{
					return Request.CreateResponse(HttpStatusCode.SeeOther, "Error: Download Failed");
				}
			}
		}
	}
}