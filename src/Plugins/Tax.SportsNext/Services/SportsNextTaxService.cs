using Grand.Domain.Data;
using Grand.Infrastructure.Caching;
using System;
using MediatR;
using Tax.SportsNext.Domain;
using Grand.Domain;
using Grand.Infrastructure.Extensions;
using Grand.Infrastructure.Plugins;

namespace Tax.SportsNext.Services
{
    public class SportsNextTaxService : ISportsNextTaxService
    {
        #region Constants
        private const string TAXCATEGORYSKU_ALL_KEY = "Grand.Tax.SportsNext.Category.all-{0}-{1}";
        private const string TAXCATEGORYSKU_PATTERN_KEY = "Grand.Tax.SportsNext.Category.";
        private const string TAXPROVIDER_ALL_KEY = "Grand.Tax.SportsNext.Provider.all-{0}-{1}";
        private const string TAXPROVIDER_PATTERN_KEY = "Grand.Tax.SportsNext.Provider.";
        #endregion

        #region Fields

        private readonly IMediator _mediator;
        private readonly IRepository<Domain.SportsNextTaxCategorySku> _taxCategorySkuRepository;
        private readonly IRepository<Domain.SportsNextTaxProvider> _taxProviderRepository;
        private readonly IRepository<Domain.SportsNextOrderTaxInvoiceMap> _orderTaxInvoiceMapRepository;
        private readonly ICacheBase _cacheBase;

        #endregion

        public SportsNextTaxService(IMediator mediator,
            ICacheBase cacheManager,
            IRepository<Domain.SportsNextTaxCategorySku> taxCategorySkuRepository,
            IRepository<Domain.SportsNextTaxProvider> taxProviderRepository,
            IRepository<Domain.SportsNextOrderTaxInvoiceMap> orderTaxInvoiceMapRepository)
        {
            _mediator = mediator;
            _cacheBase = cacheManager;
            _taxCategorySkuRepository = taxCategorySkuRepository;
            _taxProviderRepository = taxProviderRepository;
            _orderTaxInvoiceMapRepository = orderTaxInvoiceMapRepository;
        }

        public async Task DeleteCategorySku(SportsNextTaxCategorySku sku)
        {
            if (sku == null)
                throw new ArgumentNullException(nameof(sku));

            await _taxCategorySkuRepository.DeleteAsync(sku);

            await _cacheBase.RemoveByPrefix(TAXCATEGORYSKU_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(sku);
        }

        public async Task DeleteTaxProvider(Domain.SportsNextTaxProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _taxProviderRepository.DeleteAsync(provider);

            await _cacheBase.RemoveByPrefix(TAXPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(provider);
        }

        public async Task<IPagedList<SportsNextTaxCategorySku>> GetAllCategorySkus(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = string.Format(TAXCATEGORYSKU_ALL_KEY, pageIndex, pageSize);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var query = from tr in _taxCategorySkuRepository.Table
                            orderby tr.StoreId, tr.SKU, tr.Id, tr.TaxCategoryId
                            select tr;
                return await Task.FromResult(new PagedList<SportsNextTaxCategorySku>(query, pageIndex, pageSize));
            });
        }

        public async Task<IPagedList<Domain.SportsNextTaxProvider>> GetAllTaxProviders(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = string.Format(TAXPROVIDER_ALL_KEY, pageIndex, pageSize);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var query = from tr in _taxProviderRepository.Table
                            orderby tr.ClientId, tr.Id, tr.Key, tr.Secret, tr.StoreId, tr.Url
                            select tr;
                return await Task.FromResult(new PagedList<Domain.SportsNextTaxProvider>(query, pageIndex, pageSize));
            });
        }

        public Task<SportsNextTaxCategorySku> GetCategorySkuById(string skuId)
        {
            return _taxCategorySkuRepository.GetByIdAsync(skuId);
        }

        public Task<Domain.SportsNextTaxProvider> GetTaxProviderById(string providerId)
        {
            return _taxProviderRepository.GetByIdAsync(providerId);
        }

        public async Task InsertCategorySku(SportsNextTaxCategorySku sku)
        {
            if (sku == null)
                throw new ArgumentNullException(nameof(SportsNextTaxCategorySku));

            await _taxCategorySkuRepository.InsertAsync(sku);

            await _cacheBase.RemoveByPrefix(TAXCATEGORYSKU_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(sku);
        }

        public async Task InsertTaxProvider(Domain.SportsNextTaxProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _taxProviderRepository.InsertAsync(provider);

            await _cacheBase.RemoveByPrefix(TAXPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(provider);
        }

        public async Task UpdateCategorySku(SportsNextTaxCategorySku sku)
        {
            if (sku == null)
                throw new ArgumentNullException(nameof(sku));

            await _taxCategorySkuRepository.UpdateAsync(sku);

            await _cacheBase.RemoveByPrefix(TAXCATEGORYSKU_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(sku);
        }

        public async Task UpdateTaxProvider(Domain.SportsNextTaxProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _taxProviderRepository.UpdateAsync(provider);

            await _cacheBase.RemoveByPrefix(TAXPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(provider);
        }

        public async Task InsertOrderTaxInvoiceMapping(Domain.SportsNextOrderTaxInvoiceMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            await _orderTaxInvoiceMapRepository.InsertAsync(map);

            //event notification
            await _mediator.EntityInserted(map);
        }

        public Task<SportsNextOrderTaxInvoiceMap> GetOrderTaxInvoiceMappingByOrderId(string orderId)
        {
            var result = _orderTaxInvoiceMapRepository.Table.FirstOrDefault(o => o.OrderId == orderId);
            return Task.FromResult(result);
        }

        public async Task UpdateOrderTaxInvoiceMapping(SportsNextOrderTaxInvoiceMap map)
        {
            if (map == null)
                throw new ArgumentNullException(nameof(map));

            await _orderTaxInvoiceMapRepository.UpdateAsync(map);

            //event notification
            await _mediator.EntityUpdated(map);
        }
    }
}