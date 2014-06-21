namespace Owin
{
    using System;
    using LimitsMiddleware;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;
    using BuildFunc = System.Action<
        System.Func<
            System.Collections.Generic.IDictionary<string, object>,
            System.Func<
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >>>;

    public static class AppBuilderExtensions
    {
        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder builder, TimeSpan timeout)
        {
            builder.MustNotNull("builder");

            return ConnectionTimeout(builder, () => timeout);
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder builder, Func<TimeSpan> getTimeout)
        {
            builder.MustNotNull("builder");
            getTimeout.MustNotNull("getTimeout");

            return ConnectionTimeout(builder, new ConnectionTimeoutOptions(getTimeout));
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The connection timeout options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder builder, ConnectionTimeoutOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
                .UseOwin()
                .ConnectionTimeout(options);

            return builder;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxBandwidth(this IAppBuilder builder, int maxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidth(builder, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The builder instance.</returns>
        public static IAppBuilder MaxBandwidth(this IAppBuilder builder, Func<int> getMaxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidth(builder, new MaxBandwidthOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The max bandwith options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder MaxBandwidth(this IAppBuilder builder, MaxBandwidthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
                .UseOwin()
                .MaxBandwidth(options);
            return builder;
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests. Use 0 or a negative
        /// number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder builder, int maxConcurrentRequests)
        {
            builder.MustNotNull("builder");

            return MaxConcurrentRequests(builder, () => maxConcurrentRequests);
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder builder, Func<int> getMaxConcurrentRequests)
        {
            builder.MustNotNull("builder");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            return MaxConcurrentRequests(builder, new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The max concurrent request options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder builder, MaxConcurrentRequestOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
               .UseOwin()
               .MaxConcurrentRequests(options);

            return builder;
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder builder, int maxQueryStringLength)
        {
            builder.MustNotNull("builder");

            return MaxQueryStringLength(builder, () => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder builder, Func<int> getMaxQueryStringLength)
        {
            builder.MustNotNull("builder");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            return MaxQueryStringLength(builder, new MaxQueryStringLengthOptions(getMaxQueryStringLength));
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The max querystring length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder builder, MaxQueryStringLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
               .UseOwin()
               .MaxQueryStringLength(options);

            return builder;
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder builder, int maxContentLength)
        {
            builder.MustNotNull("builder");

            return MaxRequestContentLength(builder, () => maxContentLength);
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder builder, Func<int> getMaxContentLength)
        {
            builder.MustNotNull("builder");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            return MaxRequestContentLength(builder, new MaxRequestContentLengthOptions(getMaxContentLength));
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The max request content length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder builder, MaxRequestContentLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
               .UseOwin()
               .MaxRequestContentLength(options);

            return builder;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder builder, int maxUrlLength)
        {
            builder.MustNotNull("builder");

            return MaxUrlLength(builder, () => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder builder, Func<int> getMaxUrlLength)
        {
            builder.MustNotNull("builder");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            return MaxUrlLength(builder, new MaxUrlLengthOptions(getMaxUrlLength));
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The IAppBuilder instance.</param>
        /// <param name="options">The max url length options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static IAppBuilder MaxUrlLength(this IAppBuilder builder, MaxUrlLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder
                .UseOwin()
                .MaxUrlLength(options);
            return builder;
        }

        private static BuildFunc UseOwin(this IAppBuilder builder)
        {
            return middleware => builder.Use(middleware(builder.Properties));
        }
    }
}