// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        List<Parameter>? newParameters = null;

        var keyedService = context.Service as Autofac.Core.KeyedService;
        var effectiveServiceKey = keyedService?.ServiceKey;

        if (effectiveServiceKey is null || Autofac.Core.KeyedService.IsAnyKey(effectiveServiceKey))
        {
            var requestedKey = GetRequestedServiceKey(context.Parameters);
            if (requestedKey is not null)
            {
                effectiveServiceKey = requestedKey;
            }
        }

        if (keyedService is not null && effectiveServiceKey is not null && !Autofac.Core.KeyedService.IsAnyKey(effectiveServiceKey))
        {
            newParameters = new List<Parameter>(context.Parameters);
            newParameters.Add(CreateMicrosoftServiceKeyParameter(effectiveServiceKey));
        }

        if (_addFromKeyedServiceParameter)
        {
            newParameters ??= new List<Parameter>(context.Parameters);

            // [FromKeyedServices("key")] - Specifies a keyed service
            // for injection into a constructor. This is similar to the
            // Autofac [KeyFilter] attribute.
            newParameters.Add(CreateFromKeyedServicesParameter(effectiveServiceKey));
        }

        if (newParameters is not null)
        {
            context.ChangeParameters(newParameters);
        }

        next(context);
    }

    private static ResolvedParameter CreateMicrosoftServiceKeyParameter(object serviceKey)
    {
        return new ResolvedParameter(
            (p, c) =>
            {
                return Attribute.IsDefined(p, typeof(Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute), inherit: true);
            },
            (p, c) =>
            {
                return KeyTypeManipulation.ChangeToCompatibleType(serviceKey, p.ParameterType, p);
            });
    }

    private static ResolvedParameter CreateFromKeyedServicesParameter(object? currentServiceKey)
    {
        return new ResolvedParameter(
            (p, c) =>
            {
                return Attribute.IsDefined(p, typeof(FromKeyedServicesAttribute), inherit: true);
            },
            (p, c) =>
            {
                var filter = p.GetCustomAttribute<FromKeyedServicesAttribute>(inherit: true)!;
                return filter.ResolveParameter(p, c, currentServiceKey);
            });
    }

    private static object? GetRequestedServiceKey(IEnumerable<Parameter> parameters)
    {
        foreach (var parameter in parameters)
        {
            if (parameter is RequestedServiceKeyParameter requested)
            {
                return requested.ServiceKey;
            }
        }

        return null;
    }
}
