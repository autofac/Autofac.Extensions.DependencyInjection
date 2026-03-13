// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.Reflection;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Middleware that supports keyed service compatibility.
/// </summary>
internal class KeyedServiceMiddleware : IResolveMiddleware
{
    private readonly bool _addFromKeyedServiceParameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedServiceMiddleware"/> class.
    /// </summary>
    /// <param name="addFromKeyedServiceParameter">Whether or not the from-keyed-service parameter should be added.</param>
    public KeyedServiceMiddleware(bool addFromKeyedServiceParameter)
    {
        _addFromKeyedServiceParameter = addFromKeyedServiceParameter;
    }

    /// <summary>
    /// Gets a single instance of this middleware that does not add the keyed services parameter.
    /// </summary>
    public static KeyedServiceMiddleware InstanceWithoutFromKeyedServicesParameter { get; } = new(addFromKeyedServiceParameter: false);

    /// <summary>
    /// Gets a single instance of this middleware that adds the keyed services parameter.
    /// </summary>
    public static KeyedServiceMiddleware InstanceWithFromKeyedServicesParameter { get; } = new(addFromKeyedServiceParameter: true);

    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.Activation;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        Parameter? microsoftServiceKeyParameter = null;
        Parameter? fromKeyedServicesParameter = null;

        var keyedService = context.Service as Autofac.Core.KeyedService;
        object? inheritedServiceKey = null;

        if (keyedService is not null)
        {
            if (Autofac.Core.KeyedService.IsAnyKey(keyedService.ServiceKey))
            {
                context.Parameters.TryGetKeyedServiceKey<object>(out inheritedServiceKey);
            }
            else
            {
                inheritedServiceKey = keyedService.ServiceKey;
            }
        }

        var effectiveServiceKey = inheritedServiceKey;

        if (keyedService is not null &&
            effectiveServiceKey is not null &&
            !Autofac.Core.KeyedService.IsAnyKey(effectiveServiceKey))
        {
            microsoftServiceKeyParameter = new MicrosoftServiceKeyParameter(effectiveServiceKey);
        }

        if (_addFromKeyedServiceParameter)
        {
            // [FromKeyedServices("key")] - Specifies a keyed service
            // for injection into a constructor. This is similar to the
            // Autofac [KeyFilter] attribute.
            fromKeyedServicesParameter = new FromKeyedServicesParameter(inheritedServiceKey);
        }

        if (microsoftServiceKeyParameter is not null || fromKeyedServicesParameter is not null)
        {
            context.ChangeParameters(AppendParameters(context.Parameters, microsoftServiceKeyParameter, fromKeyedServicesParameter));
        }

