using System;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNet.Membership.OpenAuth;
using Xrm;

namespace DesconWeb.Account
{
    public partial class Register : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            RegisterUser.ContinueDestinationPageUrl = Request.QueryString["Profile.aspx"];
            InitializeProperties();
        }

        public Guid LeadId;
        public Guid AccountId;
        public TextBox UserName;
        public TextBox Email;
        public TextBox DesconPassword;
        public TextBox Password;

        void InitializeProperties()
        {
            AccountId = Guid.NewGuid();
            LeadId = Guid.NewGuid();
            if(Master == null)return;
            var mainContent = Master.FindControl("PortalMainContent");
            var wzd = (CreateUserWizard)mainContent.FindControl("RegisterUser");
            var step1 = (CreateUserWizardStep)wzd.FindControl("RegisterUserWizardStep");
            if (step1 != null)
            {
                UserName = (TextBox)step1.ContentTemplateContainer.FindControl("UserName");
                Email = (TextBox)step1.ContentTemplateContainer.FindControl("Email");
                DesconPassword = (TextBox)step1.ContentTemplateContainer.FindControl("DesconPassword");
                Password = (TextBox)step1.ContentTemplateContainer.FindControl("Password");
            }

        }

        protected void RegisterUser_CreatedUser(object sender, EventArgs e)
        {
            //Verify account is created
            var saveResponse = SaveAccount();
            if (saveResponse.Success)
            {
                FormsAuthentication.SetAuthCookie(RegisterUser.UserName, createPersistentCookie: false);

                string continueUrl = RegisterUser.ContinueDestinationPageUrl;
                //if (!OpenAuth.IsLocalUrl(continueUrl))
                //{
                //    continueUrl = "~/";
                //}
                Response.Redirect("Profile.aspx?_acctId=" + LeadId + "&email=" + Email.Text);
            }
            else Response.Redirect("~/Pages/Error.aspx");
            
        }

        protected GenericResponse SaveAccount()
        {
            try
            {
                var response = new GenericResponse();
                using (var context = new XrmServiceContext())
                {
                    var account = new AccountLeads
                    {
                        Id = AccountId,
                        AccountLeadId = LeadId,
                        LogicalName = UserName.Text
                    };
                    var lead = new Lead
                    {
                        Id = LeadId,
                        CompanyName = UserName.Text,
                        EMailAddress1 = Email.Text,
                    };
                    context.AddObject(account);
                    context.AddObject(lead);
                    context.SaveChanges();
                }
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {        
                return new GenericResponse{FailureInformation = ex.Message};
            }

        }

        public class GenericResponse
        {
            public GenericResponse()
            {
                Success = false;
                FailureInformation = string.Empty;
            }

            public bool Success { get; set; }

            public string FailureInformation { get; set; }
        }
    }
}