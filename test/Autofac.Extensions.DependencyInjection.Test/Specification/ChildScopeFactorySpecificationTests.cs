// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

public class ChildScopeFactorySpecificationTests : DependencyInjectionSpecificationTests
{
#if NET6_0_OR_GREATER
    public override bool SupportsIServiceProviderIsService => true;
#endif

    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        var container = new ContainerBuilder().Build();
        var rootScope = container.BeginLifetimeScope();
        var factory = new AutofacChildLifetimeScopeServiceProviderFactory(() => rootScope);
        return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
    }
}
