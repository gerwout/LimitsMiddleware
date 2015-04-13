namespace LimitsMiddleware.RateLimiters
{
    using System;

    public abstract class LeakyTokenBucket : TokenBucket
    {
        protected readonly long StepTokens;
        protected readonly long TicksRefillInterval;
        protected readonly long TicksStepInterval;
        protected long NextRefillTime;

        protected LeakyTokenBucket(long maxTokens, long refillInterval, int refillIntervalInMilliSeconds,
            long stepTokens, long stepInterval, int stepIntervalInMilliseconds)
            : base(maxTokens)
        {
            StepTokens = stepTokens;
            if (refillInterval < 0)
            {
                throw new ArgumentOutOfRangeException("refillInterval", "Refill interval cannot be negative");
            }
            if (stepInterval < 0)
            {
                throw new ArgumentOutOfRangeException("stepInterval", "Step interval cannot be negative");
            }
            if (stepTokens < 0)
            {
                throw new ArgumentOutOfRangeException("stepTokens", "Step tokens cannot be negative");
            }
            if (refillIntervalInMilliSeconds <= 0)
            {
                throw new ArgumentOutOfRangeException("refillIntervalInMilliSeconds",
                    "Refill interval in milliseconds cannot be negative");
            }
            if (stepIntervalInMilliseconds <= 0)
            {
                throw new ArgumentOutOfRangeException("stepIntervalInMilliseconds",
                    "Step interval in milliseconds cannot be negative");
            }

            TicksRefillInterval = TimeSpan.FromMilliseconds(refillInterval*refillIntervalInMilliSeconds).Ticks;
            TicksStepInterval = TimeSpan.FromMilliseconds(stepInterval*stepIntervalInMilliseconds).Ticks;
        }
    }
}