// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

namespace Autofac.Extensions.DependencyInjection.Test;

public sealed class AutofacChildLifetimeScopeConfigurationAdapterTests
{
    [Fact]
    public void AddMultipleConfigurationContainsConfigurations()
    {
        var adapter = new AutofacChildLifetimeScopeConfigurationAdapter();
        adapter.Add(builder => { });
        adapter.Add(builder => { });

        Assert.Equal(2, adapter.ConfigurationActions.Count);
    }

    [Fact]
    public void AddNullConfigurationThrows()
        => Assert.Throws<ArgumentNullException>(() => new AutofacChildLifetimeScopeConfigurationAdapter().Add(null));
}
