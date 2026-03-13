// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using KeyedService = Autofac.Core.KeyedService;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Autofac implementation of the ASP.NET Core <see cref="IServiceProvider"/>.
/// </summary>
/// <seealso cref="IServiceProvider" />
/// <seealso cref="ISupportRequiredService" />
public class AutofacServiceProvider : IServiceProvider, ISupportRequiredService, IKeyedServiceProvider, IServiceProviderIsService, IServiceProviderIsKeyedService, IDisposable, IAsyncDisposable
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
    /// Gets the underlying instance of <see cref="ILifetimeScope" />.
    /// </summary>
    public ILifetimeScope LifetimeScope => _lifetimeScope;

    /// <summary>
    /// Gets the service object of the specified type.
    /// </summary>
    /// <param name="serviceType">
    /// An object that specifies the type of service object to get.
    /// </param>
    /// <param name="serviceKey">
    /// An object that specifies the key of service object to get.
    /// </param>
    /// <returns>
    /// A service object of type <paramref name="serviceType" />; or <see langword="null" />
    /// if there is no service object of type <paramref name="serviceType" />.
    /// </returns>
    public object? GetKeyedService(Type serviceType, object? serviceKey)
    {
        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        var normalizedKey = NormalizeServiceKey(serviceType, serviceKey);

        if (normalizedKey is null)
        {
            // A null key equates to "not keyed."
            return _lifetimeScope.ResolveOptional(serviceType);
        }
        else
        {
            try
            {
                return _lifetimeScope.ResolveOptionalService(new KeyedService(normalizedKey, serviceType));
            }
            catch (DependencyResolutionException ex)
            {
                // All exceptions resolving keyed services as of .NET 10 are
                // expected to be InvalidOperationException.
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
    }

    /// <summary>
    /// Gets service of type <paramref name="serviceType" /> from the
    /// <see cref="AutofacServiceProvider" /> and requires it be present.
    /// </summary>
    /// <param name="serviceType">
    /// An object that specifies the type of service object to get.
    /// </param>
    /// <param name="serviceKey">
    /// An object that specifies the key of service object to get.
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
    public object GetRequiredKeyedService(Type serviceType, object? serviceKey)
    {
        if (serviceType is null)
        {
            throw new ArgumentNullException(nameof(serviceType));
        }

        var normalizedKey = NormalizeServiceKey(serviceType, serviceKey);

        if (normalizedKey is null)
        {
            // A null key equates to "not keyed."
            return _lifetimeScope.Resolve(serviceType);
        }
        else
        {
            try
            {
                return _lifetimeScope.ResolveKeyed(normalizedKey, serviceType);
            }
            catch (DependencyResolutionException ex)
            {
                // All exceptions resolving keyed services as of .NET 10 are
                // expected to be InvalidOperationException.
                throw new InvalidOperationException(ex.Message, ex);
            }
        }
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
        try
        {
            return _lifetimeScope.Resolve(serviceType);
        }
        catch (DependencyResolutionException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
        }
    }

    /// <inheritdoc />
    public bool IsKeyedService(Type serviceType, object? serviceKey)
    {
        // Null service key means non-keyed.
        if (serviceKey == null)
        {
            return IsService(serviceType);
        }

        return _lifetimeScope.ComponentRegistry.IsRegistered(new KeyedService(serviceKey, serviceType));
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
        try
        {
            return _lifetimeScope.ResolveOptional(serviceType);
        }
        catch (DependencyResolutionException ex)
        {
            throw new InvalidOperationException(ex.Message, ex);
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
    /// <returns>A task to await disposal.</returns>
    public async ValueTask DisposeAsync()
    {
        if (!_disposed)
        {
            _disposed = true;
            await _lifetimeScope.DisposeAsync().ConfigureAwait(false);
            GC.SuppressFinalize(this);
        }
    }

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

    private static object? NormalizeServiceKey(Type serviceType, object? serviceKey)
    {
        if (serviceKey is null)
        {
            return null;
        }

        if (ReferenceEquals(serviceKey, Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey))
        {
            if (!serviceType.IsCollection())
            {
                throw new InvalidOperationException("KeyedService.AnyKey cannot be used to resolve a single service.");
            }

            return KeyedService.AnyKey;
        }

        return serviceKey;
    }
}
