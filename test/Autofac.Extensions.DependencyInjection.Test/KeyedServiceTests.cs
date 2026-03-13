// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using MicrosoftServiceKey = Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute;

namespace Autofac.Extensions.DependencyInjection.Test;

public class KeyedServiceTests
{
    [Fact]
    public void CollectionKeyedResolutionWithAnyKeyOnNestedParameters()
    {
        var services = new ServiceCollection();

        // The key should be injected when AnyKey is used.
        services.AddKeyedTransient<IService, ServiceKeyAwareService>("a");
        services.AddKeyedTransient<IService, ServiceKeyAwareService>("b");

        var builder = new ContainerBuilder();
        builder.Populate(services);

        var container = builder.Build();
        using var serviceProvider = new AutofacServiceProvider(container);

        var result = string.Join("", serviceProvider.GetKeyedServices<IService>(KeyedService.AnyKey).Select(s => s.Value));
        Assert.Equal("ab", result);
    }

    [Fact]
    public void KeyedResolutionWithFromKeyedServicesAndNormalDependency()
    {
        var services = new ServiceCollection();
        services.AddTransient<INormalDependency, NormalDependency>();
        services.AddKeyedTransient<IKeyedDependency, KeyedDependency>("dep");
        services.AddKeyedTransient<IMixedService, MixedService>("svc");

        var builder = new ContainerBuilder();
        builder.Populate(services);

        var container = builder.Build();
        using var serviceProvider = new AutofacServiceProvider(container);

        var resolved = serviceProvider.GetRequiredKeyedService<IMixedService>("svc");

        Assert.Equal("dep:normal", resolved.Value);
    }

    private interface IService
    {
        string Value { get; }
    }

    private sealed class ServiceKeyAwareService : IService
    {
        private readonly string _resolvedKey;

        public ServiceKeyAwareService([MicrosoftServiceKey] string resolvedKey)
        {
            _resolvedKey = resolvedKey;
        }

        public string Value => _resolvedKey;
    }

    private interface IKeyedDependency
    {
        string Value { get; }
    }

    private interface INormalDependency
    {
        string Value { get; }
    }

    private interface IMixedService
    {
        string Value { get; }
    }

    private sealed class KeyedDependency : IKeyedDependency
    {
        public string Value => "dep";
    }

    private sealed class NormalDependency : INormalDependency
    {
        public string Value => "normal";
    }

    private sealed class MixedService : IMixedService
    {
        private readonly IKeyedDependency _keyedDependency;
        private readonly INormalDependency _normalDependency;

        public MixedService([FromKeyedServices("dep")] IKeyedDependency keyedDependency, INormalDependency normalDependency)
        {
            _keyedDependency = keyedDependency;
            _normalDependency = normalDependency;
        }

        public string Value => $"{_keyedDependency.Value}:{_normalDependency.Value}";
    }
}
