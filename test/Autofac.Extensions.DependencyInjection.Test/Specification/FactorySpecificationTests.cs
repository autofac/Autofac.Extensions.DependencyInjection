// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test;

public class FactorySpecificationTests : DependencyInjectionSpecificationTests
{
    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        var factory = new AutofacServiceProviderFactory();
        return factory.CreateServiceProvider(factory.CreateBuilder(serviceCollection));
    }
}
