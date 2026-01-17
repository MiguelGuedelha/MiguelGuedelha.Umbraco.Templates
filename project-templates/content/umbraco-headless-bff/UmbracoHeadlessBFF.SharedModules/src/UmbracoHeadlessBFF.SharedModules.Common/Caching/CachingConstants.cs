namespace UmbracoHeadlessBFF.SharedModules.Common.Caching;

public static class CachingConstants
{
    public const string ConnectionStringName = "Cache";

    public static class SiteApi
    {
        public const string CacheName = "SiteApi";
        public const string OutputCacheName = $"{CacheName}:OutputCache";

        public static class Tags
        {
            public const string Links = "links";
            public const string Sites = "sites";
            public const string Robots = "robots";
            public const string Sitemaps = "sitemaps";
            public const string Pages = "pages";
            public const string Redirects = "redirects";
            public const string Media = "media";

            public static readonly string[] GlobalTags =
            [
                Sites,
                Sitemaps,
                Robots,
                Redirects
            ];
        }
    }

    public static class Cms
    {
        public const string OutputCacheName = "Cms:OutputCache";
    }
}
