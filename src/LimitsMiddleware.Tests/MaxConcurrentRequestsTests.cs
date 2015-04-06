namespace LimitsMiddleware
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Testing;
    using Owin;
    using Xunit;

    public class MaxConcurrentRequestsTests
    {
        [Fact]
        public async Task When_max_concurrent_request_is_1_then_second_request_should_get_service_unavailable()
        {
            HttpClient httpClient = CreateHttpClient(1);
            Task<HttpResponseMessage> request1 = httpClient.GetAsync("http://example.com");
            Task<HttpResponseMessage> request2 = httpClient.GetAsync("http://example.com");

            await Task.WhenAll(request1, request2);

            request1.Result.StatusCode.Should().Be(HttpStatusCode.OK);
            request2.Result.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_2_then_second_request_should_get_ok()
        {
            HttpClient httpClient = CreateHttpClient(2);
            Task<HttpResponseMessage> request1 = httpClient.GetAsync("http://example.com");
            Task<HttpResponseMessage> request2 = httpClient.GetAsync("http://example.com");

            await Task.WhenAll(request1, request2);

            request1.Result.StatusCode.Should().Be(HttpStatusCode.OK);
            request2.Result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_0_then_second_request_should_get_ok()
        {
            HttpClient httpClient = CreateHttpClient(0);
            Task<HttpResponseMessage> request1 = httpClient.GetAsync("http://example.com");
            Task<HttpResponseMessage> request2 = httpClient.GetAsync("http://example.com");

            await Task.WhenAll(request1, request2);

            request1.Result.StatusCode.Should().Be(HttpStatusCode.OK);
            request2.Result.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task request_context_is_not_null()
        {
            bool requestContextIsNull = true;

            using (var client = CreateHttpClient(context =>
            {
                requestContextIsNull = context == null;
                return 1;
            }))
            {
                await client.GetAsync("http://example.com");
            }

            requestContextIsNull.Should().BeFalse();
        }

        private static HttpClient CreateHttpClient(int maxConcurrentRequests)
        {
            return CreateHttpClient(_ => maxConcurrentRequests);
        }

        private static HttpClient CreateHttpClient(Func<RequestContext, int> maxConcurrentRequests)
        {
            return TestServer.Create(builder => builder
                .MaxConcurrentRequests(maxConcurrentRequests)
                .Use(async (context, _) =>
                {
                    byte[] bytes = Enumerable.Repeat((byte) 0x1, 2).ToArray();
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength;
                    context.Response.ContentType = "application/octet-stream";
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                })).HttpClient;
        }
    }
}