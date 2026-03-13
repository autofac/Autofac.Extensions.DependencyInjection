// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extensions.DependencyInjection.Test;

public class TypeExtensionsTests
{
    [Fact]
    public void IsCollectionCachesLookups()
    {
        var type = typeof(IReadOnlyList<int>);

        var first = type.IsCollection();
        var second = type.IsCollection();

        Assert.True(first);
        Assert.Equal(first, second);
    }

    [Theory]
    [InlineData(typeof(int[]))]
    [InlineData(typeof(IEnumerable<int>))]
    [InlineData(typeof(IList<int>))]
    [InlineData(typeof(ICollection<int>))]
    [InlineData(typeof(IReadOnlyCollection<int>))]
    [InlineData(typeof(IReadOnlyList<int>))]
    public void IsCollectionSupportedCollectionTypes(Type type)
    {
        Assert.True(type.IsCollection());
    }

    [Theory]
    [InlineData(typeof(string))]
    [InlineData(typeof(List<int>))]
    [InlineData(typeof(Dictionary<string, string>))]
    [InlineData(typeof(IEnumerable<>))]
    public void IsCollectionUnsupportedTypes(Type type)
    {
        Assert.False(type.IsCollection());
    }
}
