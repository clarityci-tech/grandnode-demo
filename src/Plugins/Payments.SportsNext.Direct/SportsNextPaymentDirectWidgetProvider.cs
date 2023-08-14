using Grand.Business.Core.Interfaces.Cms;
using Grand.Business.Core.Interfaces.Common.Localization;
using System;
namespace Payments.SportsNext.Direct
{
	public class SportsNextPaymentDirectWidgetProvider : IWidgetProvider
    {
        private readonly ITranslationService _translationService;
        private readonly SportsNextPaymentsDirectSettings _paymentSettings;

        public SportsNextPaymentDirectWidgetProvider(ITranslationService translationService, SportsNextPaymentsDirectSettings paymentSettings)
        {
            _translationService = translationService;
            _paymentSettings = paymentSettings;
        }

        public string ConfigurationUrl => SNDirectDefaults.ConfigurationUrl;

        public string SystemName => SNDirectDefaults.ProviderSystemName;

        public string FriendlyName => _translationService.GetResource(SNDirectDefaults.FriendlyName);

        public int Priority => _paymentSettings.DisplayOrder;

        public IList<string> LimitedToStores => new List<string>();

        public IList<string> LimitedToGroups => new List<string>();

        public async Task<IList<string>> GetWidgetZones()
        {
            return await Task.FromResult(new[] { "checkout_payment_info_top" });
        }

        public Task<string> GetPublicViewComponentName(string widgetZone)
        {
            return Task.FromResult("PaymentsSportsNextDirectScripts");
        }
    }
}

