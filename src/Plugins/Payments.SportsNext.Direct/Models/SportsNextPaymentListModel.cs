using Grand.Infrastructure.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
namespace Payments.SportsNext.Direct.Models
{
	public class SportsNextPaymentListModel
	{
		public SportsNextPaymentListModel()
        {
            AvailableStores = new List<SelectListItem>();
        }

        // add payment providers
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.Store")]
        public string AddPaymentProviderStoreId { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url")]
        public string AddPaymentProviderUrl { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.ClientId")]
        public string AddPaymentProviderClientId { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Secret")]
        public string AddPaymentProviderSecret { get; set; }
        [GrandResourceDisplayName("Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Key")]
        public string AddPaymentProviderKey { get; set; }

        // lookups
        public IList<SelectListItem> AvailableStores { get; set; }
    }
}

