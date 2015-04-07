namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Owin;
    using Xunit;

    public class MaxBandwidthPerRequestTests
    {
        [Fact]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer()
        {
            int maxBitsPerSecond = 0;
            // ReSharper disable once AccessToModifiedClosure - yeah we want to modify it...
            Func<int> getMaxBitsPerSecond = () => maxBitsPerSecond;
            var stopwatch = new Stopwatch();

            using (HttpClient httpClient = CreateHttpClient(getMaxBitsPerSecond))
            {
                stopwatch.Start();
                
                await httpClient.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;
            maxBitsPerSecond = 1024 * 8;

            using (HttpClient httpClient = CreateHttpClient(getMaxBitsPerSecond))
            {
                stopwatch.Restart();

                await httpClient.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine(nolimitTimeSpan);
            Console.WriteLine(limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);
        }

        private static HttpClient CreateHttpClient(Func<int> getMaxBitsPerSecond)
        {
            return CreateHttpClient(_ => getMaxBitsPerSecond());
        }

        private static HttpClient CreateHttpClient(Func<RequestContext, int> getMaxBitsPerSecond)
        {
            var app = new AppBuilder();
            app.MaxBandwidthPerRequest(getMaxBitsPerSecond)
                .Use(async (context, _) =>
                {
                    byte[] bytes = Enumerable.Repeat((byte) 0x1, 1024).ToArray();
                    int batches = 3;
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength;
                    context.Response.ContentType = "application/octet-stream";
                    for (int i = 0; i < batches; i++)
                    {
                        await Task.Delay(1);
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    }
                });
            return new HttpClient(new OwinHttpMessageHandler(app.Build()))
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}