// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Concurrent;
using Autofac.Core;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Extensions for working with types.
/// </summary>
internal static class TypeExtensions
{
    private static readonly TypeExtensionsReflectionCache ReflectionCache = new();

    static TypeExtensions()
    {
        ReflectionCacheSet.Shared.RegisterExternalCache(ReflectionCache);
    }

    /// <summary>
    /// Checks a type to determine if it is some kind of collection or enumerable.
    /// </summary>
    /// <param name="serviceType">
    /// The type to check.
    /// </param>
    /// <returns><see langword="true"/> if the type is a collection or enumerable; otherwise, <see langword="false"/>.</returns>
    internal static bool IsCollection(this Type serviceType)
    {
        return ReflectionCache.GetOrAddCollectionType(
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

    [ExcludeFromCodeCoverage]
    private sealed class TypeExtensionsReflectionCache : IReflectionCache
    {
        private readonly ConcurrentDictionary<Type, bool> _collectionTypeCache = new();

        public ReflectionCacheUsage Usage => ReflectionCacheUsage.Resolution;

        public bool GetOrAddCollectionType(Type serviceType, Func<Type, bool> valueFactory)
        {
            return _collectionTypeCache.GetOrAdd(serviceType, valueFactory);
        }

        public void Clear()
        {
            _collectionTypeCache.Clear();
        }

        public void Clear(ReflectionCacheClearPredicate clearPredicate)
        {
            if (clearPredicate is null)
            {
                throw new ArgumentNullException(nameof(clearPredicate));
            }

            foreach (var type in _collectionTypeCache.Keys)
            {
                if (clearPredicate(type, new[] { type.Assembly }))
                {
                    _collectionTypeCache.TryRemove(type, out _);
                }
            }
        }
    }
}
