namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Owin;
    using Xunit;

    public class MaxBandwidthGlobalTests
    {
        [Fact]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer_for_multiple_concurrent_requests()
        {
            int bandwidth = 0;
            // ReSharper disable once AccessToModifiedClosure - yeah we want to modify it...
            Func<int> getMaxBandwidth = () => bandwidth;
            var stopwatch = new Stopwatch();

            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth, 2))
            {
                stopwatch.Start();

                await Task.WhenAll(httpClient.GetAsync("/"), httpClient.GetAsync("/"));

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;

            bandwidth = 2048;
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth, 2))
            {
                stopwatch.Restart();

                await Task.WhenAll(httpClient.GetAsync("/"), httpClient.GetAsync("/"));

                stopwatch.Stop();
            }
            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine(nolimitTimeSpan);
            Console.WriteLine(limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);
        }

        [Fact]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer_for_multiple_requests()
        {
            int bandwidth = 0;
            // ReSharper disable once AccessToModifiedClosure - yeah we want to modify it...
            Func<int> getMaxBandwidth = () => bandwidth;

            var stopwatch = new Stopwatch();
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth, 1))
            {
                stopwatch.Start();

                await httpClient.GetAsync("/");
                await httpClient.GetAsync("/");

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;

            bandwidth = 1024;
            using (HttpClient httpClient = CreateHttpClient(getMaxBandwidth, 1))
            {
                stopwatch.Restart();

                await httpClient.GetAsync("/");
                await httpClient.GetAsync("/");

                stopwatch.Stop();
            }
            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine(nolimitTimeSpan);
            Console.WriteLine(limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);
        }

        private static HttpClient CreateHttpClient(Func<int> getMaxBytesPerSecond, int forceConcurrentCount)
        {
            // This blocks client responses until the number of concurrent clients 
            // has reached the desired value.
            var tcs = new TaskCompletionSource<int>();
            Action requestReceived = () =>
            {
                if (Interlocked.Decrement(ref forceConcurrentCount) == 0)
                {
                    tcs.SetResult(0);
                }
            };

            var app = new AppBuilder();
            app.MaxBandwidthGlobal(getMaxBytesPerSecond)
                .Use(async (context, _) =>
                {
                    requestReceived();
                    var delayTask = Task.Delay(5000);
                    if (await Task.WhenAny(delayTask, tcs.Task) == delayTask)
                    {
                        throw new TimeoutException("Timedout waiting for concurrent clients.");
                    }

                    byte[] bytes = Enumerable.Repeat((byte) 0x1, 1024).ToArray();
                    const int batches = 10;
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength * batches;
                    context.Response.ContentType = "application/octet-stream";
                    for (int i = 0; i < batches; i++)
                    {
                        await Task.Delay(1); //forces actual asynchrony
                        await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                    }
                });
            return new HttpClient(new OwinHttpMessageHandler(app.Build()))
            {
                BaseAddress = new Uri("http://example.com")
            };
        }
    }
}