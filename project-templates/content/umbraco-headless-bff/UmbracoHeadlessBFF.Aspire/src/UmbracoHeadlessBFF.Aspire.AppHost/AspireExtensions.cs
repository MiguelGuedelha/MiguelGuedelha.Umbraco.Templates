namespace UmbracoHeadlessBFF.Aspire.AppHost;

internal static class AspireExtensions
{
    extension<T>(IResourceBuilder<T> builder) where T : IResourceWithEnvironment
    {
        /// <summary>
        /// Adds a parameter to the services IConfiguration section of the configured service
        /// </summary>
        /// <param name="builder">The resource builder we are configuring the parameter in</param>
        /// <param name="service">The resource builder of the service we want to configure the parameter for</param>
        /// <param name="name">The name for the parameter</param>
        /// <param name="parameter">The parameter value</param>
        /// <typeparam name="T">The resource type.</typeparam>
        /// <remarks>
        /// The resulting parameter will be located in <code>services:&lt;service&gt;:Parameters:&lt;save&gt;</code>
        /// </remarks>
        /// <returns><see cref="IResourceBuilder{T}"/></returns>
        public IResourceBuilder<T> WithServiceParameter(
            IResourceBuilder<IResource> service,
            string name,
            IResourceBuilder<ParameterResource> parameter)
        {
            ArgumentNullException.ThrowIfNull(builder);
            ArgumentNullException.ThrowIfNull(service);
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(parameter);

            builder.WithEnvironment($"services__{service.Resource.Name}__Parameters__{name}", parameter);

            return builder;
        }
    }
}
