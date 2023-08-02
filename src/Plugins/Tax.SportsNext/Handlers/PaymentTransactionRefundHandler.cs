using Grand.Business.Core.Events.Checkout.Orders;
using Grand.Business.Core.Interfaces.Catalog.Products;
using Grand.Business.Core.Interfaces.Checkout.Orders;
using Grand.Business.Core.Interfaces.Common.Directory;
using Grand.Business.Core.Interfaces.Common.Localization;
using Grand.Infrastructure;
using Grand.Infrastructure.Caching;
using MediatR;
using System;
using Tax.SportsNext.Services;

namespace Tax.SportsNext.Handlers
{
	public class PaymentTransactionRefundHandler :
        HandlerBase,
        INotificationHandler<PaymentTransactionRefundedEvent>
    {
        private readonly IOrderService _orderService;

        public PaymentTransactionRefundHandler(
        ISportsNextTaxService sportsNextService,
        IOrderService orderService) : base(sportsNextService)
        {
            _orderService = orderService;
        }

        public async Task Handle(PaymentTransactionRefundedEvent notification, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByGuid(notification.PaymentTransaction.OrderGuid);

            if(order == null)
            {
                throw new Exception($"Order not found for guid: {notification.PaymentTransaction.OrderGuid}");
            }

            var selected = await this.FindTaxProvider(order.StoreId);
            var mapping = await _sportsNextTaxService.GetOrderTaxInvoiceMappingByOrderId(order.Id);

            if(mapping == null)
            {
                throw new Exception($"Mapping not found for order id: {order.Id}");
            }

            if(selected == null)
            {
                throw new Exception($"Tax provider not found for store id: {order.StoreId}");
            }

            var taxClient = new Payments.Service.Tax.Client.TaxClient(selected.Url, selected.ClientId, selected.Secret);

            var taxInvoice = await taxClient.GetInvoiceAsync(mapping.TaxInvoiceKey);
            
            var totalLeft = (decimal)notification.Amount;

            var items = taxInvoice.LineItems.OrderByDescending(li => li.Remaining.Total).ToList();
            var instructions = new List<Payments.Service.Tax.Models.RefundLineInstruction>();

            foreach (var item in items)
            {
                var itemRemaining = (decimal)item.Remaining.Total;

                if (itemRemaining <= decimal.Zero || totalLeft <= decimal.Zero)
                {
                    continue;
                }

                var applicable = totalLeft > itemRemaining ? itemRemaining : totalLeft;
                var percentage = applicable / itemRemaining;

                var grossRefund = Math.Round(item.Remaining.Gross * percentage, 2);

                if (grossRefund > decimal.Zero)
                {
                    instructions.Add(new Payments.Service.Tax.Models.RefundLineInstruction { Amount = -grossRefund, LineNumber = item.LineNumber });
                    totalLeft -= applicable;
                }
            }

            if (instructions.Any())
            {
                var refundRequest = new Payments.Service.Tax.Models.Contracts.RefundInvoiceRequest {
                    Instructions = new Payments.Service.Tax.Models.RefundInstructions {
                        EffectiveDate = DateTime.UtcNow,
                        LineInstructions = instructions,
                        Reason = "Customer Refund",
                        RefundType = Payments.Service.Tax.Models.Enumerations.TaxRefundType.Partial
                    }
                };

                var refundResult = await taxClient.RefundInvoiceAsync(mapping.TaxInvoiceKey, refundRequest);

                if (refundResult.InvoiceKey.HasValue)
                {
                    mapping.RefundInvoiceKeys.Add(refundResult.InvoiceKey.ToString());
                    await _sportsNextTaxService.UpdateOrderTaxInvoiceMapping(mapping);
                }
            }
        }
    }
}

