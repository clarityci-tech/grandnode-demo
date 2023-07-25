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
        ITaxProvider
    {
        private readonly ISportsNextTaxService _sportsNextTaxService;
        private readonly ICountryService _countryService;
        private readonly ICurrencyService _currencyService;
        private readonly ITranslationService _translationService;
        private readonly IWorkContext _workContext;

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
            _workContext = workContext;
            _sportsNextTaxService = sportsNextService;
            _taxSettings = taxSettings;
            _countryService = countryService;
            _currencyService = currencyService;
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

            if (calculateTaxRequest == null)
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

            if (country == null)
            {
                result.Errors.Add($"Country invalid (country id:{calculateTaxRequest.Address.CountryId})");
                return result;
            }

            var region = (await _countryService.GetStateProvincesByCountryId(country.Id)).FirstOrDefault(f => f.Id == calculateTaxRequest.Address.StateProvinceId);

            if (region == null)
            {
                result.Errors.Add($"Region invalid (state / province id:{calculateTaxRequest.Address.StateProvinceId})");
                return result;
            }

            var currency = await _currencyService.GetCurrencyById(_workContext.CurrentStore.DefaultCurrencyId);

            if (currency == null)
            {
                result.Errors.Add($"Default currency for store invalid (default currency id:{_workContext.CurrentStore.DefaultCurrencyId})");
                return result;
            }

            var category = await this.FindTaxCategorySku(calculateTaxRequest.TaxCategoryId, _workContext.CurrentStore.Id);

            if (category == null)
            {
                result.Errors.Add($"Tax category not found (tax category id:{calculateTaxRequest.TaxCategoryId}, store id:{_workContext.CurrentStore.Id})");
                return result;
            }

            var selected = await this.FindTaxProvider(_workContext.CurrentStore.Id);

            if (selected == null)
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
            catch (Exception ex)
            {
                result.Errors.Add(ex.Message);
                return result;
            }

            return result;
        }
    }
}

