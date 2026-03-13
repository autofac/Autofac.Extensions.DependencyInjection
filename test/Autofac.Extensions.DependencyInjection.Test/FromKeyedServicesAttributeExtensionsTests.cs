// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test;

public class FromKeyedServicesAttributeExtensionsTests
{
    [Fact]
    public void ResolveParameterExplicitKey()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.Explicit));
        var parameter = GetParameter(nameof(ParameterTargets.Explicit));

        var value = attribute.ResolveParameter(parameter, container, parentServiceKey: "parent");

        Assert.IsType<KeyedService>(value);
    }

    [Fact]
    public void ResolveParameterInheritedAnyKeyUsedForCollection()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.InheritedCollection));
        var parameter = GetParameter(nameof(ParameterTargets.InheritedCollection));

        var value = attribute.ResolveParameter(parameter, container, parentServiceKey: Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey);

        var services = Assert.IsAssignableFrom<IEnumerable<IService>>(value);
        Assert.NotEmpty(services);
    }

    [Fact]
    public void ResolveParameterInheritedAnyKeyUsedForNonCollection()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.Inherited));
        var parameter = GetParameter(nameof(ParameterTargets.Inherited));

        Assert.Throws<DependencyResolutionException>(() =>
            attribute.ResolveParameter(parameter, container, parentServiceKey: Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey));
    }

    [Fact]
    public void ResolveParameterInheritedKey()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.Inherited));
        var parameter = GetParameter(nameof(ParameterTargets.Inherited));

        var value = attribute.ResolveParameter(parameter, container, parentServiceKey: "inherited");

        Assert.IsType<InheritedKeyedService>(value);
    }

    [Fact]
    public void ResolveParameterInheritedKeyIsNull()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.InheritedWithDefault));
        var parameter = GetParameter(nameof(ParameterTargets.InheritedWithDefault));

        var value = attribute.ResolveParameter(parameter, container, parentServiceKey: null);

        Assert.Equal("fallback", value);
    }

    [Fact]
    public void ResolveParameterKeyedServiceMissingAndNoDefault()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.ExplicitMissingNoDefault));
        var parameter = GetParameter(nameof(ParameterTargets.ExplicitMissingNoDefault));

        var ex = Assert.Throws<InvalidOperationException>(() => attribute.ResolveParameter(parameter, container, parentServiceKey: "missing"));

        Assert.Contains("using key", ex.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ResolveParameterNullAttribute()
    {
        using var container = BuildContainer();
        var parameter = GetParameter(nameof(ParameterTargets.Explicit));

        var ex = Assert.Throws<ArgumentNullException>(() =>
            FromKeyedServicesAttributeExtensions.ResolveParameter(null!, parameter, container, parentServiceKey: null));

        Assert.Equal("attribute", ex.ParamName);
    }

    [Fact]
    public void ResolveParameterNullContext()
    {
        var attribute = GetAttribute(nameof(ParameterTargets.Explicit));
        var parameter = GetParameter(nameof(ParameterTargets.Explicit));

        var ex = Assert.Throws<ArgumentNullException>(() =>
            attribute.ResolveParameter(parameter, null!, parentServiceKey: null));

        Assert.Equal("context", ex.ParamName);
    }

    [Fact]
    public void ResolveParameterNullParameter()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.Explicit));

        var ex = Assert.Throws<ArgumentNullException>(() =>
            attribute.ResolveParameter(null!, container, parentServiceKey: null));

        Assert.Equal("parameter", ex.ParamName);
    }

    [Fact]
    public void ResolveParameterResolveFails()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.ExplicitWithDefault));
        var parameter = GetParameter(nameof(ParameterTargets.ExplicitWithDefault));

        var value = attribute.ResolveParameter(parameter, container, parentServiceKey: null);

        Assert.Equal("fallback", value);
    }

    [Fact]
    public void ResolveParameterUnkeyedServiceMissingAndNoDefault()
    {
        using var container = BuildContainer();
        var attribute = GetAttribute(nameof(ParameterTargets.NullKeyNoDefault));
        var parameter = GetParameter(nameof(ParameterTargets.NullKeyNoDefault));

        var ex = Assert.Throws<InvalidOperationException>(() => attribute.ResolveParameter(parameter, container, parentServiceKey: null));

        Assert.Contains("Unable to resolve service for type", ex.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("using key", ex.Message, StringComparison.Ordinal);
    }

    private static IContainer BuildContainer()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<KeyedService>().Keyed<IService>("explicit");
        builder.RegisterType<InheritedKeyedService>().Keyed<IService>("inherited");
        return builder.Build();
    }

    private static FromKeyedServicesAttribute GetAttribute(string methodName)
    {
        return GetParameter(methodName).GetCustomAttribute<FromKeyedServicesAttribute>()!;
    }

    private static ParameterInfo GetParameter(string methodName)
    {
        return typeof(ParameterTargets).GetMethod(methodName)!.GetParameters()[0];
    }

    private interface IService
    {
    }

    private sealed class KeyedService : IService
    {
    }

    private sealed class InheritedKeyedService : IService
    {
    }

    private sealed class ParameterTargets
    {
        public void Explicit([FromKeyedServices("explicit")] IService service)
        {
        }

        public void Inherited([FromKeyedServices] IService service)
        {
        }

        public void InheritedCollection([FromKeyedServices] IEnumerable<IService> service)
        {
        }

        public void InheritedWithDefault([FromKeyedServices] string service = "fallback")
        {
        }

        public void ExplicitWithDefault([FromKeyedServices("missing")] string service = "fallback")
        {
        }

        public void ExplicitMissingNoDefault([FromKeyedServices("missing")] IService service)
        {
        }

        public void NullKeyNoDefault([FromKeyedServices(null)] string service)
        {
        }
    }
}
