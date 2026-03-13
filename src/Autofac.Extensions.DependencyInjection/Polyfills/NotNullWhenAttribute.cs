// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

#if NETSTANDARD2_0

namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// Polyfill for <see cref="NotNullWhenAttribute"/> which is not available in netstandard2.0.
/// Specifies that when a method returns <see cref="ReturnValue"/>,
/// the parameter will not be null even if the corresponding type allows it.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.
    /// </summary>
    /// <param name="returnValue">
    /// The return value condition. If the method returns this value, the associated parameter will not be null.
    /// </param>
    public NotNullWhenAttribute(bool returnValue) => ReturnValue = returnValue;

    /// <summary>
    /// Gets a value indicating whether the return value should be true or false for the parameter to be non-null.
    /// </summary>
    public bool ReturnValue { get; }
}

#endif
