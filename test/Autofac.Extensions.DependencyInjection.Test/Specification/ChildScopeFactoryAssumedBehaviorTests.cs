// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

public class ChildScopeFactoryAssumedBehaviorTests : AssumedBehaviorTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        var container = new ContainerBuilder().Build();
        var rootScope = container.BeginLifetimeScope();
        var factory = new AutofacChildLifetimeScopeServiceProviderFactory(() => rootScope);
        return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
    }
}
