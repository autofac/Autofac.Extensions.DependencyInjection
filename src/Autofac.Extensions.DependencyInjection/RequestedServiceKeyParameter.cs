// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Reflection;
using Autofac.Core;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Carries the requested service key through a resolve operation so middleware can access it.
/// </summary>
internal sealed class RequestedServiceKeyParameter : Parameter
{
    /// <summary>
    /// Initializes a new instance of the <see cref="RequestedServiceKeyParameter"/> class.
    /// </summary>
    /// <param name="serviceKey">The requested service key.</param>
    public RequestedServiceKeyParameter(object serviceKey)
    {
        ServiceKey = serviceKey ?? throw new ArgumentNullException(nameof(serviceKey));
    }

    /// <summary>
    /// Gets the requested service key value.
    /// </summary>
    public object ServiceKey { get; }

    /// <inheritdoc />
#if NETSTANDARD2_0
    public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, out Func<object?>? valueProvider)
#else
    public override bool CanSupplyValue(ParameterInfo pi, IComponentContext context, [NotNullWhen(returnValue: true)] out Func<object?>? valueProvider)
#endif
    {
        valueProvider = null;
        return false;
    }
}
