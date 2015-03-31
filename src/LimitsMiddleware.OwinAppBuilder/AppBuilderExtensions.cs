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

            return ConnectionTimeout(app, () => timeout);
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

            return ConnectionTimeout(app, new ConnectionTimeoutOptions(getTimeout));
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="options">The connection timeout options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, ConnectionTimeoutOptions options)
        {
            app.MustNotNull("app");
            options.MustNotNull("options");

            app.Use(Limits.ConnectionTimeout(options));

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
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, int maxBytesPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthPerRequest(app, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthPerRequest(app, new MaxBandwidthPerRequestOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="perRequestOptions">The max bandwith options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, MaxBandwidthPerRequestOptions perRequestOptions)
        {
            app.MustNotNull("app");
            perRequestOptions.MustNotNull("options");

            app.Use(Limits.MaxBandwidthPerRequest(perRequestOptions));
            return app;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<RequestContext, int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBytesPerSecond));
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

            return MaxConcurrentRequests(app, new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="options">The max concurrent request options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, MaxConcurrentRequestOptions options)
        {
            app.MustNotNull("app");
            options.MustNotNull("options");

            app.Use(Limits.MaxConcurrentRequests(options));

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

            return MaxQueryStringLength(app, new MaxQueryStringLengthOptions(getMaxQueryStringLength));
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="options">The max querystring length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, MaxQueryStringLengthOptions options)
        {
            app.MustNotNull("app");
            options.MustNotNull("options");

            app.Use(Limits.MaxQueryStringLength(options));

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

            return MaxRequestContentLength(app, new MaxRequestContentLengthOptions(getMaxContentLength));
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="options">The max request content length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, MaxRequestContentLengthOptions options)
        {
            app.MustNotNull("app");
            options.MustNotNull("options");

            app.Use(Limits.MaxRequestContentLength(options));

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

            return MaxUrlLength(app, new MaxUrlLengthOptions(getMaxUrlLength));
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="options">The max url length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, MaxUrlLengthOptions options)
        {
            app.MustNotNull("app");
            options.MustNotNull("options");

            app.Use(Limits.MaxUrlLength(options));
            return app;
        }
    }
}