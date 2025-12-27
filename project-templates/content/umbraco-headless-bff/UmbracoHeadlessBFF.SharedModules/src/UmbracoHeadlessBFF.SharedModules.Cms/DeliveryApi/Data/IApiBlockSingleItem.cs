namespace UmbracoHeadlessBFF.SharedModules.Cms.DeliveryApi.Data;

public interface IApiBlockSingleItem<TContent, TSettings> : IApiBlock<TContent, TSettings>
    where TContent : class, IApiElement
    where TSettings : class, IApiElement
{
}

public interface IApiBlockSingleItem<TContent> : IApiBlock<TContent>
    where TContent : class, IApiElement
{
}

public interface IApiBlockSingleItem : IApiBlock
{
}
