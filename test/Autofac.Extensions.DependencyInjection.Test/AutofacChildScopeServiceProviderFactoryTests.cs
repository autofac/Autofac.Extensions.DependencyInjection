using System;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class AutofacChildScopeServiceProviderFactoryTests
    {
        [Fact]
        public void CreateBuilderReturnsNewInstance()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(autofacChildScopeConfigurationAdapter);
        }

        [Fact]
        public void CreateBuilderExecutesConfigurationActionWhenProvided()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope, builder => builder.Register(c => "Foo"));

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(new ServiceCollection());

            var builder = new ContainerBuilder();

            foreach (var action in autofacChildScopeConfigurationAdapter.ChildScopeConfigurationActions)
            {
                action(builder);
            }

            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }

        [Fact]
        public void CreateBuilderAllowsForNullConfigurationAction()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(new ServiceCollection());

            Assert.NotNull(autofacChildScopeConfigurationAdapter);
        }

        [Fact]
        public void CreateBuilderReturnsInstanceWithServicesPopulated()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);
            var services = new ServiceCollection().AddTransient<object>();

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(services);

            var builder = new ContainerBuilder();

            foreach (var action in autofacChildScopeConfigurationAdapter.ChildScopeConfigurationActions)
            {
                action(builder);
            }

            Assert.True(builder.Build().IsRegistered<object>());
        }

        [Fact]
        public void CreateServiceProviderBuildsServiceProviderUsingAdapter()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);
            var services = new ServiceCollection().AddTransient<object>();
            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(services);

            var serviceProvider = factory.CreateServiceProvider(autofacChildScopeConfigurationAdapter);

            Assert.NotNull(serviceProvider.GetService(typeof(object)));
        }

        [Fact]
        public void CreateServiceProviderThrowsWhenProvidedNullAdapter()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);

            var exception = Assert.Throws<ArgumentNullException>(() => factory.CreateServiceProvider(null));

            Assert.Equal("autofacChildScopeConfigurationAdapter", exception.ParamName);
        }

        [Fact]
        public void CreateServiceProviderReturnsAutofacServiceProvider()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);

            var serviceProvider = factory.CreateServiceProvider(new AutofacChildScopeConfigurationAdapter());

            Assert.IsType<AutofacServiceProvider>(serviceProvider);
        }

        [Fact]
        public void CreateServiceProviderAddDepToServiceCollectionAndAddConfigurationTypesResolveable()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimeScope);

            var services = new ServiceCollection().AddTransient<DependencyOne>();

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(services);

            autofacChildScopeConfigurationAdapter.Add(builder => builder.RegisterType<DependencyTwo>());

            var serviceProvider = factory.CreateServiceProvider(autofacChildScopeConfigurationAdapter);

            serviceProvider.GetRequiredService<DependencyOne>();
            serviceProvider.GetRequiredService<DependencyTwo>();
        }

        [Fact]
        public void CreateServiceProviderAddDepToRootContainerResolveable()
        {
            var factory = new AutofacChildScopeServiceProviderFactory(GetRootLifetimescopeWithDependency<DependencyOne>(typeof(DependencyOne)));

            var autofacChildScopeConfigurationAdapter = factory.CreateBuilder(new ServiceCollection());

            var serviceProvider = factory.CreateServiceProvider(autofacChildScopeConfigurationAdapter);

            serviceProvider.GetRequiredService<DependencyOne>();
        }

        private static ILifetimeScope GetRootLifetimeScope() => new ContainerBuilder().Build();

        private static ILifetimeScope GetRootLifetimescopeWithDependency<TAs>(Type type)
        {
            var containerBuilder = new ContainerBuilder();

            containerBuilder
                .RegisterType(type)
                .As<TAs>();

            return containerBuilder.Build();
        }

        private class DependencyOne
        {
        }

        private class DependencyTwo
        {
        }
    }
}
