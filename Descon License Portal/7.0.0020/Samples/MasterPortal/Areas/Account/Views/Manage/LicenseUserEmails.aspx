<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="Restricted.Master" CodeBehind="LicenseUserEmails.aspx.cs" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.LicenseUserEmailsViewModel>" %>
<%@ Import Namespace="Microsoft.Ajax.Utilities" %>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("LicenseUserEmails", "Manage"))
    { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal" style="text-align: left !important">
            <fieldset>
            <div class="form-group">
		       <span class="col-sm-4 control-label" style="font-size: 18px; font-family: ManiXBold; margin-left: 5% !important">Add Users</span>
		    </div>
			<legend><%: Html.TextSnippet("Account/ManageLicense/ManageLicenseFormHeading", defaultValue: "Add Users", tagName: "span") %></legend>
			<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger"}) %>
               <div class="form-group">
					<div class="col-sm-8">
					    <a >To register your Descon users, please enter their email address below.  They will be sent an email containing registration, download and applicable account instructions. </a>
					</div>
				</div>
               <div class="form-group" style="margin-left: 3px;">
					<div class="col-sm-8">
                        <%for (int i = 0; i < Model.ContactEmails.Count; i++)
                            { %>
                            <div class="form-group " style=" padding: 0 !important; ">
                                <div style="display: inline-block !important;  padding: 0 !important;">
                                    <label class="control-label" style="width: 100px !important; float: left"><%: Html.TextSnippet("Account/ManageLicense/EmailLabel", defaultValue: "Email for User", tagName: "span") %></label>
                                    <label class="control-label" style="width: 30px !important; margin-right: 5px; float: left"><%: Html.TextSnippet("Account/ManageLicense/EmailLabel", defaultValue: " #" + Model.ContactEmails[i].Sequence, tagName: "span") %></label>
						            <%: Html.TextBoxFor(model => model.ContactEmails[i].Email, new {  @class = "form-control1", style="width:200px !important; float: left" }) %>
                                </div>
				            </div>
                        <% } %>
                        <a >Please contact us with any questions at sales@desconplus.com. </a>
					</div>
				</div>
                <div  class="form-group" style="display: block !important">
                    <button id="submit-license-update" class="btn2b btn-2" style="margin-top: 15px; margin-left: 20% !important"><%: Html.SnippetLiteral("Account/ManageLicense/ContinueLicense", "Confirm Users") %></button>
				</div>
		</fieldset>
		</div>
	<% } %>
    <script type="text/javascript">
        $(function () {
            $("#submit-license-update").click(function () {
                $.blockUI({ message: null, overlayCSS: { opacity: .3 } });
            });
        });

</script>
</asp:Content>
