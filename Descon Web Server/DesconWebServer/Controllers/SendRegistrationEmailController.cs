using System;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;
using DesconWebServer.Models;

namespace DesconWebServer.Controllers
{
	public class SendRegistrationEmailController : ApiController
	{
		private const string EmailToKey = "EmailToSales";
		private const string EmailFromKey = "EmailFrom";
		private const string EmailPasswordKey = "EmailPassword";

		/// <summary>
		/// Called by http://preview.desconplus.com/api/SendRegistrationEmail
		/// </summary>
		public HttpResponseMessage Post()
		{
			HttpResponseMessage result = null;

			try
			{
				string email;
				string name;
				string company;
				string mac;

				var str = HttpContext.Current.Request.Form;

				email = str.Get("email");
				name = str.Get("name");
				company = str.Get("company");
				mac = str.Get("mac");

				// Sets up the SMTP server and certificate to send an e-mail
				ServicePointManager.ServerCertificateValidationCallback = Certificates.CertificateValidation;

				using (var smtp = new SmtpClient())
				{
					smtp.Host = "mail.desconplus.com";
					smtp.Port = 587;
					smtp.Timeout = 20000;
					smtp.UseDefaultCredentials = false;
					smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
					smtp.Credentials = new NetworkCredential(ConfigurationManager.AppSettings[EmailFromKey],
						ConfigurationManager.AppSettings[EmailPasswordKey]);
					smtp.EnableSsl = true;

					var message = new MailMessage(ConfigurationManager.AppSettings[EmailFromKey],
						ConfigurationManager.AppSettings[EmailToKey]);
					message.Subject = "New Technical Preview Registration: " + DateTime.Now.ToShortDateString();
					message.SubjectEncoding = Encoding.UTF8;
					message.Body = BodyBuilder(email, name, company, mac);
					message.BodyEncoding = Encoding.UTF8;
					message.IsBodyHtml = true;

					smtp.Send(message);
				}
				result = Request.CreateResponse(HttpStatusCode.Created, "Created and Sent");
			}
			catch(HttpException ex)
			{
				result = Request.CreateResponse(ex.ErrorCode + " - " + ex.Message);
			}
			catch (SmtpException ex)
			{
				result = Request.CreateResponse(ex.StatusCode + " - " + ex.Message);
			}
			catch (Exception ex)
			{
				result = Request.CreateResponse(ex.Message);
			}

			return result;
		}

		private string BodyBuilder(string email, string name, string company, string mac)
		{
			// This prevents e-mail address and other parts of the message from being autoformatted as links
			email = email.Replace(".", "<span style=\"font-size:0.1em\">&nbsp;</span>.");
			email = email.Replace("@", "<span style=\"font-size:0.1em\">&nbsp;</span>@");

			var body = new StringBuilder();
			body.Append("<font face=\"verdana\" size=2>");
			body.Append("<b>AUTOMATED MESSAGE. DO NOT REPLY.</b><br/><br/>");
			body.Append("A user with the information below has installed the technical preview for the first time and needs to be approved. " +
			            "If the user is approved to use the technical preview and all information is valid, please forward the following information to " +
			            "Mike Trocco (m.trocco@desconplus) and he will add the user to the approval list.<br/><br/>");
			body.Append("<b>E-mail Address:</b> " + email + "<br/><br/>");
			body.Append("<b>Name:</b> " + name + "<br/><br/>");
			body.Append("<b>Company Name:</b> " + company + "<br/><br/>");
			body.Append("<b>MAC Address:</b> " + mac + "<br/><br/>");
			body.Append("</font>");

			return body.ToString();
		}
	}
}