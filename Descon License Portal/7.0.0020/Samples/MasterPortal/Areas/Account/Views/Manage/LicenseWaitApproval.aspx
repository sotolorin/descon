<%@ Page Language="C#" AutoEventWireup="true" MasterPageFile="Restricted.Master" CodeBehind="LicenseWaitApproval.aspx.cs" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.LicenseWaitApprovalViewModel>" %>

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
			<legend><%: Html.TextSnippet("Account/ManageLicense/ManageLicenseFormHeading", defaultValue: "Manage License", tagName: "span") %></legend>
			<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger"}) %>
               <div class="form-group">
					<div class="col-sm-8">
					    <%: Html.DisplayFor(model => model.DisplayString) %>
					</div>
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
