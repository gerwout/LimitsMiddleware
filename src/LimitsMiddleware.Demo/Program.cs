namespace LimitsMiddleware.Demo
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using Microsoft.Owin.Builder;
    using Mono.Web;
    using Nowin;
    using Owin;
    using Serilog;
    using Serilog.Events;

    internal class Program
    {
        private static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo
                .Console()
                .MinimumLevel.Verbose()
                .CreateLogger();

            var app = new AppBuilder();

            app.MaxQueryStringLength(80);

            app.MaxConcurrentRequests(4);

            app.MinResponseDelay(context =>
            {
                var queryParams = HttpUtility.ParseQueryString(context.Uri.Query);
                var minResponseDelayParam = queryParams.Get("minresponsedelay");
                int minResponseDelay;
                return int.TryParse(minResponseDelayParam, out minResponseDelay) 
                    ? TimeSpan.FromSeconds(minResponseDelay) 
                    : TimeSpan.Zero;
            });

            app.MaxBandwidthPerRequest(context =>
            {
                var queryParams = HttpUtility.ParseQueryString(context.Uri.Query);
                var maxBandwidthParam = queryParams.Get("maxbandwidthperrequest");
                int maxBandwidth;
                return int.TryParse(maxBandwidthParam, out maxBandwidth)
                    ? maxBandwidth
                    : -1;
            });

            app.MaxBandwidthGlobal(10000);


            app.Use(async (context, next) =>
            {
                if(!context.Request.Uri.AbsolutePath.Equals("/file", StringComparison.OrdinalIgnoreCase))
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
                context.Response.ReasonPhrase = "OK";
                var buffer = new byte[16384];
                int count = 0;
                while(count < size)
                {
                    int length = Math.Min(buffer.Length, size - count);
                    await context.Response.WriteAsync(buffer, 0, length, context.Request.CallCancelled);
                    count += length;
                }
            });

            app.Use(async (context, next) =>
            {
                context.Response.ContentType = "text/html";
                context.Response.StatusCode = 200;
                context.Response.ReasonPhrase = "OK";
                var index = File.ReadAllText("index.html");
                await context.Response.WriteAsync(index);
            });

            var server = ServerBuilder.New()
                .SetEndPoint(new IPEndPoint(IPAddress.Loopback, 8080))
                .SetOwinApp(app.Build());

            using(server.Build())
            {
                server.Start();
                Process.Start("http://localhost:8080");
                Log.Information("Server running http://localhost:8080");
                Console.ReadLine();
            }
        }
    }
}