namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using LimitsMiddleware.RateLimiters;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

    public static partial class Limits
    {
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
        /// <param name="getMaxBitsPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesToWrite</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<int> getMaxBitsPerSecond)
        {
            getMaxBitsPerSecond.MustNotNull("getMaxBytesToWrite");

            return MaxBandwidthPerRequest(_ => getMaxBitsPerSecond());
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesToWrite">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesToWrite</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<RequestContext, int> getMaxBytesToWrite)
        {
            getMaxBytesToWrite.MustNotNull("getMaxBytesToWrite");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxBandwidthPerRequest");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    var limitsRequestContext = new RequestContext(context.Request);

                    var requestTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesToWrite(limitsRequestContext), TimeSpan.FromSeconds(1));
                    var responseTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesToWrite(limitsRequestContext), TimeSpan.FromSeconds(1));

                    using (requestTokenBucket.RegisterRequest())
                    using (responseTokenBucket.RegisterRequest())
                    {

                        logger.Debug("Configure streams to be limited.");
                        context.Request.Body = new ThrottledStream(requestBodyStream, requestTokenBucket);
                        context.Response.Body = new ThrottledStream(responseBodyStream, responseTokenBucket);

                        //TODO consider SendFile interception
                        logger.Debug("With configured limit forwarded.");
                        await next(env).ConfigureAwait(false);
                    }
                };
        }
    }
}
