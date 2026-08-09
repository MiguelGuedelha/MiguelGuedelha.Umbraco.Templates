namespace UmbracoHeadlessBFF.SharedModules.Common.Correlation;

public static class CorrelationConstants
{
    public static class Headers
    {
        public const string PreviewMode = "x-site-preview-mode";
        public const string PreviewToken = "x-site-preview-token";
        public const string SiteHost = "x-site-host";
        public const string SitePath = "x-site-path";

        public const string CorrelationId = "x-correlation-id";
    }
}
