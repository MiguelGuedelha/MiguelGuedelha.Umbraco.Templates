using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;
using UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching.Policies;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Caching;

public static class CachingConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddCachingCommonModule(bool versioned = false)
        {
            builder.Services.Configure<SiteApiCachingOptions>(builder.Configuration.GetSection(DefaultCachingOptions.SectionName));

            var cacheBuilder = builder.Services.AddFusionCache(CachingConstants.SiteApi.OutputCacheName)
                .WithDefaultEntryOptions(o =>
                {
                    o.IsFailSafeEnabled = true;
                    o.Duration = TimeSpan.FromSeconds(30);
                    o.DistributedCacheDuration = TimeSpan.FromSeconds(60);
                    o.JitterMaxDuration = TimeSpan.FromSeconds(5);
                })
                .WithSerializer(new FusionCacheNeueccMessagePackSerializer())
                .WithDistributedCache(new RedisCache(new RedisCacheOptions
                {
                    Configuration = builder.Configuration.GetConnectionString(CachingConstants.ConnectionStringName)
                }))
                .WithStackExchangeRedisBackplane(o =>
                {
                    o.Configuration = builder.Configuration.GetConnectionString(CachingConstants.ConnectionStringName);
                });

            if (versioned)
            {
                cacheBuilder.WithCacheKeyPrefix($"{CachingConstants.SiteApi.OutputCacheName}:{AssemblyVersionExtensions.GetVersion()}:");
            }
            else
            {
                cacheBuilder.WithCacheKeyPrefixByCacheName();
            }

            builder.Services.AddFusionOutputCache(o =>
            {
                o.CacheName = CachingConstants.SiteApi.OutputCacheName;
            });

            builder.Services.AddOutputCache(o =>
            {
                o.AddPolicy(SiteBasedOutputCachePolicy.PolicyName, SiteBasedOutputCachePolicy.Instance);
                o.AddPolicy(SiteAndPathBasedOutputCachePolicy.PolicyName, SiteAndPathBasedOutputCachePolicy.Instance);
                o.AddPolicy(SiteAndIdBasedOutputCachePolicy.PolicyName, SiteAndIdBasedOutputCachePolicy.Instance);
            });
        }

    }
}
