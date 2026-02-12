// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using Autofac.Core.Activators.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Caches lookups for <see cref="FromKeyedServicesAttribute"/> usage on constructor parameters.
/// </summary>
internal static class FromKeyedServicesUsageCache
{
    private static readonly ConcurrentDictionary<CacheKey, bool> Cache = new();

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
        if (constructorFinder is null)
        {
            throw new InvalidOperationException("The reflection activator must provide a constructor finder.");
        }

        var cacheKey = new CacheKey(activator.LimitType, constructorFinder.GetType());

#if NETSTANDARD2_0
        if (Cache.TryGetValue(cacheKey, out var cachedResult))
        {
            return cachedResult;
        }

        var computed = ScanConstructors(constructorFinder, activator.LimitType);
        return Cache.GetOrAdd(cacheKey, computed);
#else
        return Cache.GetOrAdd(
            cacheKey,
            static (_, state) => ScanConstructors(state.ConstructorFinder, state.LimitType),
            (ConstructorFinder: constructorFinder, activator.LimitType));
#endif
    }

    private static bool ScanConstructors(IConstructorFinder constructorFinder, Type limitType)
    {
        if (constructorFinder is null)
        {
            throw new ArgumentNullException(nameof(constructorFinder));
        }

        if (limitType is null)
        {
            throw new ArgumentNullException(nameof(limitType));
        }

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

        public bool Equals(CacheKey other)
        {
            return ImplementationType == other.ImplementationType &&
                   ConstructorFinderType == other.ConstructorFinderType;
        }

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
}
