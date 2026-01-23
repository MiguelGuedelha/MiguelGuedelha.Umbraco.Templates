using System.Diagnostics;
using System.Net;
using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Routing;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;
using UmbracoHeadlessBFF.Cms.Modules.Common.Authentication;
using UmbracoHeadlessBFF.Cms.Modules.Common.Umbraco.Models;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SharedModules.Cms.Links;
using NotFound = Microsoft.AspNetCore.Http.HttpResults.NotFound;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Links;

[ApiKey]
[ApiController]
[Route($"api/v{{version:apiVersion}}/{LinksConstants.Endpoints.Group}")]
[Tags(LinksConstants.Endpoints.Tag)]
[ApiVersion(1)]
public sealed class GetRedirectLinkController : ControllerBase
{
    private readonly IVariationContextAccessor _variationContextAccessor;
    private readonly IRedirectUrlService _redirectUrlService;
    private readonly IPublishedUrlProvider _publishedUrlProvider;
    private readonly IPublishedContentCache _publishedContentCache;
    private readonly IDocumentUrlService _documentUrlService;

    private static readonly HashSet<string> s_nonValidDestinationPageTypes = [
        SiteSettings.ModelTypeAlias,
        SiteDictionary.ModelTypeAlias,
        SiteGrouping.ModelTypeAlias,
        Umbraco.Models.NotFound.ModelTypeAlias,
    ];

    public GetRedirectLinkController(
        IVariationContextAccessor variationContextAccessor,
        IRedirectUrlService redirectUrlService,
        IPublishedUrlProvider publishedUrlProvider,
        IPublishedContentCache publishedContentCache,
        IDocumentUrlService documentUrlService)
    {
        _variationContextAccessor = variationContextAccessor;
        _redirectUrlService = redirectUrlService;
        _publishedUrlProvider = publishedUrlProvider;
        _publishedContentCache = publishedContentCache;
        _documentUrlService = documentUrlService;
    }

    [HttpGet("redirects/{**path}")]
    public async Task<Results<Ok<RedirectLink>, NotFound>> GetRedirect(string path, Guid siteId, string culture)
    {
        _variationContextAccessor.VariationContext = new(culture);

        if (path.Contains("%2F", StringComparison.OrdinalIgnoreCase))
        {
            path = WebUtility.UrlDecode(path);
        }

        var siteRoot = _publishedContentCache.GetById(siteId);

        if (siteRoot is null)
        {
            return TypedResults.NotFound();
        }

        var itemRoute = $"{siteRoot.Id}/{path.Trim('/')}";

        var latestRedirect = await _redirectUrlService.GetMostRecentRedirectUrlAsync(itemRoute, culture);

        if (latestRedirect is not null)
        {
            var destination = _publishedContentCache.GetById(latestRedirect.ContentId);
            if (destination is not null)
            {
                var destinationUrl = destination.Url(_publishedUrlProvider, latestRedirect.Culture, UrlMode.Absolute);
                return TypedResults.Ok(new RedirectLink
                {
                    Location = destinationUrl,
                    StatusCode = StatusCodes.Status308PermanentRedirect
                });
            }
        }

        // You can add custom redirects
        // such as Skybrud here, as a fallback after umbraco ones,
        // or replace the above with said custom redirects


        // Handles all the pages which have the Redirect Settings composition and are not meant to be displayed on the site
        var itemKey = _documentUrlService.GetDocumentKeyByRoute(itemRoute, culture, null, false);

        if (itemKey is null)
        {
            return TypedResults.NotFound();
        }

        var item = await _publishedContentCache.GetByIdAsync(itemKey.Value);


        if (item is not IRedirectSettings redirectSettings)
        {
            return TypedResults.Ok(new RedirectLink
            {
                Location = null,
                StatusCode = null
            });
        }

        var url = redirectSettings.RedirectLinkOverride?.Url switch
        {
            var x when !string.IsNullOrWhiteSpace(x) => x,
            _ => GenerateFallbackUrl(item, redirectSettings.RedirectDirection, culture)
        };

        if (url is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(new RedirectLink
        {
            Location = url,
            StatusCode = StatusCodes.Status307TemporaryRedirect
        });
    }

    private string? GenerateFallbackUrl(IPublishedContent item, RedirectFallbackDirection redirectDirection, string culture)
    {
        var possibleItems = redirectDirection switch
        {
            RedirectFallbackDirection.Ancestor => item.Ancestors(),
            RedirectFallbackDirection.Descendant => item.Descendants(),
            RedirectFallbackDirection.Sibling => item.Siblings(),
            _ => throw new UnreachableException("Shouldn't happen, non existent redirect direction")
        };

        var destination = possibleItems
            ?.FirstOrDefault(x => !s_nonValidDestinationPageTypes.Contains(x.ContentType.Alias)
                                  && x is not IRedirectSettings);

        return destination?.Url(_publishedUrlProvider, culture, UrlMode.Absolute);
    }
}
