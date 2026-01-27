using System.Globalization;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Web;
using Umbraco.Extensions;
using UmbracoHeadlessBFF.Cms.Modules.Common.Authentication;
using UmbracoHeadlessBFF.Cms.Modules.Common.Caching;
using UmbracoHeadlessBFF.Cms.Modules.Common.Links;
using UmbracoHeadlessBFF.Cms.Modules.Common.Umbraco.Models;
using UmbracoHeadlessBFF.SharedModules.Cms.Sitemap;
using NotFound = Microsoft.AspNetCore.Http.HttpResults.NotFound;

namespace UmbracoHeadlessBFF.Cms.Modules.Pages.Sitemap;

[ApiKey]
[ApiController]
[Route($"api/v{{version:apiVersion}}/{PagesConstants.Endpoints.Group}")]
[Tags(PagesConstants.Endpoints.Tag)]
[ApiVersion(1)]
[OutputCache(PolicyName = DefaultOutputCachePolicy.PolicyName)]
public sealed class GetSitemapController : ControllerBase
{
    private readonly IUmbracoContextFactory _umbracoContextFactory;
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly LinkService _linkService;

    public GetSitemapController(IUmbracoContextFactory umbracoContextFactory,
        IVariationContextAccessor variationContextAccessor, LinkService linkService)
    {
        _umbracoContextFactory = umbracoContextFactory;
        _variationContextAccessor = variationContextAccessor;
        _linkService = linkService;
    }

    [HttpGet("sitemap")]
    public Results<Ok<SitemapData>, NotFound> GetSitemap(Guid siteId, string culture, bool preview)
    {
        _variationContextAccessor.VariationContext = new(culture);
        var context = _umbracoContextFactory.EnsureUmbracoContext().UmbracoContext;

        var contentCache = context.Content;

        if (contentCache is null)
        {
            return TypedResults.NotFound();
        }

        var siteNode = contentCache.GetById(preview, siteId);

        if (siteNode is null)
        {
            return TypedResults.NotFound();
        }

        var sitemapPages = siteNode
            .DescendantsOrSelf()
            .OfType<ISeoSettings>()
            .Where(x => x.SitemapShow)
            .Select(x => new SitemapItem
            {
                Loc = _linkService.GetUriByContentId(x.Key, culture, preview)?.AbsoluteUri ?? string.Empty,
                LastMod = x.SitemapLastModifiedOverwrite ?? DateOnly.FromDateTime(x.UpdateDate),
                ChangeFrequency = x.SitemapChangeFrequency,
                Priority = x.SitemapPriority,
                AlternateLanguages = x.Cultures
                    .Where(nodeCulture => !culture.InvariantEquals(nodeCulture.Value.Culture))
                    .Select(nodeCulture =>
                    {
                        var cultureInfo = new CultureInfo(nodeCulture.Value.Culture);

                        return new SitemapItemAlternateLanguage
                        {
                            HrefLang = nodeCulture.Value.Culture,
                            Href = _linkService.GetUriByContentId(x.Key, cultureInfo.Name, preview)
                                ?.AbsoluteUri ?? string.Empty
                        };
                    })
                    .ToArray()
            });


        return TypedResults.Ok(new SitemapData
        {
            Items = sitemapPages.ToArray()
        });
    }
}
