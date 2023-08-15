using GolfNow.Payment.Processing.API.Model;
using Grand.Business.Core.Enums.Checkout;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Checkout.Payments;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Interfaces.Customers;
using Grand.Business.Core.Utilities.Checkout;
using Grand.Domain.Orders;
using Grand.Domain.Payments;
using MediatR;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Payments.SportsNext.Direct.Models;
using Payments.SportsNext.Direct.Services;
using Payments.SportsNext.Direct.Validators;
using SportsNext.Shared;
using SportsNext.Shared.Models;
using System;
using static MassTransit.ValidationResultExtensions;

namespace Payments.SportsNext.Direct
{
	public class SportsNextPaymentsProvider : IPaymentProvider
	{
        private readonly ITranslationService _translationService;
        private readonly ICustomerService _customerService;
        private readonly IServiceProvider _serviceProvider;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICountryService _countryService;
        private readonly SportsNextPaymentsDirectSettings _sportsNextPaymentsDirectSettings;
        private readonly Services.ISportsNextPaymentService _sportsNextPaymentService;
        private readonly IOrderService _orderService;

        public SportsNextPaymentsProvider(
            ITranslationService translationService,
            ICustomerService customerService,
            IServiceProvider serviceProvider,
            IHttpContextAccessor httpContextAccessor,
            ICountryService countryService,
            SportsNextPaymentsDirectSettings sportsNextPaymentsDirectSettings,
            Services.ISportsNextPaymentService sportsNextPaymentService,
            IOrderService orderService)
        {
            _translationService = translationService;
            _customerService = customerService;
            _serviceProvider = serviceProvider;
            _httpContextAccessor = httpContextAccessor;
            _countryService = countryService;
            _sportsNextPaymentsDirectSettings = sportsNextPaymentsDirectSettings;
            _sportsNextPaymentService = sportsNextPaymentService;
            _orderService = orderService;
        }

        private async Task<Domain.SportsNextPaymentProvider> FindPaymentProvider(string storeId)
        {
            var providers = await _sportsNextPaymentService.GetAllPaymentProviders();

            var selected =
                providers.FirstOrDefault(p => string.Compare(p.StoreId, storeId, true) == 0) ??
                providers.FirstOrDefault(p => p.StoreId == null);

            return selected;
        }

