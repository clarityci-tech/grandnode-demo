using Grand.Domain.Configuration;
using System;
namespace Payments.SportsNext.Direct
{
	public class SportsNextPaymentsDirectSettings : ISettings
    {
        public bool Use3DS { get; set; } = false;
        public int DisplayOrder { get; set; }
    }
}

