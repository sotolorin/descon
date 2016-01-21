using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xrm;

namespace DesconWeb.Account
{
    public partial class Login : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            InitializeProperties();
            RegisterHyperLink.NavigateUrl = "Register";
            OpenAuthLogin.ReturnUrl = Request.QueryString["Profile.aspx"];

            var returnUrl = HttpUtility.UrlEncode(Request.QueryString["Profile.aspx"]);
            if (!String.IsNullOrEmpty(returnUrl))
            {
                RegisterHyperLink.NavigateUrl += "?ReturnUrl=" + returnUrl;
            }
        }
        public TextBox UserName;
        public TextBox Password;

        void InitializeProperties()
        {
            UserName = (TextBox)FindControl("UserName");
            Password = (TextBox)FindControl("Password");
        }

        protected void LoginUser(object sender, EventArgs e)
        {
            var context = new XrmServiceContext();
            var account = context.AccountLeadsSet.FirstOrDefault(a => a.LogicalName == UserName.Text);
            if (account == null)
            {
                return;
            }
            var id = account.AccountLeadId;
            Response.Redirect("Profile.aspx?_acctId=" + id);
        }
    }
}