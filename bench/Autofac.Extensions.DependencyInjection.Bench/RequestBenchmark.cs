using BenchmarkDotNet.Attributes;
using BenchProject.AutofacApiServer;
using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Autofac.Extensions.DependencyInjection.Bench
{
    public class RequestBenchmark
    {
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
        public async Task Request_DefaultDI()
        {
            var response = await _defaultClient.GetAsync("/api/values");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpRequestException();
            }
        }

        [Benchmark]
        public async Task Request_AutofacDI()
        {
            var response = await _autofacClient.GetAsync("/api/values");

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
