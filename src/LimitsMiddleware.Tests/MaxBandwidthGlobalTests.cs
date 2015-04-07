namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Testing;
    using Owin;
    using Xunit;

    public class MaxBandwidthGlobalTests
    {
        [Fact]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer_for_multiple_requests()
        {
            int bandwidth = 0;
            // ReSharper disable once AccessToModifiedClosure - yeah we want to modify it...
            Func<int> getMaxBandwidth = () => bandwidth;

            var stopwatch = new Stopwatch();
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth))
            {
                stopwatch.Start();

                await httpClient.GetAsync("http://example.com");
                await httpClient.GetAsync("http://example.com");

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;

            bandwidth = 1024;
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth))
            {
                stopwatch.Restart();

                await httpClient.GetAsync("http://example.com");
                await httpClient.GetAsync("http://example.com");

                stopwatch.Stop();
            }
            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine(nolimitTimeSpan);
            Console.WriteLine(limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);
        }

        [Fact]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer_for_multiple_concurrent_requests()
        {
            int bandwidth = 0;
            // ReSharper disable once AccessToModifiedClosure - yeah we want to modify it...
            Func<int> getMaxBandwidth = () => bandwidth;
            var stopwatch = new Stopwatch();

            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth))
            {
                stopwatch.Start();

                await Task.WhenAll(httpClient.GetAsync("http://example.com"), httpClient.GetAsync("http://example.com"));

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;

            bandwidth = 1024;
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth))
            {
                stopwatch.Restart();

                await Task.WhenAll(httpClient.GetAsync("http://example.com"), httpClient.GetAsync("http://example.com"));

                stopwatch.Stop();
            }
            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine(nolimitTimeSpan);
            Console.WriteLine(limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);
        }


        private static HttpClient CreateHttpClient(Func<int> getMaxBytesPerSecond)
        {
            return TestServer.Create(builder => builder
                .MaxBandwidthGlobal(getMaxBytesPerSecond)
                .Use(async (context, _) =>
                {
                    byte[] bytes = Enumerable.Repeat((byte)0x1, 2048).ToArray();
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength;
                    context.Response.ContentType = "application/octet-stream";
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                })).HttpClient;
        }
         
    }
}