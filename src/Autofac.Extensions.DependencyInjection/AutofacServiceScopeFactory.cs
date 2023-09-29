// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Autofac implementation of the ASP.NET Core <see cref="IServiceScopeFactory"/>.
/// </summary>
/// <seealso cref="IServiceScopeFactory" />
internal class AutofacServiceScopeFactory : IServiceScopeFactory
{
    private readonly ILifetimeScope _lifetimeScope;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacServiceScopeFactory"/> class.
    /// </summary>
    /// <param name="lifetimeScope">The lifetime scope.</param>
    public AutofacServiceScopeFactory(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    /// <summary>
    /// Creates an <see cref="IServiceScope" /> which contains an
    /// <see cref="System.IServiceProvider" /> used to resolve dependencies within
    /// the scope.
    /// </summary>
    /// <returns>
    /// An <see cref="IServiceScope" /> controlling the lifetime of the scope. Once
    /// this is disposed, any scoped services that have been resolved
    /// from the <see cref="IServiceScope.ServiceProvider" />
    /// will also be disposed.
    /// </returns>
    public IServiceScope CreateScope()
    {
        return new AutofacServiceScope(_lifetimeScope.BeginLifetimeScope());
    }
}
