// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Concurrent;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extensions for working with types.
/// </summary>
internal static class TypeExtensions
{
    private static readonly ConcurrentDictionary<Type, bool> CollectionTypeCache = new();

    /// <summary>
    /// Checks a type to determine if it is some kind of collection or enumerable.
    /// </summary>
    /// <param name="serviceType">
    /// The type to check.
    /// </param>
    /// <returns><see langword="true"/> if the type is a collection or enumerable; otherwise, <see langword="false"/>.</returns>
    internal static bool IsCollection(this Type serviceType)
    {
        return CollectionTypeCache.GetOrAdd(
            serviceType,
            static type =>
            {
                if (type.IsArray)
                {
                    return true;
                }

                if (!type.IsGenericType)
                {
                    return false;
                }

                return IsGenericTypeDefinedBy(type, typeof(IEnumerable<>)) ||
                       IsGenericListOrCollectionInterfaceType(type);
            });
    }

    private static bool IsGenericTypeDefinedBy(Type type, Type openGeneric)
    {
        return !type.ContainsGenericParameters &&
               type.IsGenericType &&
               type.GetGenericTypeDefinition() == openGeneric;
    }

    private static bool IsGenericListOrCollectionInterfaceType(Type type)
    {
        return IsGenericTypeDefinedBy(type, typeof(IList<>)) ||
               IsGenericTypeDefinedBy(type, typeof(ICollection<>)) ||
               IsGenericTypeDefinedBy(type, typeof(IReadOnlyCollection<>)) ||
               IsGenericTypeDefinedBy(type, typeof(IReadOnlyList<>));
    }
}
