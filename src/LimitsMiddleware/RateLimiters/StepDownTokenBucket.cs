namespace LimitsMiddleware.RateLimiters
{
    public class StepDownTokenBucket : LeakyTokenBucket
    {
        public StepDownTokenBucket(long maxTokens, long refillInterval, int refillIntervalInMilliSeconds,
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
                //set tokens to max
                Tokens = BucketTokenCapacity;

                //compute next refill time
                NextRefillTime = currentTime + ticksRefillInterval;
                return;
            }

            //calculate max tokens possible till the end
            var timeToNextRefill = NextRefillTime - currentTime;
            var stepsToNextRefill = timeToNextRefill/ticksStepInterval;

            var maxPossibleTokens = stepsToNextRefill*StepTokens;

            if ((timeToNextRefill%ticksStepInterval) > 0)
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