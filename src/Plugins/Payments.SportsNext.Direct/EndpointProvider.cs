using Grand.Infrastructure.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using System;
namespace Payments.SportsNext.Direct
{
    public class EndpointProvider : IEndpointProvider
    {
        public void RegisterEndpoint(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute("Plugins.Payments.SportsNext.Direct",
                 "Plugins/PaymentsSportsNextDirect/PaymentInfo",
                 new { controller = "PaymentsSportsNextDirect", action = "PaymentInfo", area = "" }
            );
        }
        public int Priority => 0;

    }
}

