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
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static MidFunc MaxBandwidthPerRequest(Func<int> getMaxBytesPerSecond)
        {
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            return MaxBandwidthPerRequest(_ => getMaxBytesPerSecond());
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

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    Stream requestBodyStream = context.Request.Body ?? Stream.Null;
                    Stream responseBodyStream = context.Response.Body;

                    var limitsRequestContext = new RequestContext(context.Request);

                    var requestTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesPerSecond(limitsRequestContext));
                    var responseTokenBucket = new FixedTokenBucket(
                        () => getMaxBytesPerSecond(limitsRequestContext));

                    using (requestTokenBucket.RegisterRequest())
                    using (responseTokenBucket.RegisterRequest())
                    {

                        context.Request.Body = new ThrottledStream(requestBodyStream, requestTokenBucket);
                        context.Response.Body = new ThrottledStream(responseBodyStream, responseTokenBucket);

                        //TODO consider SendFile interception
                        await next(env).ConfigureAwait(false);
                    }
                };
        }
    }
}
