using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Web.Mvc;
using Xrm;


namespace Site.Areas.Account.ViewModels
{
	public class SignUpLocalViewModel
	{
	    public string Username { get; set; }
        public string Company { get; set; }
	    public string Password { get; set; }
		public string ConfirmPassword { get; set; }
		public string Email { get; set; }
		public string Question { get; set; }
		public string Answer { get; set; }

        protected void RegisterUser()
        {
            //Verify account is created
            //var server = new WebServerInteraction();
            //var foo = server.Connect();
            //var foo2 = foo;
            //Console.WriteLine(server.Connect());

            //var saveResponse = SaveAccount();
            //if (saveResponse.Success)
            //{
            //    FormsAuthentication.SetAuthCookie(RegisterUser.UserName, createPersistentCookie: false);

            //    string continueUrl = RegisterUser.ContinueDestinationPageUrl;
            //    //if (!OpenAuth.IsLocalUrl(continueUrl))
            //    //{
            //    //    continueUrl = "~/";
            //    //}
            //    Response.Redirect("Profile.aspx?_acctId=" + LeadId + "&email=" + Email.Text);
            //}
            //else Response.Redirect("~/Pages/Error.aspx");

        }

        //protected GenericResponse SaveAccount()
        //{
        //    try
        //    {
        //        var response = new GenericResponse();
        //        using (var context = new XrmServiceContext())
        //        {
        //            var account = new AccountLeads
        //            {
        //                Id = AccountId,
        //                AccountLeadId = LeadId,
        //                LogicalName = UserName.Text
        //            };
        //            var lead = new Lead
        //            {
        //                Id = LeadId,
        //                CompanyName = UserName.Text,
        //                EMailAddress1 = Email.Text,
        //            };
        //            context.AddObject(account);
        //            context.AddObject(lead);
        //            context.SaveChanges();
        //        }
        //        response.Success = true;
        //        return response;
        //    }
        //    catch (Exception ex)
        //    {
        //        return new GenericResponse { FailureInformation = ex.Message };
        //    }
        //}

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

    public class RegistrationLead
    {
        public RegistrationLead()
        {
            Id = Guid.Empty;
            Company = string.Empty;
        }

        public string Company { get; set; }
    
        public  Guid Id { get; set; }
    }

     public partial class List : ViewPage<string>
    {
        //(...)
    }
}
