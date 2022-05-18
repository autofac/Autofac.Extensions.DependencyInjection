// Copyright (c) Autofac Project. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System.Diagnostics.CodeAnalysis;
using System.Net;
using BenchProject.AutofacApiServer;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    [SuppressMessage("CA1001", "CA1001", Justification = "Benchmark disposal happens in a global cleanup method.")]
    public class RequestBenchmark
    {
        private static readonly Uri ValuesUri = new("/api/values", UriKind.Relative);
        private WebApplicationFactory<DefaultStartup> _defaultFactory;
        private WebApplicationFactory<DefaultStartup> _autofacFactory;
        private HttpClient _defaultClient;
        private HttpClient _autofacClient;

        [GlobalSetup]
        public void Setup()
        {
            _defaultFactory = new WebApplicationFactory<DefaultStartup>();
            _autofacFactory = new AutofacWebApplicationFactory<DefaultStartup>();

            _defaultClient = _defaultFactory.CreateClient();
            _autofacClient = _autofacFactory.CreateClient();
        }

        [Benchmark(Baseline = true)]
        public async Task RequestDefaultDI()
        {
            var response = await _defaultClient.GetAsync(ValuesUri).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException();
            }
        }

        [Benchmark]
        public async Task RequestAutofacDI()
        {
            var response = await _autofacClient.GetAsync(ValuesUri).ConfigureAwait(false);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException();
            }
        }

        [GlobalCleanup]
        public void Cleanup()
        {
            _defaultFactory.Dispose();
            _autofacFactory.Dispose();
        }
    }
}
