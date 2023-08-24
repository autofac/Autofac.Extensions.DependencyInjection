// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Autofac implementation of the ASP.NET Core <see cref="IServiceProvider"/>.
/// </summary>
/// <seealso cref="IServiceProvider" />
/// <seealso cref="ISupportRequiredService" />
public partial class AutofacServiceProvider : IServiceProvider, ISupportRequiredService, IServiceProviderIsService, IDisposable, IAsyncDisposable
{
    private readonly ILifetimeScope _lifetimeScope;

    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="AutofacServiceProvider"/> class.
    /// </summary>
    /// <param name="lifetimeScope">
    /// The lifetime scope from which services will be resolved.
    /// </param>
    public AutofacServiceProvider(ILifetimeScope lifetimeScope)
    {
        _lifetimeScope = lifetimeScope;
    }

    /// <summary>
    /// Gets service of type <paramref name="serviceType" /> from the
    /// <see cref="AutofacServiceProvider" /> and requires it be present.
    /// </summary>
    /// <param name="serviceType">
    /// An object that specifies the type of service object to get.
    /// </param>
    /// <returns>
    /// A service object of type <paramref name="serviceType" />.
    /// </returns>
    /// <exception cref="Autofac.Core.Registration.ComponentNotRegisteredException">
    /// Thrown if the <paramref name="serviceType" /> isn't registered with the container.
    /// </exception>
    /// <exception cref="Autofac.Core.DependencyResolutionException">
    /// Thrown if the object can't be resolved from the container.
    /// </exception>
    public object GetRequiredService(Type serviceType)
    {
        return _lifetimeScope.Resolve(serviceType);
    }

    /// <inheritdoc />
    public bool IsService(Type serviceType) => _lifetimeScope.ComponentRegistry.IsRegistered(new TypedService(serviceType));

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">
    /// An object that specifies the type of service object to get.
    /// </param>
    /// <returns>
    /// A service object of type <paramref name="serviceType" />; or <see langword="null" />
    /// if there is no service object of type <paramref name="serviceType" />.
    /// </returns>
    public object? GetService(Type serviceType)
    {
        return _lifetimeScope.ResolveOptional(serviceType);
    }

    /// <summary>
    /// Gets the underlying instance of <see cref="ILifetimeScope" />.
    /// </summary>
    public ILifetimeScope LifetimeScope => _lifetimeScope;

    /// <summary>
    /// Releases unmanaged and - optionally - managed resources.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true" /> to release both managed and unmanaged resources;
    /// <see langword="false" /> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            _disposed = true;
            if (disposing)
            {
                _lifetimeScope.Dispose();
            }
        }
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Performs a dispose operation asynchronously.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            await _lifetimeScope.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }
}
