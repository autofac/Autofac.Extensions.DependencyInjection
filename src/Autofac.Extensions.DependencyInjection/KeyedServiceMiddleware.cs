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
        List<Parameter>? appendedParameters = null;

        var keyedService = context.Service as Autofac.Core.KeyedService;
        object? inheritedServiceKey = null;

        if (keyedService is not null)
        {
            if (Autofac.Core.KeyedService.IsAnyKey(keyedService.ServiceKey))
            {
                inheritedServiceKey = TryGetRequestedServiceKey(context.Parameters);
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
            AddParameter(ref appendedParameters, CreateMicrosoftServiceKeyParameter(effectiveServiceKey));
        }

        if (_addFromKeyedServiceParameter)
        {
            // [FromKeyedServices("key")] - Specifies a keyed service
            // for injection into a constructor. This is similar to the
            // Autofac [KeyFilter] attribute.
            AddParameter(ref appendedParameters, CreateFromKeyedServicesParameter(inheritedServiceKey));
        }

        if (appendedParameters is not null)
        {
            context.ChangeParameters(AppendParameters(context.Parameters, appendedParameters));
        }

        next(context);
    }

    private static void AddParameter(ref List<Parameter>? appendedParameters, Parameter parameter)
    {
        appendedParameters ??= new List<Parameter>();
        appendedParameters.Add(parameter);
    }

    private static IEnumerable<Parameter> AppendParameters(IEnumerable<Parameter> original, List<Parameter> appended)
    {
        foreach (var existing in original)
        {
            yield return existing;
        }

        for (var i = 0; i < appended.Count; i++)
        {
            yield return appended[i];
        }
    }

    private static ResolvedParameter CreateMicrosoftServiceKeyParameter(object serviceKey)
    {
        return new ResolvedParameter(
            (p, c) => ParameterAttributeCache.HasMicrosoftServiceKey(p),
            (p, c) => KeyTypeManipulation.ChangeToCompatibleType(serviceKey, p.ParameterType, p));
    }

    private static ResolvedParameter CreateFromKeyedServicesParameter(object? requestedServiceKey)
    {
        return new ResolvedParameter(
            (p, c) => ParameterAttributeCache.HasFromKeyedServicesAttribute(p),
            (p, c) =>
            {
                var filter = ParameterAttributeCache.GetFromKeyedServicesAttribute(p)!;
                return filter.ResolveParameter(p, c, requestedServiceKey);
            });
    }

    private static object? TryGetRequestedServiceKey(IEnumerable<Parameter> parameters)
    {
        try
        {
            return parameters.KeyedServiceKey<object?>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }

    private static class ParameterAttributeCache
    {
        private static readonly ConcurrentDictionary<ParameterInfo, bool> MicrosoftServiceKeyAttributePresence = new();
        private static readonly ConcurrentDictionary<ParameterInfo, FromKeyedServicesAttribute?> FromKeyedServicesAttributes = new();

        public static bool HasMicrosoftServiceKey(ParameterInfo parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return MicrosoftServiceKeyAttributePresence.GetOrAdd(
                parameter,
                static p => Attribute.IsDefined(p, typeof(Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute), inherit: true));
        }

        public static bool HasFromKeyedServicesAttribute(ParameterInfo parameter)
        {
            return GetFromKeyedServicesAttribute(parameter) is not null;
        }

        public static FromKeyedServicesAttribute? GetFromKeyedServicesAttribute(ParameterInfo parameter)
        {
            if (parameter is null)
            {
                throw new ArgumentNullException(nameof(parameter));
            }

            return FromKeyedServicesAttributes.GetOrAdd(
                parameter,
                static p => p.GetCustomAttribute<FromKeyedServicesAttribute>(inherit: true));
        }
    }
}
