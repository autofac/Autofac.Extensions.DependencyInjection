// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Special service keys for use with Microsoft.Extensions.DependencyInjection.
/// </summary>
public static class ServiceKey
{
    /// <summary>
    /// Placeholder for null service keys. If you register a service via
    /// Microsoft.Extensions.DependencyInjection with a null service key, you
    /// can resolve it through <see cref="IServiceProvider"/> using <see
    /// langword="null"/>. Autofac does not support null service keys, so if you
    /// use an <see cref="ILifetimeScope"/> to resolve the service, this is the
    /// key you need to use.
    /// </summary>
    public static readonly object Null = new();
}
