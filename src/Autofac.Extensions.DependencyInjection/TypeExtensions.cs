// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extensions for working with types.
/// </summary>
internal static class TypeExtensions
{
    /// <summary>
    /// Checks a type to determine if it is some kind of collection or enumerable.
    /// </summary>
    /// <param name="serviceType">
    /// The type to check.
    /// </param>
    /// <returns><see langword="true"/> if the type is a collection or enumerable; otherwise, <see langword="false"/>.</returns>
    internal static bool IsCollection(this Type serviceType)
    {
        return IsGenericTypeDefinedBy(serviceType, typeof(IEnumerable<>)) ||
                serviceType.IsArray ||
                IsGenericListOrCollectionInterfaceType(serviceType);
    }

    private static bool IsGenericTypeDefinedBy(Type type, Type openGeneric)
    {
        return !type.ContainsGenericParameters
                && type.IsGenericType
                && type.GetGenericTypeDefinition() == openGeneric;
    }

    private static bool IsGenericListOrCollectionInterfaceType(Type type)
    {
        return IsGenericTypeDefinedBy(type, typeof(IList<>))
                   || IsGenericTypeDefinedBy(type, typeof(ICollection<>))
                   || IsGenericTypeDefinedBy(type, typeof(IReadOnlyCollection<>))
                   || IsGenericTypeDefinedBy(type, typeof(IReadOnlyList<>));
    }
}
