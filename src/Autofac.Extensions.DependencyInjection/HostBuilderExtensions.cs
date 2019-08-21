// This software is part of the Autofac IoC container
// Copyright © 2019 Autofac Contributors
// https://autofac.org
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods on <see cref="IHostBuilder"/> to register the <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
    /// </summary>
    public static class HostBuilderExtensions
    {
        /// <summary>
        /// Use the <see cref="AutofacServiceProviderFactory" /> as the factory for creating the service provider.
        /// </summary>
        /// <param name="hostBuilder">The instance of the <see cref="IHostBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
        public static IHostBuilder UseAutofac(this IHostBuilder hostBuilder, Action<ContainerBuilder> configurationAction = null) =>
            hostBuilder.UseServiceProviderFactory(new AutofacServiceProviderFactory(configurationAction));

        /// <summary>
        /// Use the <see cref="AutofacChildLifetimeScopeServiceProviderFactory" /> as the factory for creating the service provider.
        /// </summary>
        /// <param name="hostBuilder">The instance of the <see cref="IHostBuilder"/>.</param>
        /// <param name="rootLifetimeScopeAccessor">A function to retrieve the root <see cref="ILifetimeScope"/> instance.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
        public static IHostBuilder UseAutofacChildLifetimeScopeFactory(this IHostBuilder hostBuilder, Func<ILifetimeScope> rootLifetimeScopeAccessor, Action<ContainerBuilder> configurationAction = null) =>
            hostBuilder.UseServiceProviderFactory(
                new AutofacChildLifetimeScopeServiceProviderFactory(rootLifetimeScopeAccessor, configurationAction));

        /// <summary>
        /// Use the <see cref="AutofacChildLifetimeScopeServiceProviderFactory" /> as the factory for creating the service provider.
        /// </summary>
        /// <param name="hostBuilder">The instance of the <see cref="IHostBuilder"/>.</param>
        /// <param name="rootLifetimeScope">The root <see cref="ILifetimeScope"/> instance.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        /// <returns>The same instance of the <see cref="IHostBuilder" /> for chaining.</returns>
        public static IHostBuilder UseAutofacChildLifetimeScopeFactory(this IHostBuilder hostBuilder, ILifetimeScope rootLifetimeScope, Action<ContainerBuilder> configurationAction = null) =>
            hostBuilder.UseServiceProviderFactory(
                new AutofacChildLifetimeScopeServiceProviderFactory(rootLifetimeScope, configurationAction));
    }
}
