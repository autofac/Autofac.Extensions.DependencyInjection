// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System.Collections.Generic;
using Autofac.Core;

namespace Autofac.Extensions.DependencyInjection
{
    /// <summary>
    /// An accessor to get a list of module registrations that will be registered for each
    /// request.
    /// </summary>
    public interface IPerRequestModuleAccessor
    {
        /// <summary>
        /// Gets a list of module registrations that will be registered for each request.
        /// </summary>
        IEnumerable<IModule> Modules { get; }
    }
}
