// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test.Specification;

public class FactoryAssumedBehaviorTests : AssumedBehaviorTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        var factory = new AutofacServiceProviderFactory();
        return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
    }
}
