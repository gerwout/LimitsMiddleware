// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    public class StepUpLeakyTokenBucket : LeakyTokenBucket
    {
        private readonly GetUtcNow _getUtcNow;
        private long _lastActivityTime;

        public StepUpLeakyTokenBucket(long maxTokens, long refillInterval, int refillIntervalInMilliSeconds,
            long stepTokens, long stepInterval, int stepIntervalInMilliseconds, GetUtcNow getUtcNow)
            : base(maxTokens, refillInterval, refillIntervalInMilliSeconds, stepTokens, stepInterval,
                stepIntervalInMilliseconds)
        {
            _getUtcNow = getUtcNow ?? SystemClock.GetUtcNow;
        }

        protected override void UpdateTokens()
        {
            var currentTime = _getUtcNow().Ticks;

            if (currentTime >= NextRefillTime)
            {
                Tokens = StepTokens;

                _lastActivityTime = currentTime;
                NextRefillTime = currentTime + TicksRefillInterval;

                return;
            }

            //calculate tokens at current step

            var elapsedTimeSinceLastActivity = currentTime - _lastActivityTime;
            var elapsedStepsSinceLastActivity = elapsedTimeSinceLastActivity/TicksStepInterval;

            Tokens += (elapsedStepsSinceLastActivity*StepTokens);

            if (Tokens > BucketTokenCapacity)
            {
                Tokens = BucketTokenCapacity;
            }
            _lastActivityTime = currentTime;
        }
    }
}