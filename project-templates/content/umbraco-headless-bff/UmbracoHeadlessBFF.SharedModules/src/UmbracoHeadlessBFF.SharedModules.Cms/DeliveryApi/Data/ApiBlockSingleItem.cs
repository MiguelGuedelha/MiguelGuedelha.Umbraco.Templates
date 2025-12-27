namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

public sealed record ApiBlockSingleItem : IApiBlockSingleItem
{
    public required IApiElement Content { get; init; }
    public IApiElement? Settings { get; init; }
}

public sealed record ApiBlockSingleItem<T> : IApiBlockSingleItem<T>
    where T : class, IApiElement
{
    public required T Content { get; init; }
    public IApiElement? Settings { get; init; }
}

public sealed record ApiBlockSingleItem<TContent, TSettings> : IApiBlock<TContent, TSettings>
    where TContent : class, IApiElement
    where TSettings : class, IApiElement
{
    public required TContent Content { get; init; }
    public required TSettings Settings { get; init; }
}
