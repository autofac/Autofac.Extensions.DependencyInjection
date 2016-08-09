# Autofac.Extensions.DependencyInjection

Autofac is an [IoC container](http://martinfowler.com/articles/injection.html) for Microsoft .NET. It manages the dependencies between classes so that **applications stay easy to change as they grow** in size and complexity. This is achieved by treating regular .NET classes as *[components](http://autofac.readthedocs.io/en/latest/glossary.html)*.

[![Build status](https://ci.appveyor.com/api/projects/status/1mhkjcqr1ug80lra/branch/develop?svg=true)](https://ci.appveyor.com/project/Autofac/autofac-extensions-dependencyinjection/branch/develop)

## Get Packages

You can get started with `Autofac.Extensions.DependencyInjection` by [grabbing the latest NuGet package](https://www.nuget.org/packages/Autofac.Extensions.DependencyInjection).

If you're feeling adventurous, [continuous integration builds are on MyGet](https://www.myget.org/gallery/autofac).

## Get Help

**Need help with Autofac?** We have [a documentation site](http://autofac.readthedocs.io/) as well as [API documentation](http://autofac.org/apidoc/). We're ready to answer your questions on [Stack Overflow](http://stackoverflow.com/questions/tagged/autofac) or check out the [discussion forum](https://groups.google.com/forum/#forum/autofac).

## Get Started

To take advantage of Autofac in your ASP.NET Core pipeline:

- Reference the `Autofac.Extensions.DependencyInjection` package from NuGet.
- In the `ConfigureServices` method of your `Startup` class...
  - Register services from the `IServiceCollection`.
  - Build your container.
  - Create an `AutofacServiceProvider` using the container and return it.
- In the `Configure` method of your `Startup` class, you can optionally register with the `IApplicationLifetime.ApplicationStopped` event to dispose of the container at app shutdown.

```C#
public class Startup
{
  public Startup(IHostingEnvironment env)
  {
    var builder = new ConfigurationBuilder()
        .SetBasePath(env.ContentRootPath)
        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
        .AddEnvironmentVariables();
    this.Configuration = builder.Build();
  }

  public IContainer ApplicationContainer { get; private set; }

  public IConfigurationRoot Configuration { get; private set; }

  // ConfigureServices is where you register dependencies. This gets
  // called by the runtime before the Configure method, below.
  public IServiceProvider ConfigureServices(IServiceCollection services)
  {
    // Add services to the collection.
    services.AddMvc();

    // Create the container builder.
    var builder = new ContainerBuilder();

    // Register dependencies, populate the services from
    // the collection, and build the container. If you want
    // to dispose of the container at the end of the app,
    // be sure to keep a reference to it as a property or field.
    builder.RegisterType<MyType>().As<IMyType>();
    builder.Populate(services);
    this.ApplicationContainer = builder.Build();

    // Create the IServiceProvider based on the container.
    return new AutofacServiceProvider(this.ApplicationContainer);
  }

  // Configure is where you add middleware. This is called after
  // ConfigureServices. You can use IApplicationBuilder.ApplicationServices
  // here if you need to resolve things from the container.
  public void Configure(
    IApplicationBuilder app,
    ILoggerFactory loggerFactory,
    IApplicationLifetime appLifetime)
  {
      loggerFactory.AddConsole(this.Configuration.GetSection("Logging"));
      loggerFactory.AddDebug();

      app.UseMvc();

      // If you want to dispose of resources that have been resolved in the
      // application container, register for the "ApplicationStopped" event.
      appLifetime.ApplicationStopped.Register(() => this.ApplicationContainer.Dispose());
  }
}
```

Our [ASP.NET Core](http://docs.autofac.org/en/latest/integration/aspnetcore.html) integration documentation contains more information about using Autofac with ASP.NET Core.

## Project

Autofac is licensed under the MIT license, so you can comfortably use it in commercial applications (we still love [contributions](http://autofac.readthedocs.io/en/latest/contributors.html) though).

## Contributing / Pull Requests

Refer to the [Readme for Autofac Developers](https://github.com/autofac/Autofac/blob/master/developers.md)
for setting up and building Autofac source. We also have a [contributors guide](http://autofac.readthedocs.io/en/latest/contributors.html) to help you get started.

