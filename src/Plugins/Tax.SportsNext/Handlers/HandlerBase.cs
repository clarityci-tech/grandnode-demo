using System;
using Tax.SportsNext.Services;

namespace Tax.SportsNext.Handlers
{
	public abstract class HandlerBase
    {
        protected readonly ISportsNextTaxService _sportsNextTaxService;
        public HandlerBase(ISportsNextTaxService sportsNextService)
		{
            _sportsNextTaxService = sportsNextService;
		}

        protected async Task<Domain.SportsNextTaxProvider> FindTaxProvider(string storeId)
        {
            var providers = await _sportsNextTaxService.GetAllTaxProviders();

            var selected =
                providers.FirstOrDefault(p => string.Compare(p.StoreId, storeId, true) == 0) ??
                providers.FirstOrDefault(p => p.StoreId == null);

            return selected;
        }

        protected async Task<Domain.SportsNextTaxCategorySku> FindTaxCategorySku(string taxCategoryId, string storeId)
        {
            var skus = await _sportsNextTaxService.GetAllCategorySkus();

            var selected =
                skus.FirstOrDefault(s => string.Compare(s.StoreId, storeId, true) == 0 && string.Compare(s.TaxCategoryId, taxCategoryId, true) == 0) ??
                skus.FirstOrDefault(s => s.StoreId == null && string.Compare(s.TaxCategoryId, taxCategoryId, true) == 0);

            return selected;
        }
    }
}

