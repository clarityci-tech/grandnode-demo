using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Business.Core.Interfaces.Catalog.Tax;
using Grand.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Tax.SportsNext
{
	public class StartupApplication : IStartupApplication
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ITaxProvider, SportsNextTaxProvider>();
            services.AddScoped<Services.ISportsNextTaxService, Services.SportsNextTaxService>();
            //services.AddScoped<INotificationHandler<OrderPaidEvent>, Handlers.OrderPaidEventHandler>();
            //services.AddScoped<INotificationHandler<PaymentTransactionRefundedEvent>, Handlers.PaymentTransactionRefundHandler>();
        }

        public int Priority => 10;
        public void Configure(IApplicationBuilder application, IWebHostEnvironment webHostEnvironment)
        {

        }
        public bool BeforeConfigure => false;
    }
}

