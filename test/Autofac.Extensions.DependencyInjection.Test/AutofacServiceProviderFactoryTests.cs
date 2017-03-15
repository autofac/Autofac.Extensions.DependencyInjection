using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class AutofacServiceProviderFactoryTests
    {
        [Fact]
        public void CreateBuilderReturnsNewInstance()
        {
            var factory = new AutofacServiceProviderFactory();

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(builder);
        }

        [Fact]
        public void CreateBuilderExecutesConfigurationActionWhenProvided()
        {
            var factory = new AutofacServiceProviderFactory(config => config.Register(c => "Foo"));

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }

        [Fact]
        public void CreateBuilderAllowsForNullConfigurationAction()
        {
            var factory = new AutofacServiceProviderFactory();

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(builder);
        }

        [Fact]
        public void CreateBuilderReturnsInstanceWithServicesPopulated()
        {
            var factory = new AutofacServiceProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<object>();

            var builder = factory.CreateBuilder(services);

            Assert.True(builder.Build().IsRegistered<object>());
        }

        [Fact]
        public void CreateServiceProviderBuildsServiceProviderUsingContainerBuilder()
        {
            var factory = new AutofacServiceProviderFactory();
            var services = new ServiceCollection().AddTransient<object>();
            var builder = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(builder);

            Assert.NotNull(serviceProvider.GetService(typeof(object)));
        }

        [Fact]
        public void CreateServiceProviderThrowsWhenProvidedNullContainerBuilder()
        {
            var factory = new AutofacServiceProviderFactory();

            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceProvider(null));

            Assert.Equal("containerBuilder", exception.ParamName);
        }

        [Fact]
        public void CreateServiceProviderReturnsAutofacServiceProvider()
        {
            var factory = new AutofacServiceProviderFactory();

            var serviceProvider = factory.CreateServiceProvider(new ContainerBuilder());

            Assert.IsType<AutofacServiceProvider>(serviceProvider);
        }
    }
}
