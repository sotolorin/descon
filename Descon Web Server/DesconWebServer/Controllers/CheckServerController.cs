using System.Net.Http;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
	/// <summary>
	/// Simply checks if the server is working
	/// </summary>
    public class CheckServerController : ApiController
    {
		/// <summary>
		/// Called by http://access.desconplus.com/api/CheckServer
		/// </summary>
		public HttpResponseMessage Get()
		{
			HttpResponseMessage result;

			result = Request.CreateResponse("0");

			return result;
		}
    }
}