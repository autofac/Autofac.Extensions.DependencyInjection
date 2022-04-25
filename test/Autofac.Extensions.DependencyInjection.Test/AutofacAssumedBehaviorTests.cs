// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class AutofacAssumedBehaviorTests : AssumedBehaviorTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var builder = new ContainerBuilder();

            builder.Populate(serviceCollection);

            var container = builder.Build();
            return container.Resolve<IServiceProvider>();
        }
    }
}
