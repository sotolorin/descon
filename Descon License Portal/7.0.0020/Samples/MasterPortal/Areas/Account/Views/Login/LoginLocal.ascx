<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Site.Areas.Account.ViewModels.LoginViewModel>" %>
<link href="~/css/Site.css" rel="stylesheet" />
<% using (Html.BeginForm("Login", "Login", new { area = "Account", ReturnUrl = ViewBag.ReturnUrl, InvitationCode = ViewBag.InvitationCode })) { %>
	<%: Html.AntiForgeryToken() %>
	<div class="form-horizontal">
		<fieldset>
		    <div class="form-group">
		       <span class="col-sm-4 control-label" style="font-size: 18px; font-family: ManiXBold; margin-left: 40% !important">Sign In</span>
		    </div>
			<legend><%: Html.TextSnippet("Account/SignIn/SignInLocalFormHeading", defaultValue: "Sign In", tagName: "span") %></legend>
			<%: Html.ValidationSummary(true, string.Empty, new {@class = "alert alert-block alert-danger"}) %>
			<% if (ViewBag.Settings.LocalLoginByEmail) { %>
				<div class="form-group">
					<label class="col-sm-4 control-label" style="font-size: 16px; font-family: ManiXBold;" for="Email"><%: Html.TextSnippet("Account/SignIn/EmailLabel", defaultValue: "Email", tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control1" }) %>
					</div>
				</div>
			<% } else { %>
				<div class="form-group">
					<label class="col-sm-4 control-label" style="font-size: 16px; font-family: ManiXBold;" for="Email"><%: Html.TextSnippet("Account/SignIn/UsernameLabel", defaultValue: "Email", tagName: "span") %></label>
					<div class="col-sm-8">
						<%: Html.TextBoxFor(model => model.Email, new { @class = "form-control1", style="background:white !important;" }) %>
					</div>
				</div>
			<% } %>
			<div class="form-group">
				<label class="col-sm-4 control-label" style="font-size: 16px; font-family: ManiXBold;" for="Password"><%: Html.TextSnippet("Account/SignIn/PasswordLabel", defaultValue: "Password", tagName: "span") %></label>
				<div class="col-sm-8">
					<%: Html.PasswordFor(model => model.Password, new { @class = "form-control1" }) %>
				</div>
			</div>
            <div>
                <% if (ViewBag.Settings.RememberMeEnabled) { %>
				    <div class="form-group" style="display: none">
					    <div class="col-sm-8">
						    <div class="checkbox">
							    <label>
								    <%: Html.CheckBoxFor(model => model.RememberMe) %>
								    <%: Html.TextSnippet("Account/SignIn/RememberMeLabel", defaultValue: "Remember me?", tagName: "span") %>
							    </label>
						    </div>
					    </div>
				    </div>
			    <% } %>
			    <div class="form-group" style="padding-left: 36% !important">
				    <div class="col-sm-8">
					    <% if (ViewBag.Settings.ResetPasswordEnabled) { %>
						    <a class="btn btn-default" style="display: none" href="<%: Url.Action("ForgotPassword") %>"><%: Html.SnippetLiteral("Account/SignIn/PasswordResetLabel", "Forgot Your Password?") %></a>
					    <% } %>
					    <button id="submit-signin-local" class="btn2b btn-2" style="width: 230px !important"  ><%: Html.SnippetLiteral("Account/SignIn/SignInLocalButtonText", "Sign in") %></button>
				    </div>
			    </div>
            </div>

		</fieldset>
	</div>
<% } %>
<script type="text/javascript">
	$(function() {
		$("#submit-signin-local").click(function () {
			$.blockUI({ message: null, overlayCSS: { opacity: .3 } });
		});
	});
</script>
