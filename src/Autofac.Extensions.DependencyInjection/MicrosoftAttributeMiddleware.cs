// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Middleware that supports the Microsoft-specific injection and filtering attributes for registrations.
/// </summary>
internal class MicrosoftAttributeMiddleware : IResolveMiddleware
{
    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.ParameterSelection;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        if (context.Service is Autofac.Core.KeyedService keyedService)
        {
            var key = keyedService.ServiceKey;
            context.ChangeParameters(context.Parameters.Union(
                new[]
                {
                    // [FromKeyedServices("key")] - Specifies a keyed service
                    // for injection into a constructor. This is similar to the
                    // Autofac [KeyFilter] attribute.
                    new ResolvedParameter(
                        (p, c) =>
                        {
                            var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).FirstOrDefault();
                            return filter is not null && filter.CanResolveParameter(p, c);
                        },
                        (p, c) =>
                        {
                            var filter = p.GetCustomAttributes<FromKeyedServicesAttribute>(true).First();
                            return filter.ResolveParameter(p, c);
                        }),

                    // [ServiceKey] - indicates that the parameter value should
                    // be the service key used during resolution.
                    new ResolvedParameter(
                        (p, c) =>
                        {
                            return p.GetCustomAttributes<ServiceKeyAttribute>(true).FirstOrDefault() is not null;
                        },
                        (p, c) =>
                        {
                            return key;
                        }),
                }));
        }

        next(context);
    }
}
