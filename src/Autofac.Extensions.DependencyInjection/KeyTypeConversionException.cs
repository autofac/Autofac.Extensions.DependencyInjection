// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Globalization;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Exception indicating that type conversion failed when trying to inject a key
/// using the
/// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
/// </summary>
public class KeyTypeConversionException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="KeyTypeConversionException" /> class.
    /// </summary>
    /// <param name="resolutionKeyType">
    /// The type of the key specified during service resolution. This is what
    /// would be injected into the constructor and needs to be converted.
    /// </param>
    /// <param name="attributeKeyType">
    /// The type of parameter marked with the
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// This is what the key was trying to be converted to.
    /// </param>
    public KeyTypeConversionException(Type resolutionKeyType, Type attributeKeyType)
        : this(resolutionKeyType, attributeKeyType, string.Format(CultureInfo.CurrentCulture, KeyTypeConversionExceptionResources.Message, resolutionKeyType, attributeKeyType))
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyTypeConversionException" /> class.
    /// </summary>
    /// <param name="resolutionKeyType">
    /// The type of the key specified during service resolution. This is what
    /// would be injected into the constructor and needs to be converted.
    /// </param>
    /// <param name="attributeKeyType">
    /// The type of parameter marked with the
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// This is what the key was trying to be converted to.
    /// </param>
    /// <param name="message">
    /// A specific message for the exception to override the default.
    /// </param>
    public KeyTypeConversionException(Type resolutionKeyType, Type attributeKeyType, string message)
        : base(message)
    {
        ResolutionKeyType = resolutionKeyType;
        AttributeKeyType = attributeKeyType;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyTypeConversionException"/> class.
    /// </summary>
    /// <param name="resolutionKeyType">
    /// The type of the key specified during service resolution. This is what
    /// would be injected into the constructor and needs to be converted.
    /// </param>
    /// <param name="attributeKeyType">
    /// The type of parameter marked with the
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// This is what the key was trying to be converted to.
    /// </param>
    /// <param name="innerException">The inner exception.</param>
    public KeyTypeConversionException(Type resolutionKeyType, Type attributeKeyType, Exception? innerException)
        : this(resolutionKeyType, attributeKeyType, string.Format(CultureInfo.CurrentCulture, KeyTypeConversionExceptionResources.Message, resolutionKeyType, attributeKeyType), innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="KeyTypeConversionException"/> class.
    /// </summary>
    /// <param name="resolutionKeyType">
    /// The type of the key specified during service resolution. This is what
    /// would be injected into the constructor and needs to be converted.
    /// </param>
    /// <param name="attributeKeyType">
    /// The type of parameter marked with the
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// This is what the key was trying to be converted to.
    /// </param>
    /// <param name="message">
    /// A specific message for the exception to override the default.
    /// </param>
    /// <param name="innerException">The inner exception.</param>
    public KeyTypeConversionException(Type resolutionKeyType, Type attributeKeyType, string message, Exception? innerException)
        : base(message, innerException)
    {
        ResolutionKeyType = resolutionKeyType;
        AttributeKeyType = attributeKeyType;
    }

    /// <summary>
    /// Gets the type of the parameter marked with the <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of parameter marked with the
    /// <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>,
    /// which is the destination where the key should be injected. This should
    /// be compatible with the key provided during resolution.
    /// </value>
    public Type AttributeKeyType { get; }

    /// <summary>
    /// Gets the type of the key specified during service resolution.
    /// </summary>
    /// <value>
    /// The <see cref="Type"/> of key passed to the keyed service resolve
    /// operation. This is what would be injected as a constructor parameter to
    /// the service being resolved.
    /// </value>
    public Type ResolutionKeyType { get; }
}
