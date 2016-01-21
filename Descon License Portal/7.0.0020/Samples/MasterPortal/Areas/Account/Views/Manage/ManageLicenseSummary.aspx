<%@ Page Title="License Summary" Language="C#" MasterPageFile="Restricted.Master" Inherits="System.Web.Mvc.ViewPage<Site.Areas.Account.ViewModels.ManageLicenseViewModel>" %>
<%@ Import Namespace="System.ComponentModel" %>
<%@ Import Namespace="Site.Areas.Account.ViewModels" %>

<asp:Content ContentPlaceHolderID="ProfileNavbar" runat="server">
	<% Html.RenderPartial("ProfileNavbar"); %>
</asp:Content>

<asp:Content ContentPlaceHolderID="MainContent" runat="server">
	<% using (Html.BeginForm("ManageLicenseSummary", "Manage"))
    { %>
		<%: Html.AntiForgeryToken() %>
		<div class="form-horizontal" style="text-align: left !important;">
            <fieldset>
            <div class="form-group">
		       <span class="col-sm-4 control-label" style="font-size: 18px; font-family: ManiXBold; ">Manage License</span>
		    </div>
             <div hidden="hidden">
                  <%: Html.TextBoxFor(model => model.CurrencyId) %>
                  <%: Html.TextBoxFor(model => model.PriceListId) %>
                  <%: Html.TextBoxFor(model => model.ProductId) %>
                  <%: Html.TextBoxFor(model => model.QuoteId) %>
                  <%: Html.TextBoxFor(model => model.PaymentOption) %>
                  <%: Html.TextBoxFor(model => model.PaymentMethod) %>
                  <%: Html.TextBoxFor(model => model.SoftwareType) %>
                  <%: Html.TextBoxFor(model => model.NumberOfUsers) %>
                  <%: Html.TextBoxFor(model => model.SupportType) %>
                  <%: Html.TextBoxFor(model => model.LicenseCost) %>
                  <%: Html.TextBoxFor(model => model.LicenseCostPerUser) %>
                 <%: Html.CheckBoxFor(model => model.EmailQuote, new { @id = "emailquote" }) %>
             </div>
			<legend><%: Html.TextSnippet("Account/ManageLicense/ManageLicenseFormHeading", defaultValue: "Manage License", tagName: "span") %></legend>
			<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger"}) %>
				<div class="form-group">
					<label class="col-sm-4 control-label" for="SoftwareType"><%: Html.TextSnippet("Account/ManageLicense/SoftwareTypeLabel", defaultValue: "Select Software Type:", tagName: "span") %></label>
					<div class="col-sm-8" style="margin-top: 5px;">
					    <%: Html.DisplayFor(model => model.SoftwareTypesMetaData.First(itm=> itm.Value == model.SoftwareType).Text ) %>
					</div>
				</div>
                <% if (Model.SoftwareType != "100000000") { %>
                <div class="form-group">
					<label class="col-sm-4 control-label" for="PaymentType"><%: Html.TextSnippet("Account/ManageLicense/PaymentTypeLabel", defaultValue: "Select Payment Type:", tagName: "span") %></label>
					<div class="col-sm-8" style="margin-top: 5px;">
					    <%: Html.DisplayFor(model => model.PaymentOptionsMetaData.First(itm=> itm.Value == model.PaymentOption).Text) %>
					</div>
				</div>
                <div class="form-group">
					<label class="col-sm-4 control-label" for="PaymentMethod"><%: Html.TextSnippet("Account/ManageLicense/PaymentMethodLabel", defaultValue: "Select Payment Method:", tagName: "span") %></label>
					<div class="col-sm-8" style="margin-top: 5px;">
					    <%: Html.DisplayFor(model => model.PaymentMethod, Model.PaymentMethods.First()) %>
					</div>
				</div>
               <div class="form-group">
					<label class="col-sm-4 control-label" for="SupportType"><%: Html.TextSnippet("Account/ManageLicense/SupportTypeLabel", defaultValue: "Select Support Type:", tagName: "span") %></label>
					<div class="col-sm-8" style="margin-top: 5px;">
					    <%: Html.DisplayFor(model => model.SupportType, Model.SupportTypes.First()) %>
					</div>
				</div>
               <% } %>
               <div class="form-group">
					<label class="col-sm-4 control-label" for="NumberOfUsers"><%: Html.TextSnippet("Account/ManageLicense/NumberOfUsersLabel", defaultValue: "# of Users", tagName: "span") %></label>
					<div class="col-sm-8" style="margin-top: 5px;">
						<%: Html.DisplayFor(model => model.NumberOfUsers) %>
					</div>
				</div>
                <div class="form-group">
					<label class="col-sm-4 control-label"  for="LicenseCostPerUser"><%: Html.TextSnippet("Account/ManageLicense/LicenseCostPerUserLabel", defaultValue: "License Cost per User: ", tagName: "span") %></label>
					<div class="col-sm-8" style="font-size:22px; color: blue; font-weight: 400; margin-top: 5px;">
						<a >$</a>
                        <%: Html.DisplayFor(model => model.LicenseCostPerUser, new { @class = "form-control1" }) %>
					</div>
				</div>
               <div class="form-group">
					<label class="col-sm-4 control-label"  for="LicenseCost"><%: Html.TextSnippet("Account/ManageLicense/LicenseCostLabel", defaultValue: "Total License Cost: ", tagName: "span") %></label>
					<div class="col-sm-8" style="font-size:22px; color: blue; font-weight: 400; margin-top: 5px;">
					    <a >$</a>
						<%: Html.DisplayFor(model => model.LicenseCost, new { @class = "form-control1" }) %>
					</div>
                   <div style="display: inline-block">
                        <button id="submit-email-quote" class="btn2b btn-2" style="margin-top: 20px; padding: 5px 5px !important; margin: 10px 10px !important" ><%: Html.SnippetLiteral("Account/ManageLicense/EmailQuote", "Email Quote") %></button>
                        <button id="submit-license-update" class="btn2b btn-2" style="margin-top: 20px; padding: 5px 5px !important; margin: 10px 10px !important""><%: Html.SnippetLiteral("Account/ManageLicense/PurchaseLicense", "Continue to Payment") %></button> 
                   </div>

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
	$(function () {
	    $("#submit-email-quote").click(function ()
	    {
	        $.blockUI({ message: null, overlayCSS: { opacity: .3 } });
	        $("#emailquote").prop('checked', true);
	    });
	});

</script>
</asp:Content>
