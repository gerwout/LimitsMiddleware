namespace LimitsMiddleware.RateLimiters
{
    using System;

    public class FixedTokenBucket : TokenBucket
    {
        private readonly long _ticksRefillInterval;
        private long _nextRefillTime;

        public FixedTokenBucket(long maxTokens, long refillInterval, long refillIntervalInMilliSeconds)
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

            _ticksRefillInterval = TimeSpan.FromMilliseconds(refillInterval*refillIntervalInMilliSeconds).Ticks;
        }

        protected override void UpdateTokens()
        {
            var currentTime = SystemTime.UtcNow.Ticks;

            if (currentTime < _nextRefillTime)
            {
                return;
            }

            Tokens = BucketTokenCapacity;
            _nextRefillTime = currentTime + _ticksRefillInterval;
        }
    }
}