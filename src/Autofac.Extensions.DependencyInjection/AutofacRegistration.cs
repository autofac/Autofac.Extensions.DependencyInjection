// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extension methods for registering ASP.NET Core dependencies with Autofac.
/// </summary>
public static class AutofacRegistration
{
    /// <summary>
    /// Populates the Autofac container builder with the set of registered service descriptors
    /// and makes <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>
    /// available in the container.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ContainerBuilder"/> into which the registrations should be made.
    /// </param>
    /// <param name="descriptors">
    /// The set of service descriptors to register in the container.
    /// </param>
    public static void Populate(
        this ContainerBuilder builder,
        IEnumerable<ServiceDescriptor> descriptors)
    {
        Populate(builder, descriptors, null);
    }

    /// <summary>
    /// Populates the Autofac container builder with the set of registered service descriptors
    /// and makes <see cref="IServiceProvider"/> and <see cref="IServiceScopeFactory"/>
    /// available in the container. Using this overload is incompatible with the ASP.NET Core
    /// support for <see cref="IServiceProviderFactory{TContainerBuilder}"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ContainerBuilder"/> into which the registrations should be made.
    /// </param>
    /// <param name="descriptors">
    /// The set of service descriptors to register in the container.
    /// </param>
    /// <param name="lifetimeScopeTagForSingletons">
    /// If provided and not <see langword="null"/> then all registrations with lifetime <see cref="ServiceLifetime.Singleton" /> are registered
    /// using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope" />
    /// with provided <paramref name="lifetimeScopeTagForSingletons"/>
    /// instead of using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.SingleInstance"/>.
    /// </param>
    /// <remarks>
    /// <para>
    /// Specifying a <paramref name="lifetimeScopeTagForSingletons"/> addresses a specific case where you have
    /// an application that uses Autofac but where you need to isolate a set of services in a child scope. For example,
    /// if you have a large application that self-hosts ASP.NET Core items, you may want to isolate the ASP.NET
    /// Core registrations in a child lifetime scope so they don't show up for the rest of the application.
    /// This overload allows that. Note it is the developer's responsibility to execute this and create an
    /// <see cref="AutofacServiceProvider"/> using the child lifetime scope.
    /// </para>
    /// </remarks>
    public static void Populate(
        this ContainerBuilder builder,
        IEnumerable<ServiceDescriptor> descriptors,
        object? lifetimeScopeTagForSingletons)
    {
        if (descriptors == null)
        {
            throw new ArgumentNullException(nameof(descriptors));
        }

        builder.RegisterType<AutofacServiceProvider>()
               .As<IServiceProvider>()
               .As<IServiceProviderIsService>()
               .ExternallyOwned();

        // Issue #83: IServiceScopeFactory must be a singleton and scopes must be flat, not hierarchical.
        builder
            .RegisterType<AutofacServiceScopeFactory>()
            .As<IServiceScopeFactory>()
            .SingleInstance();

        Register(builder, descriptors, lifetimeScopeTagForSingletons);
    }

    /// <summary>
    /// Configures the exposed service type on a service registration.
    /// </summary>
    /// <typeparam name="TActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The object registration style.</typeparam>
    /// <param name="registrationBuilder">The registration being built.</param>
    /// <param name="descriptor">The service descriptor with service type and key information.</param>
    /// <returns>
    /// The <paramref name="registrationBuilder" />, configured with the proper service type,
    /// and available for additional configuration.
    /// </returns>
    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> ConfigureServiceType<TActivatorData, TRegistrationStyle>(
        this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder,
        ServiceDescriptor descriptor)
    {
        if (descriptor.IsKeyedService)
        {
            if (descriptor.ServiceKey is null)
            {
                // TODO: Localize this message.
                throw new NotSupportedException("Null service keys are not supported.");
            }

            registrationBuilder.Keyed(descriptor.ServiceKey, descriptor.ServiceType);
        }
        else
        {
            registrationBuilder.As(descriptor.ServiceType);
        }

        return registrationBuilder;
    }

    /// <summary>
    /// Configures the lifecycle on a service registration.
    /// </summary>
    /// <typeparam name="TActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The object registration style.</typeparam>
    /// <param name="registrationBuilder">The registration being built.</param>
    /// <param name="lifecycleKind">The lifecycle specified on the service registration.</param>
    /// <param name="lifetimeScopeTagForSingleton">
    /// If not <see langword="null"/> then all registrations with lifetime <see cref="ServiceLifetime.Singleton" /> are registered
    /// using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope" />
    /// with provided <paramref name="lifetimeScopeTagForSingleton"/>
    /// instead of using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.SingleInstance"/>.
    /// </param>
    /// <returns>
    /// The <paramref name="registrationBuilder" />, configured with the proper lifetime scope,
    /// and available for additional configuration.
    /// </returns>
    private static IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> ConfigureLifecycle<TActivatorData, TRegistrationStyle>(
        this IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder,
        ServiceLifetime lifecycleKind,
        object? lifetimeScopeTagForSingleton)
    {
        switch (lifecycleKind)
        {
            case ServiceLifetime.Singleton:
                if (lifetimeScopeTagForSingleton == null)
                {
                    registrationBuilder.SingleInstance();
                }
                else
                {
                    registrationBuilder.InstancePerMatchingLifetimeScope(lifetimeScopeTagForSingleton);
                }

                break;
            case ServiceLifetime.Scoped:
                registrationBuilder.InstancePerLifetimeScope();
                break;
            case ServiceLifetime.Transient:
                registrationBuilder.InstancePerDependency();
                break;
        }

