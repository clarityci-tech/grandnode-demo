using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure.Plugins;
using Grand.Business.Core.Extensions;
using System;
namespace Tax.SportsNext
{
	public class SportsNextTaxPlugin : BasePlugin, IPlugin
    {
        private readonly ITranslationService _translationService;
        private readonly ILanguageService _languageService;

        public SportsNextTaxPlugin(ITranslationService translationService, ILanguageService languageService)
        {
            _translationService = translationService;
            _languageService = languageService;
        }


        /// <summary>
        /// Gets a configuration page URL
        /// </summary>
        public override string ConfigurationUrl()
        {
            return SportsNextTaxDefaults.ConfigurationUrl;
        }

        public override async Task Install()
        {
            //locales
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Tax.SportsNext", "Tax.SportsNext");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Tax.SportsNext.FriendlyName", "NBC Sports Next Tax Service");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.Store", "Store");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.Store.Hint", "If an asterisk is selected, then this entry will apply to all stores.");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Url", "SN Tax API Url");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Url.Hint", "The base url (including the scheme)");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.ClientId", "SN Client ID");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Secret", "SN Secret");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Key", "SN Tax Provider Key");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxCategory.Id", "Tax Category");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxCategory.SKU", "SN Tax SKU");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddProvider", "Add provider");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddProvider.Hint", "Adding a new tax provider");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddCategorySku", "Add category");
            await this.AddOrUpdatePluginTranslateResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddCategorySku.Hint", "Adding a new category sku");

            await base.Install();
        }

        /// <summary>
        /// Uninstall plugin
        /// </summary>
        public override async Task Uninstall()
        {
            //locales
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Tax.SportsNext");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Tax.SportsNext.FriendlyName");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.Store");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.Store.Hint");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Url");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Url.Hint");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.ClientId");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Secret");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxProvider.Key");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxCategory.Id");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.Fields.TaxCategory.SKU");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddProvider");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddProvider.Hint");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddCategorySku");
            await this.DeletePluginTranslationResource(_translationService, _languageService, "Plugins.Tax.SportsNext.AddCategorySku.Hint");

            await base.Uninstall();
        }
    }
}

