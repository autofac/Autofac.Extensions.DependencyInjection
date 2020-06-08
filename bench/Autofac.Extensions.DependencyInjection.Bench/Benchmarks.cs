using System;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    public static class Benchmarks
    {
        public static Type[] All { get; } = new[]
        {
            typeof(RequestBenchmark)
        };
    }
}
