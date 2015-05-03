namespace LimitsMiddleware
{
    using System;
    using System.Threading.Tasks;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

#pragma warning disable 1591
    public static partial class Limits
#pragma warning restore 1591
    {
        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay, in milliseconds.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(int minDelay)
        {
            return MinResponseDelay(_ => minDelay);
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

            return MinResponseDelay(_ => getMinDelay());
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

            return MinResponseDelay(context => TimeSpan.FromMilliseconds(getMinDelay(context)));
        }

        /// <summary>
        /// Adds a minimum delay before sending the response.
        /// </summary>
        /// <param name="minDelay">The min response delay.</param>
        /// <returns>A midfunc.</returns>
        public static MidFunc MinResponseDelay(TimeSpan minDelay)
        {
            return MinResponseDelay(_ => minDelay);
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

            return MinResponseDelay(_ => getMinDelay());
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

            var logger = LogProvider.GetLogger("LimitsMiddleware.MinResponseDelay");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    var limitsRequestContext = new RequestContext(context.Request);
                    var delay = getMinDelay(limitsRequestContext);

                    if (delay <= TimeSpan.Zero)
                    {
                        await next(env);
                        return;
                    }

                    logger.Debug("Delaying response by {0}".FormatWith(delay));
                    await Task.Delay(delay, context.Request.CallCancelled);
                    await next(env);
                };
        }
    }
}
