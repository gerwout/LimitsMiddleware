// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    using System;

    public class Throttler
    {
        private readonly IThrottleStrategy _strategy;

        public Throttler(IThrottleStrategy strategy)
        {
            if (strategy == null)
            {
                throw new ArgumentNullException("strategy");
            }
            _strategy = strategy;
        }

        public bool CanConsume()
        {
            return !_strategy.ShouldThrottle();
        }
    }
}