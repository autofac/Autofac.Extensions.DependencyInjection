// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Autofac.Core.Registration;
using Autofac.Core.Resolving.Pipeline;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Middleware source that injects Microsoft attribute support and shims into lookups for Microsoft keyed service compatibility.
/// </summary>
public class ServiceKeyMiddlewareSource : IServiceMiddlewareSource
{
    /// <inheritdoc/>
    public void ProvideMiddleware(Service service, IComponentRegistryServices availableServices, IResolvePipelineBuilder pipelineBuilder)
    {
        if (pipelineBuilder == null)
        {
            throw new ArgumentNullException(nameof(pipelineBuilder));
        }

        pipelineBuilder.Use(ServiceKeyMiddleware.Instance, MiddlewareInsertionMode.StartOfPhase);
    }
}
