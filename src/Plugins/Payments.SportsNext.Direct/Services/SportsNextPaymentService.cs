using Grand.Domain;
using Grand.Domain.Data;
using Grand.Infrastructure.Caching;
using Grand.Infrastructure.Extensions;
using MediatR;
using Payments.SportsNext.Direct.Domain;
using System;

namespace Payments.SportsNext.Direct.Services
{
	public class SportsNextPaymentService : ISportsNextPaymentService
    {
        #region Constants
        private const string PAYMENTPROVIDER_ALL_KEY = "Grand.Payment.SportsNext.Direct.Provider.all-{0}-{1}";
        private const string PAYMENTPROVIDER_PATTERN_KEY = "Grand.Payment.SportsNext.Direct.Provider.";
        #endregion

        #region Fields

        private readonly IMediator _mediator;
        private readonly IRepository<Domain.SportsNextPaymentProvider> _paymentProviderRepository;
        private readonly ICacheBase _cacheBase;

        #endregion

        public SportsNextPaymentService(IMediator mediator,
            ICacheBase cacheManager,
            IRepository<Domain.SportsNextPaymentProvider> paymentProviderRepository)
        {
            _mediator = mediator;
            _cacheBase = cacheManager;
            _paymentProviderRepository = paymentProviderRepository;
        }

        public async Task DeletePaymentProvider(SportsNextPaymentProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _paymentProviderRepository.DeleteAsync(provider);

            await _cacheBase.RemoveByPrefix(PAYMENTPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityDeleted(provider);
        }

        public async Task<IPagedList<SportsNextPaymentProvider>> GetAllPaymentProviders(int pageIndex = 0, int pageSize = int.MaxValue)
        {
            var key = string.Format(PAYMENTPROVIDER_ALL_KEY, pageIndex, pageSize);
            return await _cacheBase.GetAsync(key, async () =>
            {
                var query = from tr in _paymentProviderRepository.Table
                            orderby tr.ClientId, tr.Id, tr.Key, tr.Secret, tr.StoreId, tr.Url
                            select tr;
                return await Task.FromResult(new PagedList<Domain.SportsNextPaymentProvider>(query, pageIndex, pageSize));
            });
        }

        public Task<SportsNextPaymentProvider> GetPaymentProviderById(string providerId)
        {
            return _paymentProviderRepository.GetByIdAsync(providerId);
        }

        public async Task InsertPaymentProvider(SportsNextPaymentProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _paymentProviderRepository.InsertAsync(provider);

            await _cacheBase.RemoveByPrefix(PAYMENTPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityInserted(provider);
        }

        public async Task UpdatePaymentProvider(SportsNextPaymentProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            await _paymentProviderRepository.UpdateAsync(provider);

            await _cacheBase.RemoveByPrefix(PAYMENTPROVIDER_PATTERN_KEY);

            //event notification
            await _mediator.EntityUpdated(provider);
        }
    }
}

