// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Globalization;
using System.Reflection;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Utilities for converting keyed service key values into compatible types for
/// injection using the <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
/// This logic originally came from Autofac.Configuration but there isn't a good
/// "shared dependency" location for things like this other than core Autofac.
/// </summary>
internal class KeyTypeManipulation
{
    /// <summary>
    /// Converts an object to a type compatible with a given parameter.
    /// </summary>
    /// <param name="value">The object value to convert.</param>
    /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
    /// <param name="memberInfo">The parameter for which the <paramref name="value"/> is being converted.</param>
    /// <returns>
    /// An <see cref="object"/> of type <paramref name="destinationType"/>, converted using
    /// type converters specified on <paramref name="memberInfo"/> if available. If <paramref name="value"/>
    /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
    /// types and the default value for value types.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if conversion of the value fails.
    /// </exception>
    public static object? ChangeToCompatibleType(object? value, Type destinationType, ParameterInfo memberInfo)
    {
        TypeConverterAttribute? attrib = null;
        if (memberInfo != null)
        {
            attrib = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
        }

        return ChangeToCompatibleType(value, destinationType, attrib);
    }

    /// <summary>
    /// Converts an object to a type compatible with a given parameter.
    /// </summary>
    /// <param name="value">The object value to convert.</param>
    /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
    /// <param name="memberInfo">The parameter for which the <paramref name="value"/> is being converted.</param>
    /// <returns>
    /// An <see cref="object"/> of type <paramref name="destinationType"/>, converted using
    /// type converters specified on <paramref name="memberInfo"/> if available. If <paramref name="value"/>
    /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
    /// types and the default value for value types.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if conversion of the value fails.
    /// </exception>
    public static object? ChangeToCompatibleType(object? value, Type destinationType, MemberInfo memberInfo)
    {
        TypeConverterAttribute? attrib = null;
        if (memberInfo != null)
        {
            attrib = memberInfo.GetCustomAttributes(typeof(TypeConverterAttribute), true).Cast<TypeConverterAttribute>().FirstOrDefault();
        }

        return ChangeToCompatibleType(value, destinationType, attrib);
    }

    /// <summary>
    /// Converts an object to a type compatible with a given parameter.
    /// </summary>
    /// <param name="value">The object value to convert.</param>
    /// <param name="destinationType">The destination <see cref="Type"/> to which <paramref name="value"/> should be converted.</param>
    /// <param name="converterAttribute">A <see cref="TypeConverterAttribute"/>, if available, specifying the type of converter to use.<paramref name="value"/> is being converted.</param>
    /// <returns>
    /// An <see cref="object"/> of type <paramref name="destinationType"/>, converted using
    /// any type converters specified in <paramref name="converterAttribute"/> if available. If <paramref name="value"/>
    /// is <see langword="null"/> then the output will be <see langword="null"/> for reference
    /// types and the default value for value types.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if conversion of the value fails.
    /// </exception>
    public static object? ChangeToCompatibleType(object? value, Type destinationType, TypeConverterAttribute? converterAttribute = null)
    {
        if (destinationType == null)
        {
            throw new ArgumentNullException(nameof(destinationType));
        }

        if (value == null)
        {
            return destinationType.GetTypeInfo().IsValueType ? Activator.CreateInstance(destinationType) : null;
        }

        // Try implicit conversion.
        if (destinationType.IsInstanceOfType(value))
        {
            return value;
        }

        TypeConverter converter;

        // Try to get custom type converter information.
        if (converterAttribute != null && !string.IsNullOrEmpty(converterAttribute.ConverterTypeName))
        {
            try
            {
                converter = GetTypeConverterFromName(converterAttribute.ConverterTypeName);
            }
            catch (InvalidOperationException ex)
            {
                throw new KeyTypeConversionException(value.GetType(), destinationType, ex);
            }

            if (converter.CanConvertFrom(value.GetType()))
            {
                return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
            }
        }

        // If there's not a custom converter specified via attribute, try for a default.
        converter = TypeDescriptor.GetConverter(value.GetType());
        if (converter.CanConvertTo(destinationType))
        {
            return converter.ConvertTo(null, CultureInfo.InvariantCulture, value, destinationType);
        }

        // Try explicit opposite conversion.
        converter = TypeDescriptor.GetConverter(destinationType);
        if (converter.CanConvertFrom(value.GetType()))
        {
            return converter.ConvertFrom(null, CultureInfo.InvariantCulture, value);
        }

        // Try a TryParse method.
        if (value is string)
        {
            // Some types in later frameworks have string TryParse and ReadOnlySpan<char> TryParse
            // so they result in an AmbiguousMatchException unless we specify.
            var parser = destinationType.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Standard, new Type[] { typeof(string), destinationType.MakeByRefType() }, null);
            if (parser != null)
            {
                var parameters = new[] { value, null };
                if ((bool)parser.Invoke(null, parameters)!)
                {
                    return parameters[1];
                }
            }
        }

        throw new KeyTypeConversionException(value.GetType(), destinationType);
    }

    /// <summary>
    /// Instantiates a type converter from its type name.
    /// </summary>
    /// <param name="converterTypeName">
    /// The name of the <see cref="Type"/> of the <see cref="TypeConverter"/>.
    /// </param>
    /// <returns>
    /// The instantiated <see cref="TypeConverter"/>.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <paramref name="converterTypeName"/> does not correspond
    /// to a <see cref="TypeConverter"/>.
    /// </exception>
    private static TypeConverter GetTypeConverterFromName(string converterTypeName)
    {
        var converterType = Type.GetType(converterTypeName, true);
        return Activator.CreateInstance(converterType!) is not TypeConverter converter
            ? throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, KeyTypeManipulationResources.TypeConverterAttributeTypeNotConverter, converterTypeName))
            : converter;
    }
}
