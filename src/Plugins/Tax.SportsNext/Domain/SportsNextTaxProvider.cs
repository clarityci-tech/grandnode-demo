using Grand.Domain;
using System;
namespace Tax.SportsNext.Domain
{
	public class SportsNextTaxProvider : BaseEntity
    {
        /// <summary>
        /// Gets or sets the store identifier
        /// </summary>
        public string StoreId { get; set; }

        /// <summary>
        /// Gets or sets the tax provider key
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

