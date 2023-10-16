// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Net;

namespace Autofac.Extensions.DependencyInjection.Test;

public class TypeManipulationFixture
{
    [Fact]
    public void ChangeToCompatibleTypeLooksForTryParseMethod()
    {
        var address = "127.0.0.1";
        var value = TypeManipulation.ChangeToCompatibleType(address, typeof(IPAddress));
        Assert.Equal(value, IPAddress.Parse(address));
    }

    [Fact]
    public void ChangeToCompatibleTypeUsesTypeConverterOnParameter()
    {
        var ctor = typeof(HasTypeConverterAttributes).GetConstructor(new Type[] { typeof(Convertible) });
        var member = ctor.GetParameters().First();
        var actual = TypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
        Assert.NotNull(actual);
        Assert.Equal(25, actual.Value);
    }

    [Fact]
    public void ChangeToCompatibleTypeUsesTypeConverterOnProperty()
    {
        var member = typeof(HasTypeConverterAttributes).GetProperty("Property");
        var actual = TypeManipulation.ChangeToCompatibleType("25", typeof(Convertible), member) as Convertible;
        Assert.NotNull(actual);
        Assert.Equal(25, actual.Value);
    }

    [Fact]
    public void ChangeToCompatibleTypeNullReferenceType()
    {
        var actual = TypeManipulation.ChangeToCompatibleType(null, typeof(string));
        Assert.Null(actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeNullValueType()
    {
        var actual = TypeManipulation.ChangeToCompatibleType(null, typeof(int));
        Assert.Equal(0, actual);
    }

    [Fact]
    public void ChangeToCompatibleTypeNoConversionNeeded()
    {
        var actual = TypeManipulation.ChangeToCompatibleType(15, typeof(int));
        Assert.Equal(15, actual);
    }

    [Theory]
    [MemberData(nameof(ParsingCultures))]
    public void ChangeToCompatibleTypeUsesInvariantCulture(CultureInfo culture)
    {
        TestCulture.With(
            culture,
            () =>
            {
                var actual = TypeManipulation.ChangeToCompatibleType("123.456", typeof(double));
                Assert.Equal(123.456, actual);
            });
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
    private class HasTypeConverterAttributes
    {
        public HasTypeConverterAttributes([TypeConverter(typeof(ConvertibleConverter))] Convertible parameter)
        {
            Property = parameter;
        }

        [TypeConverter(typeof(ConvertibleConverter))]
        public Convertible Property { get; set; }
    }
}
