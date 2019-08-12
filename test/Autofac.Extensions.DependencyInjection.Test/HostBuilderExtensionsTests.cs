using Microsoft.Extensions.DependencyInjection;
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
    }
}
