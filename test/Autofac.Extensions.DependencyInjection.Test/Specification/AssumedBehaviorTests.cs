// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

/// <summary>
/// Additional tests to illustrate undocumented yet assumed behaviors in
/// the Microsoft.Extensions.DependencyInjection container/scope.
/// </summary>
public abstract class AssumedBehaviorTests
{
    [Fact]
    public void DisposingScopeAlsoDisposesServiceProvider()
    {
        // You can't resolve things from a scope's service provider
        // if you dispose the scope.
        var services = new ServiceCollection().AddScoped<DisposeTracker>();
        var rootProvider = CreateServiceProvider(services);
        var scope = rootProvider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<DisposeTracker>());
        scope.Dispose();
        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetRequiredService<DisposeTracker>());
    }

    [Fact]
    public void DisposingScopeAndProviderOnlyDisposesObjectsOnce()
    {
        // Disposing the service provider and then the scope only
        // runs one disposal on the resolved objects.
        var services = new ServiceCollection().AddScoped<DisposeTracker>();
        var rootProvider = CreateServiceProvider(services);
        var scope = rootProvider.CreateScope();
        var tracker = scope.ServiceProvider.GetRequiredService<DisposeTracker>();
        ((IDisposable)scope.ServiceProvider).Dispose();
        Assert.True(tracker.Disposed);
        Assert.Equal(1, tracker.DisposeCount);
        scope.Dispose();
        Assert.Equal(1, tracker.DisposeCount);
    }

    [Fact]
    public void DisposingScopeServiceProviderStopsNewScopes()
    {
        // You can't create a new child scope if you've disposed of
        // the parent scope service provider.
        var rootProvider = CreateServiceProvider(new ServiceCollection());
        using var scope = rootProvider.CreateScope();
        ((IDisposable)scope.ServiceProvider).Dispose();
        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.CreateScope());
    }

    [Fact]
    public void DisposingScopeServiceProviderStopsScopeResolutions()
    {
        // You can't resolve things from a scope if you dispose the
        // scope's service provider.
        var services = new ServiceCollection().AddScoped<DisposeTracker>();
        var rootProvider = CreateServiceProvider(services);
        using var scope = rootProvider.CreateScope();
        Assert.NotNull(scope.ServiceProvider.GetRequiredService<DisposeTracker>());
        ((IDisposable)scope.ServiceProvider).Dispose();
        Assert.Throws<ObjectDisposedException>(() => scope.ServiceProvider.GetRequiredService<DisposeTracker>());
    }

    [Fact]
    public void ResolvedProviderNotSameAsParent()
    {
        // Resolving a provider from another provider yields a new object.
        // (It's not just returning "this" - it's a different IServiceProvider.)
        var parent = CreateServiceProvider(new ServiceCollection());
        var resolved = parent.GetRequiredService<IServiceProvider>();
        Assert.NotSame(parent, resolved);
    }

    [Fact]
    public void ResolvedProviderUsesSameScopeAsParent()
    {
        // Resolving a provider from another provider will still resolve
        // items from the same scope.
        var services = new ServiceCollection().AddScoped<DisposeTracker>();
        var root = CreateServiceProvider(services);
        using var scope = root.CreateScope();
        var parent = scope.ServiceProvider;
        var resolved = parent.GetRequiredService<IServiceProvider>();
        Assert.Same(parent.GetRequiredService<DisposeTracker>(), resolved.GetRequiredService<DisposeTracker>());
    }

    [Fact]
    public void ServiceProviderWillNotResolveAfterDispose()
    {
        // You can't resolve things from a service provider
        // if you dispose it.
        var services = new ServiceCollection().AddScoped<DisposeTracker>();
        var rootProvider = CreateServiceProvider(services);
        Assert.NotNull(rootProvider.GetRequiredService<DisposeTracker>());
        ((IDisposable)rootProvider).Dispose();
        Assert.Throws<ObjectDisposedException>(() => rootProvider.GetRequiredService<DisposeTracker>());
    }

    [Fact]
    public async ValueTask ServiceProviderDisposesAsync()
    {
        // You can't resolve things from a service provider
        // if you dispose it.
        var services = new ServiceCollection().AddScoped<AsyncDisposeTracker>();
        var rootProvider = CreateServiceProvider(services);
        var tracker = rootProvider.GetRequiredService<AsyncDisposeTracker>();
        var asyncDisposer = (IAsyncDisposable)rootProvider;

        await asyncDisposer.DisposeAsync().ConfigureAwait(false);

        Assert.True(tracker.AsyncDisposed);
        Assert.False(tracker.SyncDisposed);
    }

    [Fact]
    public async ValueTask ServiceScopeDisposesAsync()
    {
        // You can't resolve things from a service provider
        // if you dispose it.
        var services = new ServiceCollection().AddScoped<AsyncDisposeTracker>();
        var rootProvider = CreateServiceProvider(services);

        AsyncDisposeTracker tracker;

        // Try out the new "CreateAsyncScope" method.
        var scope = rootProvider.CreateAsyncScope();
        await using (scope.ConfigureAwait(false))
        {
            tracker = scope.ServiceProvider.GetRequiredService<AsyncDisposeTracker>();
        }

        Assert.True(tracker.AsyncDisposed);
        Assert.False(tracker.SyncDisposed);
    }

    [Fact]
    public void ServiceScopeFactoryIsSingleton()
    {
        // Issue #83: M.E.DI assumes service scope factory is singleton, but
        // there's no compatibility test for it yet.
        var services = new ServiceCollection();
        var rootProvider = CreateServiceProvider(services);
        var rootFactory1 = rootProvider.GetRequiredService<IServiceScopeFactory>();
        var rootFactory2 = rootProvider.GetRequiredService<IServiceScopeFactory>();
        Assert.Same(rootFactory1, rootFactory2);

        var childScope = rootFactory1.CreateScope();
        var childFactory = childScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        Assert.Same(rootFactory1, childFactory);
    }

    [Fact]
    public void ServiceScopesAreFlat()
    {
        // Issue #83: M.E.DI assumes service scopes are flat - disposing of
        // a "parent" scope won't actually invalidate a "child" because
        // they're not related in that fashion.
        var services = new ServiceCollection().AddSingleton<DisposeTracker>();
        var rootProvider = CreateServiceProvider(services);

        var outerScope = rootProvider.CreateScope();
        var innerScope = outerScope.ServiceProvider.CreateScope();
        outerScope.Dispose();

        // This part will blow up if the scopes are hierarchical.
        innerScope.ServiceProvider.GetRequiredService<DisposeTracker>();
        innerScope.Dispose();
    }

    protected abstract IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection);

    [SuppressMessage("CA1812", "CA1812", Justification = "Instantiated through reflection.")]
    private class DisposeTrackerConsumer
    {
        public DisposeTrackerConsumer(IEnumerable<DisposeTracker> trackers)
        {
            Trackers = trackers;
        }

        public IEnumerable<DisposeTracker> Trackers { get; }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Instantiated via dependency injection.")]
    private class DisposeTracker : IDisposable
    {
        public int DisposeCount { get; set; }

        public bool Disposed { get; set; }

        public void Dispose()
        {
            Disposed = true;
            DisposeCount++;
        }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Instantiated via dependency injection.")]
    private class AsyncDisposeTracker : IDisposable, IAsyncDisposable
    {
        public bool SyncDisposed { get; set; }

        public bool AsyncDisposed { get; set; }

        public void Dispose()
        {
            SyncDisposed = true;
        }

        public async ValueTask DisposeAsync()
        {
            await Task.Delay(1).ConfigureAwait(false);

            AsyncDisposed = true;
        }
    }
}
