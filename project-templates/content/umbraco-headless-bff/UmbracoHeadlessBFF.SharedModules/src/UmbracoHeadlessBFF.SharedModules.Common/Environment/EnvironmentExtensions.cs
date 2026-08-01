using Microsoft.Extensions.Hosting;

namespace UmbracoHeadlessBFF.SharedModules.Common.Environment;

public static class EnvironmentExtensions
{
    extension(IHostEnvironment environment)
    {
        public bool IsLocal() => environment.IsEnvironment("local");

        public bool IsDev() => environment.IsEnvironment("dev") || environment.IsDevelopment();

        public bool IsQa() => environment.IsEnvironment("qa");

        public bool IsUat() => environment.IsEnvironment("uat");

        public bool IsStage() => environment.IsEnvironment("stg") || environment.IsStaging();

        public bool IsProd() => environment.IsEnvironment("prod") || environment.IsProduction();
    }
}
