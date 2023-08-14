using Microsoft.AspNetCore.Mvc;
using System;
namespace Payments.SportsNext.Direct.Components
{
    [ViewComponent(Name = "PaymentsSportsNextDirectScripts")]
    public class PaymentsSportsNextDirectScripts : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}

