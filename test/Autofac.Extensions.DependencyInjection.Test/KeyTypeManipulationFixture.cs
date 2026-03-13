// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

namespace Autofac.Extensions.DependencyInjection.Test;

public class KeyTypeManipulationFixture
{
    [Fact]
    public void ChangeToCompatibleTypeAllowsNullMemberInfo()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType("123", typeof(int), (System.Reflection.MemberInfo)null!);
        Assert.Equal(123, actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeAllowsNullParameterInfo()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType("123", typeof(int), (System.Reflection.ParameterInfo)null!);
        Assert.Equal(123, actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeLooksForTryParseMethod()
    {
        var address = "127.0.0.1";
        var value = KeyTypeManipulation.ChangeToCompatibleType(address, typeof(IPAddress));
        Assert.Equal(value, IPAddress.Parse(address));
    }

    [Fact]
    public void ChangeToCompatibleTypeNoConversionNeeded()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType(15, typeof(int));
        Assert.Equal(15, actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeNullDestinationType()
    {
        var ex = Assert.Throws<ArgumentNullException>(() => KeyTypeManipulation.ChangeToCompatibleType("15", null!));
        Assert.Equal("destinationType", ex.ParamName);
    }

    [Fact]
    public void ChangeToCompatibleTypeNullReferenceType()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType(null, typeof(string));
        Assert.Null(actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeNullValueType()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType(null, typeof(int));
        Assert.Equal(0, actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeTryParseFails()
    {
        var ex = Assert.Throws<KeyTypeConversionException>(() => KeyTypeManipulation.ChangeToCompatibleType("not-valid", typeof(ParseOnlyType)));
        Assert.Equal(typeof(string), ex.ResolutionKeyType);
        Assert.Equal(typeof(ParseOnlyType), ex.AttributeKeyType);
    }

    [Fact]
    public void ChangeToCompatibleTypeTypeConverterAttributeCannotConvertFromFallsBackToDefaultConversion()
    {
        var actual = KeyTypeManipulation.ChangeToCompatibleType(
            15,
            typeof(string),
            new TypeConverterAttribute(typeof(NonConvertingConverter)));

        Assert.Equal("15", actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeTypeConverterAttributeIsInvalid()
    {
        var ex = Assert.Throws<KeyTypeConversionException>(() =>
            KeyTypeManipulation.ChangeToCompatibleType("15", typeof(int), new TypeConverterAttribute(typeof(string))));

        Assert.Equal(typeof(string), ex.ResolutionKeyType);
        Assert.Equal(typeof(int), ex.AttributeKeyType);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }

    [Theory]
    [MemberData(nameof(ParsingCultures))]
    public void ChangeToCompatibleTypeUsesInvariantCulture(CultureInfo culture)
    {
        TestCulture.With(
            culture,
            () =>
            {
                var actual = KeyTypeManipulation.ChangeToCompatibleType("123.456", typeof(double));
                Assert.Equal(123.456, actual);
            });
    }

    [Fact]
    public void ChangeToCompatibleTypeUsesTypeConverterOnParameter()
    {
        var ctor = typeof(HasTypeConverterAttributes).GetConstructor(new Type[] { typeof(Convertible) });
        var member = ctor.GetParameters().First();
        var actual = KeyTypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
        Assert.NotNull(actual);
        Assert.Equal(25, actual.Value);
    }

    [Fact]
    public void ChangeToCompatibleTypeUsesTypeConverterOnProperty()
    {
        var member = typeof(HasTypeConverterAttributes).GetProperty("Property");
        var actual = KeyTypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
        Assert.NotNull(actual);
        Assert.Equal(25, actual.Value);
    }

    public static IEnumerable<object[]> ParsingCultures()
    {
        yield return new object[] { new CultureInfo("en-US") };
        yield return new object[] { new CultureInfo("es-MX") };
        yield return new object[] { new CultureInfo("it-IT") };
        yield return new object[] { CultureInfo.InvariantCulture };
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through reflection.")]
    private class Convertible
    {
        public int Value { get; set; }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through reflection.")]
    private class ConvertibleConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (value == null)
            {
                return null;
            }

            if (value is not string str)
            {
                return base.ConvertFrom(context, culture, value);
            }

            var converter = TypeDescriptor.GetConverter(typeof(int));
            return new Convertible { Value = (int)converter.ConvertFromString(context, culture, str) };
        }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through reflection.")]
    private class NonConvertingConverter : TypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }
    }

    [SuppressMessage("CA1812", "CA1812", Justification = "Class instantiated through reflection.")]
    private class HasTypeConverterAttributes
    {
        public HasTypeConverterAttributes([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter)
        {
            Property = parameter;
        }

        [TypeConverter(typeof(ConvertibleConverter))]
        public Convertible Property { get; set; }
    }

    private readonly struct ParseOnlyType
    {
        public static bool TryParse(string value, out ParseOnlyType parsed)
        {
            parsed = default;
            return false;
        }
    }
}
