// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddAutofacReturnsProvidedServiceCollection()
        {
            var collection = new ServiceCollection();

            var returnedCollection = collection.AddAutofac();

            Assert.Same(collection, returnedCollection);
        }

        [Fact]
        public void AddAutofacAddsAutofacServiceProviderFactoryToCollection()
        {
            var collection = new ServiceCollection();

            collection.AddAutofac();

            var service = collection.FirstOrDefault(s => s.ServiceType == typeof(IServiceProviderFactory<ContainerBuilder>));
            Assert.NotNull(service);
            Assert.Equal(ServiceLifetime.Singleton, service.Lifetime);
        }

        [Fact]
        public void AddAutofacPassesConfigurationActionToAutofacServiceProviderFactory()
        {
            var collection = new ServiceCollection();

            collection.AddAutofac(config => config.Register(c => "Foo"));

            var serviceProvider = collection.BuildServiceProvider();
            var factory = (IServiceProviderFactory<ContainerBuilder>)serviceProvider.GetService(typeof(IServiceProviderFactory<ContainerBuilder>));
            var builder = factory.CreateBuilder(collection);
            Assert.Equal("Foo", builder.Build().Resolve<string>());
        }
    }
}
