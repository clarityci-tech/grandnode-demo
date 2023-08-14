using Grand.Infrastructure.ModelBinding;
using Grand.Infrastructure.Models;
using System;
namespace Payments.SportsNext.Direct.Models
{
	public class PaymentProviderModel : BaseEntityModel
    {
		public PaymentProviderModel()
		{
		}

        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.Store")]
        public string StoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.Store")]
        public string StoreName { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Key")]
        public string Key { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url")]
        public string Url { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.ClientId")]
        public string ClientId { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Secret")]
        public string Secret { get; set; }
    }
}

