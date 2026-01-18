using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using UmbracoHeadlessBFF.SharedModules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;
using ZiggyCreatures.Caching.Fusion;
using ZiggyCreatures.Caching.Fusion.Serialization.NeueccMessagePack;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Caching;

public static class CachingConfiguration
{
    extension(WebApplicationBuilder builder)
    {
        public void AddCachingCommonModule(bool versioned = false)
        {
            var cacheBuilder = builder.Services.AddFusionCache(CachingConstants.Cms.OutputCacheName)
                .WithDefaultEntryOptions(o =>
                {
                    o.IsFailSafeEnabled = false;
                    o.AllowBackgroundBackplaneOperations = false;
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
                cacheBuilder.WithCacheKeyPrefix($"{CachingConstants.Cms.OutputCacheName}:{AssemblyVersionExtensions.GetVersion()}:");
            }
            else
            {
                cacheBuilder.WithCacheKeyPrefixByCacheName();
            }

            builder.Services.AddFusionOutputCache(o =>
            {
                o.CacheName = CachingConstants.Cms.OutputCacheName;
            });
        }
    }
}
