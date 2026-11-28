// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Bench;

/// <summary>
/// Measures converting a typical ASP.NET Core service collection into an Autofac container.
/// </summary>
public class PopulateBenchmark
{
    private ServiceCollection _services = null!;

    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceCollection();
        _services.AddLogging();
        _services.AddOptions();
        _services.AddControllersWithViews();
        _services.AddRazorPages();
        _services.AddHttpClient();
    }

    [Benchmark]
    public void PopulateAndBuild()
    {
        var builder = new ContainerBuilder();
        builder.Populate(_services);
        using var container = builder.Build();
    }
}
