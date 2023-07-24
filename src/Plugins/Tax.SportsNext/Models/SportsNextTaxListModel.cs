using Grand.Infrastructure.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
namespace Tax.SportsNext.Models
{
	public class SportsNextTaxListModel
	{
		public SportsNextTaxListModel()
		{

            AvailableStores = new List<SelectListItem>();
            AvailableTaxCategories = new List<SelectListItem>();
        }

        // add tax providers
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string AddTaxProviderStoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Url")]
        public string AddTaxProviderUrl { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.ClientId")]
        public string AddTaxProviderClientId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Secret")]
        public string AddTaxProviderSecret { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Key")]
        public string AddTaxProviderKey { get; set; }

        // add tax categories
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string AddTaxCategoryStoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxCategory.Id")]
        public string AddTaxCategoryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxCategory.SKU")]
        public string AddTaxCategorySku { get; set; }

        // lookups
        public IList<SelectListItem> AvailableStores { get; set; }
        public IList<SelectListItem> AvailableTaxCategories { get; set; }
    }
}

