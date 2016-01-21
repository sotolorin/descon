using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Text;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Wordprocessing;

namespace DesconWeb
{
    public partial class SiteMaster : MasterPage
    {
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                //tb2.Text = watermarkName;
                //tb2.Font.Name = "CleanLight";
                //tb2.Attributes.Add("onfocus", "WatermarkFocus(this, '" + watermarkName + "');");
                //tb2.Attributes.Add("onblur", "WatermarkBlur(this, '" + watermarkName + "');");

                //tb3.Text = watermarkEmail;
                //tb3.Font.Name = "CleanLight";
                //tb3.Attributes.Add("onfocus", "WatermarkFocus(this, '" + watermarkEmail + "');");
                //tb3.Attributes.Add("onblur", "WatermarkBlur(this, '" + watermarkEmail + "');");

                //tb4.Text = watermarkCompany;
                //tb4.Font.Name = "CleanLight";
                //tb4.Attributes.Add("onfocus", "WatermarkFocus(this, '" + watermarkCompany + "');");
                //tb4.Attributes.Add("onblur", "WatermarkBlur(this, '" + watermarkCompany + "');");

                //tb5.Text = watermarkMsg;
                //tb5.Font.Name = "CleanLight";
                //tb5.Attributes.Add("onfocus", "WatermarkFocus(this, '" + watermarkMsg + "');");
                //tb5.Attributes.Add("onblur", "WatermarkBlur(this, '" + watermarkMsg + "');");
            }

        }

        string watermarkName = "Your Name";
        string watermarkEmail = "Your Email";
        string watermarkCompany = "Company Name";
        string watermarkMsg = "Message";

        protected void Submit(object sender, EventArgs e)
        {
            if (Page.IsValid)
            {
                try
                {
                    using (var smtp = new SmtpClient())
                    {
                        smtp.Host = "REXCHANGE.rexww.com";
                        smtp.Port = 25;
                        smtp.Timeout = 10000;
                        smtp.Credentials = new NetworkCredential("connect@desconplus.com", "123Initial");
                        smtp.UseDefaultCredentials = false;
                        smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                        smtp.EnableSsl = true;
                        var message = new MailMessage { From = new MailAddress("noreply@desconplus.com", "Descon Website") };
                        message.To.Add(new MailAddress("connect@desconplus.com"));
                        message.Subject = "Website Inquiry: " + "Message from " + tb2.Text + " Email: " + tb3.Text + " Company: " + tb4.Text + " " + DateTime.Now.ToShortDateString();
                        message.SubjectEncoding = Encoding.UTF8;
                        message.Body = message.Subject + "<br /><br />" + tb5.Text;
                        message.BodyEncoding = Encoding.UTF8;
                        message.IsBodyHtml = true;
                        smtp.Send(message);
                        ResetContact();
                    }
                    Alert("Thank you for your inquiry. We will respond within 1-2 business days.");
                }
                catch (SmtpException ex)
                {
                    //Console.Write(ex.Message)
                    Alert("There was an error sending your message.");

                }    
            }
        }

        private void Alert(string msg)
        {
            ScriptManager.RegisterClientScriptBlock(this, this.GetType(), "AlertBox", "alert('" + msg +"');", true);
        }

        private void ResetContact()
        {     
            //tb2.Text = watermarkName; 
            //tb3.Text = watermarkEmail; 
            //tb5.Text = watermarkMsg;
        }
    }
}