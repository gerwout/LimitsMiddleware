namespace LimitsMiddleware.RateLimiters
{
    public class StepUpLeakyTokenBucket : LeakyTokenBucket
    {
        private long lastActivityTime;

        public StepUpLeakyTokenBucket(long maxTokens, long refillInterval, int refillIntervalInMilliSeconds,
            long stepTokens, long stepInterval, int stepIntervalInMilliseconds)
            : base(
                maxTokens, refillInterval, refillIntervalInMilliSeconds, stepTokens, stepInterval,
                stepIntervalInMilliseconds)
        {}

        protected override void UpdateTokens()
        {
            var currentTime = SystemTime.UtcNow.Ticks;

            if (currentTime >= NextRefillTime)
            {
                Tokens = StepTokens;

                lastActivityTime = currentTime;
                NextRefillTime = currentTime + ticksRefillInterval;

                return;
            }

            //calculate tokens at current step

            var elapsedTimeSinceLastActivity = currentTime - lastActivityTime;
            var elapsedStepsSinceLastActivity = elapsedTimeSinceLastActivity/ticksStepInterval;

            Tokens += (elapsedStepsSinceLastActivity*StepTokens);

            if (Tokens > BucketTokenCapacity)
            {
                Tokens = BucketTokenCapacity;
            }
            lastActivityTime = currentTime;
        }
    }
}