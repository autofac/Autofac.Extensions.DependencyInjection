using System;
using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// A factory for creating a <see cref="ContainerBuilder"/> and an <see cref="IServiceProvider" />.
    /// CreateScope behaviour as like as Microsoft.Extensions.DependencyInjection.
    /// </summary>
    public class AutofacFlatScopesServiceProviderFactory : IServiceProviderFactory<ContainerBuilder>
    {
        private readonly Action<ContainerBuilder> _configurationAction;
        private readonly ContainerBuildOptions _containerBuildOptions = ContainerBuildOptions.None;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacFlatScopesServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="containerBuildOptions">The container options to use when building the container.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        public AutofacFlatScopesServiceProviderFactory(
            ContainerBuildOptions containerBuildOptions,
            Action<ContainerBuilder> configurationAction = null)
            : this(configurationAction) =>
            _containerBuildOptions = containerBuildOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacFlatScopesServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container..</param>
        public AutofacFlatScopesServiceProviderFactory(Action<ContainerBuilder> configurationAction = null) =>
            _configurationAction = configurationAction ?? (builder => { });

        /// <summary>
        /// Creates a container builder from an <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <returns>A container builder that can be used to create an <see cref="IServiceProvider" />.</returns>
        public ContainerBuilder CreateBuilder(IServiceCollection services)
        {
            var builder = new ContainerBuilder();

            builder.Populate(services, null, true);

            _configurationAction(builder);

            return builder;
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider" /> from the container builder.
        /// </summary>
        /// <param name="containerBuilder">The container builder.</param>
        /// <returns>An <see cref="IServiceProvider" />.</returns>
        public IServiceProvider CreateServiceProvider(ContainerBuilder containerBuilder)
        {
            if (containerBuilder == null) throw new ArgumentNullException(nameof(containerBuilder));

            var container = containerBuilder.Build(_containerBuildOptions);

            return new AutofacServiceProvider(container);
        }
    }
}
