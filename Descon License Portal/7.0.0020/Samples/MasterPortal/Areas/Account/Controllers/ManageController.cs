using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Configuration;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Adxstudio.Xrm.AspNet.Mvc;
using Adxstudio.Xrm.Web.Mvc;
using DocumentFormat.OpenXml.InkML;
using DocumentFormat.OpenXml.Packaging;
using DotNetOpenAuth.Messaging;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using Microsoft.Security.Application;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Site.Areas.Account.Models;
using Site.Areas.Account.ViewModels;
using Site.Helpers;
using Xrm;
using Task = System.Threading.Tasks.Task;

namespace Site.Areas.Account.Controllers
{
    [Authorize]
    [PortalView]
    [OutputCache(NoStore = true, Duration = 0)]
    public class ManageController : Controller
    {
        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationOrganizationManager organizationManager, ApplicationWebsiteManager websiteManager)
        {
            UserManager = userManager;
            OrganizationManager = organizationManager;
            WebsiteManager = websiteManager;
        }

        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        private ApplicationOrganizationManager _organizationManager;
        public ApplicationOrganizationManager OrganizationManager
        {
            get
            {
                return _organizationManager ?? HttpContext.GetOwinContext().Get<ApplicationOrganizationManager>();
            }
            private set
            {
                _organizationManager = value;
            }
        }

        private ApplicationWebsiteManager _websiteManager;
        public ApplicationWebsiteManager WebsiteManager
        {
            get
            {
                return _websiteManager ?? HttpContext.GetOwinContext().Get<ApplicationWebsiteManager>();
            }
            private set
            {
                _websiteManager = value;
            }
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            var isLocal = requestContext.HttpContext.IsDebuggingEnabled && requestContext.HttpContext.Request.IsLocal;
            var website = WebsiteManager.Find(requestContext);

            ViewBag.Settings = website.GetAuthenticationSettings<ApplicationWebsite, string>(isLocal);
            ViewBag.IdentityErrors = website.GetIdentityErrors<ApplicationWebsite, string>();

            var userId = User.Identity.GetUserId();
            var user = !string.IsNullOrWhiteSpace(userId) ? UserManager.FindById(userId) : null;

            ViewBag.Nav = user == null
                ? new ManageNavSettings()
                : new ManageNavSettings
                {
                    HasPassword = UserManager.HasPassword(user.Id),
                    IsEmailConfirmed = string.IsNullOrWhiteSpace(user.Email) || user.EmailConfirmed,
                    IsMobilePhoneConfirmed = string.IsNullOrWhiteSpace(user.PhoneNumber) || user.PhoneNumberConfirmed,
                    IsTwoFactorEnabled = user.TwoFactorEnabled,
                };
        }

