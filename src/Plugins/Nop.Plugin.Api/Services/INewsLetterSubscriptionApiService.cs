using System;
using System.Collections.Generic;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Api.Infrastructure;

namespace Nop.Plugin.Api.Services
{
    public interface INewsLetterSubscriptionApiService
    {
        List<NewsLetterSubscription> GetNewsLetterSubscriptions(
            DateTime? createdAtMin = null, DateTime? createdAtMax = null,
            int limit = Constants.Configurations.DEFAULT_LIMIT, int page = Constants.Configurations.DEFAULT_PAGE_VALUE,
            int sinceId = Constants.Configurations.DEFAULT_SINCE_ID,
            bool? onlyActive = true);
    }
}
