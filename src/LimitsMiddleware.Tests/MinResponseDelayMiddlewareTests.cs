namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Owin;
    using Xunit;

    public class MinResponseDelayMiddlewareTests
    {
        [Fact]
        public async Task When_response_delay_is_applied_then_time_to_receive_data_should_be_longer()
        {
            var stopwatch = new Stopwatch();

            using (var client = CreateHttpClient(() => TimeSpan.Zero))
            {
                stopwatch.Start();

                await client.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            TimeSpan noLimitTimespan = stopwatch.Elapsed;

            using (var client = CreateHttpClient(() => TimeSpan.FromMilliseconds(10)))
            {
                stopwatch.Start();

                await client.GetAsync("http://example.com");

                stopwatch.Stop();
            }

            TimeSpan limitTimespan = stopwatch.Elapsed;

            limitTimespan.Should().BeGreaterThan(noLimitTimespan);
        }

        private static HttpClient CreateHttpClient(Func<TimeSpan> getMinDelay)
        {
            var app = new AppBuilder();
            app.MinResponseDelay(getMinDelay)
                .Use((context, _) =>
                {
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";

                    return Task.FromResult(0);
                });
            return new HttpClient(new OwinHttpMessageHandler(app.Build()));
        }
    }
}