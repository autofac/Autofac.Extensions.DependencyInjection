// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Autofac.Extensions.DependencyInjection.Bench;

public abstract class BenchWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
    where TStartup : class
{
    private static readonly Lazy<string> _contentRoot = new(ResolveContentRoot, LazyThreadSafetyMode.ExecutionAndPublication);

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.ConfigureWebHost(builder);
        builder.UseContentRoot(_contentRoot.Value);
    }

    private static string ResolveContentRoot()
        => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
}
