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
using System.Collections.Generic;

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// Configuration adapter for <see cref="AutofacChildLifetimeScopeServiceProviderFactory" />.
    /// </summary>
    public class AutofacChildLifetimeScopeConfigurationAdapter
    {
        private readonly List<Action<ContainerBuilder>> _configurationActions = new List<Action<ContainerBuilder>>();

        /// <summary>
        /// Adds a configuration action that will be executed when the child <see cref="ILifetimeScope"/> is created.
        /// </summary>
        /// <param name="configurationAction">Action on a <see cref="ContainerBuilder"/> that adds component registrations to the container.</param>
        /// <exception cref="ArgumentNullException">Throws when the passed configuration-action is null.</exception>
        public void Add(Action<ContainerBuilder> configurationAction)
        {
            if (configurationAction == null) throw new ArgumentNullException(nameof(configurationAction));

            _configurationActions.Add(configurationAction);
        }

        /// <summary>
        /// Gets the list of configuration actions to be executed on the <see cref="ContainerBuilder"/> for the child <see cref="ILifetimeScope"/>.
        /// </summary>
        public IReadOnlyList<Action<ContainerBuilder>> ConfigurationActions => _configurationActions;
    }
}