// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Autofac.Core;
using Autofac.Core.Activators.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Caches lookups for <see cref="FromKeyedServicesAttribute"/> usage on constructor parameters.
/// </summary>
internal static class FromKeyedServicesUsageCache
{
    private static readonly FromKeyedServicesUsageReflectionCache ReflectionCache = new();

    static FromKeyedServicesUsageCache()
    {
        ReflectionCacheSet.Shared.RegisterExternalCache(ReflectionCache);
    }

    /// <summary>
    /// Determines whether the resolve pipeline needs <see cref="KeyedServiceMiddleware"/> for the given activator.
    /// </summary>
    /// <param name="activator">The reflection activator being inspected.</param>
    /// <returns><see langword="true"/> when a constructor parameter uses <see cref="FromKeyedServicesAttribute"/>; otherwise <see langword="false"/>.</returns>
    public static bool RequiresFromKeyedServicesMiddleware(ReflectionActivator activator)
    {
        if (activator is null)
        {
            throw new ArgumentNullException(nameof(activator));
        }

        var constructorFinder = activator.ConstructorFinder;
        var cacheKey = new CacheKey(activator.LimitType, constructorFinder.GetType());

#if NETSTANDARD2_0
        if (ReflectionCache.TryGet(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        var computed = ScanConstructors(constructorFinder, activator.LimitType);
        return ReflectionCache.GetOrAdd(cacheKey, computed);
#else
        return ReflectionCache.GetOrAdd(
            cacheKey,
            static (_, state) => ScanConstructors(state.ConstructorFinder, state.LimitType),
            (ConstructorFinder: constructorFinder, activator.LimitType));
#endif
    }

    private static bool ScanConstructors(IConstructorFinder constructorFinder, Type limitType)
    {
        var constructors = constructorFinder.FindConstructors(limitType);

        foreach (var constructor in constructors)
        {
            foreach (var parameter in constructor.GetParameters())
            {
                if (parameter.IsDefined(typeof(FromKeyedServicesAttribute), inherit: true))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private readonly struct CacheKey : IEquatable<CacheKey>
    {
        public CacheKey(Type implementationType, Type constructorFinderType)
        {
            ImplementationType = implementationType ?? throw new ArgumentNullException(nameof(implementationType));
            ConstructorFinderType = constructorFinderType ?? throw new ArgumentNullException(nameof(constructorFinderType));
        }

        private Type ImplementationType { get; }

        private Type ConstructorFinderType { get; }

        public bool Matches(ReflectionCacheClearPredicate clearPredicate)
        {
            var implementationAssemblies = new[] { ImplementationType.Assembly };
            if (clearPredicate(ImplementationType, implementationAssemblies))
            {
                return true;
            }

            var constructorFinderAssemblies = new[] { ConstructorFinderType.Assembly };
            return clearPredicate(ConstructorFinderType, constructorFinderAssemblies);
        }

        public bool Equals(CacheKey other)
        {
            return ImplementationType == other.ImplementationType &&
                   ConstructorFinderType == other.ConstructorFinderType;
        }

        [ExcludeFromCodeCoverage]
        public override bool Equals(object? obj)
        {
            return obj is CacheKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (ImplementationType.GetHashCode() * 397) ^ ConstructorFinderType.GetHashCode();
            }
        }
    }

    private sealed class FromKeyedServicesUsageReflectionCache : IReflectionCache
    {
        private readonly ConcurrentDictionary<CacheKey, bool> _cache = new();

        public ReflectionCacheUsage Usage => ReflectionCacheUsage.Registration;

        public bool TryGet(CacheKey key, out bool result)
        {
            return _cache.TryGetValue(key, out result);
        }

        public bool GetOrAdd(CacheKey key, bool value)
        {
            return _cache.GetOrAdd(key, value);
        }

        public bool GetOrAdd(CacheKey key, Func<CacheKey, (IConstructorFinder ConstructorFinder, Type LimitType), bool> valueFactory, (IConstructorFinder ConstructorFinder, Type LimitType) state)
        {
#if NETSTANDARD2_0
            return _cache.GetOrAdd(key, cacheKey => valueFactory(cacheKey, state));
#else
            return _cache.GetOrAdd(key, valueFactory, state);
#endif
        }

        public void Clear()
        {
            _cache.Clear();
        }

        public void Clear(ReflectionCacheClearPredicate clearPredicate)
        {
            if (clearPredicate is null)
            {
                throw new ArgumentNullException(nameof(clearPredicate));
            }

            foreach (var key in _cache.Keys)
            {
                if (key.Matches(clearPredicate))
                {
                    _cache.TryRemove(key, out _);
                }
            }
        }
    }
}
