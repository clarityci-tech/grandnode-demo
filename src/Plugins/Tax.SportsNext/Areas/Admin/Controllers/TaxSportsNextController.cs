using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Business.Core.Interfaces.Common.Configuration;
using Grand.Business.Core.Interfaces.Common.Stores;
using Grand.Business.Core.Utilities.Common.Security;
using Grand.Web.Common.Controllers;
using Grand.Web.Common.DataSource;
using Grand.Web.Common.Filters;
using Grand.Web.Common.Security.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using Tax.SportsNext.Services;
using Wangkanai.Extensions;

namespace Tax.SportsNext.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.TaxSettings)]
    public class TaxSportsNextController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;
        private readonly ISportsNextTaxService _sportsNextTaxService;
        private readonly IStoreService _storeService;

        public TaxSportsNextController(
            ITaxCategoryService taxCategoryService,
            ISettingService settingService,
            ISportsNextTaxService sportsNextTaxService,
            IStoreService storeService)
        {
            _taxCategoryService = taxCategoryService;
            _settingService = settingService;
            _sportsNextTaxService = sportsNextTaxService;
            _storeService = storeService;
        }


        public async Task<IActionResult> Configure()
        {
            var model = new Models.SportsNextTaxListModel();
            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "" });
            var stores = await _storeService.GetAllStores();

            foreach (var s in stores)
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id });
            }

            //tax categories
            var taxCategories = await _taxCategoryService.GetAllTaxCategories();

            if (taxCategories.Count == 0)
                return Content("No tax categories can be loaded");

            foreach (var tc in taxCategories)
            {
                model.AvailableTaxCategories.Add(new SelectListItem { Text = tc.Name, Value = tc.Id });
            }

            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProviderList(DataSourceRequest command)
        {
            var records = await _sportsNextTaxService.GetAllTaxProviders(command.Page - 1, command.PageSize);
            var models = new List<Models.TaxProviderModel>();

            foreach (var x in records)
            {
                var m = new Models.TaxProviderModel {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    ClientId = x.ClientId,
                    Key = x.Key,
                    Secret = x.Secret,
                    Url = x.Url
                };

                //store
                var store = await _storeService.GetStoreById(x.StoreId);
                m.StoreName = store != null ? store.Shortcut : "*";

                models.Add(m);
            }

            var gridModel = new DataSourceResult {
                Data = models,
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CategoryList(DataSourceRequest command)
        {
            var records = await _sportsNextTaxService.GetAllCategorySkus(command.Page - 1, command.PageSize);
            var models = new List<Models.TaxCategoryModel>();

            foreach (var x in records)
            {
                var m = new Models.TaxCategoryModel {
                    Id = x.Id,
                    StoreId = x.StoreId,
                    Sku = x.SKU,
                    TaxCategoryId = x.TaxCategoryId,
                };

                //store
                var store = await _storeService.GetStoreById(x.StoreId);
                m.StoreName = store != null ? store.Shortcut : "*";

                var category = await _taxCategoryService.GetTaxCategoryById(x.TaxCategoryId);
                m.TaxCategoryName = category != null ? category.Name : "*";

                models.Add(m);
            }

            var gridModel = new DataSourceResult {
                Data = models,
                Total = records.TotalCount
            };

            return Json(gridModel);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProviderUpdate(Models.TaxProviderModel model)
        {
            var obj = await _sportsNextTaxService.GetTaxProviderById(model.Id);
            obj.ClientId = model.ClientId;
            obj.Secret = model.Secret;
            obj.Url = model.Url;
            obj.Key = model.Key;
            await _sportsNextTaxService.UpdateTaxProvider(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProviderDelete(string id)
        {
            var obj = await _sportsNextTaxService.GetTaxProviderById(id);
            if (obj != null)
                await _sportsNextTaxService.DeleteTaxProvider(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddProvider(Models.SportsNextTaxListModel model)
        {
            var obj = new Domain.SportsNextTaxProvider
            {
                ClientId = model.AddTaxProviderClientId,
                Key = model.AddTaxProviderKey,
                Secret = model.AddTaxProviderSecret,
                StoreId = model.AddTaxProviderStoreId,
                Url = model.AddTaxProviderUrl
            };

            await _sportsNextTaxService.InsertTaxProvider(obj);

            return Json(new { Result = true });
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CategorySkuUpdate(Models.TaxCategoryModel model)
        {
            var obj = await _sportsNextTaxService.GetCategorySkuById(model.Id);

            obj.SKU = model.Sku;

            await _sportsNextTaxService.UpdateCategorySku(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> CategorySkuDelete(string id)
        {
            var obj = await _sportsNextTaxService.GetCategorySkuById(id);
            if (obj != null)
                await _sportsNextTaxService.DeleteCategorySku(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddCategorySku(Models.SportsNextTaxListModel model)
        {
            var obj = new Domain.SportsNextTaxCategorySku {
                StoreId = model.AddTaxCategoryStoreId,
                SKU = model.AddTaxCategorySku,
                TaxCategoryId = model.AddTaxCategoryId
            };
            await _sportsNextTaxService.InsertCategorySku(obj);

            return Json(new { Result = true });
        }
    }
}

