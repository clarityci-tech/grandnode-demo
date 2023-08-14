using Grand.Domain;
using System;
namespace Payments.SportsNext.Direct.Domain
{
	public class SportsNextPaymentProvider : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the payment provider key
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the Url
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Gets or sets the client ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Gets or sets the secret
        /// </summary>
        public string Secret { get; set; }
    }
}

