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
using Payments.SportsNext.Direct.Services;
using System;
namespace Payments.SportsNext.Direct.Areas.Admin.Controllers
{
    [AuthorizeAdmin]
    [Area("Admin")]
    [PermissionAuthorize(PermissionSystemName.PaymentMethods)]
    public class PaymentsSportsNextDirectController : BasePluginController
    {
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly ISettingService _settingService;
        private readonly ISportsNextPaymentService _sportsNextPaymentService;
        private readonly IStoreService _storeService;

        public PaymentsSportsNextDirectController(
            ITaxCategoryService taxCategoryService,
            ISettingService settingService,
            ISportsNextPaymentService sportsNextPaymentService,
            IStoreService storeService)
        {
            _taxCategoryService = taxCategoryService;
            _settingService = settingService;
            _sportsNextPaymentService = sportsNextPaymentService;
            _storeService = storeService;
        }

        public async Task<IActionResult> Configure()
        {
            var model = new Models.SportsNextPaymentListModel();

            //stores
            model.AvailableStores.Add(new SelectListItem { Text = "*", Value = "" });
            var stores = await _storeService.GetAllStores();

            foreach (var s in stores)
            {
                model.AvailableStores.Add(new SelectListItem { Text = s.Shortcut, Value = s.Id });
            }

            return View(model);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProviderList(DataSourceRequest command)
        {
            var records = await _sportsNextPaymentService.GetAllPaymentProviders(command.Page - 1, command.PageSize);
            var models = new List<Models.PaymentProviderModel>();

            foreach (var x in records)
            {
                var m = new Models.PaymentProviderModel {
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
        public async Task<IActionResult> ProviderUpdate(Models.PaymentProviderModel model)
        {
            var obj = await _sportsNextPaymentService.GetPaymentProviderById(model.Id);
            obj.ClientId = model.ClientId;
            obj.Secret = model.Secret;
            obj.Url = model.Url;
            obj.Key = model.Key;
            await _sportsNextPaymentService.UpdatePaymentProvider(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> ProviderDelete(string id)
        {
            var obj = await _sportsNextPaymentService.GetPaymentProviderById(id);
            if (obj != null)
                await _sportsNextPaymentService.DeletePaymentProvider(obj);

            return new JsonResult("");
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> AddProvider(Models.SportsNextPaymentListModel model)
        {
            var obj = new Domain.SportsNextPaymentProvider {
                ClientId = model.AddPaymentProviderClientId,
                Key = model.AddPaymentProviderKey,
                Secret = model.AddPaymentProviderSecret,
                StoreId = model.AddPaymentProviderStoreId,
                Url = model.AddPaymentProviderUrl
            };

            await _sportsNextPaymentService.InsertPaymentProvider(obj);

            return Json(new { Result = true });
        }
    }
}

