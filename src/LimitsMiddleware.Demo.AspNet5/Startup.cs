namespace LimitsMiddleware.Demo.AspNet5
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using Microsoft.AspNet.Builder;
    using Microsoft.AspNet.Http;
    using Microsoft.AspNet.WebUtilities;
    using Microsoft.Framework.DependencyInjection;

    public class Startup
    {
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {}

        public void Configure(IApplicationBuilder app)
        {
            app.MaxUrlLength(100);

            app.MaxQueryStringLength(80);

            app.MaxConcurrentRequests(4);

            app.MinResponseDelay(context =>
            {
                var queryStringDictionary = QueryHelpers.ParseQuery(QueryString.FromUriComponent(context.Uri).Value);
                string[] values;
                if(!queryStringDictionary.TryGetValue("minresponsedelay", out values))
                {
                    return TimeSpan.Zero;
                }
                int minResponseDelay;
                return int.TryParse(values[0], out minResponseDelay)
                    ? TimeSpan.FromSeconds(minResponseDelay)
                    : TimeSpan.Zero;
            });

            app.MaxBandwidthPerRequest(context =>
            {
                var queryStringDictionary = QueryHelpers.ParseQuery(QueryString.FromUriComponent(context.Uri).Value);
                string[] values;
                if (!queryStringDictionary.TryGetValue("maxbandwidthperrequest", out values))
                {
                    return -1;
                }
                int maxBandwidth;
                return int.TryParse(values[0], out maxBandwidth)
                    ? maxBandwidth
                    : -1;
            });

            app.MaxBandwidthGlobal(10*1024*1024);

            app.Use(async (context, next) =>
            {
                if(!context.Request.Path.Value.Equals("/file", StringComparison.OrdinalIgnoreCase))
                {
                    await next();
                    return;
                }
                int size;
                if(!int.TryParse(context.Request.Query.Get("size"), out size))
                {
                    await next();
                    return;
                }

                context.Response.ContentLength = size;
                context.Response.ContentType = "application/octect-stream";
                context.Response.StatusCode = 200;
                var buffer = new byte[16384];
                var count = 0;
                while(count < size)
                {
                    var length = Math.Min(buffer.Length, size - count);
                    await context.Response.Body.WriteAsync(buffer, 0, length, context.RequestAborted);
                    await context.Response.Body.FlushAsync();
                    count += length;
                }
            });
        }
    }
}