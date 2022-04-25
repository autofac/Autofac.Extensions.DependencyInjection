// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Running;

namespace Autofac.Extensions.DependencyInjection.Bench;

public class Harness
{
    [Fact]
    public void Request() => RunBenchmark<RequestBenchmark>();

    /// <remarks>
    /// This method is used to enforce that benchmark types are added to <see cref="Benchmarks.All"/>
    /// so that they can be used directly from the command line in <see cref="Program.Main"/> as well.
    /// </remarks>
    private static void RunBenchmark<TBenchmark>()
    {
        var targetType = typeof(TBenchmark);
        var benchmarkType = Benchmarks.All.Single(type => type == targetType);
        BenchmarkRunner.Run(benchmarkType, new BenchmarkConfig());
    }
}
