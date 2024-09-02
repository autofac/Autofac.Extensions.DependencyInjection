# Autofac.Extensions.DependencyInjection

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](https://autofac.readthedocs.io/en/latest/glossary.html)*.

[![Build status](https://ci.appveyor.com/api/projects/status/1mhkjcqr1ug80lra/branch/develop?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-extensions-dependencyinjection/branch/develop) [![codecov](https://codecov.io/gh/Autofac/Autofac.Extensions.DependencyInjection/branch/develop/graph/badge.svg)](https://codecov.io/gh/Autofac/Autofac.Extensions.DependencyInjection)

Please file issues and pull requests for this package in this repository rather than in the Autofac core repo.

- [Documentation - .NET Core Integration](https://autofac.readthedocs.io/en/latest/integration/netcore.html)
- [Documentation - ASP.NET Core Integration](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html)
- [NuGet](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection)
- [Contributing](https://autofac.readthedocs.io/en/latest/contributors.html)
- [Open in Visual Studio Code](https://open.vscode.dev/autofac/Autofac.Extensions.DependencyInjection)

## Get Started in ASP.NET Core

This quick start shows how to use the `IServiceProviderFactory{T}` integration that ASP.NET Core supports to help automatically build the root service provider for you. If you want more manual control, [check out the documentation for examples](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html).

- Reference the `Autofac.Extensions.DependencyInjection` package from NuGet.
- In your `Program.Main` method, where you configure the `HostBuilder`, call `UseAutofac` to hook Autofac into the startup pipeline.
- In the `ConfigureServices` method of your `Startup` class register things into the `IServiceCollection` using extension methods provided by other libraries.
- In the `ConfigureContainer` method of your `Startup` class register things directly into an Autofac `ContainerBuilder`.

The `IServiceProvider` will automatically be created for you, so there's nothing you have to do but *register things*.

```C#
public class Program
{
  public static async Task Main(string[] args)
  {
    // The service provider factory used here allows for
    // ConfigureContainer to be supported in Startup with
    // a strongly-typed ContainerBuilder.
    var host = Host.CreateDefaultBuilder(args)
      .UseServiceProviderFactory(new AutofacServiceProviderFactory())
      .ConfigureWebHostDefaults(webHostBuilder => {
        webHostBuilder
          .UseContentRoot(Directory.GetCurrentDirectory())
          .UseIISIntegration()
          .UseStartup<Startup>()
      })
      .Build();

    await host.RunAsync();
  }
}

public class Startup
{
  public Startup(IWebHostEnvironment env)
  {
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
    this.Configuration = builder.Build();
  }

  public IConfiguration Configuration { get; private set; }

  // ConfigureServices is where you register dependencies. This gets
  // called by the runtime before the ConfigureContainer method, below.
  public void ConfigureServices(IServiceCollection services)
  {
    // Add services to the collection. Don't build or return
    // any IServiceProvider or the ConfigureContainer method
    // won't get called.
    services.AddOptions();
  }

  // ConfigureContainer is where you can register things directly
  // with Autofac. This runs after ConfigureServices so the things
  // here will override registrations made in ConfigureServices.
  // Don't build the container; that gets done for you. If you
  // need a reference to the container, you need to use the
  // "Without ConfigureContainer" mechanism shown later.
  public void ConfigureContainer(ContainerBuilder builder)
  {
      builder.RegisterModule(new AutofacModule());
  }

  // Configure is where you add middleware. This is called after
  // ConfigureContainer. You can use IApplicationBuilder.ApplicationServices
  // here if you need to resolve things from the container.
  public void Configure(
    IApplicationBuilder app,
    ILoggerFactory loggerFactory)
  {
      loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();
      app.UseMvc();
  }
}
```

Our [ASP.NET Core](https://autofac.readthedocs.io/en/latest/integration/aspnetcore.html) integration documentation contains more information about using Autofac with ASP.NET Core.

## Get Help

**Need help with Autofac?** We have [a documentation site](https://autofac.readthedocs.io/) as well as [API documentation](https://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](https://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).
