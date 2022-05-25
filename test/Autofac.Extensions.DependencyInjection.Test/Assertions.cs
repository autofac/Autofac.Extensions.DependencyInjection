// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;

namespace Autofac.Extensions.DependencyInjection.Test;

internal static class Assertions
{
    public static void AssertRegistered<TService>(this IComponentContext context)
    {
        Assert.True(context.IsRegistered<TService>());
    }

    public static void AssertNotRegistered<TService>(this IComponentContext context)
    {
        Assert.False(context.IsRegistered<TService>());
    }

    public static void AssertImplementation<TService, TImplementation>(this IComponentContext context)
    {
        var service = context.Resolve<TService>();
        Assert.IsAssignableFrom<TImplementation>(service);
    }

    public static void AssertSharing<TService>(this IComponentContext context, InstanceSharing sharing)
    {
        var cr = context.RegistrationFor<TService>();
        Assert.Equal(sharing, cr.Sharing);
    }

    public static void AssertLifetime<TService, TLifetime>(this IComponentContext context)
    {
        var cr = context.RegistrationFor<TService>();
        Assert.IsType<TLifetime>(cr.Lifetime);
    }

    public static void AssertOwnership<TService>(this IComponentContext context, InstanceOwnership ownership)
    {
        var cr = context.RegistrationFor<TService>();
        Assert.Equal(ownership, cr.Ownership);
    }

    public static IComponentRegistration RegistrationFor<TService>(this IComponentContext context)
    {
        Assert.True(context.ComponentRegistry.TryGetRegistration(new TypedService(typeof(TService)), out IComponentRegistration r));
        return r;
    }
}
