namespace LimitsMiddleware.RateLimiters
{
    public class StepDownTokenBucket : LeakyTokenBucket
    {
        private readonly GetUtcNow _getUtcNow;

        public StepDownTokenBucket(long maxTokens, long refillInterval, int refillIntervalInMilliSeconds,
            long stepTokens, long stepInterval, int stepIntervalInMilliseconds, GetUtcNow getUtcNow = null)
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
                //set tokens to max
                Tokens = BucketTokenCapacity;

                //compute next refill time
                NextRefillTime = currentTime + TicksRefillInterval;
                return;
            }

            //calculate max tokens possible till the end
            var timeToNextRefill = NextRefillTime - currentTime;
            var stepsToNextRefill = timeToNextRefill/TicksStepInterval;

            var maxPossibleTokens = stepsToNextRefill*StepTokens;

            if ((timeToNextRefill%TicksStepInterval) > 0)
            {
                maxPossibleTokens += StepTokens;
            }
            if (maxPossibleTokens < Tokens)
            {
                Tokens = maxPossibleTokens;
            }
        }
    }
}