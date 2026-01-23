using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;
using UmbracoHeadlessBFF.SiteApi.Modules.Pages.Models.BuildingBlocks;

namespace UmbracoHeadlessBFF.SiteApi.Modules.Pages.Mappers.BuildingBlocks;

internal interface IImageMapper : IMapper<ApiMediaWithCrops, Image>
{
}

internal sealed class ImageMapper : IImageMapper
{
    private static readonly HashSet<string> s_processableImageTypes = ["png", "webp", "jpg", "jpeg", "avif", "gif"];

    public Task<Image?> Map(ApiMediaWithCrops? model)
    {
        if (model is null or { Extension: null })
        {
            return Task.FromResult<Image?>(null);
        }

        object? altText = null;
        model.Properties?.TryGetValue("altText", out altText);

        altText = altText is not JsonElement { ValueKind: JsonValueKind.String } altTextElement
            ? null
            : altTextElement.GetString();

        if (!s_processableImageTypes.Contains(model.Extension))
        {
            return Task.FromResult<Image?>(new()
            {
                Src = model.Url,
                AltText = altText as string
            });
        }

        var query = new QueryBuilder();

        if (model.FocalPoint is not null)
        {
            query.Add("rxy", $"{model.FocalPoint.Left},{model.FocalPoint.Top}");
        }

        if (!model.Extension.Equals("webp", StringComparison.OrdinalIgnoreCase) &&
            !model.Extension.Equals("avif", StringComparison.OrdinalIgnoreCase))
        {
            query.Add("format", "webp");
        }

        var url = new UriBuilder(model.Url)
        {
            Query = query.ToString()
        };

        return Task.FromResult<Image?>(new()
        {
            Src = url.Uri.AbsoluteUri,
            AltText = altText as string
        });
    }
}
