using ZiggyCreatures.Caching.Fusion;

namespace UmbracoHeadlessBFF.SharedModules.Common.Caching;

public static class CachingExtensions
{
    extension(FusionCacheEntryOptions options)
    {
        public void SetAllDurations(TimeSpan duration) => options
            .SetMemoryCacheDuration(duration)
            .SetDistributedCacheDuration(duration);
    }
}
