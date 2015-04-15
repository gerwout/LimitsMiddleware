namespace LimitsMiddleware.RateLimiters
{
    using System;

    internal class FixedTokenBucket
    {
        private readonly long _bucketTokenCapacty;
        private readonly long _refillIntervalTicks;
        private readonly GetUtcNow _getUtcNow;
        private readonly object _syncObject = new object();
        private long _nextRefillTime;
        private long _tokens;

        public FixedTokenBucket(long bucketTokenCapacty, TimeSpan refillInterval, GetUtcNow getUtcNow = null)
        {
            _bucketTokenCapacty = bucketTokenCapacty;
            _refillIntervalTicks = refillInterval.Ticks;
            _getUtcNow = getUtcNow ?? SystemClock.GetUtcNow;
        }

        public bool ShouldThrottle(long tokenCount)
        {
            TimeSpan _;
            return ShouldThrottle(tokenCount, out _);
        }

        public bool ShouldThrottle(long tokenCount, out TimeSpan waitTimeSpan)
        {
            waitTimeSpan = TimeSpan.Zero;
            lock (_syncObject)
            {
                UpdateTokens();
                if (_tokens < tokenCount)
                {
                    var currentTime = _getUtcNow().Ticks;
                    var waitTicks = _nextRefillTime - currentTime;
                    if (waitTicks < 0)
                    {
                        return false;
                    }
                    waitTimeSpan = TimeSpan.FromTicks(waitTicks);
                    return true;
                }
                _tokens -= tokenCount;
                return false;
            }
        }

        public long CurrentTokenCount
        {
            get
            {
                lock (_syncObject)
                {
                    UpdateTokens();
                    return _tokens;
                }
            }
        }

        private void UpdateTokens()
        {
            var currentTime = _getUtcNow().Ticks;

            if (currentTime < _nextRefillTime)
            {
                return;
            }

            _tokens = _bucketTokenCapacty;
            _nextRefillTime = currentTime + _refillIntervalTicks;
        }
    }
}