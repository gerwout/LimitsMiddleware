namespace LimitsMiddleware
{
    using System;

    /// <summary>
    /// Options for a timeout connection.
    /// </summary>
    public class ConnectionTimeoutOptions : OptionsBase
    {
        private readonly Func<RequestContext, TimeSpan> _getTimeout;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTimeoutOptions"/> class.
        /// </summary>
        /// <param name="timeout">The timeout.</param>
        public ConnectionTimeoutOptions(TimeSpan timeout) : this(_ => timeout)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTimeoutOptions"/> class.
        /// </summary>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        public ConnectionTimeoutOptions(Func<TimeSpan> getTimeout)
            : this(_ => getTimeout())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionTimeoutOptions"/> class.
        /// </summary>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        public ConnectionTimeoutOptions(Func<RequestContext, TimeSpan> getTimeout)
        {
            getTimeout.MustNotNull("getTimeout");

            _getTimeout = getTimeout;
        }
        
        /// <summary>
        /// The Timeout.
        /// </summary>
        [Obsolete("Use GetTimeout instead.", true)]
        public TimeSpan Timeout
        {
            get { throw new NotSupportedException(); }
        }

        public TimeSpan GetTimeout(RequestContext requestContext)
        {
            return _getTimeout(requestContext);
        }
    }
}