        return registrationBuilder;
    }

    /// <summary>
    /// Applies attribute-based filtering on constructor dependencies for use with M.E.DI attributes.
    /// </summary>
    /// <typeparam name="TLimit">The type of the registration limit.</typeparam>
    /// <typeparam name="TReflectionActivatorData">Activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">Registration style type.</typeparam>
    /// <param name="builder">The registration builder containing registration data.</param>
    /// <returns>Registration builder allowing the registration to be configured.</returns>
    public static IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle>
        WithMicrosoftAttributeFiltering<TLimit, TReflectionActivatorData, TRegistrationStyle>(
            this IRegistrationBuilder<TLimit, TReflectionActivatorData, TRegistrationStyle> builder)
        where TReflectionActivatorData : ReflectionActivatorData
    {
        return builder.WithParameter(
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).FirstOrDefault();
                return filter is not null && filter.CanResolveParameter(p, c);
            },
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).First();
                return filter.ResolveParameter(p, c);
            });
    }

    /// <summary>
    /// Populates the Autofac container builder with the set of registered service descriptors.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="ContainerBuilder"/> into which the registrations should be made.
    /// </param>
    /// <param name="descriptors">
    /// The set of service descriptors to register in the container.
    /// </param>
    /// <param name="lifetimeScopeTagForSingletons">
    /// If not <see langword="null"/> then all registrations with lifetime <see cref="ServiceLifetime.Singleton" /> are registered
    /// using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.InstancePerMatchingLifetimeScope" />
    /// with provided <paramref name="lifetimeScopeTagForSingletons"/>
    /// instead of using <see cref="IRegistrationBuilder{TLimit,TActivatorData,TRegistrationStyle}.SingleInstance"/>.
    /// </param>
    [SuppressMessage("CA2000", "CA2000", Justification = "Registrations created here are disposed when the built container is disposed.")]
    private static void Register(
        ContainerBuilder builder,
        IEnumerable<ServiceDescriptor> descriptors,
        object? lifetimeScopeTagForSingletons)
    {
        foreach (var descriptor in descriptors)
        {
            // TODO: Update to register keyed services.
            // Check to see if descriptor.IsKeyedService. There are now mirrored
            // properties for keyed vs. un-keyed - KeyedImplementationType vs.
            // ImplementationType, KeyedServiceType vs. ServiceType. If you
            // access the wrong one, ServiceDescriptor throws an exception.
            //
            // Registration for a delegate also passes the key along with the
            // delegate, so the KeyedImplementationFactory vs.
            // ImplementationFactory method signatures are different.
            var implementationType = descriptor.NormalizedImplementationType();
            if (implementationType != null)
            {
                // Test if the an open generic type is being registered
                var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                if (serviceTypeInfo.IsGenericTypeDefinition)
                {
                    builder
                        .RegisterGeneric(implementationType)
                        .ConfigureServiceType(descriptor)
                        .ConfigureLifecycle(descriptor.Lifetime, lifetimeScopeTagForSingletons)
                        .WithMicrosoftAttributeFiltering();
                }
                else
                {
                    builder
                        .RegisterType(implementationType)
                        .ConfigureServiceType(descriptor)
                        .ConfigureLifecycle(descriptor.Lifetime, lifetimeScopeTagForSingletons)
                        .WithMicrosoftAttributeFiltering();
                }

                continue;
            }

            if (descriptor.ImplementationFactory != null)
            {
                // TODO: Unsure what to do about the delegate for keyed services right now.
                var registration = RegistrationBuilder.ForDelegate(descriptor.ServiceType, (context, parameters) =>
                    {
                        var serviceProvider = context.Resolve<IServiceProvider>();
                        return descriptor.ImplementationFactory(serviceProvider);
                    })
                    .ConfigureLifecycle(descriptor.Lifetime, lifetimeScopeTagForSingletons)
                    .CreateRegistration();

                builder.RegisterComponent(registration);
                continue;
            }

            // It's not a type or factory, so it must be an instance.
            builder
                .RegisterInstance(descriptor.NormalizedImplementationInstance()!)
                .ConfigureServiceType(descriptor)
                .ConfigureLifecycle(descriptor.Lifetime, null);
        }
    }
}
