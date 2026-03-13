// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Autofac implementation of the ASP.NET Core <see cref="IServiceScope"/>.
/// Inherits from <see cref="AutofacServiceProvider"/> to avoid a separate
/// allocation per scope — every scope is itself a service provider.
/// </summary>
/// <seealso cref="IServiceScope" />
internal class AutofacServiceScope : AutofacServiceProvider, IServiceScope
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacServiceScope"/> class.
    /// </summary>
    /// <param name="lifetimeScope">
    /// The lifetime scope from which services should be resolved for this service scope.
    /// </param>
    public AutofacServiceScope(ILifetimeScope lifetimeScope)
        : base(lifetimeScope)
    {
    }

    /// <summary>
    /// Gets an <see cref="IServiceProvider" /> corresponding to this service scope.
    /// </summary>
    /// <value>
    /// An <see cref="IServiceProvider" /> that can be used to resolve dependencies from the scope.
    /// </value>
    public IServiceProvider ServiceProvider => this;
}
