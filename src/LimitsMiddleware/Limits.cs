namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;

    /// <summary>
    /// Represent a set of middleware functions to apply limits to an OWIN pipeline.
    /// </summary>
    public static partial class Limits
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
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static MidFunc ConnectionTimeout(Func<RequestContext, TimeSpan> getTimeout)
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.ConnectionTimeout");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    var limitsRequestContext = new RequestContext(context.Request);

                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    logger.Debug("Configure timeouts.");
                    TimeSpan connectionTimeout = options.GetTimeout(limitsRequestContext);
                    context.Request.Body = new TimeoutStream(requestBodyStream, connectionTimeout);
                    context.Response.Body = new TimeoutStream(responseBodyStream, connectionTimeout);

                    logger.Debug("Request with configured timeout forwarded.");
                    return next(env);
                };
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

            return MaxBandwidthPerRequest(new MaxBandwidthPerRequestOptions(getMaxBytesPerSecond));
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

            return MaxBandwidthPerRequest(new MaxBandwidthPerRequestOptions(getMaxBytesPerSecond));
        }
        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="perRequestOptions">The max bandwidth options.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MaxBandwidthPerRequest(MaxBandwidthPerRequestOptions perRequestOptions)
        {
            perRequestOptions.MustNotNull("options");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxBandwidthPerRequest");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    var limitsRequestContext = new RequestContext(context.Request);

                    logger.Debug("Configure streams to be limited.");
                    context.Request.Body = new ThrottledStream(requestBodyStream, new RateLimiter(() => perRequestOptions.GetMaxBytesPerSecond(limitsRequestContext)));
                    context.Response.Body = new ThrottledStream(responseBodyStream, new RateLimiter(() => perRequestOptions.GetMaxBytesPerSecond(limitsRequestContext)));

                    //TODO consider SendFile interception
                    logger.Debug("With configured limit forwarded.");
                    return next(env);
                };
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxBandwidthGlobal");
            var rateLimiter = new RateLimiter(() => options.MaxBytesPerSecond);

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    logger.Debug("Configure streams to be limited.");
                    context.Request.Body = new ThrottledStream(requestBodyStream, rateLimiter);
                    context.Response.Body = new ThrottledStream(responseBodyStream, rateLimiter);

                    //TODO consider SendFile interception
                    logger.Debug("With configured limit forwarded.");
                    return next(env);
                };
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxConcurrentRequests");
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
                        logger.Debug("Concurrent counter incremented.");
                        logger.Debug("Checking concurrent request #{0}.".FormatWith(concurrentRequests));
                        if (concurrentRequests > maxConcurrentRequests)
                        {
                            logger.Info("Limit of {0} exceeded with #{1}. Request rejected."
                                .FormatWith(maxConcurrentRequests, concurrentRequests));
                            IOwinResponse response = new OwinContext(env).Response;
                            response.StatusCode = 503;
                            response.ReasonPhrase = options.LimitReachedReasonPhrase(response.StatusCode);
                            return;
                        }
                        logger.Debug("Request forwarded.");
                        await next(env);
                    }
                    finally
                    {
                        Interlocked.Decrement(ref concurrentRequestCounter);
                        logger.Debug("Concurrent counter decremented.");
                    }
                };
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxQueryStringLength");

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
                        logger.Debug("Querystring of request with an unescaped length of {0}".FormatWith(unescapedQueryString.Length));
                        if (unescapedQueryString.Length > maxQueryStringLength)
                        {
                            logger.Info("Querystring (Length {0}) too long (allowed {1}). Request rejected.".FormatWith(
                                unescapedQueryString.Length,
                                maxQueryStringLength));
                            context.Response.StatusCode = 414;
                            context.Response.ReasonPhrase = options.LimitReachedReasonPhrase(context.Response.StatusCode);
                            return;
                        }
                        logger.Debug("Querystring length check passed.");
                    }
                    else
                    {
                        logger.Debug("No querystring.");
                    }
                    await next(env);
                };
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
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxContentLength</exception>
        public static MidFunc MaxRequestContentLength(Func<RequestContext, int> getMaxContentLength)
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxRequestContentLength");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    IOwinRequest request = context.Request;
                    var limitsRequestContext = new RequestContext(request);
                    string requestMethod = request.Method.Trim().ToUpper();

                    if (requestMethod == "GET" || requestMethod == "HEAD")
                    {
                        logger.Debug("GET or HEAD request without checking forwarded.");
                        await next(env);
                        return;
                    }
                    int maxContentLength = options.GetMaxContentLength(limitsRequestContext);
                    logger.Debug("Max valid content length is {0}.".FormatWith(maxContentLength));
                    if (!IsChunkedRequest(request))
                    {
                        logger.Debug("Not a chunked request. Checking content lengt header.");
                        string contentLengthHeaderValue = request.Headers.Get("Content-Length");
                        if (contentLengthHeaderValue == null)
                        {
                            logger.Info("No content length header provided. Request rejected.");
                            SetResponseStatusCodeAndReasonPhrase(context, 411, options);
                            return;
                        }
                        int contentLength;
                        if (!int.TryParse(contentLengthHeaderValue, out contentLength))
                        {
                            logger.Info("Invalid content length header value. Value: {0}".FormatWith(contentLengthHeaderValue));
                            SetResponseStatusCodeAndReasonPhrase(context, 400, options);
                            return;
                        }
                        if (contentLength > maxContentLength)
                        {
                            logger.Info("Content length of {0} exceeds maximum of {1}. Request rejected.".FormatWith(
                                contentLength,
                                maxContentLength));
                            SetResponseStatusCodeAndReasonPhrase(context, 413, options);
                            return;
                        }
                        logger.Debug("Content length header check passed.");
                    }
                    else
                    {
                        logger.Debug("Chunked request. Content length header not checked.");
                    }

                    request.Body = new ContentLengthLimitingStream(request.Body, maxContentLength);
                    logger.Debug("Request body stream configured with length limiting stream of {0}.".FormatWith(maxContentLength));

                    try
                    {
                        logger.Debug("Request forwarded.");
                        await next(env);
                        logger.Debug("Processing finished.");
                    }
                    catch (ContentLengthExceededException)
                    {
                        logger.Info("Content length of {0} exceeded. Request canceled and rejected.".FormatWith(maxContentLength));
                        SetResponseStatusCodeAndReasonPhrase(context, 413, options);
                    }
                };
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxUrlLength");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    int maxUrlLength = options.MaxUrlLength;
                    string unescapedUri = Uri.UnescapeDataString(context.Request.Uri.AbsoluteUri);

                    logger.Debug("Checking request url length.");
                    if (unescapedUri.Length > maxUrlLength)
                    {
                        logger.Info(
                            "Url \"{0}\"(Length: {2}) exceeds allowed length of {1}. Request rejected.".FormatWith(
                            unescapedUri,
                            maxUrlLength,
                            unescapedUri.Length));
                        context.Response.StatusCode = 414;
                        context.Response.ReasonPhrase = options.LimitReachedReasonPhrase(context.Response.StatusCode);
                        return Task.FromResult(0);
                    }
                    logger.Debug("Check passed. Request forwarded.");
                    return next(env);
                };
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay, in milliseconds.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(int minDelay)
        {
            return MinResponseDelay(new MinResponseDelayOptions(minDelay));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<int> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            return MinResponseDelay(new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<RequestContext, int> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            return MinResponseDelay(new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(TimeSpan minDelay)
        {
            return MinResponseDelay(new MinResponseDelayOptions(minDelay));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<TimeSpan> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            return MinResponseDelay(new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="getMinDelay">A delegate to return the min response delay.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">getMinDelay</exception>
        public static MidFunc MinResponseDelay(Func<RequestContext, TimeSpan> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            return MinResponseDelay(new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="options">The min response delay options.</param>
        /// <returns>The OWIN builder instance.</returns>
        /// <exception cref="System.ArgumentNullException">options</exception>
        public static MidFunc MinResponseDelay(MinResponseDelayOptions options)
        {
            options.MustNotNull("options");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MinResponseDelay");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    var limitsRequestContext = new RequestContext(context.Request);
                    var delay = options.GetMinDelay(limitsRequestContext);

                    if (delay <= TimeSpan.Zero)
                    {
                        return next(env);
                    }

                    // using explicit continuation because async / await adds some overhead!
                    return Task.Delay(delay, context.Request.CallCancelled)
                        .ContinueWith(_ => next(env), context.Request.CallCancelled);
                };
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