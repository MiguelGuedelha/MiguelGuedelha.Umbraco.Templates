namespace UmbracoHeadlessBFF.SharedModules.Common.Caching;

public record DefaultCachingOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; init; }

    public DefaultRegion? Default { get; init; }

    public sealed record DefaultRegion
    {
        public TimeSpan Duration { get; init; }
        public TimeSpan DurationDistributed { get; init; }
        public TimeSpan NullDuration { get; init; }
        public TimeSpan DistributedCacheCircuitBreakerDuration { get; init; }
        public bool FailSafeIsEnabled { get; init; }
        public TimeSpan FailSafeMaxDuration { get; init; }
        public TimeSpan DistributedCacheFailSafeMaxDuration { get; set; }
        public bool DistributedLocking { get; set; }
        public TimeSpan FailSafeThrottleDuration { get; init; }
        public TimeSpan FactorySoftTimeout { get; init; }
        public TimeSpan FactoryHardTimeout { get; init; }
        public TimeSpan DistributedCacheSoftTimeout { get; init; }
        public TimeSpan DistributedCacheHardTimeout { get; init; }
        public bool AllowBackgroundDistributedCacheOperations { get; init; }
        public bool AllowBackgroundBackplaneOperations { get; init; }
        public TimeSpan JitterMaxDuration { get; init; }
        public float EagerRefreshThreshold { get; init; }
    }
}
