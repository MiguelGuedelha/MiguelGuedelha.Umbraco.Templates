namespace UmbracoHeadlessBFF.SharedModules.Common.Caching;

public record DefaultCachingOptions
{
    public const string SectionName = "Caching";

    public bool Enabled { get; init; }

    public DefaultRegion? Default { get; init; }

    public sealed record DefaultRegion
    {
        public int Duration { get; init; }
        public int DurationDistributed { get; init; }
        public int NullDuration { get; init; }
        public int DistributedCacheCircuitBreakerDuration { get; init; }
        public bool FailSafeIsEnabled { get; init; }
        public int FailSafeMaxDuration { get; init; }
        public int DistributedCacheFailSafeMaxDuration { get; set; }
        public bool DistributedLocking { get; set; }
        public int FailSafeThrottleDuration { get; init; }
        public int FactorySoftTimeoutMs { get; init; }
        public int FactoryHardTimeout { get; init; }
        public int DistributedCacheSoftTimeoutMs { get; init; }
        public int DistributedCacheHardTimeout { get; init; }
        public bool AllowBackgroundDistributedCacheOperations { get; init; }
        public bool AllowBackgroundBackplaneOperations { get; init; }
        public int JitterMaxDuration { get; init; }
        public float EagerRefreshThreshold { get; init; }
    }
}
