using System;
using System.Net;
using System.Text;
using Descon.Data;

namespace Descon.WebAccess
{
	public class WebServerInteraction
	{
		/// <summary>
		/// Contacts the web server and checks for a valid technical preview license
		/// </summary>
		public bool CheckServer()
		{
			using (var myWebClient = new WebClient())
			{
				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				try
				{
					var responseArray = myWebClient.DownloadString("http://access.desconplus.com/api/CheckServer");
					if (responseArray == "\"0\"")
						return true;
					else // try a second time to make sure there wasn't just a connection hiccup
					{
						responseArray = myWebClient.DownloadString("http://access.desconplus.com/api/CheckServer");
						if (responseArray == "\"0\"")
							return true;
					}
				}
				catch
				{
					return false;
				}
				return false;
			}
		}

		/// <summary>
		/// Contacts the web server and checks for a valid technical preview license
		/// </summary>
		public DateTime GetTime()
		{
			DateTime dateTime;

			using (var myWebClient = new WebClient())
			{
				myWebClient.Headers[HttpRequestHeader.ContentType] = "application/x-www-form-urlencoded";
				myWebClient.Encoding = Encoding.UTF7;
				try
				{
					var responseArray = myWebClient.DownloadString("http://access.desconplus.com/api/GetTime");
					responseArray = MiscMethods.ConvertHTMLReponseToString(responseArray);
					if (DateTime.TryParse(responseArray, out dateTime))
						return dateTime;
					else
						return DateTime.MaxValue;
				}
				catch
				{
					return DateTime.MaxValue;
				}
			}
		}

		public string SendFeedbackEmail(string fileName)
		{
			string response;

			try
			{
				using (var myWebClient = new WebClient())
				{
					var responseArray = myWebClient.UploadFile("http://access.desconplus.com/api/SubmitFeedback", fileName);
					response = MiscMethods.ConvertHTMLReponseToString(responseArray);
					if (response == "\"0\"")
						return "0";
				}

				return response;
			}
			catch (WebException ex)
			{
				string resultText = "Web Exception: " + ex.Response + " - " + ex.Message;
				if (ex.InnerException != null)
					resultText += " - Inner Exception: " + ex.InnerException.Message;
				return resultText;
			}
			catch (Exception ex)
			{
				string resultText = "General Exception: " + ex.Message;
				if (ex.InnerException != null)
					resultText += " - Inner Exception: " + ex.InnerException.Message;
				return resultText;
			}
		}
	}
}