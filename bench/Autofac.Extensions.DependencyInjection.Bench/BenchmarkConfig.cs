// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace Autofac.Extensions.DependencyInjection.Bench;

internal class BenchmarkConfig : ManualConfig
{
    internal BenchmarkConfig()
    {
        Add(DefaultConfig.Instance);

        AddJob(Job.InProcess);

        var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase));
        var runFolder = DateTime.Now.ToString("u").Replace(' ', '_').Replace(':', '-');
        ArtifactsPath = Path.Join(rootFolder, "BenchmarkDotNet.Artifacts", runFolder);

        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
