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

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// A factory for creating a child-scope based on a root-scope using <see cref="ILifetimeScope"/> and producing an <see cref="IServiceProvider" />.
    /// </summary>
    public class AutofacChildScopeServiceProviderFactory : IServiceProviderFactory<AutofacChildScopeConfigurationAdapter>
    {
        private readonly Action<ContainerBuilder> _containerConfigurationAction;
        private readonly ILifetimeScope _rootLifetimeScope;
        private static readonly Action<ContainerBuilder> FallbackConfigurationAction = builder => { };

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacChildScopeServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="getRootLifetimeScopeFunc">Function to retrieve the root-container instance built using <see cref="ContainerBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        public AutofacChildScopeServiceProviderFactory(Func<ILifetimeScope> getRootLifetimeScopeFunc, Action<ContainerBuilder> configurationAction = null)
        {
            if (getRootLifetimeScopeFunc == null) throw new ArgumentNullException(nameof(getRootLifetimeScopeFunc));

            _rootLifetimeScope = getRootLifetimeScopeFunc();
            _containerConfigurationAction = configurationAction ?? AutofacChildScopeServiceProviderFactory.FallbackConfigurationAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AutofacChildScopeServiceProviderFactory"/> class.
        /// </summary>
        /// <param name="rootLifetimeScope">The root-container instance built using <see cref="ContainerBuilder"/>.</param>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        public AutofacChildScopeServiceProviderFactory(ILifetimeScope rootLifetimeScope, Action<ContainerBuilder> configurationAction = null)
        {
            _rootLifetimeScope = rootLifetimeScope ?? throw new ArgumentNullException(nameof(rootLifetimeScope));
            _containerConfigurationAction = configurationAction ?? AutofacChildScopeServiceProviderFactory.FallbackConfigurationAction;
        }

        /// <summary>
        /// Creates a container builder from an <see cref="IServiceCollection" />.
        /// </summary>
        /// <param name="services">The collection of services.</param>
        /// <returns>A container builder that can be used to create an <see cref="IServiceProvider" />.</returns>
        public AutofacChildScopeConfigurationAdapter CreateBuilder(IServiceCollection services)
        {
            var actions = new AutofacChildScopeConfigurationAdapter();

            actions.Add(builder => builder.Populate(services));
            actions.Add(builder => _containerConfigurationAction(builder));

            return actions;
        }

        /// <summary>
        /// Creates an <see cref="IServiceProvider" /> from the container builder.
        /// </summary>
        /// <param name="autofacChildScopeConfigurationAdapter">The adapter holding configuration applied to <see cref="ContainerBuilder"/> creating the <see cref="IServiceProvider"/>.</param>
        /// <returns>An <see cref="IServiceProvider" />.</returns>
        public IServiceProvider CreateServiceProvider(AutofacChildScopeConfigurationAdapter autofacChildScopeConfigurationAdapter)
        {
            if (autofacChildScopeConfigurationAdapter == null) throw new ArgumentNullException(nameof(autofacChildScopeConfigurationAdapter));

            var scope = _rootLifetimeScope.BeginLifetimeScope(scopeBuilder =>
            {
                foreach (var action in autofacChildScopeConfigurationAdapter.ChildScopeConfigurationActions)
                {
                    action(scopeBuilder);
                }
            });

            return new AutofacServiceProvider(scope);
        }
    }
}