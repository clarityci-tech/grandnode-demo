using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure;
using Grand.Infrastructure.Caching;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using Tax.SportsNext.Services;

namespace Tax.SportsNext.Handlers
{
	public class OrderPaidEventHandler :
        HandlerBase,
        INotificationHandler<OrderPaidEvent>

    {
        private readonly ICountryService _countryService;
        private readonly IProductService _productService;

        private readonly SportsNextTaxSettings _taxSettings;

        public OrderPaidEventHandler(
        ISportsNextTaxService sportsNextService,
        ICountryService countryService,
        IProductService productService,
        SportsNextTaxSettings taxSettings) : base(sportsNextService)
        {
            _taxSettings = taxSettings;
            _countryService = countryService;
            _productService = productService;
        }



        public async Task Handle(OrderPaidEvent notification, CancellationToken cancellationToken)
        {
            var order = notification.Order;
            var address = order.ShippingAddress ?? order.BillingAddress;

            if (address == null)
            {
                throw new Exception("address not set on order");
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
                    Description = product.Name
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
                    Addresses = new List<Payments.Service.Tax.Models.Address> { sourceAddress, destinationAddress },
                    ReferenceKey = order.OrderGuid.ToString()
                };

                var queryResult = await taxClient.CommitInvoiceAsync(taxRequest);

                await _sportsNextTaxService.InsertOrderTaxInvoiceMapping(new Domain.SportsNextOrderTaxInvoiceMap {
                    OrderGuid = order.OrderGuid,
                    OrderId = order.Id,
                    TaxInvoiceKey = queryResult.Invoice.InvoiceKey
                });
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

