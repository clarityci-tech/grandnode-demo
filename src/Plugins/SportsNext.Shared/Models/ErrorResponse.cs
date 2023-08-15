using System;
namespace SportsNext.Shared.Models
{
	public class ErrorResponse
	{
		public string Message { get; set; }

		public string ErrorLogId { get; set; }

		public string ErrorType { get; set; }

		public string ErrorDetail { get; set; }

		public int? TransactionStatusCode { get; set; }
	}
}

