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
    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.ResolveRequestStart;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        var newParameters = new List<Parameter>(context.Parameters);

        if (context.Service is Autofac.Core.KeyedService keyedService)
        {
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

        // [FromKeyedServices("key")] - Specifies a keyed service
        // for injection into a constructor. This is similar to the
        // Autofac [KeyFilter] attribute.
        newParameters.Add(new ResolvedParameter(
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).FirstOrDefault();
                return filter is not null && filter.CanResolveParameter(p, c);
            },
            (p, c) =>
            {
                var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).First();
                return filter.ResolveParameter(p, c);
            }));

        context.ChangeParameters(newParameters);

        next(context);
    }
}
