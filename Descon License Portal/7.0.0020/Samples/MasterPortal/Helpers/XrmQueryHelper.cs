using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Adxstudio.Xrm.Web.Mvc;
using Descon.WebAccess;
using DocumentFormat.OpenXml.EMMA;
using Lucene.Net.Search;
using Microsoft.Ajax.Utilities;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Portal.Configuration;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Site.Areas.Account.ViewModels;
using Xrm;
using List = DocumentFormat.OpenXml.Office2010.ExcelAc.List;

namespace Site.Helpers
{
    public class XrmQueryHelper : PortalViewUserControl
    {
        public static Guid usrId = Guid.Parse("a407dcef-ec1c-e411-80d3-0050568f4ec4");
        private static readonly Guid _companyNextId = Guid.Parse("9053f3f4-3a71-e511-80bf-00155d7d0f02");
        private static readonly string uniqueEntityName = "adx_descon_unique_number";
        public static GenericResponse WriteContacts(XrmServiceContext xrm, Contact cont)
        {
            var response = new GenericResponse();
            try
            {
                var contact = xrm.ContactSet.FirstOrDefault(c => c.ContactId == cont.ContactId);
                if (contact != null) contact = cont;
                else xrm.AddObject(cont);
                xrm.SaveChanges();
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GenericResponse{Success = false, FailureInformation = ex.Message };
            }
        }

        public static void AssociateContactsToAccount(EntityReference contact, EntityReference account, IOrganizationService service, bool isPrimary = false)
        {
            var relatedEntities = new EntityReferenceCollection();
            relatedEntities.Add(contact);
            if (isPrimary)
            {
                var primaryRelationship = new Relationship("account_primary_contact");
                service.Associate(account.LogicalName, account.Id,primaryRelationship, relatedEntities);
            }
            var relationship = new Relationship("contact_customer_accounts");
            service.Associate(account.LogicalName, account.Id, relationship, relatedEntities);
        }

        public static void DisassociateContactsToAccount(EntityReference contact, EntityReference account, IOrganizationService service)
        {

            var relatedEntities = new EntityReferenceCollection();
            relatedEntities.Add(contact);
            var relationship = new Relationship("contact_customer_accounts");
            service.Disassociate(account.LogicalName, account.Id, relationship, relatedEntities);
        }

        public static GetSupportNumbersResponse UpdateCompanyNumbers(Account account, IOrganizationService context)
        {
            try
            {
                var response = new GetSupportNumbersResponse();
                var retrieveresponse = context.Retrieve(uniqueEntityName, _companyNextId, new ColumnSet(new[] { "adx_descon_next_number", "adx_descon_unique_numberid" }));
                var nextNo = ((int)retrieveresponse.Attributes.First(at => at.Key == "adx_descon_next_number").Value) + 100;
                retrieveresponse["adx_descon_next_number"] = nextNo;

                var entityValue = new Entity(uniqueEntityName);
                entityValue["adx_descon_company_name"] = account.Name;
                entityValue["adx_descon_next_number"] = nextNo;
                entityValue["adx_descon_related_next_number"] = nextNo + 1;
                entityValue["adx_descon_type"] = "CompanyID";
                var newRecord = context.Create(entityValue);
                context.Update(retrieveresponse);
                response.RecordId = newRecord;
                response.NextCompanyNumber = nextNo;
                response.NextContactNumber = nextNo + 1;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GetSupportNumbersResponse{Success = false, FailureInformation = ex.Message};
            }
        }

        public static void IterateNextContactNumber(Guid recordId, IOrganizationService context)
        {
            var retrieveresponse = context.Retrieve(uniqueEntityName, recordId,
                new ColumnSet(new[] { "adx_descon_related_next_number", "adx_descon_unique_numberid" }));
            var nextNo = ((int) retrieveresponse.Attributes.First(at => at.Key == "adx_descon_related_next_number").Value) + 1;
            retrieveresponse["adx_descon_related_next_number"] = nextNo;
            context.Update(retrieveresponse);
        }

