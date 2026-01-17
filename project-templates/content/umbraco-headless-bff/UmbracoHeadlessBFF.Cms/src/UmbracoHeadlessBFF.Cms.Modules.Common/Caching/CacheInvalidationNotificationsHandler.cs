using Umbraco.Cms.Core.Cache;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Services.Changes;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using ZiggyCreatures.Caching.Fusion;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Caching;

internal sealed class CacheInvalidationNotificationsHandler :
    INotificationAsyncHandler<ContentCacheRefresherNotification>,
    INotificationAsyncHandler<DomainCacheRefresherNotification>,
    INotificationAsyncHandler<MediaCacheRefresherNotification>
{
    private readonly IContentService _contentService;
    private readonly IMediaService _mediaService;
    private readonly IFusionCache _siteApiFusionCache;

    public CacheInvalidationNotificationsHandler(
        IContentService contentService,
        IMediaService mediaService,
        IFusionCacheProvider fusionCacheProvider)
    {
        _contentService = contentService;
        _mediaService = mediaService;
        _siteApiFusionCache = fusionCacheProvider.GetCache(CachingConstants.SiteApi.CacheName);
    }

    public async Task HandleAsync(ContentCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageObject is not ContentCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        var items = new List<string>();

        foreach (var payload in payloads.Where(x => x.ChangeTypes is not TreeChangeTypes.Remove and not TreeChangeTypes.None))
        {
            if (payload.ChangeTypes is TreeChangeTypes.RefreshAll)
            {
                items = null;
                break;
            }

            var content = _contentService.GetById(payload.Id);

            if (content is null)
            {
                break;
            }

            items.Add(content.Key.ToString());

            var changeType = payload.ChangeTypes;

            if (changeType != TreeChangeTypes.RefreshBranch)
            {
                continue;
            }

            var descendants = _contentService
                .GetPagedDescendants(content.Id, 0, int.MaxValue, out _)
                .Select(x => x.Key.ToString());

            items.AddRange(descendants);
        }

        if (items is null)
        {
            await _siteApiFusionCache.ClearAsync(token: cancellationToken);
        }
        else
        {
            await _siteApiFusionCache.RemoveByTagAsync(items, token: cancellationToken);
            await _siteApiFusionCache.RemoveByTagAsync(CachingConstants.SiteApi.Tags.GlobalTags, token: cancellationToken);
        }
    }

    public async Task HandleAsync(DomainCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageObject is not DomainCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        if (!payloads.Any(x => x.ChangeType is not DomainChangeTypes.None))
        {
            return;
        }

        await _siteApiFusionCache.ClearAsync(token: cancellationToken);
    }

    public async Task HandleAsync(MediaCacheRefresherNotification notification, CancellationToken cancellationToken)
    {
        if (notification.MessageObject is not MediaCacheRefresher.JsonPayload[] payloads)
        {
            return;
        }

        var items = new List<string>();

        foreach (var payload in payloads.Where(x => x.ChangeTypes is not TreeChangeTypes.Remove and not TreeChangeTypes.None))
        {
            if (payload.ChangeTypes is TreeChangeTypes.RefreshAll)
            {
                items = null;
                break;
            }

            var media = _mediaService.GetById(payload.Id);

            if (media is null)
            {
                break;
            }

            items.Add(media.Key.ToString());

            var changeType = payload.ChangeTypes;

            if (changeType != TreeChangeTypes.RefreshBranch)
            {
                continue;
            }

            var descendants = _mediaService
                .GetPagedDescendants(media.Id, 0, int.MaxValue, out _)
                .Select(x => x.Key.ToString());

            items.AddRange(descendants);
        }

        if (items is null)
        {
            await _siteApiFusionCache.RemoveByTagAsync(CachingConstants.SiteApi.Tags.Media, token: cancellationToken);
        }
        else
        {
            await _siteApiFusionCache.RemoveByTagAsync(items, token: cancellationToken);
        }

        await _siteApiFusionCache.RemoveByTagAsync(CachingConstants.SiteApi.Tags.GlobalTags, token: cancellationToken);
    }
}
