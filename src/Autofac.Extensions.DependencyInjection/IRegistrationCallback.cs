// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Represents a callback that can be used to configure a registration.
/// </summary>
public interface IRegistrationCallback
{
    /// <summary>
    /// Invoked when a registration is being registered.
    /// </summary>
    /// <param name="registrationBuilder">The registration being built.</param>
    /// <typeparam name="TActivatorData">The activator data type.</typeparam>
    /// <typeparam name="TRegistrationStyle">The object registration style.</typeparam>
    void OnRegister<TActivatorData, TRegistrationStyle>(
        IRegistrationBuilder<object, TActivatorData, TRegistrationStyle> registrationBuilder);
}
