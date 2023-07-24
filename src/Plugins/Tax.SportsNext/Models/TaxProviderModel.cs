using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using System;
namespace Tax.SportsNext.Models
{
	public class TaxProviderModel : BaseEntityModel
    {
		public TaxProviderModel()
		{
		}

        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string StoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.Store")]
        public string StoreName { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Key")]
        public string Key { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Url")]
        public string Url { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.ClientId")]
        public string ClientId { get; set; }
        [GrandResourceDisplayName("Plugins.Tax.SportsNext.Fields.TaxProvider.Secret")]
        public string Secret { get; set; }
    }
}

