// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> to register the <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <see cref="AutofacServiceProviderFactory"/> to the service collection. ONLY FOR PRE-ASP.NET 3.0 HOSTING. THIS WON'T WORK
    /// FOR ASP.NET CORE 3.0+ OR GENERIC HOSTING.
    /// </summary>
    /// <param name="services">The service collection to add the factory to.</param>
    /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddAutofac(this IServiceCollection services, Action<ContainerBuilder>? configurationAction = null)
    {
        return services.AddSingleton<IServiceProviderFactory<ContainerBuilder>>(new AutofacServiceProviderFactory(configurationAction));
    }
}
