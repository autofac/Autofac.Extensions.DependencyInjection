// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;
using Autofac.Core.Resolving.Pipeline;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Middleware that supports injecting service keys onto parameters with the <see cref="ServiceKeyAttribute"/>.
/// </summary>
internal class ServiceKeyMiddleware : IResolveMiddleware
{
    /// <summary>
    /// Gets the singleton instance of the middleware.
    /// </summary>
    public static ServiceKeyMiddleware Instance { get; } = new();

    /// <inheritdoc />
    public PipelinePhase Phase => PipelinePhase.ResolveRequestStart;

    /// <inheritdoc />
    public void Execute(ResolveRequestContext context, Action<ResolveRequestContext> next)
    {
        if (context.Service is Core.KeyedService keyedService)
        {
            var serviceKeyParameter = new ResolvedParameter(
                (p, c) =>
                {
                    return p.GetCustomAttributes<ServiceKeyAttribute>(true).FirstOrDefault() is not null;
                },
                (p, c) =>
                {
                    // If the key is an object but the constructor takes
                    // a string, we need to safely convert that. This is
                    // particularly interesting in the AnyKey scenario.
                    return KeyTypeManipulation.ChangeToCompatibleType(keyedService.ServiceKey, p.ParameterType, p);
                });

            context.ChangeParameters(context.Parameters.Concat([serviceKeyParameter]));
        }

        next(context);
    }
}
