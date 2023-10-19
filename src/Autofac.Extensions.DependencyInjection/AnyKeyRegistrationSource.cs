// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Autofac.Core;

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

        if (service is not KeyedService keyedService || registrationAccessor(service).Any())
        {
            // It's not a keyed service or there's already a service specifically registered for that key.
            return Enumerable.Empty<IComponentRegistration>();
        }

        var anyKeyService = new KeyedService(Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey, keyedService.ServiceType);
        var anyKeyRegistration = registrationAccessor(anyKeyService).ToArray();
        if (anyKeyRegistration.Length == 0)
        {
            return Enumerable.Empty<IComponentRegistration>();
        }

        // TODO: Something up here where I can't just return an existing registration. When DefaultRegisteredServicesTracker.TryGetServiceRegistration calls ServiceRegistrationInfo.TryGetRegistration, it isn't found. Something is not initialized right.
        return new[] { anyKeyRegistration[0].Registration };
    }
}