        public static GetQuoteResponse UpdateQuoteBasedModel(ManageLicenseViewModel model, AccountQuickDataModel account, XrmServiceContext context)
        {
            try
            {
                var response = new GetQuoteResponse();
                var quote = new Quote
                {
                    Name = "Online Quote",
                    TotalAmount = model.LicenseCost,
                    BillTo_Line1 = account.Address1,
                    BillTo_Line2 = account.Address2,
                    BillTo_City =  account.City,
                    BillTo_StateOrProvince = account.State,
                    BillTo_Country = account.Country,
                    BillTo_PostalCode = account.Zip,
                    PriceLevelId = model.PriceListId == Guid.Empty ? null : new CrmEntityReference(Xrm.PriceLevel.EntityLogicalName, model.PriceListId),
                    //Description = Enum.GetName(typeof(LicensePurchasePaymentOptions), model.PaymentOption).Replace('_', ' '),
                    CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    TransactionCurrencyId = model.CurrencyId == Guid.Empty ? null: new CrmEntityReference(Xrm.TransactionCurrency.EntityLogicalName, model.CurrencyId),
                    OwnerId = usrId == Guid.Empty ? null: new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                    ImportSequenceNumber = model.NumberOfUsers,  //Using this as number of users
                    
                };
                var quoteId = context.Create(quote);
                var quote1 = context.QuoteSet.First(qt => qt.Id == quoteId);
                model.QuoteId = quoteId;
                response.QuoteId = quoteId;
                //context.SaveChanges();
                if (model.PriceListId != Guid.Empty)
                {
                    var relatedEntities2 = new EntityReferenceCollection();
                    relatedEntities2.Add(new EntityReference(PriceLevel.EntityLogicalName, model.PriceListId));
                    var relationship2 = new Relationship("price_level_quotes");
                    context.Associate(Quote.EntityLogicalName, quoteId, relationship2, relatedEntities2);
                    context.UpdateObject(quote1);
                }

                //context.SaveChanges();
                if (model.ProductId != Guid.Empty)
                {
                    var prod = context.ProductSet.First(it => it.Id == model.ProductId);
                    var uomId = prod.DefaultUoMId.Id;
                    var quoteDetail = new QuoteDetail
                    {
                        QuoteId = new CrmEntityReference(Xrm.Quote.EntityLogicalName, quoteId),
                        ProductId = new CrmEntityReference(Xrm.Product.EntityLogicalName, model.ProductId),
                        UoMId = new CrmEntityReference(Xrm.UoM.EntityLogicalName, uomId),
                        Quantity = model.NumberOfUsers
                    };
                    var detailId = context.Create(quoteDetail);
                    var relatedEntity = new EntityReferenceCollection
                    {
                        new EntityReference(QuoteDetail.EntityLogicalName, detailId)
                    };
                    var relations = new Relationship("quote_details");
                    context.Associate(Quote.EntityLogicalName, quoteId, relations, relatedEntity);
                    var relatedEntity2 = new EntityReferenceCollection
                    {
                        new EntityReference(Product.EntityLogicalName, model.ProductId)
                    };
                    var relations2 = new Relationship("product_quote_details");
                    context.Associate(QuoteDetail.EntityLogicalName, detailId, relations2, relatedEntity2);
                    var quoteDetail1 = context.QuoteDetailSet.First(q => q.Id == detailId);
                    context.UpdateObject(quoteDetail1);
                    context.UpdateObject(quote1);
                    if(quoteDetail1.PricePerUnit !=null)response.CostPerUser = (decimal)quoteDetail1.PricePerUnit;
                    if (quoteDetail1.ExtendedAmount != null) response.TotalCost = (decimal)quoteDetail1.ExtendedAmount;
                }

                var relatedEntities = new EntityReferenceCollection();
                relatedEntities.Add(new EntityReference(Quote.EntityLogicalName, quoteId));
                var relationship = new Relationship("quote_customer_accounts");
                context.Associate(Account.EntityLogicalName, account.Id, relationship, relatedEntities);
                context.UpdateObject(quote1);
                context.SaveChanges();

                var quotes = context.QuoteSet.OrderByDescending(t => t.CreatedOn).ToList();
                var foo = quotes;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GetQuoteResponse { Success = false, FailureInformation = ex.Message };
            }
        }

