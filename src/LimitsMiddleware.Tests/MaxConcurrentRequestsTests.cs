namespace LimitsMiddleware
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Owin;
    using Xunit;

    public class MaxConcurrentRequestsTests
    {
        [Fact]
        public async Task When_max_concurrent_request_is_1_then_second_request_should_get_service_unavailable()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(1, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_2_then_second_request_should_get_ok()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(2, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_concurrent_request_is_0_then_second_request_should_get_ok()
        {
            var tcs = new TaskCompletionSource<int>();
            var client = CreateHttpClient(0, tcs.Task);
            var response1 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);
            var response2 = await client.GetAsync("/", HttpCompletionOption.ResponseHeadersRead);

            tcs.SetResult(0);

            response1.StatusCode.Should().Be(HttpStatusCode.OK);
            response2.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        private static HttpClient CreateHttpClient(int maxConcurrentRequests, Task waitHandle)
        {
            return CreateHttpClient(_ => maxConcurrentRequests, waitHandle);
        }

        private static HttpClient CreateHttpClient(
            Func<RequestContext, int> maxConcurrentRequests,
            Task waitHandle)
        {
            var app = new AppBuilder();
            app.MaxConcurrentRequests(maxConcurrentRequests)
                .Use(async (context, _) =>
                {
                    byte[] bytes = Enumerable.Repeat((byte) 0x1, 2).ToArray();
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength * 2;
                    context.Response.ContentType = "application/octet-stream";

                    // writing the response body flushes the headers
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    await waitHandle;
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                });
            return new HttpClient(new OwinHttpMessageHandler(app.Build()))
            {
                BaseAddress = new Uri("http://example.com")
            };
        }
    }
}