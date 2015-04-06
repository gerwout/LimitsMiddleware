namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Testing;
    using Owin;
    using Xunit;

    public class ConnectionTimeoutTests
    {
        [Fact]
        public void When_time_out_delegate_is_null_then_should_throw()
        {
            Action act = () => Limits.ConnectionTimeout((Func<TimeSpan>) null);

            act.ShouldThrow<ArgumentNullException>();
        }

        [Fact]
        public async Task When_time_out_is_triggered_then_should_throw()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
            Func<TimeSpan> getConnectionTimeout = () => timeout;
            HttpClient httpClient = CreateHttpClient(getConnectionTimeout);

            var stream = new DelayedReadStream(timeout.Add(TimeSpan.FromMilliseconds(500)));
            stream.Write(new byte[2048], 0, 2048);
            stream.Position = 0;
            await ThrowsAsync<ObjectDisposedException>(() => httpClient.PostAsync("http://example.com", new StreamContent(stream)));
        }

        [Fact]
        public async Task When_time_out_is_not_triggered_then_get_OK()
        {
            TimeSpan timeout = TimeSpan.FromMilliseconds(1000);
            Func<TimeSpan> getConnectionTimeout = () => timeout;
            HttpClient httpClient = CreateHttpClient(getConnectionTimeout);

            var stream = new DelayedReadStream(timeout.Add(TimeSpan.FromMilliseconds(-500)));
            stream.Write(new byte[2048], 0, 2048);
            stream.Position = 0;
            HttpResponseMessage response = await httpClient.PostAsync("http://example.com", new StreamContent(stream));
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task request_context_is_not_null()
        {
            bool requestContextIsNull = true;

            using (var client = CreateHttpClient(context =>
            {
                requestContextIsNull = context == null;
                return TimeSpan.FromSeconds(1);
            }))
            {
                await client.GetAsync("http://example.com");
            }

            requestContextIsNull.Should().BeFalse();
        }

        private static HttpClient CreateHttpClient(Func<TimeSpan> getConnectionTimeout)
        {
            return CreateHttpClient(_ => getConnectionTimeout());
        }

        private static HttpClient CreateHttpClient(Func<RequestContext, TimeSpan> getConnectionTimeout)
        {
            return TestServer.Create(builder => builder
                .ConnectionTimeout(getConnectionTimeout)
                .Use(async (context, _) =>
                {
                    var buffer = new byte[1024];
                    await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
                    byte[] bytes = Enumerable.Repeat((byte) 0x1, 1024).ToArray();
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                    context.Response.ContentLength = bytes.LongLength;
                    context.Response.ContentType = "application/octet-stream";
                    await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
                })).HttpClient;
        }

        private static async Task ThrowsAsync<TException>(Func<Task> func)
        {
            Type expected = typeof (TException);
            Type actual = null;
            try
            {
                await func();
            }
            catch (Exception e)
            {
                actual = e.GetType();
            }
            expected.Should().Be(actual);
        }

        private class DelayedReadStream : MemoryStream
        {
            private readonly TimeSpan _delay;

            public DelayedReadStream(TimeSpan delay)
            {
                _delay = delay;
            }

            public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                await Task.Delay(_delay, cancellationToken);
                return await base.ReadAsync(buffer, offset, count, cancellationToken);
            }
        }
    }
}