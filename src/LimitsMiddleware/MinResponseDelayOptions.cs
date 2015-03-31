namespace LimitsMiddleware
{
    using System;

    /// <summary>
    /// Options for setting a delay to each response
    /// </summary>
    public class MinResponseDelayOptions
    {
        private readonly Func<RequestContext, TimeSpan> _getMinDelay;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minDelay">The delay, in milliseconds</param>
        public MinResponseDelayOptions(int minDelay)
            : this(TimeSpan.FromSeconds(minDelay))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMinDelay">returns the delay, in miliseconds</param>
        public MinResponseDelayOptions(Func<int> getMinDelay)
            : this(_ => TimeSpan.FromMilliseconds(getMinDelay()))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMinDelay">returns the delay, in miliseconds</param>
        public MinResponseDelayOptions(Func<RequestContext, int> getMinDelay)
            : this(context => TimeSpan.FromMilliseconds(getMinDelay(context)))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minDelay">The delay</param>
        public MinResponseDelayOptions(TimeSpan minDelay)
            : this(_ => minDelay)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMinDelay">returns the delay</param>
        public MinResponseDelayOptions(Func<TimeSpan> getMinDelay)
            : this(_ => getMinDelay())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMinDelay">returns the delay</param>
        public MinResponseDelayOptions(Func<RequestContext, TimeSpan> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            _getMinDelay = getMinDelay;
        }

        /// <summary>
        /// Returns the minimum delay.
        /// </summary>
        public TimeSpan GetMinDelay(RequestContext requestContext)
        {
            return _getMinDelay(requestContext);
        }
    }
}