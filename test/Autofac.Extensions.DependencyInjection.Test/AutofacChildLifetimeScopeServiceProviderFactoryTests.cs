using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class AutofacChildLifetimeScopeServiceProviderFactoryTests
    {
        [Fact]
        public void CreateBuilderReturnsNewInstance()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);

            var configurationAdapter = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(configurationAdapter);
        }

        [Fact]
        public void CreateBuilderExecutesConfigurationActionWhenProvided()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope, b => b.Register(c => "Foo"));

            var configurationAdapter = factory.CreateBuilder(new ServiceCollection());

            var builder = new ContainerBuilder();

            foreach (var action in configurationAdapter.ConfigurationActions)
            {
                action(builder);
            }

            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }

        [Fact]
        public void CreateBuilderAllowsForNullConfigurationAction()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);

            var configurationAdapter = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(configurationAdapter);
        }

        [Fact]
        public void CreateBuilderReturnsInstanceWithServicesPopulated()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);
            var services = new ServiceCollection().AddTransient<object>();

            var configurationAdapter = factory.CreateBuilder(services);

            var builder = new ContainerBuilder();

            foreach (var action in configurationAdapter.ConfigurationActions)
            {
                action(builder);
            }

            Assert.True(builder.Build().IsRegistered<object>());
        }

        [Fact]
        public void CreateServiceProviderBuildsServiceProviderUsingAdapter()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);
            var services = new ServiceCollection().AddTransient<object>();
            var configurationAdapter = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(configurationAdapter);

            Assert.NotNull(serviceProvider.GetService(typeof(object)));
        }

        [Fact]
        public void CreateServiceProviderThrowsWhenProvidedNullAdapter()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);

            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceProvider(null));

            Assert.Equal("containerBuilder", exception.ParamName);
        }

        [Fact]
        public void CreateServiceProviderReturnsAutofacServiceProvider()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);

            var serviceProvider = factory.CreateServiceProvider(new AutofacChildLifetimeScopeConfigurationAdapter());

            Assert.IsType<AutofacServiceProvider>(serviceProvider);
        }

        [Fact]
        public void CreateServiceProviderAddDepToServiceCollectionAndAddConfigurationTypesResolveable()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScope);

            var services = new ServiceCollection().AddTransient<DependencyOne>();

            var configurationAdapter = factory.CreateBuilder(services);

            configurationAdapter.Add(builder => builder.RegisterType<DependencyTwo>());

            var serviceProvider = factory.CreateServiceProvider(configurationAdapter);

            serviceProvider.GetRequiredService<DependencyOne>();
            serviceProvider.GetRequiredService<DependencyTwo>();
        }

        [Fact]
        public void CreateServiceProviderAddDepToRootContainerResolveable()
        {
            var factory = new AutofacChildLifetimeScopeServiceProviderFactory(GetRootLifetimeScopeWithDependency<DependencyOne>(typeof(DependencyOne)));

            var configurationAdapter = factory.CreateBuilder(new ServiceCollection());

            var serviceProvider = factory.CreateServiceProvider(configurationAdapter);

            serviceProvider.GetRequiredService<DependencyOne>();
        }

        private static ILifetimeScope GetRootLifetimeScope() => new ContainerBuilder().Build();

        private static ILifetimeScope GetRootLifetimeScopeWithDependency<TAs>(Type type)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterType(type)
                .As<TAs>();

            return containerBuilder.Build();
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Instantiated via dependency injection.")]
        private class DependencyOne
        {
        }

        [SuppressMessage("CA1812", "CA1812", Justification = "Instantiated via dependency injection.")]
        private class DependencyTwo
        {
        }
    }
}
