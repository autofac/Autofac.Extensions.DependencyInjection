// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Hosting;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    public class AutofacWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup>
        where TStartup : class
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            ArgumentNullException.ThrowIfNull(builder);
            builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());

            return base.CreateHost(builder);
        }
    }
}
