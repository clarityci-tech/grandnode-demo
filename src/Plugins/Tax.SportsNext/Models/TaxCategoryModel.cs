using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using System;
namespace Tax.SportsNext.Models
{
	public class TaxCategoryModel : BaseEntityModel
    {
		public TaxCategoryModel()
		{
		}

        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string StoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string StoreName { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxCategory.Id")]
        public string TaxCategoryId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxCategory.Id")]
        public string TaxCategoryName { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxCategory.SKU")]
        public string Sku { get; set; }
    }
}

