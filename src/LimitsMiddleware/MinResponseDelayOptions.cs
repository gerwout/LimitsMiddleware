namespace LimitsMiddleware
{
    using System;

    /// <summary>
    /// Options for setting a delay to each response
    /// </summary>
    public class MinResponseDelayOptions : OptionsBase
    {
        private readonly Func<TimeSpan> _getMinDelay;

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
            : this(() => TimeSpan.FromMilliseconds(getMinDelay()))
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="minDelay">The delay</param>
        public MinResponseDelayOptions(TimeSpan minDelay)
            : this(() => minDelay)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="getMinDelay">returns the delay</param>
        public MinResponseDelayOptions(Func<TimeSpan> getMinDelay)
        {
            getMinDelay.MustNotNull("getMinDelay");

            _getMinDelay = getMinDelay;
        }

        /// <summary>
        /// Returns the minimum delay.
        /// </summary>
        public TimeSpan MinDelay
        {
            get { return _getMinDelay(); }
        }
    }
}