        /// <summary>
        /// Captures payment
        /// </summary>
        /// <returns>Capture payment result</returns>
        public async Task<CapturePaymentResult> Capture(PaymentTransaction paymentTransaction)
        {
            var result = new CapturePaymentResult();
            result.AddError("Capture method not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Refunds a payment
        /// </summary>
        /// <param name="refundPaymentRequest">Request</param>
        /// <returns>Result</returns>
        public async Task<RefundPaymentResult> Refund(RefundPaymentRequest refundPaymentRequest)
        {
            var result = new RefundPaymentResult();

            // get customer
            var customer = await _customerService.GetCustomerById(refundPaymentRequest.PaymentTransaction.CustomerId);

            //get settings
            var provider = await this.FindPaymentProvider(refundPaymentRequest.PaymentTransaction.StoreId);
            var merchantKey = provider.Key;
            var clientId = provider.ClientId;
            var secret = provider.Secret;
            var url = provider.Url;

            //new gateway
            var client = new PaymentProcessingClient(url, clientId, secret);

            try
            {
                var response = await client.PartialTransactionItemCredit(new SingleItemReversal {
                    ExternalOrderID = refundPaymentRequest.PaymentTransaction.Id,
                    ConfirmationID = refundPaymentRequest.PaymentTransaction.CaptureTransactionId,
                    Amount = Convert.ToDecimal(refundPaymentRequest.AmountToRefund),
                    MerchantKey = merchantKey,
                    ExternalAccountID = customer.CustomerGuid.ToString(),
                });

                var existing = (await client.GetTransactions(
                    refundPaymentRequest.PaymentTransaction.CaptureTransactionId,
                    refundPaymentRequest.PaymentTransaction.Id,
                    merchantKey)).FirstOrDefault(t => string.Compare(t.Type,"payment",true) == 0);

                result.NewTransactionStatus = existing == null || existing.Amount.Net <= decimal.Zero ?
                    TransactionStatus.Refunded :
                    TransactionStatus.PartiallyRefunded;
            }
            catch (ClientException<ErrorResponse> ex)
            {
                result.AddError(ex.Response.ErrorDetail);
            }
            catch (Exception ex)
            {
                result.AddError("Unable to refund transaction." + ex.ToString());
            }

            return await Task.FromResult(result);
        }

        /// <summary>
        /// Voids a payment
        /// </summary>
        /// <returns>Result</returns>
        public async Task<VoidPaymentResult> Void(PaymentTransaction paymentTransaction)
        {
            var result = new VoidPaymentResult();
            result.AddError("Void method not supported");
            return await Task.FromResult(result);
        }

        /// <summary>
        /// Cancel a payment
        /// </summary>
        /// <returns>Result</returns>
        public Task CancelPayment(PaymentTransaction paymentTransaction)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Gets a value indicating whether customers can complete a payment after order is placed but not completed (for redirection payment methods)
        /// </summary>
        /// <param name="paymentTransaction"></param>
        /// <returns>Result</returns>
        public async Task<bool> CanRePostRedirectPayment(PaymentTransaction paymentTransaction)
        {
            if (paymentTransaction == null)
                throw new ArgumentNullException(nameof(paymentTransaction));

            //it's not a redirection payment method. So we always return false
            return await Task.FromResult(false);
        }


        /// <summary>
        /// Gets a value indicating whether capture is supported
        /// </summary>
        public async Task<bool> SupportCapture()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a value indicating whether partial refund is supported
        /// </summary>
        public async Task<bool> SupportPartiallyRefund()
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Gets a value indicating whether refund is supported
        /// </summary>
        public async Task<bool> SupportRefund()
        {
            return await Task.FromResult(true);
        }

        /// <summary>
        /// Gets a value indicating whether void is supported
        /// </summary>
        public async Task<bool> SupportVoid()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a payment method type
        /// </summary>
        public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

        public string ConfigurationUrl => SNDirectDefaults.ConfigurationUrl;

        public string SystemName => SNDirectDefaults.ProviderSystemName;

        public string FriendlyName => _translationService.GetResource(SNDirectDefaults.FriendlyName);

        public int Priority => _sportsNextPaymentsDirectSettings.DisplayOrder;

        public IList<string> LimitedToStores => new List<string>();

        public IList<string> LimitedToGroups => new List<string>();

        /// <summary>
        /// Gets a value indicating whether we should display a payment information page for this plugin
        /// </summary>
        public async Task<bool> SkipPaymentInfo()
        {
            return await Task.FromResult(false);
        }

        /// <summary>
        /// Gets a payment method description that will be displayed on checkout pages in the public store
        /// </summary>
        public async Task<string> Description()
        {
            return await Task.FromResult(
                _translationService.GetResource("Plugins.Payments.SportsNext.Direct.PaymentMethodDescription"));
        }

        public Task<PaymentTransaction> InitPaymentTransaction()
        {
            return Task.FromResult<PaymentTransaction>(null);
        }

        /// <summary>
        /// Process a payment
        /// </summary>
        /// <returns>Process payment result</returns>
        public async Task<ProcessPaymentResult> ProcessPayment(PaymentTransaction paymentTransaction)
        {
            var processPaymentResult = new ProcessPaymentResult();

            // get customer
            var customer = await _customerService.GetCustomerById(paymentTransaction.CustomerId);

            // var region stuff
            var country = await _countryService.GetCountryById(customer.BillingAddress.CountryId);
            var region = (await _countryService.GetStateProvincesByCountryId(country.Id)).FirstOrDefault(f => f.Id == customer.BillingAddress.StateProvinceId);

            //get settings
            var provider = await this.FindPaymentProvider(paymentTransaction.StoreId);
            var merchantKey = provider.Key;
            var clientId = provider.ClientId;
            var secret = provider.Secret;
            var url = provider.Url;

            //new gateway
            var client = new PaymentProcessingClient(url, clientId, secret);

            try
            {
                var response = await client.ProcessItem(new GolfNow.Payment.Processing.API.Model.SingleItem {
                    Amount = Convert.ToDecimal(paymentTransaction.TransactionAmount),
                    MerchantKey = merchantKey,
                    ExternalOrderID = paymentTransaction.Id,
                    Currency = paymentTransaction.CurrencyCode,
                    Payment = new CreditCardPaymentMethod {
                        CardNumber = _httpContextAccessor.HttpContext!.Session.GetString("CardNumber")!,
                        ExpirationMonth = _httpContextAccessor.HttpContext.Session.GetString("ExpireMonth")!,
                        ExpirationYear = _httpContextAccessor.HttpContext.Session.GetString("ExpireYear")!,
                        NameOnCard = _httpContextAccessor.HttpContext.Session.GetString("CardholderName")!,
                        ExternalAccountId = customer.CustomerGuid.ToString(),
                        Address = new GolfNow.Payment.Processing.API.Common.Model.Address {
                            Line1 = customer.BillingAddress.Address1,
                            PostalCode = customer.BillingAddress.ZipPostalCode,
                            City = customer.BillingAddress.City,
                            State = region.Name,
                            CountryCode = country.ThreeLetterIsoCode
                        }
                    },
                    CVV = _httpContextAccessor.HttpContext.Session.GetString("CardCode")!
                });

                var result = response?.Results?.FirstOrDefault();

                if (result != null && result.IsSuccessful)
                {
                    processPaymentResult.PaidAmount = paymentTransaction.TransactionAmount;
                    paymentTransaction.AuthorizationTransactionId = result.TransactionID;
                    paymentTransaction.CaptureTransactionId = response.ConfirmationID;
                    processPaymentResult.NewPaymentTransactionStatus = Grand.Domain.Payments.TransactionStatus.Paid;
                }
                else if(result != null)
                {
                    processPaymentResult.AddError("Error processing payment." + result.TransactionStatus);
                }
                else
                {
                    processPaymentResult.AddError("Error processing payment.");
                }
            }
            catch(ClientException<ErrorResponse> ex)
            {
                processPaymentResult.AddError(ex.Response.ErrorDetail);
            }
            catch(Exception ex)
            {
                processPaymentResult.AddError("Error processing payment." + ex.Message);
            }

            return processPaymentResult;
        }

        public Task PostProcessPayment(PaymentTransaction paymentTransaction)
        {
            return Task.CompletedTask;
        }

        public Task PostRedirectPayment(PaymentTransaction paymentTransaction)
        {
            return Task.CompletedTask;
        }

        public Task<bool> HidePaymentMethod(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(false);
        }

        public Task<double> GetAdditionalHandlingFee(IList<ShoppingCartItem> cart)
        {
            return Task.FromResult(0.00);
        }

        public async Task<IList<string>> ValidatePaymentForm(IDictionary<string, string> model)
        {
            var warnings = new List<string>();
            //validate
            var validator = new PaymentInfoValidator(_sportsNextPaymentsDirectSettings, _translationService);
            var paymentInfoModel = new PaymentInfoModel();
            if (model.TryGetValue("CardholderName", out var cardholderName))
                paymentInfoModel.CardholderName = cardholderName;
            if (model.TryGetValue("CardNumber", out var cardNumber))
                paymentInfoModel.CardNumber = cardNumber;
            if (model.TryGetValue("CardCode", out var cardCode))
                paymentInfoModel.CardCode = cardCode;
            if (model.TryGetValue("ExpireMonth", out var expireMonth))
                paymentInfoModel.ExpireMonth = expireMonth;
            if (model.TryGetValue("ExpireYear", out var expireYear))
                paymentInfoModel.ExpireYear = expireYear;
            if (model.TryGetValue("CardNonce", out var cardNonce))
                paymentInfoModel.CardNonce = cardNonce;

            var validationResult = await validator.ValidateAsync(paymentInfoModel);
            if (validationResult.IsValid) return await Task.FromResult(warnings);
            warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
            return await Task.FromResult(warnings);
        }

        public async Task<PaymentTransaction> SavePaymentInfo(IDictionary<string, string> model)
        {
            if (model.TryGetValue("CardNonce", out var cardNonce) && !StringValues.IsNullOrEmpty(cardNonce))
                _httpContextAccessor.HttpContext!.Session.SetString("CardNonce", cardNonce);

            if (model.TryGetValue("CardholderName", out var cardholderName) &&
                !StringValues.IsNullOrEmpty(cardholderName))
                _httpContextAccessor.HttpContext!.Session.SetString("CardholderName", cardholderName);

            if (model.TryGetValue("CardNumber", out var cardNumber) && !StringValues.IsNullOrEmpty(cardNumber))
                _httpContextAccessor.HttpContext!.Session.SetString("CardNumber", cardNumber);

            if (model.TryGetValue("ExpireMonth", out var expireMonth) && !StringValues.IsNullOrEmpty(expireMonth))
                _httpContextAccessor.HttpContext!.Session.SetString("ExpireMonth", expireMonth);

            if (model.TryGetValue("ExpireYear", out var expireYear) && !StringValues.IsNullOrEmpty(expireYear))
                _httpContextAccessor.HttpContext!.Session.SetString("ExpireYear", expireYear);

            if (model.TryGetValue("CardCode", out var creditCardCvv2) && !StringValues.IsNullOrEmpty(creditCardCvv2))
                _httpContextAccessor.HttpContext!.Session.SetString("CardCode", creditCardCvv2);

            return await Task.FromResult<PaymentTransaction>(null);
        }

        public Task<string> GetControllerRouteName()
        {
            return Task.FromResult("Plugins.Payments.SportsNext.Direct");
        }

        public string LogoURL => "/Plugins/Payments.SportsNext.Direct/logo.jpg";
    }
}