        public static GenericResponse CreateInvoice(Guid? quoteId, Guid accountId, ManageLicenseViewModel paymentModel, XrmServiceContext context)
        {
            try
            {
                var response = new GenericResponse();
                //Create Invoice and add license details to account and create contract
                var account = context.AccountSet.FirstOrDefault(q => q.Id == accountId);
                if (account == null)
                {
                    response.FailureInformation = "No account found";
                    return response;
                }
                var opt = Enum.GetName(typeof (LicensePurchasePaymentOptions), paymentModel.PaymentOption);
                if (opt == null)
                {
                    response.FailureInformation = "Invalid Payment option";
                    return response;
                }
                var paymentOption =opt.Replace('_', ' ') ;

                var soft = Enum.GetName(typeof (SoftwareTypeOptions), paymentModel.SoftwareType);
                if (soft == null)
                {
                    response.FailureInformation = "Invalid Software option";
                    return response;
                }
                var softwareOption =soft.Replace('_', ' ') ;
                    var quote = context.QuoteSet.FirstOrDefault(q => q.Id == quoteId);
                    if (quote == null)
                    {
                        response.FailureInformation = "No quote found";
                        return response;
                    }
                    var invoice = new Xrm.Invoice
                    {
                        Name = account.Name,
                        CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                        TransactionCurrencyId =
                            quote.TransactionCurrencyId == null
                                ? null
                                : new CrmEntityReference(Xrm.TransactionCurrency.EntityLogicalName,
                                    quote.TransactionCurrencyId.Id),
                        PriceLevelId = paymentModel.PriceListId == Guid.Empty ? null : new CrmEntityReference(Xrm.PriceLevel.EntityLogicalName, paymentModel.PriceListId),
                        BillTo_Line1 = account.Address1_Line1,
                        BillTo_Line2 = account.Address1_Line2,
                        BillTo_City = account.Address1_City,
                        BillTo_StateOrProvince = account.Address1_StateOrProvince,
                        BillTo_Country = account.Address1_Country,
                        BillTo_PostalCode = account.Address1_PostalCode,
                        psa_Approved = false,
                        OwnerId = new CrmEntityReference(Xrm.SystemUser.EntityLogicalName, usrId),
                        TotalAmount = paymentModel.LicenseCost,
                        ImportSequenceNumber = paymentModel.NumberOfUsers   //Use ImportSequence Number for # of users
                    };
                    var invoiceId = context.Create(invoice);
                    var invoice1 = context.InvoiceSet.First(inv => inv.Id == invoiceId);
                    context.SaveChanges();

                    if (quote.quote_details.Any())
                    {
                        var invoiceDetail = new InvoiceDetail
                        {
                            InvoiceId = new CrmEntityReference(Xrm.Invoice.EntityLogicalName, invoiceId),
                            ProductId =
                                new CrmEntityReference(Xrm.Product.EntityLogicalName,
                                    quote.quote_details.First().ProductId.Id)
                        };
                        var detailId = context.Create(invoiceDetail);
                        var relatedEntity = new EntityReferenceCollection();
                        relatedEntity.Add(new EntityReference(InvoiceDetail.EntityLogicalName, detailId));
                        var relations = new Relationship("invoice_details");
                        context.Associate(Invoice.EntityLogicalName, invoiceId, relations, relatedEntity);
                        invoice1.psa_Approved = true;
                        invoice1.psa_ApprovedOn = DateTime.Now;
                        context.UpdateObject(invoice1);
                        context.SaveChanges();
                    }

                    var contract = new Xrm.Contract
                    {
                        Title = paymentOption,
                        CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                        BillingStartOn = DateTime.Now,
                        BillingEndOn =
                            paymentOption == "SET"
                                ? (DateTime?) null
                                : (paymentOption == "MONTH" ? DateTime.Now.AddMonths(1) : DateTime.Now.AddYears(1)),
                        OwnerId =
                            usrId == Guid.Empty ? null : new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                        BillingCustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    };
                    var contractId = context.Create(contract);
                    account.new_ContractEndDate = contract.BillingEndOn.ToString();
                    context.SaveChanges();

                    var acctrelatedEntity = new EntityReferenceCollection();
                    acctrelatedEntity.Add(new EntityReference(Contract.EntityLogicalName, contractId));
                    var acctrelatedEntity2 = new EntityReferenceCollection();
                    acctrelatedEntity2.Add(new EntityReference(Invoice.EntityLogicalName, invoiceId));
                    var acctrelations = new Relationship("contract_customer_accounts");
                    var bacctrelations = new Relationship("contract_billingcustomer_accounts");
                    var iacctrelations = new Relationship("invoice_customer_accounts");
                    context.Associate(Account.EntityLogicalName, accountId, acctrelations, acctrelatedEntity);
                    context.Associate(Account.EntityLogicalName, accountId, bacctrelations, acctrelatedEntity);
                    context.Associate(Account.EntityLogicalName, accountId, iacctrelations, acctrelatedEntity2);
                
                //update account
                account.new_DesconUsers = paymentModel.NumberOfUsers; 
                //account.new_License = true;
                //account.new_onamp = !isOpen && (paymentOption == "ANNUAL");
                int softtype =0;
                int paytype = 0;
                Int32.TryParse(paymentModel.SoftwareType, out softtype);
                Int32.TryParse(paymentModel.PaymentOption, out paytype);
                account.new_DesconLevel =  softtype;
                account.new_DesconPayment = paytype;
                account.new_DesconV8Start = DateTime.Now;
                context.UpdateObject(account);
                context.SaveChanges();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GenericResponse { Success = false, FailureInformation = ex.Message };
            }
        }

