// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using MicrosoftServiceKey = Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute;

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

        services.AddTransient<IService, DefaultService>();

        services.AddKeyedTransient<IService, AlphaService>(AlphaKey);
        services.AddKeyedTransient<IService, BetaService>(BetaKey);
        services.AddKeyedTransient<IService, ServiceKeyAwareService>(ServiceKeyAwareKey);

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
        return _serviceProvider.GetRequiredService<IService>().Value;
    }

    [Benchmark]
    public int KeyedResolutionWithoutAttributes()
    {
        return _serviceProvider.GetRequiredKeyedService<IService>(AlphaKey).Value;
    }

    [Benchmark]
    public int KeyedResolutionWithServiceKeyAttribute()
    {
        return _serviceProvider.GetRequiredKeyedService<IService>(ServiceKeyAwareKey).Value;
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
        var result = _serviceProvider.GetKeyedServices<IService>(KeyedService.AnyKey);
        var total = 0;
        foreach (var service in result)
        {
            total += service.Value;
        }

        return total;
    }

    private interface IService
    {
        int Value { get; }
    }

    private sealed class DefaultService : IService
    {
        public int Value => 1;
    }

    private sealed class AlphaService : IService
    {
        public int Value => 2;
    }

    private sealed class BetaService : IService
    {
        public int Value => 3;
    }

    private sealed class FromKeyedServicesConsumer
    {
        private readonly IService _service;

        public FromKeyedServicesConsumer([FromKeyedServices(BetaKey)] IService service)
        {
            _service = service;
        }

        public int Value => _service.Value;
    }

    private sealed class CombinedConsumer
    {
        private readonly IService _fromKeyedService;
        private readonly object? _requestedKey;

        public CombinedConsumer(
            [FromKeyedServices(BetaKey)] IService fromKeyedService,
            [MicrosoftServiceKey] object? requestedKey)
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

    private sealed class ServiceKeyAwareService : IService
    {
        private readonly string _resolvedKey;

        public ServiceKeyAwareService([MicrosoftServiceKey] string resolvedKey)
        {
            _resolvedKey = resolvedKey;
        }

        public int Value => _resolvedKey.Length;
    }
}
