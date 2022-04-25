// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection
{
#if NET6_0_OR_GREATER
    /// <summary>
    /// .NET 6 specific addition to AutofacServiceProvider that supports new interfaces.
    /// </summary>
    public partial class AutofacServiceProvider : IServiceProviderIsService
    {
        /// <inheritdoc />
        public bool IsService(Type serviceType) => _lifetimeScope.ComponentRegistry.IsRegistered(new TypedService(serviceType));
    }
#endif
}
