using Microsoft.AspNetCore.Http;
using UmbracoHeadlessBFF.SharedModules.Common.Correlation;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Cms.SiteResolution;

public sealed class SiteResolutionMiddleware : IMiddleware
{
    private readonly SiteResolutionService _siteResolutionService;
    private readonly SiteResolutionContext _siteResolutionContext;

    public SiteResolutionMiddleware(SiteResolutionService siteResolutionService, SiteResolutionContext siteResolutionContext)
    {
        _siteResolutionService = siteResolutionService;
        _siteResolutionContext = siteResolutionContext;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        _ = context.Request.Headers.TryGetValue(CorrelationConstants.Headers.PreviewMode, out var preview);
        _ = bool.TryParse(preview, out var isPreview);

        context.Request.Headers.TryGetValue(CorrelationConstants.Headers.PreviewToken, out var previewToken);

        _siteResolutionContext.IsPreview = isPreview;
        _siteResolutionContext.PreviewToken = previewToken.ToString();

        var site = await _siteResolutionService.ResolveSite();

        if (site is null)
        {
            await next(context);
            return;
        }

        _siteResolutionContext.SiteId = site.Value.SiteId;
        _siteResolutionContext.Site = site.Value.SiteDefinition;

        var hasSiteHost = context.Request.Headers.TryGetValue(CorrelationConstants.Headers.SiteHost, out var siteHost);
        if (hasSiteHost)
        {
            _siteResolutionContext.Domain = siteHost.ToString();
        }

        var hasSitePath = context.Request.Headers.TryGetValue(CorrelationConstants.Headers.SitePath, out var sitePath);
        if (hasSitePath)
        {
            _siteResolutionContext.Path = sitePath.ToString();
        }

        await next(context);
    }
}
