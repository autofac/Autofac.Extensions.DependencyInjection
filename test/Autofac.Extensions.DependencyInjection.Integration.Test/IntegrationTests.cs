// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

#if NET8_0
using Integration.Net6;
#endif
#if NET7_0
using Integration.Net6;
#endif
#if NET6_0
using Integration.Net6;
#endif

namespace Autofac.Extensions.DependencyInjection.Integration.Test;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Startup>>
{
    public IntegrationTests(WebApplicationFactory<Startup> appFactory)
    {
        AppFactory = appFactory;
    }

    public WebApplicationFactory<Startup> AppFactory { get; }

    [Fact]
    public async Task GetDate()
    {
        var client = AppFactory.CreateClient();
        var response = await client.GetAsync(new Uri("/Date", UriKind.Relative)).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
    }
}
