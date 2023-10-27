// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Handles fallback for an individual component registered with <see cref="Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey"/>.
/// </summary>
public class AnyKeyRegistrationSource : IRegistrationSource
{
    /// <inheritdoc/>
    public bool IsAdapterForIndividualComponents => false;

    /// <summary>
    /// Provides a registration for a keyed service that's registered with <see cref="Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey"/>. Allows the registration to respond to any keyed service request, not just one with a specific key.
    /// </summary>
    /// <param name="service">The service that was requested.</param>
    /// <param name="registrationAccessor">A function that will return existing registrations for a service.</param>
    /// <returns>Registrations providing the service.</returns>
    public IEnumerable<IComponentRegistration> RegistrationsFor(Service service, Func<Service, IEnumerable<ServiceRegistration>> registrationAccessor)
    {
        if (registrationAccessor == null)
        {
            throw new ArgumentNullException(nameof(registrationAccessor));
        }

        if (service is not KeyedService keyedService)
        {
            // It's not a keyed service, bail. If it is, we ALWAYS have to look for an AnyKey service.
            return Enumerable.Empty<IComponentRegistration>();
        }

        var anyKeyService = new KeyedService(Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey, keyedService.ServiceType);
        var anyKeyRegistrationSet = registrationAccessor(anyKeyService).ToArray();
        if (anyKeyRegistrationSet.Length == 0)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        var anyKeyRegistration = anyKeyRegistrationSet[0];

        // TODO: This gets messed up with singletons and lifetime scope sharing - we need to make sure a singleton registration doesn't get used multiple times in a single activation request.
        // The provided instance of 'Microsoft.Extensions.DependencyInjection.Specification.Fakes.FakeService' has already been used in an activation request. Did you combine a provided instance with non-root/single-instance lifetime/sharing?
        var registrationMappedToOriginalService = new ComponentRegistration(
            Guid.NewGuid(),
            anyKeyRegistration.Registration.Activator,
            anyKeyRegistration.Registration.Lifetime,
            anyKeyRegistration.Registration.Sharing,
            anyKeyRegistration.Registration.Ownership,
            new[] { service },
            anyKeyRegistration.Registration.Metadata);

        return new[] { registrationMappedToOriginalService };
    }
}
