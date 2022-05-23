// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Specification;

namespace Autofac.Extensions.DependencyInjection.Test;

public class BuilderSpecificationTests : DependencyInjectionSpecificationTests
{
    public override bool SupportsIServiceProviderIsService => true;

    protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
    {
        var builder = new ContainerBuilder();
        builder.Populate(serviceCollection);
        var container = builder.Build();
        return container.Resolve<IServiceProvider>();
    }
}
