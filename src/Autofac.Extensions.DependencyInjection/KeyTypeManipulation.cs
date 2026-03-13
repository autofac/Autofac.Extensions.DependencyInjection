// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Collections.Concurrent;
using System.ComponentModel;
using System.Globalization;
using System.Reflection;
using Autofac.Core;

namespace Autofac.Extensions.DependencyInjection;

/// <summary>
/// Utilities for converting keyed service key values into compatible types for
/// injection using the <see cref="Microsoft.Extensions.DependencyInjection.ServiceKeyAttribute"/>.
/// This logic originally came from Autofac.Configuration but there isn't a good
/// "shared dependency" location for things like this other than core Autofac.
/// </summary>
internal class KeyTypeManipulation
{
    private static readonly KeyTypeManipulationReflectionCache ReflectionCache = new();

    static KeyTypeManipulation()
    {
        ReflectionCacheSet.Shared.RegisterExternalCache(ReflectionCache);
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
    public static object? ChangeToCompatibleType(object? value, Type destinationType, ParameterInfo memberInfo)
    {
        TypeConverterAttribute? attrib = null;
        if (memberInfo != null)
        {
            attrib = ReflectionCache.GetOrAddParameterConverterAttribute(memberInfo);
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
            attrib = ReflectionCache.GetOrAddMemberConverterAttribute(memberInfo);
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
            return destinationType.GetTypeInfo().IsValueType
                ? ReflectionCache.GetOrAddDefaultValue(destinationType)
                : null;
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
        if (value is string stringValue)
        {
            // Some types in later frameworks have string TryParse and ReadOnlySpan<char> TryParse
            // so they result in an AmbiguousMatchException unless we specify.
            var parser = ReflectionCache.GetOrAddTryParseMethod(destinationType);
            if (parser != null)
            {
                var parameters = new object?[] { stringValue, null };
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
        var converterType = ReflectionCache.GetOrAddConverterType(converterTypeName);

        return (TypeConverter)Activator.CreateInstance(converterType)!;
    }

    [ExcludeFromCodeCoverage]
    private sealed class KeyTypeManipulationReflectionCache : IReflectionCache
    {
        private readonly ConcurrentDictionary<ParameterInfo, TypeConverterAttribute?> _parameterConverterAttributes = new();
        private readonly ConcurrentDictionary<MemberInfo, TypeConverterAttribute?> _memberConverterAttributes = new();
        private readonly ConcurrentDictionary<string, Type> _converterTypeCache = new(StringComparer.Ordinal);
        private readonly ConcurrentDictionary<Type, MethodInfo?> _tryParseMethodCache = new();
        private readonly ConcurrentDictionary<Type, object?> _defaultValueCache = new();

        public ReflectionCacheUsage Usage => ReflectionCacheUsage.Resolution;

        public TypeConverterAttribute? GetOrAddParameterConverterAttribute(ParameterInfo parameter)
        {
            return _parameterConverterAttributes.GetOrAdd(
                parameter,
                static p => p.GetCustomAttributes(typeof(TypeConverterAttribute), true)
                    .Cast<TypeConverterAttribute>()
                    .FirstOrDefault());
        }

        public TypeConverterAttribute? GetOrAddMemberConverterAttribute(MemberInfo member)
        {
            return _memberConverterAttributes.GetOrAdd(
                member,
                static m => m.GetCustomAttributes(typeof(TypeConverterAttribute), true)
                    .Cast<TypeConverterAttribute>()
                    .FirstOrDefault());
        }

        public Type GetOrAddConverterType(string converterTypeName)
        {
            return _converterTypeCache.GetOrAdd(
                converterTypeName,
                static name =>
                {
                    var resolvedType = Type.GetType(name, true)!;
                    if (!typeof(TypeConverter).GetTypeInfo().IsAssignableFrom(resolvedType.GetTypeInfo()))
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, KeyTypeManipulationResources.TypeConverterAttributeTypeNotConverter, name));
                    }

                    return resolvedType;
                });
        }

        public MethodInfo? GetOrAddTryParseMethod(Type destinationType)
        {
            return _tryParseMethodCache.GetOrAdd(
                destinationType,
                static type =>
                {
                    var parameterTypes = new[] { typeof(string), type.MakeByRefType() };
                    return type.GetMethod("TryParse", BindingFlags.Static | BindingFlags.Public, null, CallingConventions.Standard, parameterTypes, null);
                });
        }

        public object? GetOrAddDefaultValue(Type destinationType)
        {
            return _defaultValueCache.GetOrAdd(destinationType, static t => Activator.CreateInstance(t));
        }

        public void Clear()
        {
            _parameterConverterAttributes.Clear();
            _memberConverterAttributes.Clear();
            _converterTypeCache.Clear();
            _tryParseMethodCache.Clear();
            _defaultValueCache.Clear();
        }

        public void Clear(ReflectionCacheClearPredicate clearPredicate)
        {
            if (clearPredicate is null)
            {
                throw new ArgumentNullException(nameof(clearPredicate));
            }

            foreach (var parameter in _parameterConverterAttributes.Keys)
            {
                var member = parameter.Member;
                var assemblies = GetParameterAssemblies(parameter);
                if (clearPredicate(member, assemblies))
                {
                    _parameterConverterAttributes.TryRemove(parameter, out _);
                }
            }

            foreach (var member in _memberConverterAttributes.Keys)
            {
                if (clearPredicate(member, new[] { member.Module.Assembly }))
                {
                    _memberConverterAttributes.TryRemove(member, out _);
                }
            }

            foreach (var type in _tryParseMethodCache.Keys)
            {
                if (clearPredicate(type, new[] { type.Assembly }))
                {
                    _tryParseMethodCache.TryRemove(type, out _);
                }
            }

            foreach (var type in _defaultValueCache.Keys)
            {
                if (clearPredicate(type, new[] { type.Assembly }))
                {
                    _defaultValueCache.TryRemove(type, out _);
                }
            }

            foreach (var entry in _converterTypeCache)
            {
                if (clearPredicate(entry.Value, new[] { entry.Value.Assembly }))
                {
                    _converterTypeCache.TryRemove(entry.Key, out _);
                }
            }
        }

        private static IEnumerable<Assembly> GetParameterAssemblies(ParameterInfo parameter)
        {
            var memberAssembly = parameter.Member.Module.Assembly;
            var parameterAssembly = parameter.ParameterType.Assembly;

            if (ReferenceEquals(memberAssembly, parameterAssembly))
            {
                yield return memberAssembly;
                yield break;
            }

            yield return memberAssembly;
            yield return parameterAssembly;
        }
    }
}
