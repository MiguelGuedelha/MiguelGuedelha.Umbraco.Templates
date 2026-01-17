using System.Reflection;

namespace UmbracoHeadlessBFF.SharedModules.Common.Versioning;

public static class AssemblyVersionExtensions
{
    public static string GetVersion()
    {
        var assembly = Assembly.GetExecutingAssembly();

        var infoVersion = assembly
            ?.GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        var version = infoVersion
            ?? assembly?.GetName()?.Version?.ToString()
            ?? "0.0.1";

        return version;
    }
}
