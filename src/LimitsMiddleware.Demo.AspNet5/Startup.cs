using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Http;
using Microsoft.Framework.DependencyInjection;

namespace LimitsMiddleware.Demo.AspNet5
{
    using System;
    using System.Collections.Specialized;
    using System.IO;

    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
        }

        public void Configure(IApplicationBuilder app)
        {
            app.MaxUrlLength(100);

            app.MaxQueryStringLength(80);
            
            app.MaxConcurrentRequests(4);

            app.MinResponseDelay(context =>
            {
                var queryString = ParseQueryString(QueryString.FromUriComponent(context.Uri).Value);
                var minResponseDelayParam = queryString.Get("minresponsedelay");
                int minResponseDelay;
                return int.TryParse(minResponseDelayParam, out minResponseDelay)
                    ? TimeSpan.FromSeconds(minResponseDelay)
                    : TimeSpan.Zero;
            });           
           
            app.MaxBandwidthPerRequest(context =>
            {
                var queryString = ParseQueryString(QueryString.FromUriComponent(context.Uri).Value);
                var maxBandwidthParam = queryString.Get("maxbandwidthperrequest");
                int maxBandwidth;
                return int.TryParse(maxBandwidthParam, out maxBandwidth)
                    ? maxBandwidth
                    : -1;
            });

            app.MaxBandwidthGlobal(10 * 1024 * 1024);

            app.Use(async (context, next) =>
            {
                if (!context.Request.Path.Equals("/file", StringComparison.OrdinalIgnoreCase))
                {
                    await next();
                    return;
                }
                int size;
                if (!int.TryParse(context.Request.Query.Get("size"), out size))
                {
                    await next();
                    return;
                }

                context.Response.ContentLength = size;
                context.Response.ContentType = "application/octect-stream";
                context.Response.StatusCode = 200;
                var buffer = new byte[16384];
                int count = 0;
                while (count < size)
                {
                    int length = Math.Min(buffer.Length, size - count);
                    await context.Response.Body.WriteAsync(buffer, 0, length, context.RequestAborted);
                    count += length;
                }
            });

            app.Use(async (context, next) =>
            {
                context.Response.ContentType = "text/html";
                context.Response.StatusCode = 200;
                var index = File.ReadAllText("index.html");
                await context.Response.WriteAsync(index);
            });
        }

        // There is no HttpUtility.ParseQueryString yet in AspNet5. Doing this as a lame interm thing.
        private static NameValueCollection ParseQueryString(string queryString)
        {
            var result = new NameValueCollection();
            var pairs = queryString.Split('&');
            foreach(var pair in pairs)
            {
                var nameAndValue = pair.Split('=');
                result.Add(nameAndValue[0], nameAndValue[1]);
            }
            return result;
        }
    }
}
