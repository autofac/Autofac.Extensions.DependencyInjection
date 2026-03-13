// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;
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
    /// <param name="parentServiceKey">The key used for the containing resolve request.</param>
    /// <returns>
    /// The instance of the object that should be used for the parameter value.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="parameter" /> or <paramref name="context" /> is <see langword="null" />.
    /// </exception>
    public static object? ResolveParameter(this FromKeyedServicesAttribute attribute, ParameterInfo parameter, IComponentContext context, object? parentServiceKey)
    {
        if (attribute is null)
        {
            throw new ArgumentNullException(nameof(attribute));
        }

        if (parameter is null)
        {
            throw new ArgumentNullException(nameof(parameter));
        }

        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        return attribute.LookupMode switch
        {
            ServiceKeyLookupMode.ExplicitKey => ResolveKeyed(parameter, context, NormalizeKey(attribute.Key)),
            ServiceKeyLookupMode.InheritKey => ResolveKeyed(parameter, context, NormalizeKey(parentServiceKey)),
            _ => ResolveUnkeyed(parameter, context),
        };
    }

    private static object? ResolveUnkeyed(ParameterInfo parameter, IComponentContext context)
    {
        if (context.TryResolve(parameter.ParameterType, out var instance))
        {
            return instance;
        }

        return GetDefaultValueOrThrow(parameter, key: null);
    }

    private static object? ResolveKeyed(ParameterInfo parameter, IComponentContext context, object? key)
    {
        if (key is null)
        {
            return GetDefaultValueOrThrow(parameter, key);
        }

        if (context.TryResolveKeyed(key, parameter.ParameterType, out var instance))
        {
            return instance;
        }

        return GetDefaultValueOrThrow(parameter, key);
    }

    private static object? GetDefaultValueOrThrow(ParameterInfo parameter, object? key)
    {
        if (parameter.HasDefaultValue)
        {
            return parameter.DefaultValue;
        }

        if (key is null)
        {
            throw new InvalidOperationException(
                string.Format(
                    CultureInfo.CurrentCulture,
                    "Unable to resolve service for type '{0}'.",
                    parameter.ParameterType));
        }

        throw new InvalidOperationException(
            string.Format(
                CultureInfo.CurrentCulture,
                "Unable to resolve service for type '{0}' using key '{1}'.",
                parameter.ParameterType,
                key));
    }

    private static object? NormalizeKey(object? key)
    {
        if (key is null)
        {
            return null;
        }

        return ReferenceEquals(key, Microsoft.Extensions.DependencyInjection.KeyedService.AnyKey)
            ? Autofac.Core.KeyedService.AnyKey
            : key;
    }
}
