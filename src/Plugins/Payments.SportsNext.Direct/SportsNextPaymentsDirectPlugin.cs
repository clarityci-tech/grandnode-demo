using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Plugins;
using Grand.Business.Core.Extensions;
using System;

namespace Payments.SportsNext.Direct
{
	public class SportsNextPaymentsDirectPlugin : BasePlugin, IPlugin
    {
        private readonly ITranslationService _translationService;
        private readonly ILanguageService _languageService;

        public SportsNextPaymentsDirectPlugin(ITranslationService translationService, ILanguageService languageService)
        {
            _translationService = translationService;
            _languageService = languageService;
        }


        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string ConfigurationUrl()
        {
            return SNDirectDefaults.ConfigurationUrl;
        }

        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Payments.SportsNext.Direct", "Payments.SportsNext.Direct");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.PaymentMethodDescription", "NBC Sports Next Payment Service (Direct)");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Payments.SportsNext.Direct.FriendlyName", "NBC Sports Next Payment Service (Direct)");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.Store", "Store");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.Store.Hint", "If an asterisk is selected, then this entry will apply to all stores.");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url", "SN Payments API Url");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url.Hint", "The base url (including the scheme)");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.ClientId", "SN Client ID");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Secret", "SN Secret");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Key", "SN Payments Merchant Key");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.AddProvider", "Add provider");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.AddProvider.Hint", "Adding a new payment provider");
            //plugins.payments.sportsnext.direct.paymentmethoddescription

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Payments.SportsNext.Direct");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.PaymentMethodDescription");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Payments.SportsNext.Direct.FriendlyName");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.Store");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.Store.Hint");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Url.Hint");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.ClientId");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Secret");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.Fields.PaymentProvider.Key");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.AddProvider");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Payments.SportsNext.Direct.AddProvider.Hint");

            await base.Uninstall();
        }
    }
}

