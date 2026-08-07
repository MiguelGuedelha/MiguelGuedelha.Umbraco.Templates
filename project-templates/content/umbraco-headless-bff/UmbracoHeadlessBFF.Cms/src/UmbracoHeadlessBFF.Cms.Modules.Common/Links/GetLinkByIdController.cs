using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using UmbracoHeadlessBFF.Cms.Modules.Common.Authentication;
using UmbracoHeadlessBFF.Cms.Modules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Cms.Links;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Links;

[ApiKey]
[ApiController]
[Route($"api/v{{version:apiVersion}}/{LinksConstants.Endpoints.Group}")]
[Tags(LinksConstants.Endpoints.Tag)]
[ApiVersion(1)]
[OutputCache(PolicyName = DefaultOutputCachePolicy.PolicyName)]
public sealed class GetLinkByIdController : ControllerBase
{
    private readonly LinkService _linkService;

    public GetLinkByIdController(LinkService linkService)
    {
        _linkService = linkService;
    }

    [HttpGet("{id:guid}")]
    public Results<Ok<Link>, NotFound, ProblemHttpResult> GetLinkById(Guid id, string culture, string domain, bool preview)
    {
        var link = _linkService.GetUriByContentId(id, culture, domain, preview);

        return link is not null ? TypedResults.Ok(new Link{ Url = link }) : TypedResults.NotFound();
    }
}
