// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test;

public class FromKeyedServicesUsageCacheTests
{
    [Fact]
    public void RequiresFromKeyedServicesMiddlewareAttributeNotUsed()
    {
        var activator = GetReflectionActivator<NoFromKeyedServices>();

        var needsMiddleware = FromKeyedServicesUsageCache.RequiresFromKeyedServicesMiddleware(activator);

        Assert.False(needsMiddleware);
    }

    [Fact]
    public void RequiresFromKeyedServicesMiddlewareAttributeUsed()
    {
        var activator = GetReflectionActivator<UsesFromKeyedServices>();

        var needsMiddleware = FromKeyedServicesUsageCache.RequiresFromKeyedServicesMiddleware(activator);

        Assert.True(needsMiddleware);
    }

    [Fact]
    public void RequiresFromKeyedServicesMiddlewareCachesForSubsequentCalls()
    {
        var activator = GetReflectionActivator<UsesFromKeyedServices>();

        var first = FromKeyedServicesUsageCache.RequiresFromKeyedServicesMiddleware(activator);
        var second = FromKeyedServicesUsageCache.RequiresFromKeyedServicesMiddleware(activator);

        Assert.True(first);
        Assert.Equal(first, second);
    }

    [Fact]
    public void RequiresFromKeyedServicesMiddlewareNullActivator()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => FromKeyedServicesUsageCache.RequiresFromKeyedServicesMiddleware(null!));
        Assert.Equal("activator", ex.ParamName);
    }

    private static ReflectionActivator GetReflectionActivator<T>()
        where T : class
    {
        var services = new ServiceCollection();
        services.AddTransient<T>();

        var builder = new ContainerBuilder();
        builder.Populate(services);

        using var container = builder.Build();
        var typedService = new TypedService(typeof(T));
        var registration = container.ComponentRegistry.RegistrationsFor(typedService).Single();

        return Assert.IsType<ReflectionActivator>(registration.Activator);
    }

    private sealed class UsesFromKeyedServices
    {
        public UsesFromKeyedServices([FromKeyedServices("k")] object service)
        {
        }
    }

    private sealed class NoFromKeyedServices
    {
        public NoFromKeyedServices(object service)
        {
        }
    }
}
