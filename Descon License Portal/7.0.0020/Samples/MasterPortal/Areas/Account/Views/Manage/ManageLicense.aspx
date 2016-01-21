<%@ Page Title="Manage License" Language="C#" MasterPageFile="Restricted.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ManageLicenseViewModel>" %>
<%@ Import Namespace="Site.Areas.Account.ViewModels" %>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ManageLicense", "Manage"))
    { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal" style="text-align: left !important">
            <fieldset>
            <div class="form-group">
		       <span class="col-sm-4 control-label" style="font-size: 18px; font-family: ManiXBold; ">Manage License</span>
		    </div>
			<legend><%: Html.TextSnippet("Account/ManageLicense/ManageLicenseFormHeading", defaultValue: "Manage License", tagName: "span") %></legend>
			<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="SoftwareType"><%: Html.TextSnippet("Account/ManageLicense/SoftwareTypeLabel", defaultValue: "Select Software Type:", tagName: "span") %></label>
					<div class="col-sm-8">
					    <%: Html.DropDownListFor(model => model.SoftwareType, new SelectList(Model.SoftwareTypesMetaData, "Value", "Text", Model.SoftwareType) , new { @id="softtype"}) %>
					</div>
				</div>
                <div id="paydiv" style="display:none">                  
                    <div class="form-group">
					    <label class="col-sm-4 control-label" for="PaymentType"><%: Html.TextSnippet("Account/ManageLicense/PaymentTypeLabel", defaultValue: "Select Payment Type:", tagName: "span") %></label>
					    <div class="col-sm-8">
					        <%: Html.DropDownListFor(model => model.PaymentOption, Model.PaymentOptionsMetaData) %>
					    </div>
				    </div>
                    <div class="form-group">
					    <label class="col-sm-4 control-label" for="PaymentMethod"><%: Html.TextSnippet("Account/ManageLicense/PaymentMethodLabel", defaultValue: "Select Payment Method:", tagName: "span") %></label>
					    <div class="col-sm-8">
					        <%: Html.EnumDropDownListFor(model => model.PaymentMethod, Model.PaymentMethods.First()) %>
					    </div>
				    </div>
                   <div class="form-group">
					    <label class="col-sm-4 control-label" for="SupportType"><%: Html.TextSnippet("Account/ManageLicense/SupportTypeLabel", defaultValue: "Select Support Type:", tagName: "span") %></label>
					    <div class="col-sm-8">
					        <%: Html.EnumDropDownListFor(model => model.SupportType, Model.SupportTypes.First()) %>
					    </div>
				    </div>
               </div>
               <div class="form-group">
					<label class="col-sm-4 control-label" for="NumberOfUsers"><%: Html.TextSnippet("Account/ManageLicense/NumberOfUsersLabel", defaultValue: "# of Users", tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.TextBoxFor(model => model.NumberOfUsers, new { @type = "number", @min = "1", @class = "form-control1", style="width:200px !important; height:40px !important;" }) %>
					</div>
				</div>
<%--                <div class="form-group">
					<label class="col-sm-4 control-label" style="font-size: 22px; font-weight: 400;" for="LicenseCostPerUser"><%: Html.TextSnippet("Account/ManageLicense/LicenseCostPerUserLabel", defaultValue: "License Cost per User: ", tagName: "span") %></label>
					<div class="col-sm-8" style="font-size:22px; color: red; font-weight: 400;">
						<%: Html.TextBoxFor(model => model.LicenseCostPerUser, new { @class = "form-control1" }) %>
					</div>
				</div>--%>
               <div class="form-group">
<%--					<label class="col-sm-4 control-label" style="font-size: 22px; font-weight: 400;" for="LicenseCost"><%: Html.TextSnippet("Account/ManageLicense/LicenseCostLabel", defaultValue: "Total License Cost: ", tagName: "span") %></label>
					<div class="col-sm-8" style="font-size:22px; color: red; font-weight: 400;">
						<%: Html.TextBoxFor(model => model.LicenseCost, new { @class = "form-control1" }) %>
					</div>--%>

                   <button id="submit-license-update" class="btn2b btn-2" style="margin-top: 15px; margin-left: 20%;"><%: Html.SnippetLiteral("Account/ManageLicense/ContinueLicense", "Continue") %></button>
				</div>
		</fieldset>
		</div>
	<% } %>
    <script type="text/javascript">
	$(function() {
	    $("#submit-license-update").click(function () {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
	    });

	});
	$(document).ready(function () {
	    $('#softtype').on('change', function () {
	        if (this.value == '100000000') {
	            $("#paydiv").hide();
	        }
	        else {
	            $("#paydiv").show();
	        }
	    });
	});
	$(document).ready(function () {
	    $('#softtype').on('load', function () {
	        if (this.value == '100000000') {
	            $("#paydiv").hide();
	        }
	        else {
	            $("#paydiv").show();
	        }
	    });
	});
</script>
</asp:Content>
