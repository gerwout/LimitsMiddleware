namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

    public static partial class Limits
    {
        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="maxBitsPerSecond">The maximum number of bits per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxBandwidthGlobal(int maxBitsPerSecond)
        {
            return MaxBandwidthGlobal(() => maxBitsPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBitsPerSecond">A delegate to retrieve the maximum number of bits per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesToWrite</exception>
        public static MidFunc MaxBandwidthGlobal(Func<int> getMaxBitsPerSecond)
        {
            getMaxBitsPerSecond.MustNotNull("getMaxBytesToWrite");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxBandwidthGlobal");
            var rateLimiter = new GlobalRateLimiter(getMaxBitsPerSecond);

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
    }
}
