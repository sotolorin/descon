<%@ Page Language="C#" MasterPageFile="~/Areas/Account/Views/Manage/Restricted.Master" Inherits="System.Web.Mvc.ViewPage" %>

<%--<asp:Content ContentPlaceHolderID="AccountNavBar" runat="server">
	<% Html.RenderPartial("AccountNavBar", new ViewDataDictionary(ViewData) { { "SubArea", "SignIn" } }); %>
</asp:Content>--%>

<asp:Content ContentPlaceHolderID="PageCopy" runat="server">
	<%: Html.HtmlSnippet("Account/SignIn/PageCopy", "page-copy") %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
<%--	<% if (!string.IsNullOrWhiteSpace(ViewBag.InvitationCode)) { %>
		<div class="alert alert-info"><%: Html.SnippetLiteral("Account/Redeem/InvitationCodeAlert", "Redeeming code:") %> <strong><%: ViewBag.InvitationCode %></strong></div>
	<% } %>
	<div class="row">
		<% if (ViewBag.Settings.LocalLoginEnabled) { %>
			<div class="col-md-6">
				<% Html.RenderPartial("LoginLocal", ViewData["local"]); %>
			</div>
		<% } %>--%>
<%--		<% if (ViewBag.Settings.ExternalLoginEnabled) { %>
			<div class="col-md-6">
				<% Html.RenderPartial("LoginExternal", ViewData["external"]); %>
			</div>
		<% } %>--%>
	<%--</div>--%>
    
        <div class="form-group">
            <div style="display: inline-block">
                <a runat="server" class="btn btn-primary" href="~/Areas/Account/Views/Login/Login.aspx" style="color:white; font-size: 22px"><span>LOGIN</span></a>
                <a runat="server" class="btn btn-primary" href="~/Areas/Account/Views/Login/Register.aspx" style="color:white; font-size: 22px"><span>REGISTER</span></a>
<%--                <button id="submit-email-quote" class="btn btn-primary" style="margin-top: 20px;" ><%: Html.SnippetLiteral("Account/ManageLicense/EmailQuote", "Email Quote") %></button>
                <button id="submit-license-update" class="btn btn-primary" style="margin-top: 20px;"><%: Html.SnippetLiteral("Account/ManageLicense/PurchaseLicense", "Continue to Payment") %></button> --%>
            </div>

		</div>

</asp:Content>
