namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Microsoft.Owin.FileSystems;
    using Microsoft.Owin.StaticFiles;
    using Owin;
    using Xunit;

    public class MaxBandwidthPerRequestTests
    {
        [Theory]
        [InlineData("file_64KB.txt", 8000, 8)]
        [InlineData("file_64KB.txt", 16000, 4)]
        [InlineData("file_512KB.txt", 100000, 5)]
        [InlineData("file_512KB.txt", 200000, 2)]
        public async Task When_bandwidth_is_applied_then_time_to_receive_data_should_be_longer(
            string file,
            int maxKiloBytesPerSeconds,
            int approximateSeconds)
        {
            var stopwatch = new Stopwatch();
            using (HttpClient httpClient = CreateHttpClient())
            {
                stopwatch.Start();

                var response = await httpClient.GetAsync(file);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                stopwatch.Stop();
            }
            TimeSpan nolimitTimeSpan = stopwatch.Elapsed;
            
            using (HttpClient httpClient = CreateHttpClient(maxKiloBytesPerSeconds))
            {
                stopwatch.Restart();

                var response = await httpClient.GetAsync(file);
                response.StatusCode.Should().Be(HttpStatusCode.OK);

                stopwatch.Stop();
            }


            TimeSpan limitedTimeSpan = stopwatch.Elapsed;

            Console.WriteLine("No limits: {0}", nolimitTimeSpan);
            Console.WriteLine("Limited  : {0}", limitedTimeSpan);

            limitedTimeSpan.Should().BeGreaterThan(nolimitTimeSpan);

            var abs = Math.Abs((limitedTimeSpan.TotalSeconds - nolimitTimeSpan.TotalSeconds) - approximateSeconds);
            (abs < 1).Should().BeTrue("value {0} >= 1", abs);
        }

        private static HttpClient CreateHttpClient(int maxKiloBytesPerSecond = -1)
        {
            var staticFileOptions = new StaticFileOptions
            {
                FileSystem = new EmbeddedResourceFileSystem(
                    typeof(MaxBandwidthPerRequestTests).Assembly,
                    "LimitsMiddleware.Files")
            };
            var appFunc = new AppBuilder()
                .MaxBandwidthPerRequest(maxKiloBytesPerSecond)
                .UseStaticFiles(staticFileOptions)
                .Build();

            return new HttpClient(new OwinHttpMessageHandler(appFunc))
            {
                BaseAddress = new Uri("http://localhost")
            };
        }
    }
}