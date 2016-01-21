<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="Restricted.Master" CodeBehind="LicenseSuccess.aspx.cs" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.LicenseSuccessViewModel>" %>
<%@ Import Namespace="Site.Areas.Account.ViewModels" %>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/ChangeEmail/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("LicenseSuccess", "Manage"))
    { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal">
		    <fieldset>
		    <div class="form-group">
		       <span class="col-sm-4 control-label" style="font-size: 18px; font-family: ManiXBold; margin-left: 5% !important">Thank You</span>
		    </div>
				<legend><%: Html.TextSnippet("Account/ChangeEmail/ChangeEmailFormHeading", defaultValue: "Thank you", tagName: "span") %></legend>
				<%: Html.ValidationSummary(string.Empty, new {@class = "alert alert-block alert-danger"}) %>
               <div class="form-group">
					<div class="col-sm-8">
					    <%: Html.DisplayFor(model => model.DisplayString, new {  @class = "form-control1" }) %>
<%--					    <% if (!Model.SignedUpRefresh) %><%: Html.DisplayFor(model => model.DisplayString, new {  @class = "form-control1" }) %>
                        <% if (Model.SignedUpRefresh) %><%: Html.DisplayFor(model => model.DisplayString2, new {  @class = "form-control1" }) %>
					    <a style="display: none;">Thank you!  Your users should have everything that they need to start using Descon Version 8 today. 
                            Coming soon: You will be able to access administrative tools for your company’s Descon software.  Your users will also be able to access support and tools right on this site very soon. 
                            Interested in receiving a notice when more tools are available? Sign up with the button below. In the meantime, please contact us with any questions at sales@desconplus.com.</a>--%>
					</div>
<%--                    <% if (!Model.SignedUpRefresh) { %>
                   <button id="submit-license-update" class="btn2b btn-2" style="margin-top: 15px; margin-left: 50%;"><%: Html.SnippetLiteral("Account/ManageLicense/ContinueLicense", "Sign Up") %></button>
                   <% } %>--%>
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
