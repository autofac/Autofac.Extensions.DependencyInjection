// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;

namespace Autofac.Extensions.DependencyInjection.Bench;

internal sealed class BenchmarkConfig : ManualConfig
{
    private const string BenchmarkArtifactsFolder = "BenchmarkDotNet.Artifacts";

    internal BenchmarkConfig()
    {
        Add(DefaultConfig.Instance);

        var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase));
        var runFolder = DateTime.Now.ToString("u").Replace(' ', '_').Replace(':', '-');
        ArtifactsPath = Path.Combine(rootFolder, BenchmarkArtifactsFolder, runFolder);

        AddDiagnoser(MemoryDiagnoser.Default);
    }
}