        public static GenericResponse CreateSalesOrder(Guid quoteId, Guid accountId, ManageLicenseViewModel paymentModel, XrmServiceContext context)
        {
            try
            {
                var response = new GenericResponse();
                //Create Invoice and add license details to account and create contract
                var account = context.AccountSet.FirstOrDefault(q => q.Id == accountId);
                if (account == null)
                {
                    response.FailureInformation = "No account found";
                    return response;
                }
                var paymentOption = paymentModel.PaymentOptionsMetaData.First(itm => itm.Value == paymentModel.PaymentOption).Text;
                if (paymentOption == null)
                {
                    response.FailureInformation = "Invalid Payment option";
                    return response;
                }
                //var paymentOption = opt.Replace('_', ' ');
                if (paymentModel.SoftwareType == "100000000") paymentOption = "SET"; //Open

                var softwareOption = paymentModel.SoftwareTypesMetaData.First(itm => itm.Value == paymentModel.SoftwareType).Text;
                if (softwareOption == null)
                {
                    response.FailureInformation = "Invalid Software option";
                    return response;
                }
                //var softwareOption = soft.Replace('_', ' ');
                var quote = context.QuoteSet.FirstOrDefault(q => q.Id == quoteId);
                if (quote == null)
                {
                    response.FailureInformation = "No quote found";
                    return response;
                }
                var sqlServerIntraction = new SQLServerIntraction();
                var dbresponse = sqlServerIntraction.TestDatabaseConnection();
                if (dbresponse != string.Empty)
                {
                    response.FailureInformation = "Error initializing Descon database. Please try again later.";
                    return response;
                }

                var salesOrder = new Xrm.SalesOrder
                {
                    Name = account.Name,
                    Description = quote.Description,
                    SubmitDate = DateTime.Now,
                    TotalAmount = paymentModel.LicenseCost,
                    BillTo_Line1 = account.Address1_Line1,
                    BillTo_Line2 = account.Address1_Line2,
                    BillTo_City = account.Address1_City,
                    BillTo_StateOrProvince = account.Address1_StateOrProvince,
                    BillTo_Country = account.Address1_Country,
                    BillTo_PostalCode = account.Address1_PostalCode,
                    PriceLevelId = quote.PriceLevelId,
                    QuoteId = new CrmEntityReference(Xrm.Quote.EntityLogicalName, quoteId),
                    CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    TransactionCurrencyId = quote.TransactionCurrencyId == null ? null : new CrmEntityReference(Xrm.TransactionCurrency.EntityLogicalName, quote.TransactionCurrencyId.Id),
                    OwnerId = usrId == Guid.Empty ? null : new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                    ImportSequenceNumber = quote.ImportSequenceNumber,  //Using this as number of users
                    BillTo_ContactName = account.account_primary_contact.FirstName + " " + account.account_primary_contact.LastName,
                };

                var salesOrderId = context.Create(salesOrder);
                var salesOrder1 = context.SalesOrderSet.First(inv => inv.Id == salesOrderId);
                salesOrder1["psa_approved"] = true;
                salesOrder1["psa_approvedon"] = DateTime.Now;
                salesOrder1.RequestDeliveryBy = DateTime.Today.AddDays(30);
                var relatedEntity1 = new EntityReferenceCollection();
                relatedEntity1.Add(new EntityReference(Xrm.Account.EntityLogicalName, accountId));
                var relations1 = new Relationship("order_customer_accounts");
                context.Associate(SalesOrder.EntityLogicalName, salesOrderId, relations1, relatedEntity1);
                if (!context.GetAttachedEntities().Contains(salesOrder1)) context.Attach(salesOrder1);
                context.UpdateObject(salesOrder1);

                var template= context.ContractTemplateSet.FirstOrDefault(c => c.Name == "service");
                var templateId = template==null?(Guid?)null:template.Id;
                var contract = new Xrm.Contract
                {
                    Title = paymentOption,
                    CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    ActiveOn = DateTime.Today,
                    ExpiresOn = paymentOption == "SET"
                            ? (DateTime?)null
                            : (paymentOption == "MONTH" ? DateTime.Today.AddMonths(1) : DateTime.Today.AddYears(1)),
                    BillingStartOn = DateTime.Today,
                    BillingEndOn =
                        paymentOption == "SET"
                            ? (DateTime?)null
                            : (paymentOption == "MONTH" ? DateTime.Today.AddMonths(1) : DateTime.Today.AddYears(1)),
                    OwnerId =
                        usrId == Guid.Empty ? null : new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                    BillingCustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    ContractTemplateId = templateId == null ? null : new CrmEntityReference(Xrm.ContractTemplate.EntityLogicalName, (Guid)templateId),          
                };

                var contractId = context.Create(contract);
                var contract1 = context.ContractSet.FirstOrDefault(c => c.Id == contractId);
                if (quote.quote_details!=null && quote.quote_details.Any() && contract1!=null)
                {
                    var contractDetail = new ContractDetail
                    {
                        ContractId = new CrmEntityReference(Xrm.Contract.EntityLogicalName, contractId),
                        ProductId = new CrmEntityReference(Xrm.Product.EntityLogicalName, quote.quote_details.First().ProductId.Id),
                        ActiveOn = contract1.ActiveOn,
                        ExpiresOn = contract1.ExpiresOn,
                        Price = quote.TotalAmount
                    };
                    var detailId = context.Create(contractDetail);
                    var relatedEntity = new EntityReferenceCollection();
                    relatedEntity.Add(new EntityReference(ContractDetail.EntityLogicalName, detailId));
                    var relations = new Relationship("contract_line_items");
                    context.Associate(Contract.EntityLogicalName, contractId, relations, relatedEntity);
                    if (!context.GetAttachedEntities().Contains(contract1)) context.Attach(contract1);
                    context.UpdateObject(contract1);
                }
                account.new_ContractEndDate = contract.BillingEndOn.ToString();
                var acctrelatedEntity = new EntityReferenceCollection();
                acctrelatedEntity.Add(new EntityReference(Contract.EntityLogicalName, contractId));
                var acctrelatedEntity2 = new EntityReferenceCollection();
                acctrelatedEntity2.Add(new EntityReference(SalesOrder.EntityLogicalName, salesOrderId));
                var acctrelations = new Relationship("contract_customer_accounts");
                var bacctrelations = new Relationship("contract_billingcustomer_accounts");
                var iacctrelations = new Relationship("order_customer_accounts");
                context.Associate(Account.EntityLogicalName, accountId, acctrelations, acctrelatedEntity);
                context.Associate(Account.EntityLogicalName, accountId, bacctrelations, acctrelatedEntity);
                context.Associate(Account.EntityLogicalName, accountId, iacctrelations, acctrelatedEntity2);

                //update account
                account.new_DesconUsers = paymentModel.NumberOfUsers;
                //account.new_License = true;
                int softtype = 0;
                int paytype = 0;
                Int32.TryParse(paymentModel.SoftwareType, out softtype);
                Int32.TryParse(paymentModel.PaymentOption, out paytype);
                account.new_DesconLevel = softtype;
                account.new_DesconPayment = paytype;
                account.new_DesconPayment = paymentModel.SoftwareType == "100000000" ? 100000000 : paytype;
                account.new_DesconV8Start = DateTime.Now;
                if (!context.GetAttachedEntities().Contains(account)) context.Attach(account);
                context.UpdateObject(account);
                context.SaveChanges();

                //TODO: update this with just contract.ExpiresOn
                if(contract.ExpiresOn !=null)sqlServerIntraction.UpdateCompany(account.Name, softwareOption, paymentModel.NumberOfUsers, (DateTime)contract.ExpiresOn);

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GenericResponse { Success = false, FailureInformation = ex.Message };
            }
        }

