using Grand.Domain;
using System;
namespace Tax.SportsNext.Domain
{
	public class SportsNextOrderTaxInvoiceMap : BaseEntity
    {
		public Guid OrderGuid { get; set; }

		public string OrderId { get; set; }

		public string TaxInvoiceKey { get; set; }

		public List<string> RefundInvoiceKeys { get; set; } = new List<string>();
	}
}

