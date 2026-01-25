using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using UmbracoHeadlessBFF.SharedModules.Common.Versioning;

namespace UmbracoHeadlessBFF.Cms.Modules.Common.Versioning;

[ApiController]
[Route("version")]
[Tags("Version")]
public sealed class GetVersionController : ControllerBase
{
    private static readonly string s_version = AssemblyVersionExtensions.GetVersion();

    [HttpGet("")]
    public IActionResult GetVersion()
    {
        return Ok(new { Version = s_version });
    }
}
