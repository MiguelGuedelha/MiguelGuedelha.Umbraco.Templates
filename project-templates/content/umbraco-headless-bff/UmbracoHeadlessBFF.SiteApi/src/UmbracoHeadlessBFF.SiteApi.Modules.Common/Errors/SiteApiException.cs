using Microsoft.AspNetCore.Http;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Common.Errors;

public sealed class SiteApiException : Exception
{
    public int StatusCode { get; private set; }

    public SiteApiException()
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }

    public SiteApiException(int? statusCode)
    {
        StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
    }

    public SiteApiException(string? message) : base(message)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }

    public SiteApiException(int? statusCode, string? message) : base(message)
    {
        StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
    }

    public SiteApiException(string? message, Exception? inner) : base(message, inner)
    {
        StatusCode = StatusCodes.Status500InternalServerError;
    }

    public SiteApiException(int? statusCode, string? message, Exception? inner) : base(message, inner)
    {
        StatusCode = statusCode ?? StatusCodes.Status500InternalServerError;
    }
}
