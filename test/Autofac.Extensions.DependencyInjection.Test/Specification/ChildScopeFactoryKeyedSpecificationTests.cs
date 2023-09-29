// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

public class ChildScopeFactoryKeyedSpecificationTests : KeyedDependencyInjectionSpecificationTests
{
    public override bool SupportsIServiceProviderIsKeyedService => true;

    protected override IServiceProvider CreateServiceProvider(IServiceCollection collection)
    {
        var container = new ContainerBuilder().Build();
        var rootScope = container.BeginLifetimeScope();
        var factory = new AutofacChildLifetimeScopeServiceProviderFactory(() => rootScope);
        return factory.CreateServiceProvider(factory.CreateBuilder(collection));
    }
}
