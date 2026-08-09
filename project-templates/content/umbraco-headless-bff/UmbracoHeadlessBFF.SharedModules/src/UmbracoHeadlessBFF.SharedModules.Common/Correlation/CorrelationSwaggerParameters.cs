using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace UmbracoHeadlessBFF.SharedModules.Common.Correlation;

public sealed class CorrelationSwaggerParameters : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= [];

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationConstants.Headers.SiteHost,
            Description = "The frontend host, used to find the correct CMS instance and site node",
            In = ParameterLocation.Header,
            Required = false
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationConstants.Headers.SitePath,
            Description = "The frontend path, used to find the correct CMS instance and site node",
            In = ParameterLocation.Header,
            Required = false
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationConstants.Headers.PreviewMode,
            Description = "Whether or not the API should return preview content",
            In = ParameterLocation.Header,
            Required = false,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.Boolean
            }
        });

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = CorrelationConstants.Headers.PreviewToken,
            Description = "The preview token to be used when preview mode is true",
            In = ParameterLocation.Header,
            Required = false
        });
    }
}
