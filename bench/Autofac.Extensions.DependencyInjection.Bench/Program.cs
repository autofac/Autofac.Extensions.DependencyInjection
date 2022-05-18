// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using BenchmarkDotNet.Running;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    internal static class Program
    {
        internal static void Main(string[] args) =>
            new BenchmarkSwitcher(Benchmarks.All).Run(args, new BenchmarkConfig());
    }
}
