using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using Xrm;

namespace DesconWeb.Account
{
    public partial class Profile : Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.ToString().Contains("_acctId")) _acctId = Guid.Parse(Request.QueryString["_acctId"]);
            if (Request.ToString().Contains("email")) Email.Text = Request.QueryString["email"];
            InitializeProperties();
        }

        public Guid _acctId;
        public TextBox CompanyName;
        public TextBox Address1;
        public TextBox Address2;
        public TextBox City;
        public DropDownList State;
        public TextBox Zip;
        public TextBox UserCount;
        public TextBox Email;

        private void InitializeProperties()
        {
            if(Master == null)return;
            var mainContent = Master.FindControl("PortalMainContent");
            if (mainContent != null)
            {
                CompanyName = (TextBox)mainContent.FindControl("CompanyName");
                Address1 = (TextBox)mainContent.FindControl("Address1");
                Address2 = (TextBox)mainContent.FindControl("Address2");
                City = (TextBox)mainContent.FindControl("City");
                State = (DropDownList)mainContent.FindControl("State");
                Zip = (TextBox)mainContent.FindControl("Zip");
                UserCount = (TextBox)mainContent.FindControl("UserCount");
                Email = (TextBox)mainContent.FindControl("Email");
            }
            LoadAccount();
        }

        protected void LoadAccount()
        {
            if(_acctId == Guid.Empty)return;
            using (var context = new XrmServiceContext())
            {
                var lead0 = context.LeadSet.FirstOrDefault(itm => itm.Id == _acctId);
                if (lead0 != null)
                {
                    foreach (ListItem itm in State.Items)
                    {
                        if (itm.Value == lead0.Address1_StateOrProvince)
                        {
                            itm.Selected = true;
                            break;
                        }
                    }

                    CompanyName.Text = lead0.CompanyName;
                    Address1.Text= lead0.Address1_Line1;
                    Address2.Text=lead0.Address1_Line2;
                    City.Text=lead0.Address1_City;
                    Zip.Text = lead0.Address1_PostalCode;
                    if(!string.IsNullOrEmpty(lead0.EMailAddress1))Email.Text = lead0.EMailAddress1;
                }
            }
        }

        protected void SaveAccount(object sender, EventArgs e)
        {
            using (var context = new XrmServiceContext())
            {
                var lead0 = context.LeadSet.FirstOrDefault(itm => itm.Id == _acctId);
                if (lead0 != null)
                {
                    var state = "";
                    foreach (ListItem itm in State.Items)
                    {
                        if (itm.Selected)
                        {
                            state = itm.Value;
                            break;
                        }
                    }

                    lead0.CompanyName = CompanyName.Text;
                    lead0.Address1_Line1 = Address1.Text;
                    lead0.Address1_Line2 = Address2.Text;
                    lead0.Address1_City = City.Text;
                    lead0.Address1_StateOrProvince = state;
                    lead0.Address1_PostalCode = Zip.Text;
                    lead0.EMailAddress1 = Email.Text;
                }
                context.SaveChanges();
            }
        }
    }
}