        public static GenericResponse CreatePurchaseOrder(Guid quoteId, Guid accountId, XrmServiceContext context)
        {
            try
            {
                var response = new GenericResponse();
                var account = context.AccountSet.FirstOrDefault(q => q.Id == accountId);
                if (account == null)
                {
                    response.FailureInformation = "No account found";
                    return response;
                }
                var quote = context.QuoteSet.FirstOrDefault(q => q.Id == quoteId);
                if (quote == null)
                {
                    response.FailureInformation = "No quote found";
                    return response;
                }
                var po = new Xrm.SalesOrder
                {
                    Name = account.Name,
                    Description = quote.Description,
                    SubmitDate = DateTime.Now,
                    TotalAmount = quote.TotalAmount,
                    BillTo_Line1 = account.Address1_Line1,
                    BillTo_Line2 = account.Address1_Line2,
                    BillTo_City = account.Address1_City,
                    BillTo_StateOrProvince = account.Address1_StateOrProvince,
                    BillTo_Country = account.Address1_Country,
                    BillTo_PostalCode = account.Address1_PostalCode,
                    PriceLevelId = quote.PriceLevelId,
                    CustomerId = new CrmEntityReference(Xrm.Account.EntityLogicalName, account.Id),
                    TransactionCurrencyId = quote.TransactionCurrencyId == null ? null : new CrmEntityReference(Xrm.TransactionCurrency.EntityLogicalName, quote.TransactionCurrencyId.Id),
                    OwnerId = usrId == Guid.Empty ? null : new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                    ImportSequenceNumber = quote.ImportSequenceNumber,  //Using this as number of users
                    BillTo_ContactName = account.account_primary_contact.FirstName + " " + account.account_primary_contact.LastName,
                };

                var orderId = context.Create(po);
                var po1 = context.SalesOrderSet.First(sa => sa.Id == orderId);
                context.SaveChanges();
                var acctrelatedEntity = new EntityReferenceCollection();
                acctrelatedEntity.Add(new EntityReference(SalesOrder.EntityLogicalName, orderId));
                var acctrelations = new Relationship("order_customer_accounts");
                context.Associate(Account.EntityLogicalName, accountId, acctrelations, acctrelatedEntity);

                context.UpdateObject(po1);
                context.SaveChanges();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GenericResponse { Success = false, FailureInformation = ex.Message };
            }
        }

