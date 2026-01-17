using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Sync;
using Umbraco.Cms.Infrastructure.DependencyInjection;
using Umbraco.Extensions;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.LoadBalancing;

public static class LoadBalancingConfiguration
{
    extension(IUmbracoBuilder builder)
    {
        public void AddLoadBalancing()
        {
            builder
                .SetServerRegistrar(new StaticServerAccessor())
                .LoadBalanceIsolatedCaches();

            var connectionString = builder.Config.GetUmbracoConnectionString();
            if (connectionString is null)
            {
                return;
            }

            builder.Services.AddSignalR().AddSqlServer(connectionString);
        }
    }

    private class StaticServerAccessor : IServerRoleAccessor
    {
        public ServerRole CurrentServerRole => ServerRole.SchedulingPublisher;
    }
}
