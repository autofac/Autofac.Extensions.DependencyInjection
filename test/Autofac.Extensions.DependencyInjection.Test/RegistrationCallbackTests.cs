using Autofac.Builder;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test;

public class RegistrationCallbackTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegisterType(bool isKeyedService)
    {
        var registrationCallback = new RegistrationCallback();
        var builder = new ContainerBuilder();
        var descriptor = new ServiceDescriptor(typeof(IService), isKeyedService ? KeyedService.AnyKey : null, typeof(Service), ServiceLifetime.Singleton);
        builder.Populate([descriptor], registrationCallback, null);

        var registrationBuilder = Assert.Single(registrationCallback.RegistrationBuilders);

        Assert.IsAssignableFrom<IRegistrationBuilder<object, ConcreteReflectionActivatorData, SingleRegistrationStyle>>(registrationBuilder);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegisterGenericType(bool isKeyedService)
    {
        var registrationCallback = new RegistrationCallback();
        var builder = new ContainerBuilder();
        var descriptor = new ServiceDescriptor(typeof(IService<>), isKeyedService ? KeyedService.AnyKey : null, typeof(Service<>), ServiceLifetime.Singleton);
        builder.Populate([descriptor], registrationCallback, null);

        var registrationBuilder = Assert.Single(registrationCallback.RegistrationBuilders);

        Assert.IsAssignableFrom<IRegistrationBuilder<object, ReflectionActivatorData, DynamicRegistrationStyle>>(registrationBuilder);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegisterDelegate(bool isKeyedService)
    {
        var registrationCallback = new RegistrationCallback();
        var builder = new ContainerBuilder();
        var descriptor = isKeyedService
            ? new ServiceDescriptor(typeof(IService), KeyedService.AnyKey, (_, _) => new Service(), ServiceLifetime.Singleton)
            : new ServiceDescriptor(typeof(IService), _ => new Service(), ServiceLifetime.Singleton);
        builder.Populate([descriptor], registrationCallback, null);

        var registrationBuilder = Assert.Single(registrationCallback.RegistrationBuilders);

        Assert.IsAssignableFrom<IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>>(registrationBuilder);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RegisterInstance(bool isKeyedService)
    {
        var registrationCallback = new RegistrationCallback();
        var builder = new ContainerBuilder();
        var descriptor = isKeyedService
            ? new ServiceDescriptor(typeof(IService), KeyedService.AnyKey, new Service())
            : new ServiceDescriptor(typeof(IService), new Service());
        builder.Populate([descriptor], registrationCallback, null);

        var registrationBuilder = Assert.Single(registrationCallback.RegistrationBuilders);

        Assert.IsAssignableFrom<IRegistrationBuilder<object, SimpleActivatorData, SingleRegistrationStyle>>(registrationBuilder);
    }

    [Fact]
    public void RegisterInterceptor()
    {
        var builder = new ContainerBuilder();
        builder.RegisterType<CallLogger>().SingleInstance();

        var descriptor = new ServiceDescriptor(typeof(IService<>), typeof(Service<>), ServiceLifetime.Singleton);
        builder.Populate([descriptor], new RegistrationCallback(), null);
        var container = builder.Build();

        var service = container.Resolve<IService<int>>();
        var callLogger = container.Resolve<CallLogger>();

        Assert.False(callLogger.Called);

        service.Test();

        Assert.True(callLogger.Called);
    }

    private class RegistrationCallback : IRegistrationCallback
    {
        public List<object> RegistrationBuilders { get; } = new();

        public void OnRegister<TActivatorData, TRegistrationStyle>(
            IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder)
        {
            RegistrationBuilders.Add(registrationBuilder);

            registrationBuilder.EnableInterfaceInterceptors();
        }
    }

    private class Service : IService
    {
    }

    private class Service<T>(CallLogger logger) : IService<T>
    {
        public CallLogger Logger { get; } = logger;

        public virtual void Test()
        {
        }
    }

    private interface IService
    {
    }

    [Intercept(typeof(CallLogger))]
    public interface IService<T>
    {
        CallLogger Logger { get; }

        void Test();
    }

    public class CallLogger : IInterceptor
    {
        public bool Called { get; set; }

        public void Intercept(IInvocation invocation)
        {
            Called = true;

            invocation.Proceed();
        }
    }
}
