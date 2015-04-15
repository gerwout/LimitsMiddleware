// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    using System;

    public abstract class TokenBucket : IThrottleStrategy
    {
        private readonly object _syncRoot = new object();
        protected readonly long BucketTokenCapacity;
        //number of tokens in the bucket
        protected long Tokens;

        protected TokenBucket(long bucketTokenCapacity)
        {
            if (bucketTokenCapacity <= 0)
            {
                throw new ArgumentException("bucket token capacity can not be negative");
            }
            BucketTokenCapacity = bucketTokenCapacity;
        }

        public bool ShouldThrottle(long n)
        {
            int waitTime;
            return ShouldThrottle(n, out waitTime);
        }

        public bool ShouldThrottle()
        {
            int waitTime;
            return ShouldThrottle(1, out waitTime);
        }

        public bool ShouldThrottle(long n, out int waitTime)
        {
            if (n <= 0)
            {
                throw new ArgumentException("Should be positive integer", "n");
            }

            lock (_syncRoot)
            {
                UpdateTokens();
                if (Tokens < n)
                {
                    waitTime = 0;
                    return true;
                }
                Tokens -= n;

                waitTime = 0;
                return false;
            }
        }

        public bool ShouldThrottle(out int waitTime)
        {
            return ShouldThrottle(1, out waitTime);
        }

        public long CurrentTokenCount
        {
            get
            {
                lock (_syncRoot)
                {
                    UpdateTokens();
                    return Tokens;
                }
            }
        }

        protected abstract void UpdateTokens();
    }
}