// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test;

public sealed class ServiceProviderExtensionsTests
{
    [Fact]
    public void GetAutofacRootReturnsLifetimeScope()
    {
        var containerBuilder = new ContainerBuilder();
        containerBuilder.Populate(new ServiceCollection());

        var container = containerBuilder.Build();
        var serviceProvider = container.Resolve<IServiceProvider>();

        Assert.NotNull(serviceProvider.GetAutofacRoot());
    }

    [Fact]
    public void GetAutofacRootServiceProviderNotAutofacServiceProviderThrows()
        => Assert.Throws<InvalidOperationException>(() =>
            new ServiceCollection().BuildServiceProvider().GetAutofacRoot());
}
