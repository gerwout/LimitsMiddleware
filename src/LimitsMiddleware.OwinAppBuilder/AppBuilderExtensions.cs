namespace Owin
{
    using System;
    using LimitsMiddleware;

    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;

    /// <summary>
    /// Represents a set of extension methods around <see cref="IAppBuilder"/> that expose limits middleware.
    /// </summary>
    public static partial class AppBuilderExtensions
    {
        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, TimeSpan timeout)
        {
            app.MustNotNull("app");

            app.Use(Limits.ConnectionTimeout(timeout));

            return app;
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, Func<TimeSpan> getTimeout)
        {
            app.MustNotNull("app");
            getTimeout.MustNotNull("getTimeout");

            app.Use(Limits.ConnectionTimeout(getTimeout));

            return app;
        }


        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, Func<RequestContext, TimeSpan> getTimeout)
        {
            app.MustNotNull("app");
            getTimeout.MustNotNull("getTimeout");

            app.Use(Limits.ConnectionTimeout(getTimeout));

            return app;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxBitsPerSecond">The maximum number of bits per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, int maxBitsPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthPerRequest(app, () => maxBitsPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxBitsPerSecond">A delegate to retrieve the maximum number of bits per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<int> getMaxBitsPerSecond)
        {
            app.MustNotNull("app");

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBitsPerSecond));
            return app;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBitsPerSecond">A delegate to retrieve the maximum number of bits per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBitsPerSecond</exception>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<RequestContext, int> getMaxBitsPerSecond)
        {
            app.MustNotNull("app");
            getMaxBitsPerSecond.MustNotNull("getMaxBitsPerSecond");

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBitsPerSecond));
            return app;
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests. Use 0 or a negative
        /// number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, int maxConcurrentRequests)
        {
            app.MustNotNull("app");

            return MaxConcurrentRequests(app, () => maxConcurrentRequests);
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, Func<int> getMaxConcurrentRequests)
        {
            app.MustNotNull("app");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests));

            return app;
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, Func<RequestContext, int> getMaxConcurrentRequests)
        {
            app.MustNotNull("app");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests));

            return app;
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, int maxQueryStringLength)
        {
            app.MustNotNull("app");

            return MaxQueryStringLength(app, () => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, Func<int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }


        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, Func<RequestContext, int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, int maxContentLength)
        {
            app.MustNotNull("app");

            return MaxRequestContentLength(app, () => maxContentLength);
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, Func<int> getMaxContentLength)
        {
            app.MustNotNull("app");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength));

            return app;
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, Func<RequestContext, int> getMaxContentLength)
        {
            app.MustNotNull("app");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength));

            return app;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, int maxUrlLength)
        {
            app.MustNotNull("app");

            return MaxUrlLength(app, () => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, Func<int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, Func<RequestContext, int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }
    }
}