        //
        // GET: /Manage/Index
        [HttpGet]
        public ActionResult Index(ManageMessageId? message)
        {
            return RedirectToProfile(message);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExternalLogin]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);
            var userLogins = await UserManager.GetLoginsAsync(userId);

            if (user != null && userLogins != null)
            {
                if (user.PasswordHash == null && userLogins.Count() <= 1)
                {
                    return RedirectToAction("ChangeLogin", new { Message = ManageMessageId.RemoveLoginFailure });
                }
            }

            var result = await UserManager.RemoveLoginAsync(userId, new UserLoginInfo(loginProvider, providerKey));

            if (result.Succeeded)
            {
                if (user != null)
                {
                    await SignInAsync(user, isPersistent: false);
                }

                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.RemoveLoginFailure;
            }

            return RedirectToAction("ChangeLogin", new { Message = message });
        }

        //
        // GET: /Manage/ChangePhoneNumber
        [HttpGet]
        public async Task<ActionResult> ChangePhoneNumber()
        {
            if (!ViewBag.Settings.MobilePhoneEnabled)
            {
                return HttpNotFound();
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            ViewBag.ShowRemoveButton = user.PhoneNumber != null;

            return View(new AddPhoneNumberViewModel { Number = user.PhoneNumber });
        }

        //
        // POST: /Manage/ChangePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ViewBag.Settings.MobilePhoneEnabled)
            {
                return HttpNotFound();
            }

            if (!ModelState.IsValid)
            {
                return await ChangePhoneNumber();
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(user.Id, model.Number);
            var parameters = new Dictionary<string, object> { { "Code", code }, { "PhoneNumber", model.Number } };
            await OrganizationManager.InvokeProcessAsync("adx_SendSmsConfirmationToContact", user.ContactId, parameters);

            //if (UserManager.SmsService != null)
            //{
            //	var message = new IdentityMessage
            //	{
            //		Destination = model.Number,
            //		Body = "Your security code is: " + code
            //	};
            //	await UserManager.SmsService.SendAsync(message);
            //}

            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/RememberBrowser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RememberBrowser()
        {
            if (!ViewBag.Settings.TwoFactorEnabled || !ViewBag.Settings.RememberBrowserEnabled)
            {
                return HttpNotFound();
            }

            var rememberBrowserIdentity = AuthenticationManager.CreateTwoFactorRememberBrowserIdentity(User.Identity.GetUserId());
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = true }, rememberBrowserIdentity);
            return RedirectToProfile(ManageMessageId.RememberBrowserSuccess);
        }

        //
        // POST: /Manage/ForgetBrowser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgetBrowser()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.TwoFactorRememberBrowserCookie);
            return RedirectToProfile(ManageMessageId.ForgetBrowserSuccess);
        }

        //
        // POST: /Manage/EnableTFA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTFA()
        {
            if (!ViewBag.Settings.TwoFactorEnabled)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            await UserManager.SetTwoFactorEnabledAsync(userId, true);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToProfile(ManageMessageId.EnableTwoFactorSuccess);
        }

        //
        // POST: /Manage/DisableTFA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTFA()
        {
            if (!ViewBag.Settings.TwoFactorEnabled)
            {
                return HttpNotFound();
            }

            var userId = User.Identity.GetUserId();
            await UserManager.SetTwoFactorEnabledAsync(userId, false);
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToProfile(ManageMessageId.DisableTwoFactorSuccess);
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        [HttpGet]
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return HttpNotFound();
            }

            if (ViewBag.Settings.IsDemoMode)
            {
                // This code allows you exercise the flow without actually sending codes
                // For production use please register a SMS provider in IdentityConfig and generate a code here.
                var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
                ViewBag.DemoModeCode = code;
            }

            return View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var result = await UserManager.ChangePhoneNumberAsync(userId, model.PhoneNumber, model.Code);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToProfile(ManageMessageId.ChangePhoneNumberSuccess);
                }
            }

            // If we got this far, something failed, redisplay form
            ViewBag.Message = ManageMessageId.ChangePhoneNumberFailure.ToString();
            return await VerifyPhoneNumber(model.PhoneNumber);
        }

        //
        // GET: /Manage/RemovePhoneNumber
        [HttpGet]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var userId = User.Identity.GetUserId();
            var result = await UserManager.SetPhoneNumberAsync(userId, null);
            if (!result.Succeeded)
            {
                throw new ApplicationException("Account error.");
            }
            var user = await UserManager.FindByIdAsync(userId);
            if (user != null)
            {
                await SignInAsync(user, isPersistent: false);
            }
            return RedirectToProfile(ManageMessageId.RemovePhoneNumberSuccess);
        }

        //
        // GET: /Manage/ChangePassword
        [HttpGet]
        [LocalLogin]
        public async Task<ActionResult> ChangePassword()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            return View(new ChangePasswordViewModel { Username = user.UserName, Email = user.Email });
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [LocalLogin]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!string.Equals(model.NewPassword, model.ConfirmPassword))
            {
                ModelState.AddModelError("Password", ViewBag.IdentityErrors.PasswordConfirmationFailure().Description);
            }

            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();
                var result = await UserManager.ChangePasswordAsync(userId, model.OldPassword, model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToProfile(ManageMessageId.ChangePasswordSuccess);
                }
                AddErrors(result);
            }

            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        [HttpGet]
        [LocalLogin]
        public async Task<ActionResult> SetPassword()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            ViewBag.HasEmail = !string.IsNullOrWhiteSpace(user.Email);

            return View(new SetPasswordViewModel { Email = user.Email });
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        [LocalLogin]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (!ViewBag.Settings.LocalLoginByEmail && string.IsNullOrWhiteSpace(model.Username))
            {
                ModelState.AddModelError("Username", ViewBag.IdentityErrors.UserNameRequired().Description);
            }

            if (!string.Equals(model.NewPassword, model.ConfirmPassword))
            {
                ModelState.AddModelError("Password", ViewBag.IdentityErrors.PasswordConfirmationFailure().Description);
            }

            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();

                var result = ViewBag.Settings.LocalLoginByEmail
                    ? await UserManager.AddPasswordAsync(userId, model.NewPassword)
                    : await UserManager.AddUsernameAndPasswordAsync(userId, model.Username, model.NewPassword);

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }
                    return RedirectToProfile(ManageMessageId.SetPasswordSuccess);
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return await SetPassword();
        }

        //
        // POST: /Manage/LinkLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        [ExternalLogin]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new LoginController.ChallengeResult(provider, Url.Action("LinkLoginCallback", "Manage"), User.Identity.GetUserId());
        }

        //
        // GET: /Manage/LinkLoginCallback
        [ExternalLogin]
        public async Task<ActionResult> LinkLoginCallback()
        {
            var userId = User.Identity.GetUserId();
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, userId);
            if (loginInfo == null)
            {
                return RedirectToAction("ChangeLogin", new { Message = ManageMessageId.LinkLoginFailure });
            }
            var result = await UserManager.AddLoginAsync(userId, loginInfo.Login);
            return RedirectToAction("ChangeLogin", new { Message = result.Succeeded ? ManageMessageId.LinkLoginSuccess : ManageMessageId.LinkLoginFailure });
        }

        //
        // GET: /Manage/ConfirmEmailRequest
        [HttpGet]
        public async Task<ActionResult> ConfirmEmailRequest()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            var code = await UserManager.GenerateEmailConfirmationTokenAsync(user.Id);
            var callbackUrl = Url.Action("ConfirmEmail", "Manage", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
            var parameters = new Dictionary<string, object> { { "UserId", user.Id }, { "Code", code }, { "UrlCode", Encoder.UrlEncode(code) }, { "CallbackUrl", callbackUrl } };
            await OrganizationManager.InvokeProcessAsync("adx_SendEmailConfirmationToContact", user.ContactId, parameters);
            //await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking this link: <a href=\"" + callbackUrl + "\">link</a>");

            if (ViewBag.Settings.IsDemoMode)
            {
                ViewBag.DemoModeLink = callbackUrl;
            }

            return View(new RegisterViewModel { Email = user.Email });
        }

        //
        // GET: /Manage/ConfirmEmail
        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return HttpNotFound();
            }

            var result = await UserManager.ConfirmEmailAsync(userId, code);
            var message = result.Succeeded ? ManageMessageId.ConfirmEmailSuccess : ManageMessageId.ConfirmEmailFailure;

            if (User.Identity.IsAuthenticated && userId == User.Identity.GetUserId())
            {
                return RedirectToProfile(message);
            }

            return RedirectToAction("ConfirmEmail", "Login", new { Message = message });
        }

        //
        // GET: /Manage/ChangeEmail
        [HttpGet]
        public async Task<ActionResult> ChangeEmail()
        {
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            return View(new ChangeEmailViewModel { Email = user.Email });
        }

        //
        // POST: /Manage/ChangeEmail
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangeEmail(ChangeEmailViewModel model)
        {
            if (ModelState.IsValid)
            {
                var userId = User.Identity.GetUserId();

                var result = ViewBag.Settings.LocalLoginByEmail
                    ? await UserManager.SetUsernameAndEmailAsync(userId, model.Email, model.Email)
                    : await UserManager.SetEmailAsync(userId, model.Email);

                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(userId);
                    if (user != null)
                    {
                        await SignInAsync(user, isPersistent: false);
                    }

                    return RedirectToAction("ConfirmEmailRequest", new { Message = ManageMessageId.ChangeEmailSuccess });
                }

                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ChangeTwoFactor
        [HttpGet]
        public async Task<ActionResult> ChangeTwoFactor()
        {
            if (!ViewBag.Settings.TwoFactorEnabled)
            {
                return HttpNotFound();
            }

            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            ViewBag.TwoFactorBrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(user.Id);
            ViewBag.HasEmail = !string.IsNullOrWhiteSpace(user.Email);
            ViewBag.IsEmailConfirmed = user.EmailConfirmed;
            ViewBag.IsTwoFactorEnabled = user.TwoFactorEnabled;

            return View();
        }

        //
        // GET: /Manage/ChangeLogin
        [HttpGet]
        [ExternalLogin]
        public async Task<ActionResult> ChangeLogin()
        {
            var userId = User.Identity.GetUserId();
            var user = await UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                throw new ApplicationException("Account error.");
            }

            var id = 0;
            var userLogins = await UserManager.GetLoginsAsync(userId);
            var logins = AuthenticationManager.GetExternalAuthenticationTypes()
                .Select(p => new LoginPair { Id = id++, Provider = p, User = userLogins.SingleOrDefault(u => u.LoginProvider == p.AuthenticationType) })
                .OrderBy(pair => pair.Provider.Caption)
                .ToList();

            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count() > 1;

            return View(new ChangeLoginViewModel { Logins = logins });
        }

        // GET: /Manage/ManageLicense
        [HttpGet]
        public async Task<ActionResult> ManageLicense()
        {
            return View(new ManageLicenseViewModel());
        }

        //
        // POST: /Manage/ManageLicense
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageLicense(ManageLicenseViewModel model)
        {
            if (string.IsNullOrEmpty(model.NumberOfUsers.ToString()))
            {
                ModelState.AddModelError("NumberOfUsers", "Invalid user number");
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    using (var context = new XrmServiceContext())
                    {
                        var contactId = Guid.Parse(userId);
                        var account = context.AccountSet.FirstOrDefault(act => act.PrimaryContactId.Id == contactId);
                        if (account != null)
                        {
                            //if (model.SoftwareType == SoftwareTypeOptions.Open)
                            //{
                            //    var invoiceResponse = XrmQueryHelper.CreateInvoice(model.QuoteId, account.Id, model, context, true);
                            //    if (invoiceResponse.Success)
                            //    {
                            //        return View("LicenseSuccess");
                            //    }
                            //}
                            var prod = string.Empty;
                            var price = string.Empty;
                            switch (model.SoftwareType)
                            {
                                case "100000000":
                                    prod = "Descon Open";
                                    price = "Descon Open";
                                    break;
                                default:
                                    prod = "Descon Basic ";
                                    price = "Descon Basic";
                                    if (model.PaymentOption == "100000001")
                                        prod += "Flex";

                                    if (model.PaymentOption == "100000002")
                                        prod += "AMP";

                                    if (model.PaymentOption == "100000003")
                                        prod += "Set";
                                    break;
                            }
                            var product = context.ProductSet.FirstOrDefault(pd => pd.Name == prod);
                            if (product != null)
                            {
                                model.ProductId = product.Id;
                                if (product.Price != null)model.LicenseCost = (decimal) product.Price;
                                else model.LicenseCost = 0;

                                if (model.LicenseCost.Equals(0) && product.CurrentCost != null)
                                    model.LicenseCost = (decimal) product.CurrentCost;
                                model.LicenseCostPerUser = model.LicenseCost/model.NumberOfUsers;
                                var acct = new AccountQuickDataModel
                                {
                                    Id = account.Id,
                                    CompanyName = account.Name,
                                    Address1 = account.Address1_Line1,
                                    Address2 = account.Address1_Line2,
                                    City = account.Address1_City,
                                    State = account.Address1_StateOrProvince,
                                    Country = account.Address1_Country,
                                    Zip = account.Address1_PostalCode,
                                };
                                var currency =
                                    context.TransactionCurrencySet.FirstOrDefault(itm => itm.CurrencyName == "US Dollar");
                                if (currency != null) model.CurrencyId = currency.Id;
                                var pricelist =
                                    context.PriceLevelSet.FirstOrDefault(itm => itm.Name == price);
                                if (pricelist != null) model.PriceListId = pricelist.Id;
                                var quoteResponse = XrmQueryHelper.UpdateQuoteBasedModel(model, acct, context);
                                if (!quoteResponse.Success)
                                {
                                    ModelState.AddModelError("", "An error occured updating the Xrm Quote context");
                                    return View(model);
                                }
                                model.LicenseCost = quoteResponse.TotalCost;
                                model.LicenseCostPerUser = quoteResponse.CostPerUser;
                                context.SaveChanges();
                                //model.QuoteId = quoteResponse.QuoteId;
                                //Take to validation page
                                //return RedirectToAction("ManageLicenseSummary", new {newmodel=new ManageLicenseViewModel(), dummy = true});
                                return View("ManageLicenseSummary", model);
                            }
                            else
                            {
                                ModelState.AddModelError("", "This software product could not be found.");
                                return View(model);
                            }

                            
                        }
                        {
                            ModelState.AddModelError("", "Company not found");
                            return View(model);
                        }
                    }

                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occured updating the Xrm context");
                    return View(model);
                }
            }
            ModelState.AddModelError("", "Form is missing fields");
            return View(model);
        }

        // GET: /Manage/ManageLicenseSummary
        [HttpGet]
        public ActionResult ManageLicenseSummary(ManageLicenseViewModel newmodel, object mo=null)
        {
            if (newmodel != null)
            {
                return View(new ManageLicenseViewModel
                {
                    CurrencyId = newmodel.CurrencyId,
                    PriceListId = newmodel.PriceListId,
                    ProductId = newmodel.ProductId,
                    PaymentOption = newmodel.PaymentOption,
                    PaymentMethod = newmodel.PaymentMethod,
                    SoftwareType = newmodel.SoftwareType,
                    NumberOfUsers = newmodel.NumberOfUsers,
                    SupportType = newmodel.SupportType,
                    LicenseCost = newmodel.LicenseCost,
                    LicenseCostPerUser = newmodel.LicenseCostPerUser,
                    QuoteId = newmodel.QuoteId,
                });
            }
            return null;
        }

        //
        // POST: /Manage/ManageLicenseSummary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ManageLicenseSummary(ManageLicenseViewModel model)
        {
            if (model.QuoteId == null)
            {
                ModelState.AddModelError("", "No Quote Id generated.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    using (var context = new XrmServiceContext())
                    {
                        var contactId = Guid.Parse(userId);
                        var account = context.AccountSet.FirstOrDefault(act => act.PrimaryContactId.Id == contactId);
                        var quote = context.QuoteSet.FirstOrDefault(act => act.Id == model.QuoteId);
                        if (account != null && quote != null)
                        {
                            if (model.EmailQuote)
                            {
                                quote.Name = "Online Quote Emailed";
                                context.UpdateObject(quote);
                                context.SaveChanges();
                                return View("LicenseWaitApproval",new LicenseWaitApprovalViewModel{DisplayString ="Thank you for your inquiry. An email with quote details has been sent to your registered email address."});
                            }
                            if (model.SoftwareType == "100000000")
                            {
                                var invoiceResponse = XrmQueryHelper.CreateSalesOrder((Guid) model.QuoteId, account.Id,
                                    model, context);
                                if (invoiceResponse.Success)
                                {
                                    return View("LicenseUserEmails", new LicenseUserEmailsViewModel(model.NumberOfUsers));
                                }

                                ModelState.AddModelError("","Invoice generation failed. Please contact Descon Support to process payment.");
                                return View(model);
                            }
                            if (model.PaymentMethod == LicensePaymentMethodOptions.Online_Payment)
                            {
                                quote.Name = "Online Quote for CC";
                                context.UpdateObject(quote);
                                context.SaveChanges();
                                return View("LicensePaymentProcessing",new LicensePaymentViewModel {PaymentModel = model});
                            }

                            quote.Name = "Online Quote for PO";
                            context.UpdateObject(quote);
                            context.SaveChanges();
                            return View("LicenseWaitApproval",new LicenseWaitApprovalViewModel{ DisplayString = "Thank you for your purchase. We will send out an email with further instructions"});
                        }

                        ModelState.AddModelError("", "This software product could not be found.");
                        return View(model);
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occured updating the Xrm context");
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: /Manage/ManageLicenseSummary
        [HttpGet]
        public ActionResult LicenseSuccess()
        {
            return View(new LicenseSuccessViewModel());
        }

        // POST: /Manage/ManageLicenseSummary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LicenseSuccess(LicenseSuccessViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    var contactId = Guid.Parse(userId);
                    using (var context = new XrmServiceContext())
                    {
                        var account = context.AccountSet.FirstOrDefault(act => act.PrimaryContactId.Id == contactId);
                        if (account != null)
                        {

                            context.UpdateObject(account);
                            return
                                View(new LicenseSuccessViewModel
                                {
                                    SignedUpRefresh = true,
                                });
                        }
                        else
                        {
                            ModelState.AddModelError("", "Account not found!");
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occured updating the Xrm context");
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: /Manage/ManageLicenseSummary
        [HttpGet]
        public async Task<ActionResult> LicenseWaitApproval(LicenseWaitApprovalViewModel model)
        {
            return View(model);
        }

        // GET: /Manage/ManageLicenseSummary
        [HttpGet]
        public async Task<ActionResult> LicensePaymentProcessing(LicensePaymentViewModel model, object dummy = null)
        {
            return View(model);
        }
        //
        // POST: /Manage/ManageLicenseSummary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LicensePaymentProcessing(LicensePaymentViewModel model)
        {
            if (model.PaymentModel.QuoteId == null)
            {
                ModelState.AddModelError("", "Quote not found.");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    using (var context = new XrmServiceContext())
                    {
                        var contactId = Guid.Parse(userId);
                        var account = context.AccountSet.FirstOrDefault(act => act.PrimaryContactId.Id == contactId);
                        if (account != null)
                        {
                            //Update Bill to address for account before this step
                                var invoiceResponse = XrmQueryHelper.CreateSalesOrder((Guid)model.PaymentModel.QuoteId, account.Id, model.PaymentModel, context);
                                if (invoiceResponse.Success)
                                {
                                    return View("LicenseUserEmails", new LicenseUserEmailsViewModel(model.PaymentModel.NumberOfUsers));
                                    return View("LicenseSuccess");
                                }
                                else
                                {
                                    ModelState.AddModelError("", "Invoice generation failed. Please contact Descon Support to process payment.");
                                    return View(model);
                                }
                        }
                        else
                        {
                            ModelState.AddModelError("", "This software product could not be found.");
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occured updating the Xrm context");
                    return View(model);
                }
            }
            return View(model);
        }

        // GET: /Manage/ManageLicenseSummary
        [HttpGet]
        public async Task<ActionResult> LicenseUserEmails(LicenseUserEmailsViewModel model, bool dummy = false)
        {
            return View(model);
        }
        //
        // POST: /Manage/ManageLicenseSummary
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> LicenseUserEmails(LicenseUserEmailsViewModel model)
        {
            if (model.ContactEmails.Any(em => string.IsNullOrEmpty(em.Email)))
            {
                ModelState.AddModelError("", "All emails must be filled to continue");
                return View(model);
            }
            if (model.ContactEmails.Select(em=> em.Email).Distinct().Count() != model.ContactEmails.Count)
            {
                ModelState.AddModelError("", "All user emails must be unique");
                return View(model);
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var userId = User.Identity.GetUserId();
                    using (var context = new XrmServiceContext())
                    {
                        var allContactEmails = context.ContactSet.Select(b => b.EMailAddress1).ToList();
                        var allEmails = allContactEmails.Where(b => b!= null).ToList();
                        var emailsToAdd = model.ContactEmails.Select(e=> e.Email).ToList();

                        var notUnique = allEmails.Intersect(emailsToAdd).FirstOrDefault();
                        if (notUnique !=null)
                        {
                            ModelState.AddModelError("", "A Unique Email is required for " + notUnique);
                            return View(model);
                        }
                        var contactId = Guid.Parse(userId);
                        var account = context.AccountSet.FirstOrDefault(act => act.PrimaryContactId.Id == contactId);
                        if (account != null)
                        {
                            foreach (var usr in model.ContactEmails)
                            {
                                var response = XrmQueryHelper.CreateBaseContact(account.Id, usr.Email, context, true);
                                if (!response.Success)
                                {
                                    ModelState.AddModelError("", "Contact creation for " + usr.Email + "failed.");
                                    return View(model);
                                }
                            }
                            return View("LicenseSuccess", new LicenseSuccessViewModel());
                        }
                        else
                        {
                            ModelState.AddModelError("", "The account could not be found.");
                            return View(model);
                        }
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "An error occured updating the Xrm context");
                    return View(model);
                }
            }
            ModelState.AddModelError("", "Invalid Email format");
            return View(model);
        }

        #region Helpers

        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private async Task SignInAsync(ApplicationUser user, bool isPersistent)
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ExternalCookie, DefaultAuthenticationTypes.TwoFactorCookie);
            AuthenticationManager.SignIn(new AuthenticationProperties { IsPersistent = isPersistent }, await user.GenerateUserIdentityAsync(UserManager));
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        public enum ManageMessageId
        {
            SetPasswordSuccess,
            ChangePasswordSuccess,

            ChangeEmailSuccess,

            ConfirmEmailSuccess,
            ConfirmEmailFailure,

            ChangePhoneNumberSuccess,
            ChangePhoneNumberFailure,
            RemovePhoneNumberSuccess,

            ForgetBrowserSuccess,
            RememberBrowserSuccess,

            DisableTwoFactorSuccess,
            EnableTwoFactorSuccess,

            RemoveLoginSuccess,
            RemoveLoginFailure,
            LinkLoginSuccess,
            LinkLoginFailure,
        }

        private static ActionResult RedirectToProfile(ManageMessageId? message)
        {
            var query = message != null ? new NameValueCollection { { "Message", message.Value.ToString() } } : null;

            return new RedirectToSiteMarkerResult("Profile", query);
        }

        #endregion
    }
}