using System;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
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
}
