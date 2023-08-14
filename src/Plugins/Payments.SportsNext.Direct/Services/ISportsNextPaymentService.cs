using Grand.Domain;
using System;
namespace Payments.SportsNext.Direct.Services
{
	public interface ISportsNextPaymentService
	{
		Task DeletePaymentProvider(Domain.SportsNextPaymentProvider provider);

		Task<IPagedList<Domain.SportsNextPaymentProvider>> GetAllPaymentProviders(int pageIndex = 0, int pageSize = int.MaxValue);

		Task<Domain.SportsNextPaymentProvider> GetPaymentProviderById(string providerId);

		Task InsertPaymentProvider(Domain.SportsNextPaymentProvider provider);

		Task UpdatePaymentProvider(Domain.SportsNextPaymentProvider provider);
	}
}

