// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using Autofac.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Autofac.Extensions.DependencyInjection.Test;

// Most of the functionality is verified via the specification tests. These are some extras for error handling, etc.
public class AutofacServiceProviderTests
{
    [Fact]
    public void Ctor_NullLifetimeScope()
    {
        Assert.Throws<ArgumentNullException>(() => new AutofacServiceProvider(null!));
    }

    [Fact]
    public void GetKeyedService_NullServiceType()
    {
        using var provider = new AutofacServiceProvider(new ContainerBuilder().Build());
        Assert.Throws<ArgumentNullException>(() => provider.GetKeyedService(null!, "key"));
    }

    [Fact]
    public void GetRequiredKeyedService_NullServiceType()
    {
        using var provider = new AutofacServiceProvider(new ContainerBuilder().Build());
        Assert.Throws<ArgumentNullException>(() => provider.GetRequiredKeyedService(null!, "key"));
    }

    [Fact]
    public void GetRequiredService_DependencyResolutionFails()
    {
        var builder = new ContainerBuilder();
        using var provider = new AutofacServiceProvider(builder.Build());
        Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<DivideByZeroException>());
    }
}
