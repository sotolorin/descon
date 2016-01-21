using System;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;
using System.Web.UI.WebControls;
using Microsoft.Ajax.Utilities;
using Microsoft.AspNet.Identity;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Xrm;

namespace Site.Areas.Account.ViewModels
{
    public class IndexViewModel
    {
        public bool HasPassword { get; set; }
        public IList<UserLoginInfo> Logins { get; set; }
        public string PhoneNumber { get; set; }
        public bool TwoFactor { get; set; }
        public bool BrowserRemembered { get; set; }
    }

    public class ManageLoginsViewModel
    {
        public IList<UserLoginInfo> CurrentLogins { get; set; }
        public IList<AuthenticationDescription> OtherLogins { get; set; }
    }

    public class FactorViewModel
    {
        public string Purpose { get; set; }
    }

    public class SetPasswordViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }

    public class ChangePasswordViewModel
    {
        [EmailAddress]
        public string Email { get; set; }

        public string Username { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Current password")]
        public string OldPassword { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        public string ConfirmPassword { get; set; }
    }


    public class AddPhoneNumberViewModel
    {
        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Number { get; set; }
    }

    public class VerifyPhoneNumberViewModel
    {
        [Required]
        public string Code { get; set; }

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
    }

    public class ConfigureTwoFactorViewModel
    {
        public string SelectedProvider { get; set; }
        public ICollection<System.Web.Mvc.SelectListItem> Providers { get; set; }
    }

    public class ChangeEmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class LoginPair
    {
        public int Id { get; set; }
        public AuthenticationDescription Provider { get; set; }
        public UserLoginInfo User { get; set; }
    }

    public class ChangeLoginViewModel
    {
        public IList<LoginPair> Logins { get; set; }
    }

    public class ManageNavSettings
    {
        public bool HasPassword { get; set; }
        public bool IsEmailConfirmed { get; set; }
        public bool IsMobilePhoneConfirmed { get; set; }
        public bool IsTwoFactorEnabled { get; set; }
    }

    public class ManageLicenseViewModel
    {
        public ManageLicenseViewModel()
        {
            SoftwareTypesMetaData = new List<SelectListItem>();
            PaymentOptionsMetaData = new List<SelectListItem>();
            using (var context = new XrmServiceContext())
            {
                try
                {
                    // Use the RetrieveAttributeRequest message to retrieve  
                    // a attribute by it's logical name.
                    var retrieveType =
                        new RetrieveAttributeRequest
                        {
                            EntityLogicalName = Xrm.Account.EntityLogicalName,
                            LogicalName = "new_desconlevel",
                            RetrieveAsIfPublished = true
                        };
                    var retrievePayment =
                        new RetrieveAttributeRequest
                        {
                            EntityLogicalName = Xrm.Account.EntityLogicalName,
                            LogicalName = "new_desconpayment",
                            RetrieveAsIfPublished = true
                        };

                    // Execute the request.
                    var retrieveTypeResponse =(RetrieveAttributeResponse)context.Execute(retrieveType);
                    var retrievePayResponse = (RetrieveAttributeResponse)context.Execute(retrievePayment);

                    var retrievedTypeAttributeMetadata =(PicklistAttributeMetadata)retrieveTypeResponse.AttributeMetadata;
                    var retrievedPayAttributeMetadata = (PicklistAttributeMetadata)retrievePayResponse.AttributeMetadata;

                    var optionListType = retrievedTypeAttributeMetadata.OptionSet.Options.ToArray();
                    var optionListPay = retrievedPayAttributeMetadata.OptionSet.Options.ToArray();

                    var typeOptionList = optionListType.OrderBy(x => x.Value).ToList();
                    var payOptionList = optionListPay.OrderBy(x => x.Value).ToList();

                    foreach (OptionMetadata o in typeOptionList)
                    {
                        SoftwareTypesMetaData.Add(new SelectListItem { Text = o.Label.LocalizedLabels.FirstOrDefault(e => e.LanguageCode == 1033).Label.ToString(), Value = o.Value.Value.ToString(),});
                    }
                    foreach (OptionMetadata o in payOptionList)
                    {
                        if(o.Value == 100000000)continue; //Open
                        PaymentOptionsMetaData.Add(new SelectListItem { Text = o.Label.LocalizedLabels.FirstOrDefault(e => e.LanguageCode == 1033).Label.ToString(), Value = o.Value.Value.ToString() });
                    }

                }
                catch (Exception ex)
                {
                    _errorMsg = ex.Message;
                }
            }

            SoftwareType = SoftwareTypesMetaData.First().Value;
            PaymentOption = PaymentOptionsMetaData.First().Value;
        }

        private string _errorMsg;
        private string _paymentOption;
        private LicensePaymentMethodOptions _paymentMethod;
        private string _softwareType;
        private int _numberOfUsers =1;
        private LicenseSupportType _supportType;
        public Guid CurrencyId { get; set; }
        public Guid PriceListId { get; set; }
        public Guid ProductId { get; set; }
        public Guid? QuoteId { get; set; }
        public bool EmailQuote { get; set; }

        [Required]
        public string PaymentOption
        {
            get { return _paymentOption; }
            set { _paymentOption = value; }
        }

        [Required]
        public LicensePaymentMethodOptions PaymentMethod
        {
            get { return _paymentMethod; }
            set { _paymentMethod = value; }
        }

        [Required]
        public string SoftwareType
        {
            get { return _softwareType; }
            set { _softwareType = value; }
        }

        [Required]
        public int NumberOfUsers
        {
            get { return _numberOfUsers; }
            set { _numberOfUsers = value; }
        }

        [Required]
        public LicenseSupportType SupportType
        {
            get { return _supportType; }
            set { _supportType = value; }
        }

        public decimal LicenseCost { get; set; }

        public decimal LicenseCostPerUser { get; set; }

        public string LicenseCostDisplay { get { return "$" + LicenseCost; } }

        public string LicenseCostPerUserDisplay { get { return "$" + LicenseCostPerUser; } }

        //public List<SoftwareTypeOptions> SoftwareTypes { get { return Enum.GetValues(typeof(SoftwareTypeOptions)).Cast<SoftwareTypeOptions>().ToList(); } }
        //public List<LicensePurchasePaymentOptions> PaymentOptions { get { return Enum.GetValues(typeof(LicensePurchasePaymentOptions)).Cast<LicensePurchasePaymentOptions>().ToList(); } }
        public List<LicensePaymentMethodOptions> PaymentMethods { get { return Enum.GetValues(typeof(LicensePaymentMethodOptions)).Cast<LicensePaymentMethodOptions>().ToList(); } }
        public List<LicenseSupportType> SupportTypes { get { return Enum.GetValues(typeof(LicenseSupportType)).Cast<LicenseSupportType>().ToList(); } }

        public List<SelectListItem> SoftwareTypesMetaData { get; set; }
        public List<SelectListItem> PaymentOptionsMetaData { get; set; }
    }

    public enum LicensePurchasePaymentOptions
    {
        MONTH,
        ANNUAL,
        SET
    }

    public enum LicensePaymentMethodOptions
    {
        [Description("Online Payment")]
        Online_Payment,

        [Description("Purchase Order")]
        Purchase_Order,
    }

    public enum SoftwareTypeOptions
    {
        Open,
        Basic,
        Standard,
        Next
    }

    public enum LicenseSupportType
    {
        Low,
        Normal,
        High,
    }

    public class LicensePaymentViewModel
    {
        public LicensePaymentViewModel()
        {
            QuoteId = Guid.Empty;
            PaymentModel = new ManageLicenseViewModel();
        }

        public Guid QuoteId { get; set; }

        public  ManageLicenseViewModel PaymentModel { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Zip { get; set; }
    }


    public class LicenseUserEmailsViewModel
    {
        public LicenseUserEmailsViewModel()
        {
            
        }
        public LicenseUserEmailsViewModel(int noUsers)
        {
            ContactEmails = new List<EmailViewModel>();
            for (var i = 0; i < noUsers; i++)
            {
                ContactEmails.Add(new EmailViewModel{Sequence = i +1});
            }
        }

        public List<EmailViewModel> ContactEmails { get; set; }
    }

    public class EmailViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public int Sequence { get; set; }
    }

    public class LicenseSuccessViewModel
    {
        public LicenseSuccessViewModel()
        {
            DisplayString = "Thank you!  Your users should have everything that they need to start using Descon Version 8 today. Soon you will be able to access the administrative tools for your company’s Descon software.  Your users will also be able to access support tools, forums and more. In the meantime, please don't hesitate to contact us with any questions at sales@desconplus.com.";
            DisplayString2 = "Thank you for signing up for our dashboard newsletter. Expect further details in you email soon!";
            SignedUpRefresh = false;
        }
        public bool SignedUpRefresh { get; set; }
        public string DisplayString { get; set; }
        public string DisplayString2 { get; set; }
    }

    public class LicenseWaitApprovalViewModel
    {
        public LicenseWaitApprovalViewModel()
        {
            DisplayString = string.Empty;
        }

        public string DisplayString { get; set; }
    }

}
