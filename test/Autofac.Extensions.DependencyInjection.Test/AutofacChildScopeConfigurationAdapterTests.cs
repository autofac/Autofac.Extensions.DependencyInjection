using System;
using Xunit;

namespace Autofac.Extensions.DependencyInjection.Test
{
    public sealed class AutofacChildScopeConfigurationAdapterTests
    {
        [Fact]
        public void AddMultipleConfigurationContainsConfigurations()
        {
            var autofacChildScopeConfigurationAdapter = new AutofacChildScopeConfigurationAdapter();
            autofacChildScopeConfigurationAdapter.Add(builder => { });
            autofacChildScopeConfigurationAdapter.Add(builder => { });

            Assert.Equal(2, autofacChildScopeConfigurationAdapter.ChildScopeConfigurationActions.Count);
        }

        [Fact]
        public void AddNullConfigurationThrows()
            => Assert.Throws<ArgumentNullException>(() => new AutofacChildScopeConfigurationAdapter().Add(null));
    }
}
