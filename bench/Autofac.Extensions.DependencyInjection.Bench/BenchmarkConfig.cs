using System;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Jobs;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    internal class BenchmarkConfig : ManualConfig
    {
        internal BenchmarkConfig()
        {
            Add(DefaultConfig.Instance);

            var rootFolder = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.LastIndexOf("bin", StringComparison.OrdinalIgnoreCase));
            var runFolder = DateTime.Now.ToString("u").Replace(' ', '_').Replace(':', '-');
            ArtifactsPath = $"{rootFolder}\\BenchmarkDotNet.Artifacts\\{runFolder}";

            AddDiagnoser(MemoryDiagnoser.Default);
        }
    }
}
