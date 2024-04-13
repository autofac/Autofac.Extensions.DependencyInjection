// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Middleware that adds the ability to use <see cref="FromKeyedServicesAttribute"/> on resolve parameters.
/// </summary>
internal class FromKeyedServiceMiddleware : IResolveMiddleware
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

    private static readonly Parameter[] FromKeyedServicesParameterArray = [FromKeyedServicesParameter];

    /// <summary>
    /// Gets the singleton instance of the middleware.
    /// </summary>
    public static FromKeyedServiceMiddleware Instance { get; } = new();

    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.Activation;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        context.ChangeParameters(context.Parameters.Concat(FromKeyedServicesParameterArray));

        next(context);
    }
}
