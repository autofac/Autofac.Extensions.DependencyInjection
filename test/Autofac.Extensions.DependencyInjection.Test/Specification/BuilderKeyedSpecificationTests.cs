// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

public class BuilderKeyedSpecificationTests : KeyedDependencyInjectionSpecificationTests
{
    public override bool SupportsIServiceProviderIsKeyedService => true;

    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection)
    {
        var builder = new ContainerBuilder();
        builder.Populate(collection);
        var container = builder.Build();
        return container.Resolve<IServiceProvider>();
    }

    [Fact]
    public void DebugCombinationalRegistration()
    {
        Service service1 = new();
        Service service2 = new();
        Service keyedService1 = new();
        Service keyedService2 = new();
        Service anykeyService1 = new();
        Service anykeyService2 = new();
        Service nullkeyService1 = new();
        Service nullkeyService2 = new();

        ServiceCollection serviceCollection = new();
        serviceCollection.AddSingleton<IService>(service1);
        serviceCollection.AddSingleton<IService>(service2);
        serviceCollection.AddKeyedSingleton<IService>(null, nullkeyService1);
        serviceCollection.AddKeyedSingleton<IService>(null, nullkeyService2);
        serviceCollection.AddKeyedSingleton<IService>(KeyedService.AnyKey, anykeyService1);
        serviceCollection.AddKeyedSingleton<IService>(KeyedService.AnyKey, anykeyService2);
        serviceCollection.AddKeyedSingleton<IService>("keyedService", keyedService1);
        serviceCollection.AddKeyedSingleton<IService>("keyedService", keyedService2);

        IServiceProvider provider = CreateServiceProvider(serviceCollection);

        /*
         * Table for what results are included:
         *
         * Query                     | Keyed? | Unkeyed? | AnyKey? | null key?
         * -------------------------------------------------------------------
         * GetServices(Type)         | no     | yes      | no      | yes
         * GetService(Type)          | no     | yes      | no      | yes
         *
         * GetKeyedServices(null)    | no     | yes      | no      | yes
         * GetKeyedService(null)     | no     | yes      | no      | yes
         *
         * GetKeyedServices(AnyKey)  | yes    | no       | no      | no
         * GetKeyedService(AnyKey)   | throw  | throw    | throw   | throw
         *
         * GetKeyedServices(key)     | yes    | no       | no      | no
         * GetKeyedService(key)      | yes    | no       | yes     | no
         *
         * Summary:
         * - A null key is the same as unkeyed. This allows the KeyServices APIs to support both keyed and unkeyed.
         * - AnyKey is a special case of Keyed.
         * - AnyKey registrations are not returned with GetKeyedServices(AnyKey) and GetKeyedService(AnyKey) always throws.
         * - For IEnumerable, the ordering of the results are in registration order.
         * - For a singleton resolve, the last match wins.
         */

        // Unkeyed (which is really keyed by Type).
        Assert.Equal(
            new[] { service1, service2, nullkeyService1, nullkeyService2 },
            provider.GetServices<IService>());

        Assert.Equal(nullkeyService2, provider.GetService<IService>());

        // Null key.
        Assert.Equal(
            new[] { service1, service2, nullkeyService1, nullkeyService2 },
            provider.GetKeyedServices<IService>(null));

        Assert.Equal(nullkeyService2, provider.GetKeyedService<IService>(null));

        // AnyKey.
        Assert.Equal(
            new[] { keyedService1, keyedService2 },
            provider.GetKeyedServices<IService>(KeyedService.AnyKey));

        Assert.Throws<InvalidOperationException>(() => provider.GetKeyedService<IService>(KeyedService.AnyKey));

        // Keyed.
        Assert.Equal(
            new[] { keyedService1, keyedService2 },
            provider.GetKeyedServices<IService>("keyedService"));

        Assert.Equal(keyedService2, provider.GetKeyedService<IService>("keyedService"));
    }
}
