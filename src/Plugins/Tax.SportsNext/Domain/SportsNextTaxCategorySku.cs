using Grand.Domain;
using System;
namespace Tax.SportsNext.Domain
{
	public class SportsNextTaxCategorySku : BaseEntity
    {
            /// <summary>
            /// Gets or sets the store identifier
            /// </summary>
            public string StoreId { get; set; }

            /// <summary>
            /// Gets or sets the tax category identifier
            /// </summary>
            public string TaxCategoryId { get; set; }

            /// <summary>
            /// Gets or sets the SKU
            /// </summary>
            public string SKU { get; set; }
    }
}