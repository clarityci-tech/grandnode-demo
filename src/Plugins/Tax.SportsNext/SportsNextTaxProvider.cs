using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Business.Core.Utilities.Catalog;
using Grand.Infrastructure;
using Grand.Infrastructure.Caching;
using MediatR;
using System;
using Tax.SportsNext.Services;
using static MassTransit.ValidationResultExtensions;

namespace Tax.SportsNext
{
	public class SportsNextTaxProvider :
        ITaxProvider,
        INotificationHandler<OrderPlacedEvent>,
        INotificationHandler<PaymentTransactionRefundedEvent>
    {
        private readonly ISportsNextTaxService _sportsNextTaxService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ITranslationService _translationService;
        private readonly IProductService _productService;
        private readonly ICacheBase _cacheBase;
        private readonly IWorkContext _workContext;
        private readonly IOrderService _orderService;

        private readonly SportsNextTaxSettings _taxSettings;

        public SportsNextTaxProvider(ITranslationService translationService,
        ICacheBase cacheBase,
        IWorkContext workContext,
        ISportsNextTaxService sportsNextService,
        ICountryService countryService,
        ICurrencyService currencyService,
        IProductService productService,
        IOrderService orderService,
        SportsNextTaxSettings taxSettings)
        {
            _translationService = translationService;
            _cacheBase = cacheBase;
            _workContext = workContext;
            _sportsNextTaxService = sportsNextService;
            _taxSettings = taxSettings;
            _countryService = countryService;
            _currencyService = currencyService;
            _productService = productService;
            _orderService = orderService;
        }

        public string ConfigurationUrl => SportsNextTaxDefaults.ConfigurationUrl;

        public string SystemName => SportsNextTaxDefaults.ProviderSystemName;

        public string FriendlyName => _translationService.GetResource(SportsNextTaxDefaults.FriendlyName);

        public int Priority => _taxSettings.DisplayOrder;

        public IList<string> LimitedToStores => new List<string>();

        public IList<string> LimitedToGroups => new List<string>();

        private async Task<Domain.SportsNextTaxProvider> FindTaxProvider(string storeId)
        {
            var providers = await _sportsNextTaxService.GetAllTaxProviders();

            var selected =
                providers.FirstOrDefault(p => string.Compare(p.StoreId, storeId, true) == 0) ??
                providers.FirstOrDefault(p => p.StoreId == null);

            return selected;
        }

        private async Task<Domain.SportsNextTaxCategorySku> FindTaxCategorySku(string taxCategoryId, string storeId)
        {
            var skus = await _sportsNextTaxService.GetAllCategorySkus();

            var selected =
                skus.FirstOrDefault(s => string.Compare(s.StoreId, storeId, true) == 0 && string.Compare(s.TaxCategoryId, taxCategoryId, true) == 0) ??
                skus.FirstOrDefault(s => s.StoreId == null && string.Compare(s.TaxCategoryId, taxCategoryId, true) == 0);

            return selected;
        }

        public async Task<TaxResult> GetTaxRate(TaxRequest calculateTaxRequest)
        {
            var result = new TaxResult { };

            if(calculateTaxRequest == null)
            {
                result.Errors.Add("Invalid tax request");
                return result;
            }

            if (calculateTaxRequest.Address == null)
            {
                result.Errors.Add("Address is not set");
                return result;
            }

            var country = await _countryService.GetCountryById(calculateTaxRequest.Address.CountryId);

            if(country == null)
            {
                result.Errors.Add($"Country invalid (country id:{calculateTaxRequest.Address.CountryId})");
                return result;
            }

            var region = (await _countryService.GetStateProvincesByCountryId(country.Id)).FirstOrDefault(f => f.Id == calculateTaxRequest.Address.StateProvinceId);

            if(region == null)
            {
                result.Errors.Add($"Region invalid (state / province id:{calculateTaxRequest.Address.StateProvinceId})");
                return result;
            }

            var currency = await _currencyService.GetCurrencyById(_workContext.CurrentStore.DefaultCurrencyId);

            if(currency == null)
            {
                result.Errors.Add($"Default currency for store invalid (default currency id:{_workContext.CurrentStore.DefaultCurrencyId})");
                return result;
            }

            var category = await this.FindTaxCategorySku(calculateTaxRequest.TaxCategoryId, _workContext.CurrentStore.Id);

            if(category == null)
            {
                result.Errors.Add($"Tax category not found (tax category id:{calculateTaxRequest.TaxCategoryId}, store id:{_workContext.CurrentStore.Id})");
                return result;
            }

            var selected = await this.FindTaxProvider(_workContext.CurrentStore.Id);

            if(selected == null)
            {
                result.Errors.Add($"Tax provider not found (store id:{_workContext.CurrentStore.Id})");
                return result;
            }

            var taxClient = new Payments.Service.Tax.Client.TaxClient(selected.Url, selected.ClientId, selected.Secret);

            var destinationAddress = new Payments.Service.Tax.Models.Address {
                AddressType = Payments.Service.Tax.Models.Enumerations.AddressType.PhysicalDestination,
                City = calculateTaxRequest.Address.City,
                Line1 = calculateTaxRequest.Address.Address1,
                Line2 = calculateTaxRequest.Address.Address2,
                PostalCode = calculateTaxRequest.Address.ZipPostalCode,
                Country = country.ThreeLetterIsoCode,
                Region = region.Name
            };

            var sourceAddress = new Payments.Service.Tax.Models.Address {
                AddressType = Payments.Service.Tax.Models.Enumerations.AddressType.PhysicalOrigin,
                City = calculateTaxRequest.Address.City,
                Line1 = calculateTaxRequest.Address.Address1,
                Line2 = calculateTaxRequest.Address.Address2,
                PostalCode = calculateTaxRequest.Address.ZipPostalCode,
                Country = country.ThreeLetterIsoCode,
                Region = region.Name
            };

            var lineItem = new Payments.Service.Tax.Models.LineItem {
                Amount = (decimal)calculateTaxRequest.Price,
                Addresses = new List<Payments.Service.Tax.Models.Address> {
                    destinationAddress
                },
                IsTaxIncluded = false,
                Quantity = 1,
                TaxCode = category.SKU,
            };

            try
            {
                var taxRequest = new Payments.Service.Tax.Models.Contracts.QueryInvoiceRequest {
                    LineItems = new List<Payments.Service.Tax.Models.LineItem> { lineItem },
                    CurrencyCode = currency.CurrencyCode,
                    EffectiveDate = DateTime.UtcNow,
                    TaxEntityKey = selected.Key,
                    Addresses = new List<Payments.Service.Tax.Models.Address> { sourceAddress }
                };

                //result.Errors.Add(Newtonsoft.Json.JsonConvert.SerializeObject(taxRequest));

                var queryResult = await taxClient.QueryInvoiceAsync(taxRequest);

                result.TaxRate = (double)(queryResult.Invoice.LineItems.First().Rate * 100);
            }
            catch(Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }

            return result;
        }

        public async Task Handle(OrderPlacedEvent notification, CancellationToken cancellationToken)
        {
            var order = notification.Order;
            var address = order.ShippingAddress;

            if (address == null)
            {
                return;
            }

            var country = await _countryService.GetCountryById(address.CountryId);
            var region = (await _countryService.GetStateProvincesByCountryId(country.Id)).FirstOrDefault(f => f.Id == address.StateProvinceId);
            var selected = await this.FindTaxProvider(order.StoreId);

            var destinationAddress = new Payments.Service.Tax.Models.Address {
                AddressType = Payments.Service.Tax.Models.Enumerations.AddressType.PhysicalDestination,
                City = address.City,
                Line1 = address.Address1,
                Line2 = address.Address2,
                PostalCode = address.ZipPostalCode,
                Country = country.ThreeLetterIsoCode,
                Region = region.Name
            };

            var sourceAddress = new Payments.Service.Tax.Models.Address {
                AddressType = Payments.Service.Tax.Models.Enumerations.AddressType.PhysicalOrigin,
                City = address.City,
                Line1 = address.Address1,
                Line2 = address.Address2,
                PostalCode = address.ZipPostalCode,
                Country = country.ThreeLetterIsoCode,
                Region = region.Name
            };

            var items = new List<Payments.Service.Tax.Models.LineItem>();

            foreach (var item in order.OrderItems)
            {
                var product = await _productService.GetProductById(item.ProductId);
                var category = await this.FindTaxCategorySku(product.TaxCategoryId, order.StoreId);

                var lineItem = new Payments.Service.Tax.Models.LineItem {
                    Amount = (decimal)item.UnitPriceExclTax,
                    Addresses = new List<Payments.Service.Tax.Models.Address> {
                    destinationAddress
                },
                    IsTaxIncluded = false,
                    Quantity = item.Quantity,
                    TaxCode = category.SKU,
                };

                items.Add(lineItem);
            }

            try
            {
                var taxClient = new Payments.Service.Tax.Client.TaxClient(selected.Url, selected.ClientId, selected.Secret);

                var taxRequest = new Payments.Service.Tax.Models.Contracts.CommitInvoiceRequest {
                    LineItems = items,
                    CurrencyCode = order.PrimaryCurrencyCode ?? order.CustomerCurrencyCode,
                    EffectiveDate = DateTime.UtcNow,
                    TaxEntityKey = selected.Key,
                    Addresses = new List<Payments.Service.Tax.Models.Address> { sourceAddress },
                    ReferenceKey = order.OrderGuid.ToString()
                };

                var queryResult = await taxClient.CommitInvoiceAsync(taxRequest);

                await _sportsNextTaxService.InsertOrderTaxInvoiceMapping(new Domain.SportsNextOrderTaxInvoiceMap {
                    OrderGuid = order.OrderGuid,
                    TaxInvoiceKey = queryResult.Invoice.InvoiceKey
                });
            }
            catch (Exception ex)
            {
                return;
            }
        }

        public async Task Handle(PaymentTransactionRefundedEvent notification, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByGuid(notification.PaymentTransaction.OrderGuid);
            var selected = await this.FindTaxProvider(order.StoreId);
            var mapping = await _sportsNextTaxService.GetOrderTaxInvoiceMappingById(order.Id);

            var taxClient = new Payments.Service.Tax.Client.TaxClient(selected.Url, selected.ClientId, selected.Secret);

            var taxInvoice = await taxClient.GetInvoiceAsync(mapping.TaxInvoiceKey);

            var totalLeft = (decimal)notification.PaymentTransaction.RefundedAmount;
            var items = taxInvoice.LineItems.OrderByDescending(li => li.Remaining.Total).ToList();
            var instructions = new List<Payments.Service.Tax.Models.RefundLineInstruction>();

            foreach (var item in items)
            {
                var itemRemaining = (decimal)item.Remaining.Total;

                if(itemRemaining <= decimal.Zero)
                {
                    continue;
                }

                var applicable = totalLeft > itemRemaining ? itemRemaining : totalLeft;
                var percentage = applicable / itemRemaining;

                var grossRefund = Math.Round(item.Remaining.Gross * percentage, 2);

                if (grossRefund > decimal.Zero)
                {
                    instructions.Add(new Payments.Service.Tax.Models.RefundLineInstruction { Amount = grossRefund, LineNumber = item.LineNumber });
                    totalLeft -= applicable;
                }
            }

            var refundRequest = new Payments.Service.Tax.Models.Contracts.RefundInvoiceRequest
            {
                Instructions = new Payments.Service.Tax.Models.RefundInstructions {
                    EffectiveDate = DateTime.UtcNow,
                    LineInstructions = instructions,
                    Reason = "Customer Refund",
                    RefundType = Payments.Service.Tax.Models.Enumerations.TaxRefundType.Partial
                }
            };

            var refundResult = await taxClient.RefundInvoiceAsync(mapping.TaxInvoiceKey, refundRequest);

            if (refundResult.InvoiceKey.HasValue)
            {
                mapping.RefundInvoiceKeys.Add(refundResult.InvoiceKey.ToString());
                await _sportsNextTaxService.UpdateOrderTaxInvoiceMapping(mapping);
            }
        }
    }
}

