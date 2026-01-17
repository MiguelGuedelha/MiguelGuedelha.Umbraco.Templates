using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Caching;

internal sealed class CachingComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.AddNotificationAsyncHandler<ContentCacheRefresherNotification, CacheInvalidationNotificationsHandler>();
        builder.AddNotificationAsyncHandler<DomainCacheRefresherNotification, CacheInvalidationNotificationsHandler>();
    }
}
