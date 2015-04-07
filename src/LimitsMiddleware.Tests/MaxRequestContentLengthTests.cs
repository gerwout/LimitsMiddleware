namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Microsoft.Owin.Builder;
    using Owin;
    using Xunit;

    public class MaxRequestContentLengthTests
    {
        [Fact]
        public async Task When_max_contentLength_is_20_and_a_get_request_is_coming_it_should_be_served()
        {
            var client = CreateHttpClient(20);

            HttpResponseMessage response = await client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }


        [Fact]
        public async Task
            When_max_contentLength_is_20_and_a_get_request_with_contentLength_header_is_coming_then_the_header_should_be_ignored_and_the_request_served()
        {
            var client = CreateHttpClient(20);
            client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Length", 10.ToString());

            HttpResponseMessage response = await client.GetAsync("/");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_head_request_then_it_should_be_served()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Head, "/");

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_post_with_contentLength_15_then_it_should_be_served()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Post, "/");
            AddContent(request, 5);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_post_with_contentLength_21_then_it_should_be_rejected()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Post, "/");
            AddContent(request, 21);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_put_with_contentLength_15_then_it_should_be_served()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Put, "/");
            AddContent(request, 15);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_put_with_contentLength_21_then_it_should_be_rejected()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Put, "/");
            AddContent(request, 21);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_patch_with_contentLength_15_then_it_should_be_served()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), "/");
            AddContent(request, 5);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_a_patch_with_contentLength_21_then_it_should_be_rejected()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), "/");
            AddContent(request, 21);

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_the_contentLength_header_in_a_post_is_absent_then_it_should_be_rejected()
        {
            var client = CreateHttpClient(20);

            HttpResponseMessage response = await client.PostAsync("/", null);

            response.StatusCode.Should().Be(HttpStatusCode.LengthRequired);
        }

        [Fact]
        public async Task When_max_contentLength_is_20_and_the_contentLength_header_is_not_a_valid_number_then_it_should_be_rejected()
        {
            var client = CreateHttpClient(20);
            var request = new HttpRequestMessage(HttpMethod.Post, "/");
            request.Content = new ByteArrayContent(new byte[] { 0 } );
            request.Content.Headers.TryAddWithoutValidation("Content-Length", "NotAValidNumber");

            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task When_max_contentLength_is_2_and_the_request_is_chunked_and_longer_it_should_be_rejected()
        {
            var client = CreateHttpClient(2);
            var request = new HttpRequestMessage(HttpMethod.Post, "/")
            {
                Content = new StringContent("4\r\nWiki\r\n5\r\npedia\r\ne\r\nin\r\n\r\nchunks.\r\n0\r\n\r\n")
            };
            request.Headers.TransferEncodingChunked = true;
            HttpResponseMessage response = await client.SendAsync(request);

            response.StatusCode.Should().Be(HttpStatusCode.RequestEntityTooLarge);
        }

        private static HttpClient CreateHttpClient(int maxContentLength)
        {
            return CreateHttpClient(_ => maxContentLength);
        }

        private static HttpClient CreateHttpClient(Func<RequestContext, int> getMaxContentLength)
        {
            var app = new AppBuilder();
            app.MaxRequestContentLength(getMaxContentLength)
                .Use(async (context, _) =>
                {
                    await new StreamReader(context.Request.Body).ReadToEndAsync();
                    await new StreamWriter(context.Response.Body).WriteAsync("Lorem ipsum");
                    context.Response.StatusCode = 200;
                    context.Response.ReasonPhrase = "OK";
                });
            return new HttpClient(new OwinHttpMessageHandler(app.Build()))
            {
                BaseAddress = new Uri("http://example.com")
            };
        }

        private static void AddContent(HttpRequestMessage request, int contentLength)
        {
            request.Content = new ByteArrayContent(Enumerable.Repeat(Byte.MinValue, contentLength).ToArray());
            request.Content.Headers.ContentLength = contentLength;
        }
    }
}