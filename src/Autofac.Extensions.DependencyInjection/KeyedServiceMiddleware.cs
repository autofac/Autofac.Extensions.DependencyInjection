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
    // [FromKeyedServices("key")] - Specifies a keyed service
    // for injection into a constructor. This is similar to the
    // Autofac [KeyFilter] attribute.
    private static readonly Parameter FromKeyedServicesParameter = new ResolvedParameter(
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).FirstOrDefault();
                return filter is not null && filter.CanResolveParameter(p, c);
            },
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).First();
                return filter.ResolveParameter(p, c);
            });

    /// <summary>
    /// Gets a single instance of this middleware that does not add the keyed services parameter.
    /// </summary>
    public static KeyedServiceMiddleware InstanceWithoutFromKeyedServicesParameter { get; } = new(addFromKeyedServiceParameter: false);

    /// <summary>
    /// Gets a single instance of this middleware that adds the keyed services parameter.
    /// </summary>
    public static KeyedServiceMiddleware InstanceWithFromKeyedServicesParameter { get; } = new(addFromKeyedServiceParameter: true);

    private readonly bool _addFromKeyedServiceParameter;

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyedServiceMiddleware"/> class.
    /// </summary>
    /// <param name="addFromKeyedServiceParameter">Whether or not the from-keyed-service parameter should be added.</param>
    public KeyedServiceMiddleware(bool addFromKeyedServiceParameter)
    {
        _addFromKeyedServiceParameter = addFromKeyedServiceParameter;
    }

    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.Activation;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        List<Parameter>? newParameters = null;

        if (context.Service is Autofac.Core.KeyedService keyedService)
        {
            newParameters = new List<Parameter>(context.Parameters);

            var key = keyedService.ServiceKey;

            // [ServiceKey] - indicates that the parameter value should
            // be the service key used during resolution.
            newParameters.Add(new ResolvedParameter(
                (p, c) =>
                {
                    return p.GetCustomAttributes<ServiceKeyAttribute>(true).FirstOrDefault() is not null;
                },
                (p, c) =>
                {
                    // If the key is an object but the constructor takes
                    // a string, we need to safely convert that. This is
                    // particularly interesting in the AnyKey scenario.
                    return KeyTypeManipulation.ChangeToCompatibleType(key, p.ParameterType, p);
                }));
        }

        if (_addFromKeyedServiceParameter)
        {
            newParameters ??= new List<Parameter>(context.Parameters);

            // [FromKeyedServices("key")] - Specifies a keyed service
            // for injection into a constructor. This is similar to the
            // Autofac [KeyFilter] attribute.
            newParameters.Add(FromKeyedServicesParameter);
        }

        if (newParameters is not null)
        {
            context.ChangeParameters(newParameters);
        }

        next(context);
    }
}
