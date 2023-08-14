using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Domain.Orders;
using Grand.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Payments.SportsNext.Direct.Models;
using System;
using System.Net;
using Grand.Web.Common.Controllers;
using Payments.SportsNext.Direct.Validators;

namespace Payments.SportsNext.Direct.Controllers
{
	public class PaymentsSportsNextDirectController : BasePaymentController
    {
        private readonly SportsNextPaymentsDirectSettings _paymentSettings;
        private readonly IOrderCalculationService _orderTotalCalculationService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IWorkContext _workContext;
        private readonly ITranslationService _translationService;

        public PaymentsSportsNextDirectController(SportsNextPaymentsDirectSettings paymentSettings,
            IOrderCalculationService orderTotalCalculationService,
            IShoppingCartService shoppingCartService,
            IWorkContext workContext,
            ITranslationService translationService)
        {
            _paymentSettings = paymentSettings;
            _orderTotalCalculationService = orderTotalCalculationService;
            _shoppingCartService = shoppingCartService;
            _workContext = workContext;
            _translationService = translationService;
        }

        public async Task<IActionResult> PaymentInfo()
        {
            var model = new PaymentInfoModel();

            //years
            for (var i = 0; i < 15; i++)
            {
                var year = Convert.ToString(DateTime.Now.Year + i);
                model.ExpireYears.Add(new SelectListItem {
                    Text = year,
                    Value = year
                });
            }

            //months
            for (var i = 1; i <= 12; i++)
            {
                var text = i < 10 ? "0" + i : i.ToString();
                model.ExpireMonths.Add(new SelectListItem {
                    Text = text,
                    Value = i.ToString()
                });
            }

            //set postback values (we cannot access "Form" with "GET" requests)
            if (Request.Method == WebRequestMethods.Http.Get)
                return View("PaymentInfo", model);

            var form = await HttpContext.Request.ReadFormAsync();

            model.CardholderName = form["CardholderName"];
            model.CardNumber = form["CardNumber"];
            model.CardCode = form["CardCode"];

            var selectedMonth = model.ExpireMonths
                .FirstOrDefault(x => x.Value.Equals(form["ExpireMonth"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedMonth != null)
                selectedMonth.Selected = true;
            var selectedYear = model.ExpireYears
                .FirstOrDefault(x => x.Value.Equals(form["ExpireYear"], StringComparison.InvariantCultureIgnoreCase));
            if (selectedYear != null)
                selectedYear.Selected = true;

            var validator = new PaymentInfoValidator(_paymentSettings, _translationService);
            var results = await validator.ValidateAsync(model);
            if (results.IsValid) return View("PaymentInfo", model);
            var query = from error in results.Errors
                        select error.ErrorMessage;
            model.Errors = string.Join(", ", query);
            return View("PaymentInfo", model);
        }
    }
}