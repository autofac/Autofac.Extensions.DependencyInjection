using System;
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
