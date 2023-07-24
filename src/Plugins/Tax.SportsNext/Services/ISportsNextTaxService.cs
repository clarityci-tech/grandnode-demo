using Grand.Domain;
using System;
namespace Tax.SportsNext.Services
{
	public interface ISportsNextTaxService
	{
        Task DeleteCategorySku(Domain.SportsNextTaxCategorySku sku);

        Task<IPagedList<Domain.SportsNextTaxCategorySku>> GetAllCategorySkus(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<Domain.SportsNextTaxCategorySku> GetCategorySkuById(string skuId);

        Task InsertCategorySku(Domain.SportsNextTaxCategorySku sku);

        Task UpdateCategorySku(Domain.SportsNextTaxCategorySku sku);

        Task DeleteTaxProvider(Domain.SportsNextTaxProvider provider);

        Task<IPagedList<Domain.SportsNextTaxProvider>> GetAllTaxProviders(int pageIndex = 0, int pageSize = int.MaxValue);

        Task<Domain.SportsNextTaxProvider> GetTaxProviderById(string providerId);

        Task InsertTaxProvider(Domain.SportsNextTaxProvider provider);

        Task UpdateTaxProvider(Domain.SportsNextTaxProvider provider);

        Task InsertOrderTaxInvoiceMapping(Domain.SportsNextOrderTaxInvoiceMap map);

        Task<Domain.SportsNextOrderTaxInvoiceMap> GetOrderTaxInvoiceMappingById(string orderId);

        Task UpdateOrderTaxInvoiceMapping(Domain.SportsNextOrderTaxInvoiceMap map);
    }
}

