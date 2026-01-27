using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OutputCaching;
using UmbracoHeadlessBFF.Cms.Modules.Common.Caching;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Versioning;

[ApiController]
[Route("version")]
[Tags("Version")]
[OutputCache(PolicyName = DefaultOutputCachePolicy.PolicyName)]
public sealed class GetVersionController : ControllerBase
{
    private static readonly string s_version = AssemblyVersionExtensions.GetVersion();

    [HttpGet("")]
    public IActionResult GetVersion()
    {
        return Ok(new { Version = s_version });
    }
}