        public static GenericResponse CreateBaseContact(Guid accountId, string Email,  XrmServiceContext context, bool isRegisteredUser = false)
        {
            try
            {
                var response = new GenericResponse();
                var act = context.AccountSet.FirstOrDefault(q => q.Id == accountId);
                if (act == null)
                {
                    response.FailureInformation = "No account found";
                    return response;
                }
                var sqlServerIntraction = new SQLServerIntraction();
                var dbresponse = sqlServerIntraction.TestDatabaseConnection();
                if (dbresponse != string.Empty)
                {
                    response.FailureInformation = "Error initializing Descon database. Please try again later.";
                    return response;
                }
                var contact = new Contact
                {
                    EMailAddress1 = Email,
                    Company = act.Name,
                    OwnerId = usrId == Guid.Empty ? null : new CrmEntityReference(SystemUser.EntityLogicalName, usrId),
                    ParentCustomerId = new EntityReference(Xrm.Account.EntityLogicalName, act.Id),
                    new_RegisteredV8User = isRegisteredUser
                };
                var contactId = context.Create(contact);

                //TODO: Add back once company numbers working again
                //var getCompanyNumber = XrmQueryHelper.UpdateCompanyNumbers(newact, context);
                //if (getCompanyNumber.Success)
                //{
                //    XrmQueryHelper.IterateNextContactNumber(getCompanyNumber.RecordId,
                //        context);
                //    contact["adx_descon_supportid"] = getCompanyNumber.NextContactNumber;
                //}
                //TODO: Add contact number parameter to ADDNEWUSER method
                sqlServerIntraction.AddNewUser(act.Name, contact.EMailAddress1, "");
                AssociateContactsToAccount(new EntityReference(Contact.EntityLogicalName, contactId),new EntityReference(Account.EntityLogicalName, act.Id), context);
                context.SaveChanges();

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                return new GenericResponse { Success = false, FailureInformation = ex.Message };
            }
        }
    }

    public class GetQuoteResponse : GenericResponse
    {
        public GetQuoteResponse()
        {
            QuoteId = Guid.Empty;
            TotalCost = 0m;
            CostPerUser = 0m;
        }

        public Guid QuoteId { get; set; }

        public decimal TotalCost { get; set; }

        public decimal CostPerUser { get; set; }
    }

    public class GetSupportNumbersResponse:GenericResponse
    {
        public GetSupportNumbersResponse()
        {
            RecordId = Guid.Empty;
            NextCompanyNumber = 0;
            NextContactNumber = 0;
        }
        public long NextCompanyNumber { get; set; }

        public long NextContactNumber { get; set; }

        public Guid RecordId { get; set; }
    }

    public class AccountQuickDataModel
    {
        public AccountQuickDataModel()
        {
            Id = Guid.Empty;
            CompanyName = string.Empty;
            Address1 = string.Empty;
            Address2 = string.Empty;
            City = string.Empty;
            State = string.Empty;
            Country = string.Empty;
            Zip = string.Empty;
        }

        public string CompanyName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public string City { get; set; }

        public string State { get; set; }

        public string Country { get; set; }

        public string Zip { get; set; }

        public Guid Id { get; set; }
    }
}