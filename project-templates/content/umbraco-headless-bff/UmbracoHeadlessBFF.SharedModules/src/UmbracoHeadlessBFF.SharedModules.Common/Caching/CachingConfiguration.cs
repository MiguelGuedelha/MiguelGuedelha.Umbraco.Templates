using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using StackExchange.Redis;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Locking.Distributed.Redis;
using ZiggyCreatures.Caching.Fusion.Serialization.SystemTextJson;

namespace UmbracoHeadlessBFF.SharedModules.Common.Caching;

public static class CachingConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddCachingSharedModule(
            string cachePrefix,
            bool namedCache = false,
            bool keyedService = false,
            bool versioned = false,
            Action<FusionCacheOptions>? configureOptions = null,
            Action<FusionCacheEntryOptions>? configureEntryOptions = null,
            Action<JsonSerializerOptions>? configureJsonSerializerOptions = null)
        {
            var section = builder.Configuration
                .GetSection(DefaultCachingOptions.SectionName);

            builder.Services.Configure<DefaultCachingOptions>(section);

            var cacheName = namedCache ? cachePrefix.Trim(':') : FusionCacheOptions.DefaultCacheName;

            var defaultCachingOptions = section.Get<DefaultCachingOptions>() ?? throw new ArgumentException("Caching options can't be parsed");

            var cacheBuilder = builder.Services.AddFusionCache(cacheName);

            if (versioned)
            {
                cacheBuilder.WithCacheKeyPrefix($"{cacheName}:{AssemblyVersionExtensions.GetVersion()}:");
            }
            else
            {
                cacheBuilder.WithCacheKeyPrefixByCacheName();
            }

            if (keyedService)
            {
                cacheBuilder.AsKeyedServiceByCacheName();
            }

            if (!defaultCachingOptions.Enabled)
            {
                var nullBuilder = cacheBuilder.WithNullImplementation();

                nullBuilder.ThrowIfMissingMemoryCache = false;
                return;
            }

            builder.Services.AddMemoryCache();

            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
            configureJsonSerializerOptions?.Invoke(jsonOptions);

            cacheBuilder
                .WithOptions(o =>
                {
                    if (defaultCachingOptions.Default is not null)
                    {
                        //Backs off the Distributed Cache if having issues
                        o.DistributedCacheCircuitBreakerDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.DistributedCacheCircuitBreakerDuration);
                    }

                    configureOptions?.Invoke(o);

                    //Log Levels
                    o.FailSafeActivationLogLevel = LogLevel.Debug;
                    o.DistributedCacheSyntheticTimeoutsLogLevel = LogLevel.Information;
                    o.DistributedCacheErrorsLogLevel = LogLevel.Error;
                    o.FactorySyntheticTimeoutsLogLevel = LogLevel.Information;
                    o.FactoryErrorsLogLevel = LogLevel.Error;
                })
                .WithDefaultEntryOptions(o =>
                {
                    if (defaultCachingOptions.Default is not null)
                    {
                        o.Duration = TimeSpan.FromSeconds(defaultCachingOptions.Default.Duration);
                        o.DistributedCacheDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.DurationDistributed);

                        o.IsFailSafeEnabled = defaultCachingOptions.Default.FailSafeIsEnabled;
                        o.FailSafeMaxDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.FailSafeMaxDuration);
                        o.DistributedCacheFailSafeMaxDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.DistributedCacheFailSafeMaxDuration);
                        o.FailSafeThrottleDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.FailSafeThrottleDuration);

                        o.FactorySoftTimeout = TimeSpan.FromMilliseconds(defaultCachingOptions.Default.FactorySoftTimeoutMs);
                        o.FactoryHardTimeout = TimeSpan.FromSeconds(defaultCachingOptions.Default.FactoryHardTimeout);

                        o.DistributedCacheSoftTimeout = TimeSpan.FromMilliseconds(defaultCachingOptions.Default.DistributedCacheSoftTimeoutMs);
                        o.DistributedCacheHardTimeout = TimeSpan.FromSeconds(defaultCachingOptions.Default.DistributedCacheHardTimeout);
                        o.AllowBackgroundDistributedCacheOperations = defaultCachingOptions.Default.AllowBackgroundDistributedCacheOperations;
                        o.AllowBackgroundBackplaneOperations = defaultCachingOptions.Default.AllowBackgroundBackplaneOperations;

                        o.JitterMaxDuration = TimeSpan.FromSeconds(defaultCachingOptions.Default.JitterMaxDuration);
                        o.EagerRefreshThreshold = defaultCachingOptions.Default.EagerRefreshThreshold;
                        o.SkipDistributedLocker = !defaultCachingOptions.Default.DistributedLocking;
                    }

                    configureEntryOptions?.Invoke(o);
                })
                .WithSerializer(new FusionCacheSystemTextJsonSerializer(jsonOptions));

            var redisConnectionString = builder.Configuration.GetConnectionString(CachingConstants.ConnectionStringName);

            if (!string.IsNullOrWhiteSpace(redisConnectionString))
            {
                var multiplexer = ConnectionMultiplexer.ConnectAsync(redisConnectionString);

                cacheBuilder
                    .WithDistributedCache(new RedisCache(new RedisCacheOptions
                    {
                        ConnectionMultiplexerFactory = async () => await multiplexer
                    }))
                    .WithStackExchangeRedisBackplane(o =>
                    {
                        o.ConnectionMultiplexerFactory = async () => await multiplexer;
                    })
                    .WithDistributedLocker(new RedisDistributedLocker(new RedisDistributedLockerOptions
                    {
                        ConnectionMultiplexerFactory = async () => await multiplexer
                    }));
            }

            builder.Services.AddOpenTelemetry()
                .WithTracing(tracing => tracing.AddFusionCacheInstrumentation())
                .WithMetrics(metrics => metrics.AddFusionCacheInstrumentation());
        }
    }
}
