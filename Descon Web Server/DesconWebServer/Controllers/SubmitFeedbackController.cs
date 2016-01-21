using System;
using System.Configuration;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Web;
using System.Web.Http;

namespace DesconWebServer.Controllers
{
	public class SubmitFeedbackController : ApiController
	{
		private const string EmailToKey = "EmailTo"; //SupportDesk@desconplus.com
		private const string EmailFromKey = "EmailFrom";
		private const string ReportLocationKey = "FolderFeedbackReports";

		// Report Format:
		// Line 1: Company Name
		// Line 2: E-mail
		// Line 3: Version
		// Line 4: License Level
		// Line 5: Description
		// Line 6: Details

		/// <summary>
		/// Called by myWebClient.UploadFile("http://access.desconplus.com/api/SubmitFeedback", fileName);
		/// fileName is the path of the file to be uploaded on the local machine 
		/// </summary>
		public HttpResponseMessage Post()
		{
			HttpResponseMessage result;
			const string bugReportFileName = "BugReport.txt";
			string[] bugReportText;

			try
			{
				var httpRequest = HttpContext.Current.Request;
				if (httpRequest.Files.Count > 0)
				{
					foreach (string file in httpRequest.Files)
					{
						var postedFile = httpRequest.Files[file];

						if (postedFile != null)
						{
							string fullFilePath = ConfigurationManager.AppSettings[ReportLocationKey] + postedFile.FileName;
							string emailAddress = ConfigurationManager.AppSettings[EmailFromKey];
							string emailSubject = string.Empty;

							postedFile.SaveAs(fullFilePath);

							using (var archive = ZipFile.OpenRead(fullFilePath))
							{
								foreach (var entry in archive.Entries)
								{
									if (entry.FullName == bugReportFileName)
										entry.ExtractToFile(ConfigurationManager.AppSettings[ReportLocationKey] + bugReportFileName, true);
								}
								
								bugReportText = File.ReadAllLines(ConfigurationManager.AppSettings[ReportLocationKey] + bugReportFileName);

								if (bugReportText[1] != string.Empty)
									emailAddress = bugReportText[1];
								if (bugReportText[4] != string.Empty)
									emailSubject = bugReportText[4];
							}

							using (var smtp = new SmtpClient())
							{
								smtp.Host = "REXCHANGE.rexww.com";
								smtp.Port = 25;
								smtp.Timeout = 10000;
								smtp.UseDefaultCredentials = true;
								smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
								smtp.EnableSsl = true;

								var message = new MailMessage(emailAddress, ConfigurationManager.AppSettings[EmailToKey]);
								message.Subject = "Feedback Report: (" + emailSubject + " - " + DateTime.Now.ToShortDateString() + ")";
								message.SubjectEncoding = Encoding.UTF8;
								message.Body = BodyBuilder(bugReportText);
								message.BodyEncoding = Encoding.UTF8;
								message.IsBodyHtml = true;
								message.Attachments.Add(new Attachment(fullFilePath));

								smtp.Send(message);
							}
						}
					}
				}

				result = Request.CreateResponse("0");
			}
			catch (SmtpException ex)
			{
				string resultText = "SMTP Exception: " + ex.StatusCode + " - " + ex.Message;
				if (ex.InnerException != null)
					resultText += " - Inner Exception: " + ex.InnerException.Message;

				result = Request.CreateResponse(resultText);
			}
			catch (Exception ex)
			{
				string resultText = "General Exception: " + ex.Message;
				if (ex.InnerException != null)
					resultText += " - Inner Exception: " + ex.InnerException.Message;

				result = Request.CreateResponse(resultText);
			}

			return result;
		}

		private string BodyBuilder(string[] bugReportText)
		{
			var body = new StringBuilder();
			body.Append("<font face=\"verdana\" size=2>");
			body.Append("<b>Message sent through Descon feedback form.</b><br/><br/>");
			body.Append("<b>Date:</b> " + DateTime.Now + "<br/><br/>");
			body.Append("<b>Company Name: </b>" + bugReportText[0] + "<br>");
			body.Append("<b>E-mail: </b>" + bugReportText[1] + "<br>");
			body.Append("<b>Version: </b>" + bugReportText[2] + "<br>");
			body.Append("<b>License Level: </b>" + bugReportText[3] + "<br>");
			body.Append("<b>Description: </b>" + bugReportText[4] + "<br>");
			body.Append("<b>Details: </b>" + bugReportText[5] + "<br>");
			body.Append("</font>");

			return body.ToString();
		}
	}
}