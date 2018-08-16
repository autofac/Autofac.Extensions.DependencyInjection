using System;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public class MicrosoftAssumedBehaviorTests : AssumedBehaviorTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            return serviceCollection.BuildServiceProvider();
        }
    }
}
