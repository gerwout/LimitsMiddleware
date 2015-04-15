// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    using System;

    public class FixedTokenBucket : TokenBucket
    {
        private readonly GetUtcNow _getUtcNow;
        private readonly long _ticksRefillInterval;
        private long _nextRefillTime;

        public FixedTokenBucket(
            long maxTokens,
            long refillInterval,
            long refillIntervalInMilliSeconds,
            GetUtcNow getUtcNow = null)
            : base(maxTokens)
        {
            if (refillInterval < 0)
            {
                throw new ArgumentOutOfRangeException("refillInterval", "Refill interval cannot be negative");
            }
            if (refillIntervalInMilliSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException("refillIntervalInMilliSeconds",
                    "Refill interval in milliseconds cannot be negative");
            }
            _getUtcNow = getUtcNow ?? SystemClock.GetUtcNow;
            _ticksRefillInterval = TimeSpan.FromMilliseconds(refillInterval*refillIntervalInMilliSeconds).Ticks;

        }

        protected override void UpdateTokens()
        {
            var currentTime = _getUtcNow().Ticks;

            if (currentTime < _nextRefillTime)
            {
                return;
            }

            Tokens = BucketTokenCapacity;
            _nextRefillTime = currentTime + _ticksRefillInterval;
        }
    }
}