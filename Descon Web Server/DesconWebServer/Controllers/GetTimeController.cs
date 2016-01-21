using System;
using System.Net.Http;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
    public class GetTimeController : ApiController
    {
		/// <summary>
		/// Called by http://access.desconplus.com/api/GetTime
		/// </summary>
		public HttpResponseMessage Get()
		{
			HttpResponseMessage result;

			result = Request.CreateResponse(DateTime.Now.ToShortDateString());

			return result;
		}
    }
}