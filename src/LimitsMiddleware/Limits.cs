namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LimitsMiddleware.LibOwin;
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

    /// <summary>
    /// Represent a set of middleware functions to apply limits to an OWIN pipeline.
    /// </summary>
    public static class Limits
    {
        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc ConnectionTimeout(TimeSpan timeout)
        {
            timeout.MustNotNull("options");

            return ConnectionTimeout(() => timeout);
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static MidFunc ConnectionTimeout(Func<TimeSpan> getTimeout)
        {
            getTimeout.MustNotNull("getTimeout");

            return ConnectionTimeout(new ConnectionTimeoutOptions(getTimeout));
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="options">The connection timeout options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc ConnectionTimeout(ConnectionTimeoutOptions options)
        {
            options.MustNotNull("options");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    options.Tracer.AsVerbose("Configure timeouts.");
                    TimeSpan connectionTimeout = options.Timeout;
                    context.Request.Body = new TimeoutStream(requestBodyStream, connectionTimeout, options.Tracer);
                    context.Response.Body = new TimeoutStream(responseBodyStream, connectionTimeout, options.Tracer);

                    options.Tracer.AsVerbose("Request with configured timeout forwarded.");
                    return next(env);
                };
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc ConnectionTimeout(this BuildFunc builder, TimeSpan timeout)
        {
            builder.MustNotNull("builder");
            
            return ConnectionTimeout(builder, () => timeout);
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static BuildFunc ConnectionTimeout(this BuildFunc builder, Func<TimeSpan> getTimeout)
        {
            builder.MustNotNull("builder");
            getTimeout.MustNotNull("getTimeout");

            return ConnectionTimeout(builder, new ConnectionTimeoutOptions(getTimeout));
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The connection timeout options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc ConnectionTimeout(this BuildFunc builder, ConnectionTimeoutOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => ConnectionTimeout(options));
            return builder;
        }

        /// <summary>
        /// Limits the bandwith used per request by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static MidFunc MaxBandwidth(int maxBytesPerSecond)
        {
            return MaxBandwidthPerRequest(maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used per request by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static MidFunc MaxBandwidth(Func<int> getMaxBytesPerSecond)
        {
            return MaxBandwidthPerRequest(getMaxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith per request used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="options">The max bandwidth options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static MidFunc MaxBandwidth(MaxBandwidthOptions options)
        {
            return MaxBandwidthPerRequest(options);
        }

        /// <summary>
        /// Limits the bandwith used per request by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static BuildFunc MaxBandwidth(this BuildFunc builder, int maxBytesPerSecond)
        {
            return MaxBandwidthPerRequest(builder, maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used per request by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static BuildFunc MaxBandwidth(this BuildFunc builder, Func<int> getMaxBytesPerSecond)
        {
            return MaxBandwidthPerRequest(builder, getMaxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max bandwith options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        [Obsolete("Use MaxBandwidthPerRequest instead.")]
        public static BuildFunc MaxBandwidth(this BuildFunc builder, MaxBandwidthOptions options)
        {
            return MaxBandwidthPerRequest(builder, options);
        }


        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxBandwidthPerRequest(int maxBytesPerSecond)
        {
            return MaxBandwidthPerRequest(() => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<int> getMaxBytesPerSecond)
        {
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            return MaxBandwidthPerRequest(new MaxBandwidthOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<RequestContext, int> getMaxBytesPerSecond)
        {
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            return MaxBandwidthPerRequest(new MaxBandwidthOptions(getMaxBytesPerSecond));
        }
        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="options">The max bandwidth options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxBandwidthPerRequest(MaxBandwidthOptions options)
        {
            options.MustNotNull("options");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    var limitsRequestContext = new RequestContext(context.Request);

                    options.Tracer.AsVerbose("Configure streams to be limited.");
                    context.Request.Body = new ThrottledStream(requestBodyStream, new RateLimiter(() => options.GetMaxBytesPerSecond(limitsRequestContext)));
                    context.Response.Body = new ThrottledStream(responseBodyStream, new RateLimiter(() => options.GetMaxBytesPerSecond(limitsRequestContext)));

                    //TODO consider SendFile interception
                    options.Tracer.AsVerbose("With configured limit forwarded.");
                    return next(env);
                };
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxBandwidthPerRequest(this BuildFunc builder, int maxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidthPerRequest(builder, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static BuildFunc MaxBandwidthPerRequest(this BuildFunc builder, Func<int> getMaxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidthPerRequest(builder, new MaxBandwidthOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static BuildFunc MaxBandwidthPerRequest(this BuildFunc builder, Func<RequestContext, int> getMaxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidthPerRequest(builder, new MaxBandwidthOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max bandwith options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxBandwidthPerRequest(this BuildFunc builder, MaxBandwidthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxBandwidthPerRequest(options));
            return builder;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxBandwidthGlobal(int maxBytesPerSecond)
        {
            return MaxBandwidthGlobal(() => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthGlobal(Func<int> getMaxBytesPerSecond)
        {
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            return MaxBandwidthGlobal(new MaxBandwidthGlobalOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="options">The max bandwidth options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxBandwidthGlobal(MaxBandwidthGlobalOptions options)
        {
            options.MustNotNull("options");

            var rateLimiter = new RateLimiter(() => options.MaxBytesPerSecond);

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    options.Tracer.AsVerbose("Configure streams to be limited.");
                    context.Request.Body = new ThrottledStream(requestBodyStream, rateLimiter);
                    context.Response.Body = new ThrottledStream(responseBodyStream, rateLimiter);

                    //TODO consider SendFile interception
                    options.Tracer.AsVerbose("With configured limit forwarded.");
                    return next(env);
                };
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxBandwidthGlobal(this BuildFunc builder, int maxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidthGlobal(builder, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static BuildFunc MaxBandwidthGlobal(this BuildFunc builder, Func<int> getMaxBytesPerSecond)
        {
            builder.MustNotNull("builder");

            return MaxBandwidthGlobal(builder, new MaxBandwidthGlobalOptions(getMaxBytesPerSecond));
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max bandwith options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxBandwidthGlobal(this BuildFunc builder, MaxBandwidthGlobalOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxBandwidthGlobal(options));
            return builder;
        }
        
        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests. Use 0 or a negative
        /// number to specify unlimited number of concurrent requests.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxConcurrentRequests(int maxConcurrentRequests)
        {
            return MaxConcurrentRequests(() => maxConcurrentRequests);
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static MidFunc MaxConcurrentRequests(Func<int> getMaxConcurrentRequests)
        {
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            return MaxConcurrentRequests(new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static MidFunc MaxConcurrentRequests(Func<RequestContext, int> getMaxConcurrentRequests)
        {
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            return MaxConcurrentRequests(new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="options">The max concurrent request options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxConcurrentRequests(MaxConcurrentRequestOptions options)
        {
            options.MustNotNull("options");

            int concurrentRequestCounter = 0;
            return
                next =>
                async env =>
                {
                    var owinRequest = new OwinRequest(env);
                    var limitsRequestContext = new RequestContext(owinRequest);
                    int maxConcurrentRequests = options.GetMaxConcurrentRequests(limitsRequestContext);
                    if (maxConcurrentRequests <= 0)
                    {
                        maxConcurrentRequests = int.MaxValue;
                    }
                    try
                    {
                        int concurrentRequests = Interlocked.Increment(ref concurrentRequestCounter);
                        options.Tracer.AsVerbose("Concurrent counter incremented.");
                        options.Tracer.AsVerbose("Checking concurrent request #{0}.", concurrentRequests);
                        if (concurrentRequests > maxConcurrentRequests)
                        {
                            options.Tracer.AsInfo("Limit of {0} exceeded with #{1}. Request rejected.", maxConcurrentRequests, concurrentRequests);
                            IOwinResponse response = new OwinContext(env).Response;
                            response.StatusCode = 503;
                            response.ReasonPhrase = options.LimitReachedReasonPhrase(response.StatusCode);
                            return;
                        }
                        options.Tracer.AsVerbose("Request forwarded.");
                        await next(env);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref concurrentRequestCounter);
                        options.Tracer.AsVerbose("Concurrent counter decremented.");
                    }
                };
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests. Use 0 or a negative
        /// number to specify unlimited number of concurrent requests.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxConcurrentRequests(this BuildFunc builder, int maxConcurrentRequests)
        {
            builder.MustNotNull("builder");

            return MaxConcurrentRequests(builder, () => maxConcurrentRequests);
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static BuildFunc MaxConcurrentRequests(this BuildFunc builder, Func<int> getMaxConcurrentRequests)
        {
            builder.MustNotNull("builder");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            return MaxConcurrentRequests(builder, new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxConcurrentRequests</exception>
        public static BuildFunc MaxConcurrentRequests(this BuildFunc builder, Func<RequestContext, int> getMaxConcurrentRequests)
        {
            builder.MustNotNull("builder");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            return MaxConcurrentRequests(builder, new MaxConcurrentRequestOptions(getMaxConcurrentRequests));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max concurrent request options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxConcurrentRequests(this BuildFunc builder, MaxConcurrentRequestOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxConcurrentRequests(options));
            return builder;
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxQueryStringLength(int maxQueryStringLength)
        {
            return MaxQueryStringLength(() => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxQueryStringLength</exception>
        public static MidFunc MaxQueryStringLength(Func<int> getMaxQueryStringLength)
        {
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            return MaxQueryStringLength(new MaxQueryStringLengthOptions(getMaxQueryStringLength));
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="options">The max concurrent request options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxQueryStringLength(MaxQueryStringLengthOptions options)
        {
            options.MustNotNull("options");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    QueryString queryString = context.Request.QueryString;
                    if (queryString.HasValue)
                    {
                        int maxQueryStringLength = options.MaxQueryStringLength;
                        string unescapedQueryString = Uri.UnescapeDataString(queryString.Value);
                        options.Tracer.AsVerbose("Querystring of request with an unescaped length of {0}", unescapedQueryString.Length);
                        if (unescapedQueryString.Length > maxQueryStringLength)
                        {
                            options.Tracer.AsInfo("Querystring (Length {0}) too long (allowed {1}). Request rejected.",
                                unescapedQueryString.Length,
                                maxQueryStringLength);
                            context.Response.StatusCode = 414;
                            context.Response.ReasonPhrase = options.LimitReachedReasonPhrase(context.Response.StatusCode);
                            return;
                        }
                        options.Tracer.AsVerbose("Querystring length check passed.");
                    }
                    else
                    {
                        options.Tracer.AsVerbose("No querystring.");
                    }
                    await next(env);
                };
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxQueryStringLength(this BuildFunc builder, int maxQueryStringLength)
        {
            builder.MustNotNull("builder");

            return MaxQueryStringLength(builder, () => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxQueryStringLength</exception>
        public static BuildFunc MaxQueryStringLength(this BuildFunc builder, Func<int> getMaxQueryStringLength)
        {
            builder.MustNotNull("builder");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            return MaxQueryStringLength(builder, new MaxQueryStringLengthOptions(getMaxQueryStringLength));
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max querystring length options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxQueryStringLength(this BuildFunc builder, MaxQueryStringLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxQueryStringLength(options));
            return builder;
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxRequestContentLength(int maxContentLength)
        {
            return MaxRequestContentLength(() => maxContentLength);
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxContentLength</exception>
        public static MidFunc MaxRequestContentLength(Func<int> getMaxContentLength)
        {
            getMaxContentLength.MustNotNull("getMaxContentLength");

            return MaxRequestContentLength(new MaxRequestContentLengthOptions(getMaxContentLength));
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="options">The max request content lenght options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxRequestContentLength(MaxRequestContentLengthOptions options)
        {
            options.MustNotNull("options");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    IOwinRequest request = context.Request;
                    string requestMethod = request.Method.Trim().ToUpper();

                    if (requestMethod == "GET" || requestMethod == "HEAD")
                    {
                        options.Tracer.AsVerbose("GET or HEAD request without checking forwarded.");
                        await next(env);
                        return;
                    }
                    int maxContentLength = options.MaxContentLength;
                    options.Tracer.AsVerbose("Max valid content length is {0}.", maxContentLength);
                    if (!IsChunkedRequest(request))
                    {
                        options.Tracer.AsVerbose("Not a chunked request. Checking content lengt header.");
                        string contentLengthHeaderValue = request.Headers.Get("Content-Length");
                        if (contentLengthHeaderValue == null)
                        {
                            options.Tracer.AsInfo("No content length header provided. Request rejected.");
                            SetResponseStatusCodeAndReasonPhrase(context, 411, options);
                            return;
                        }
                        int contentLength;
                        if (!int.TryParse(contentLengthHeaderValue, out contentLength))
                        {
                            options.Tracer.AsInfo("Invalid content length header value. Value: {0}", contentLengthHeaderValue);
                            SetResponseStatusCodeAndReasonPhrase(context, 400, options);
                            return;
                        }
                        if (contentLength > maxContentLength)
                        {
                            options.Tracer.AsInfo("Content length of {0} exceeds maximum of {1}. Request rejected.", contentLength, maxContentLength);
                            SetResponseStatusCodeAndReasonPhrase(context, 413, options);
                            return;
                        }
                        options.Tracer.AsVerbose("Content length header check passed.");
                    }
                    else
                    {
                        options.Tracer.AsVerbose("Chunked request. Content length header not checked.");
                    }

                    request.Body = new ContentLengthLimitingStream(request.Body, maxContentLength);
                    options.Tracer.AsVerbose("Request body stream configured with length limiting stream of {0}.", maxContentLength);

                    try
                    {
                        options.Tracer.AsVerbose("Request forwarded.");
                        await next(env);
                        options.Tracer.AsVerbose("Processing finished.");
                    }
                    catch (ContentLengthExceededException)
                    {
                        options.Tracer.AsInfo("Content length of {0} exceeded. Request canceled and rejected.", maxContentLength);
                        SetResponseStatusCodeAndReasonPhrase(context, 413, options);
                    }
                };
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxRequestContentLength(this BuildFunc builder, int maxContentLength)
        {
            builder.MustNotNull("builder");

            return MaxRequestContentLength(builder, () => maxContentLength);
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxContentLength</exception>
        public static BuildFunc MaxRequestContentLength(this BuildFunc builder, Func<int> getMaxContentLength)
        {
            builder.MustNotNull("builder");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            return MaxRequestContentLength(builder, new MaxRequestContentLengthOptions(getMaxContentLength));
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max request content length options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxRequestContentLength(this BuildFunc builder, MaxRequestContentLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxRequestContentLength(options));
            return builder;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxUrlLength(int maxUrlLength)
        {
            return MaxUrlLength(() => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxUrlLength</exception>
        public static MidFunc MaxUrlLength(Func<int> getMaxUrlLength)
        {
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            return MaxUrlLength(new MaxUrlLengthOptions(getMaxUrlLength));
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="options">The max request content lenght options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxUrlLength(MaxUrlLengthOptions options)
        {
            options.MustNotNull("options");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    int maxUrlLength = options.MaxUrlLength;
                    string unescapedUri = Uri.UnescapeDataString(context.Request.Uri.AbsoluteUri);

                    options.Tracer.AsVerbose("Checking request url length.");
                    if (unescapedUri.Length > maxUrlLength)
                    {
                        options.Tracer.AsInfo(
                            "Url \"{0}\"(Length: {2}) exceeds allowed length of {1}. Request rejected.",
                            unescapedUri,
                            maxUrlLength,
                            unescapedUri.Length);
                        context.Response.StatusCode = 414;
                        context.Response.ReasonPhrase = options.LimitReachedReasonPhrase(context.Response.StatusCode);
                        return Task.FromResult(0);
                    }
                    options.Tracer.AsVerbose("Check passed. Request forwarded.");
                    return next(env);
                };
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        public static BuildFunc MaxUrlLength(this BuildFunc builder, int maxUrlLength)
        {
            builder.MustNotNull("builder");

            return MaxUrlLength(builder, () => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">getMaxUrlLength</exception>
        public static BuildFunc MaxUrlLength(this BuildFunc builder, Func<int> getMaxUrlLength)
        {
            builder.MustNotNull("builder");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            return MaxUrlLength(builder, new MaxUrlLengthOptions(getMaxUrlLength));
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="builder">The OWIN builder instance.</param>
        /// <param name="options">The max url length options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">builder</exception>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static BuildFunc MaxUrlLength(this BuildFunc builder, MaxUrlLengthOptions options)
        {
            builder.MustNotNull("builder");
            options.MustNotNull("options");

            builder(_ => MaxUrlLength(options));
            return builder;
        }

        private static bool IsChunkedRequest(IOwinRequest request)
        {
            string header = request.Headers.Get("Transfer-Encoding");
            return header != null && header.Equals("chunked", StringComparison.OrdinalIgnoreCase);
        }

        private static void SetResponseStatusCodeAndReasonPhrase(IOwinContext context, int statusCode, MaxRequestContentLengthOptions options)
        {
            context.Response.StatusCode = statusCode;
            context.Response.ReasonPhrase = options.LimitReachedReasonPhrase(context.Response.StatusCode);
        }
    }
}