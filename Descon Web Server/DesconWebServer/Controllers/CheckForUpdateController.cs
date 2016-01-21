using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using DesconWebServer.Models;

namespace DesconWebServer.Controllers
{
	public class CheckForUpdateController : ApiController
	{
		private IFileProvider FileProvider { get; set; }

		//GET: http://access.desconplus.com/api/CheckForUpdate/1
		/// <summary>
		/// Either returns the latest version or downloads the latest version depending on the parameter
		/// </summary>
		/// <param name="id">1 = Return latest version as text, 2 = Returns latest installer file</param>
		public HttpResponseMessage Get(int id)
		{
			switch (id)
			{
				case 1:
					return GetLatestVersionNumber(true);
				case 2:
					return DownloadLatestVersion(true);
			}
			return Request.CreateResponse(HttpStatusCode.BadRequest, "Bad Request");
		}

		private HttpResponseMessage GetLatestVersionNumber(bool publicRelease)
		{
			FileProvider = new FileProvider(publicRelease);

			var response = Request.CreateResponse(HttpStatusCode.Accepted, FileProvider.GetLatestVersionText());
			return response;
		}

		public HttpResponseMessage DownloadLatestVersion(bool publicRelease)
		{
			FileProvider = new FileProvider(publicRelease);

			string filePath = FileProvider.GetFilePath(FileProvider.GetLatestVersionText() + ".exe");

			if (!FileProvider.Exists(filePath))
				return Request.CreateResponse(HttpStatusCode.NotFound, "Error: File Not Found");
			else
			{
				try
				{
					FileStream fileStream = FileProvider.Open(filePath);
					var response = new HttpResponseMessage();
					response.Content = new StreamContent(fileStream);
					response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
					if (publicRelease)
						response.Content.Headers.ContentDisposition.FileName = "Descon Setup.exe";
					else
						response.Content.Headers.ContentDisposition.FileName = filePath;
					response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
					response.Content.Headers.ContentLength = FileProvider.GetLength(filePath);
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