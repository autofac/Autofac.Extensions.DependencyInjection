// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Integration.Net7;

[SuppressMessage("CA1052", "CA1052", Justification = "Startup must not be a static class.")]
public class Startup
{
    public static void Configure(IApplicationBuilder app)
    {
        app.UseRouting()
           .UseEndpoints(endpoints => endpoints.MapControllers());
    }

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddMvc();
    }

    public static void ConfigureContainer(ContainerBuilder builder)
    {
        builder.RegisterType<DateProvider>().As<IDateProvider>();
    }
}
