// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using AutofacKeyedService = Autofac.Core.KeyedService;
using ServiceKeyAttributeAlias = Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute;

namespace Autofac.Extensions.DependencyInjection.Bench;

[MemoryDiagnoser]
[SuppressMessage("CA1001", "CA1001", Justification = "Benchmark disposal happens in GlobalCleanup.")]
public class KeyedResolutionBenchmark
{
    private const string AlphaKey = "alpha";
    private const string BetaKey = "beta";
    private const string ServiceKeyAwareKey = "gamma";
    private const string CombinedKey = "combined";

    private AutofacServiceProvider _serviceProvider = null!;
    private ILifetimeScope _rootScope = null!;

    [GlobalSetup]
    public void Setup()
    {
        var services = new ServiceCollection();

        services.AddTransient<IRegularService, DefaultRegularService>();

        services.AddKeyedTransient<IRegularService, AlphaRegularService>(AlphaKey);
        services.AddKeyedTransient<IRegularService, BetaRegularService>(BetaKey);
        services.AddKeyedTransient<IRegularService, ServiceKeyAwareRegularService>(ServiceKeyAwareKey);

        services.AddTransient<FromKeyedServicesConsumer>();
        services.AddKeyedTransient<CombinedConsumer>(CombinedKey);

        var builder = new ContainerBuilder();
        builder.Populate(services);

        _rootScope = builder.Build();
        _serviceProvider = new AutofacServiceProvider(_rootScope);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _serviceProvider.Dispose();
    }

    [Benchmark(Baseline = true)]
    public int StandardTypedResolution()
    {
        return _serviceProvider.GetRequiredService<IRegularService>().Value;
    }

    [Benchmark]
    public int KeyedResolutionWithoutAttributes()
    {
        return _serviceProvider.GetRequiredKeyedService<IRegularService>(AlphaKey).Value;
    }

    [Benchmark]
    public int KeyedResolutionWithServiceKeyAttribute()
    {
        return _serviceProvider.GetRequiredKeyedService<IRegularService>(ServiceKeyAwareKey).Value;
    }

    [Benchmark]
    public int KeyedResolutionWithFromKeyedServicesAttribute()
    {
        return _serviceProvider.GetRequiredService<FromKeyedServicesConsumer>().Value;
    }

    [Benchmark]
    public int KeyedResolutionWithBothAttributes()
    {
        return _serviceProvider.GetRequiredKeyedService<CombinedConsumer>(CombinedKey).Value;
    }

    [Benchmark]
    public int KeyedResolutionWithAnyKey()
    {
        var result = (IEnumerable<IRegularService>?)_serviceProvider.GetKeyedService(
            typeof(IEnumerable<IRegularService>),
            AutofacKeyedService.AnyKey);

        if (result is null)
        {
            throw new InvalidOperationException("Unable to resolve AnyKey enumerable.");
        }

        var total = 0;
        foreach (var service in result)
        {
            total += service.Value;
        }

        return total;
    }

    private interface IRegularService
    {
        int Value { get; }
    }

    private sealed class DefaultRegularService : IRegularService
    {
        public int Value => 1;
    }

    private sealed class AlphaRegularService : IRegularService
    {
        public int Value => 2;
    }

    private sealed class BetaRegularService : IRegularService
    {
        public int Value => 3;
    }

    private sealed class FromKeyedServicesConsumer
    {
        private readonly IRegularService _service;

        public FromKeyedServicesConsumer([FromKeyedServices(BetaKey)] IRegularService service)
        {
            _service = service;
        }

        public int Value => _service.Value;
    }

    private sealed class CombinedConsumer
    {
        private readonly IRegularService _fromKeyedService;
        private readonly object? _requestedKey;

        public CombinedConsumer(
            [FromKeyedServices(BetaKey)] IRegularService fromKeyedService,
            [ServiceKeyAttributeAlias] object? requestedKey)
        {
            _fromKeyedService = fromKeyedService;
            _requestedKey = requestedKey;
        }

        public int Value
        {
            get
            {
                var keyValue = _requestedKey is string s ? s.Length : 0;
                return _fromKeyedService.Value + keyValue;
            }
        }
    }

    private sealed class ServiceKeyAwareRegularService : IRegularService
    {
        private readonly object? _resolvedKey;

        public ServiceKeyAwareRegularService([ServiceKeyAttributeAlias] object? resolvedKey)
        {
            _resolvedKey = resolvedKey;
        }

        public int Value => _resolvedKey is string s ? s.Length : 0;
    }
}
