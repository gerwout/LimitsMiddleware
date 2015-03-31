namespace LimitsMiddleware
{
    using System;

    /// <summary>
    /// Options for limiting the max bandwidth.
    /// </summary>
    public class MaxBandwidthPerRequestOptions
    {
        private readonly Func<RequestContext, int> _getMaxBytesPerSecond;

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxBandwidthPerRequestOptions"/> class.
        /// </summary>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        public MaxBandwidthPerRequestOptions(int maxBytesPerSecond) : this(_ => maxBytesPerSecond)
        {}

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxBandwidthPerRequestOptions"/> class.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        public MaxBandwidthPerRequestOptions(Func<int> getMaxBytesPerSecond)
            : this(_ => getMaxBytesPerSecond())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MaxBandwidthPerRequestOptions"/> class.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred,
        /// based on the <see cref="RequestContext"/>.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        public MaxBandwidthPerRequestOptions(Func<RequestContext, int> getMaxBytesPerSecond)
        {
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            _getMaxBytesPerSecond = getMaxBytesPerSecond;
        }

        /// <summary>
        /// Gets the maximum bytes per second.
        /// </summary>
        /// <param name="requestContext">The <see cref="RequestContext"/>.</param>
        /// <returns>THe maximum bytes per second.</returns>
        public int GetMaxBytesPerSecond(RequestContext requestContext)
        {
            return _getMaxBytesPerSecond(requestContext);
        }
    }
}