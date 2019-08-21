using Microsoft.Extensions.Hosting;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class HostBuilderExtensionsTests
    {
        [Fact]
        public void UseAutofacAutofacServiceProviderResolveable()
        {
            var host = Host.CreateDefaultBuilder(null)
                .UseAutofac()
                .Build();

            Assert.IsAssignableFrom<AutofacServiceProvider>(host.Services);
        }

        [Fact]
        public void UseAutofacChildScopeFactoryWithDelegateAutofacServiceProviderResolveable()
        {
            var host = Host.CreateDefaultBuilder(null)
                .UseAutofacChildLifetimeScopeFactory(GetRootLifetimeScope)
                .Build();

            Assert.IsAssignableFrom<AutofacServiceProvider>(host.Services);
        }

        [Fact]
        public void UseAutofacChildScopeFactoryWithInstanceAutofacServiceProviderResolveable()
        {
            var rootLifetimeScope = GetRootLifetimeScope();

            var host = Host.CreateDefaultBuilder(null)
                .UseAutofacChildLifetimeScopeFactory(rootLifetimeScope)
                .Build();

            Assert.IsAssignableFrom<AutofacServiceProvider>(host.Services);
        }

        private static ILifetimeScope GetRootLifetimeScope() => new ContainerBuilder().Build();
    }
}
