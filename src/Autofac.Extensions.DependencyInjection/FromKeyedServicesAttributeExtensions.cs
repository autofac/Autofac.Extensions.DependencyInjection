// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extensions for working with <see cref="FromKeyedServicesAttribute"/>.
/// </summary>
internal static class FromKeyedServicesAttributeExtensions
{
    /// <summary>
    /// Resolves a constructor parameter based on keyed service requirements.
    /// </summary>
    /// <param name="attribute">The attribute marking the keyed service dependency in a constructor.</param>
    /// <param name="parameter">The specific parameter being resolved that is marked with this attribute.</param>
    /// <param name="context">The component context under which the parameter is being resolved.</param>
    /// <returns>
    /// The instance of the object that should be used for the parameter value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parameter" /> or <paramref name="context" /> is <see langword="null" />.
    /// </exception>
    public static object? ResolveParameter(this FromKeyedServicesAttribute attribute, ParameterInfo parameter, IComponentContext context)
    {
        // Adapter for FromKeyedServicesAttribute to work like Autofac.Features.AttributeFilters.KeyFilterAttribute.
        object? instance;
        if (attribute.Key is null)
        {
            // No key in the attribute means resolve by type.
            context.TryResolve(parameter.ParameterType, out instance);
            return instance;
        }

        context.TryResolveKeyed(attribute.Key, parameter.ParameterType, out instance);
        return instance;
    }

    /// <summary>
    /// Checks a constructor parameter can be resolved based on keyed service requirements.
    /// </summary>
    /// <param name="attribute">The attribute marking the keyed service dependency in a constructor.</param>
    /// <param name="parameter">The specific parameter being resolved that is marked with this attribute.</param>
    /// <param name="context">The component context under which the parameter is being resolved.</param>
    /// <returns>true if parameter can be resolved; otherwise, false.</returns>
    public static bool CanResolveParameter(this FromKeyedServicesAttribute attribute, ParameterInfo parameter, IComponentContext context)
    {
        // Adapter for FromKeyedServicesAttribute to work like Autofac.Features.AttributeFilters.KeyFilterAttribute.
        if (attribute.Key is null)
        {
            // No key in the attribute means resolve by type.
            return context.ComponentRegistry.IsRegistered(new Autofac.Core.TypedService(parameter.ParameterType));
        }

        return context.ComponentRegistry.IsRegistered(new Autofac.Core.KeyedService(attribute.Key, parameter.ParameterType));
    }
}