        next(context);
    }

    private static List<Parameter> AppendParameters(IEnumerable<Parameter> original, Parameter? first, Parameter? second)
    {
        // Build a concrete list rather than using yield return, which would
        // allocate a compiler-generated state machine on every call.
        var list = new List<Parameter>(original is ICollection<Parameter> col ? col.Count + 2 : 4);
        list.AddRange(original);

        if (first is not null)
        {
            list.Add(first);
        }

        if (second is not null)
        {
            list.Add(second);
        }

        return list;
    }

    /// <summary>
    /// Caches the presence of relevant attributes on parameters to avoid repeated reflection calls.
    /// </summary>
    private static class ParameterAttributeCache
    {
        private static readonly ParameterAttributeReflectionCache ReflectionCache = new();

        static ParameterAttributeCache()
        {
            ReflectionCacheSet.Shared.RegisterExternalCache(ReflectionCache);
        }

        /// <summary>
        /// Determines whether the parameter has the <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/> defined.
        /// </summary>
        /// <param name="parameter">The parameter to check.</param>
        /// <returns>
        /// <see langword="true"/> if the parameter has the attribute; otherwise, <see langword="false"/>.
        /// </returns>
        public static bool HasMicrosoftServiceKey(ParameterInfo parameter)
        {
            return ReflectionCache.GetOrAddMicrosoftServiceKeyAttributePresence(parameter);
        }

        /// <summary>
        /// Gets the <see cref="FromKeyedServicesAttribute"/> defined on the parameter, if any.
        /// </summary>
        /// <param name="parameter">The parameter to check.</param>
        /// <returns>
        /// The <see cref="FromKeyedServicesAttribute"/> if defined; otherwise, <see langword="null"/>.
        /// </returns>
        public static FromKeyedServicesAttribute? GetFromKeyedServicesAttribute(ParameterInfo parameter)
        {
            return ReflectionCache.GetOrAddFromKeyedServicesAttribute(parameter);
        }

        [ExcludeFromCodeCoverage]
        private sealed class ParameterAttributeReflectionCache : IReflectionCache
        {
            private readonly ConcurrentDictionary<ParameterInfo, bool> _microsoftServiceKeyAttributePresence = new();
            private readonly ConcurrentDictionary<ParameterInfo, FromKeyedServicesAttribute?> _fromKeyedServicesAttributes = new();

            public ReflectionCacheUsage Usage => ReflectionCacheUsage.Resolution;

            public bool GetOrAddMicrosoftServiceKeyAttributePresence(ParameterInfo parameter)
            {
                return _microsoftServiceKeyAttributePresence.GetOrAdd(
                    parameter,
                    static p => Attribute.IsDefined(p, typeof(Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute), inherit: true));
            }

            public FromKeyedServicesAttribute? GetOrAddFromKeyedServicesAttribute(ParameterInfo parameter)
            {
                return _fromKeyedServicesAttributes.GetOrAdd(
                    parameter,
                    static p => p.GetCustomAttribute<FromKeyedServicesAttribute>(inherit: true));
            }

            public void Clear()
            {
                _microsoftServiceKeyAttributePresence.Clear();
                _fromKeyedServicesAttributes.Clear();
            }

            public void Clear(ReflectionCacheClearPredicate clearPredicate)
            {
                if (clearPredicate is null)
                {
                    throw new ArgumentNullException(nameof(clearPredicate));
                }

                foreach (var parameter in _microsoftServiceKeyAttributePresence.Keys)
                {
                    if (Matches(clearPredicate, parameter))
                    {
                        _microsoftServiceKeyAttributePresence.TryRemove(parameter, out _);
                    }
                }

                foreach (var parameter in _fromKeyedServicesAttributes.Keys)
                {
                    if (Matches(clearPredicate, parameter))
                    {
                        _fromKeyedServicesAttributes.TryRemove(parameter, out _);
                    }
                }
            }

            private static bool Matches(ReflectionCacheClearPredicate clearPredicate, ParameterInfo parameter)
            {
                var member = parameter.Member;
                var memberAssembly = member.Module.Assembly;
                var parameterAssembly = parameter.ParameterType.Assembly;

                if (ReferenceEquals(memberAssembly, parameterAssembly))
                {
                    return clearPredicate(member, new[] { memberAssembly });
                }

                return clearPredicate(member, new[] { memberAssembly, parameterAssembly });
            }
        }
    }

    /// <summary>
    /// A <see cref="Parameter"/> that supplies the service key for parameters
    /// marked with <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// Uses a field instead of a closure to avoid delegate/closure allocations.
    /// </summary>
    private sealed class MicrosoftServiceKeyParameter : Parameter
    {
        private readonly object _serviceKey;

        public MicrosoftServiceKeyParameter(object serviceKey)
        {
            _serviceKey = serviceKey;
        }

        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(true)] out Func<object?>? valueProvider)
        {
            if (ParameterAttributeCache.HasMicrosoftServiceKey(pi))
            {
                var key = _serviceKey;
                valueProvider = () => KeyTypeManipulation.ChangeToCompatibleType(key, pi.ParameterType, pi);
                return true;
            }

            valueProvider = null;
            return false;
        }
    }

    /// <summary>
    /// A <see cref="Parameter"/> that supplies keyed service dependencies for parameters
    /// marked with <see cref="FromKeyedServicesAttribute"/>.
    /// Uses a field instead of a closure to avoid delegate/closure allocations.
    /// </summary>
    private sealed class FromKeyedServicesParameter : Parameter
    {
        private readonly object? _requestedServiceKey;

        public FromKeyedServicesParameter(object? requestedServiceKey)
        {
            _requestedServiceKey = requestedServiceKey;
        }

        public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(true)] out Func<object?>? valueProvider)
        {
            var filter = ParameterAttributeCache.GetFromKeyedServicesAttribute(pi);
            if (filter is not null)
            {
                var key = _requestedServiceKey;
                valueProvider = () => filter.ResolveParameter(pi, context, key);
                return true;
            }

            valueProvider = null;
            return false;
        }
    }
}
