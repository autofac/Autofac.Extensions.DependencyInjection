using System;
using Autofac.Builder;
using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class AutofacFlatScopesServiceProviderFactoryTests
    {
        [Fact]
        public void CreateBuilderReturnsNewInstance()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(builder);
        }

        [Fact]
        public void CreateBuilderExecutesConfigurationActionWhenProvided()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory(config => config.Register(c => "Foo"));

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }

        [Fact]
        public void CreateBuilderAllowsForNullConfigurationAction()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();

            var builder = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(builder);
        }

        [Fact]
        public void CreateBuilderReturnsInstanceWithServicesPopulated()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();
            var services = new ServiceCollection();
            services.AddTransient<object>();

            var builder = factory.CreateBuilder(services);

            Assert.True(builder.Build().IsRegistered<object>());
        }

        [Fact]
        public void CreateServiceProviderBuildsServiceProviderUsingContainerBuilder()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();
            var services = new ServiceCollection().AddTransient<object>();
            var builder = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(builder);

            Assert.NotNull(serviceProvider.GetService(typeof(object)));
        }

        [Fact]
        public void CreateServiceProviderThrowsWhenProvidedNullContainerBuilder()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();

            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceProvider(null));

            Assert.Equal("containerBuilder", exception.ParamName);
        }

        [Fact]
        public void CreateServiceProviderReturnsAutofacServiceProvider()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();

            var serviceProvider = factory.CreateServiceProvider(new ContainerBuilder());

            Assert.IsType<AutofacServiceProvider>(serviceProvider);
        }

        [Fact]
        public void CreateServiceProviderUsesDefaultContainerBuildOptionsWhenNotProvided()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();
            var services = new ServiceCollection().AddSingleton("Foo");
            var builder = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(builder);

            Assert.NotNull(serviceProvider.GetService<Lazy<string>>());
        }

        [Fact]
        public void CreateServiceProviderUsesContainerBuildOptionsWhenProvided()
        {
            var options = ContainerBuildOptions.ExcludeDefaultModules;
            var factory = new AutofacFlatScopesServiceProviderFactory(options);
            var services = new ServiceCollection().AddSingleton("Foo");
            var builder = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(builder);

            Assert.Null(serviceProvider.GetService<Lazy<string>>());
        }

        [Fact]
        public void CanProvideContainerBuildOptionsAndConfigurationAction()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory(
                ContainerBuildOptions.ExcludeDefaultModules,
                config => config.Register(c => "Foo"));
            var builder = factory.CreateBuilder(new ServiceCollection());

            var serviceProvider = factory.CreateServiceProvider(builder);

            Assert.NotNull(serviceProvider.GetService<string>());
            Assert.Null(serviceProvider.GetService<Lazy<string>>());
        }

        [Fact]
        public void CreateScopeShouldBeSilingScope()
        {
            var factory = new AutofacFlatScopesServiceProviderFactory();

            var builder = factory.CreateBuilder(new ServiceCollection());

            var serviceProvider = factory.CreateServiceProvider(builder);

            using var subScope = serviceProvider.CreateScope();

            Assert.Same(serviceProvider.GetRequiredService<ILifetimeScope>(), ((ISharingLifetimeScope)subScope.ServiceProvider.GetAutofacRoot()).ParentLifetimeScope);

            using var subScope2 = subScope.ServiceProvider.CreateScope();

            Assert.Same(subScope.ServiceProvider.GetRequiredService<IServiceScopeFactory>(), subScope2.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

            Assert.Same(((ISharingLifetimeScope)subScope.ServiceProvider.GetAutofacRoot()).ParentLifetimeScope, ((ISharingLifetimeScope)subScope2.ServiceProvider.GetAutofacRoot()).ParentLifetimeScope);
        }
    }
}
