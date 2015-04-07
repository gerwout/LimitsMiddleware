namespace LimitsMiddleware
{
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PerRequestRateLimiter : RateLimiterBase
    {
        private readonly int _maxBitsPerSecond;
        private Stopwatch _stopWatch;
        private long _bitCount;
        private const int MaxSleepMilliseconds = 10000;

        internal PerRequestRateLimiter(int maxBitsPerSecond)
        {
            _maxBitsPerSecond = maxBitsPerSecond;
        }

        public override async Task Throttle(int bitsToWrite)
        {
            if (_maxBitsPerSecond <= 0)
            {
                // no throttling
                return;
            }
            _bitCount += bitsToWrite;
            if (_stopWatch == null)
            {
                // don't start timer until first write and only throttle on second and subsequent writes
                _stopWatch = Stopwatch.StartNew();
                return;
            }
            long elapsedMilliseconds = _stopWatch.ElapsedMilliseconds;
            if (elapsedMilliseconds > 0)
            {
                // Calculate the current bps.
                double elapsedSeconds = elapsedMilliseconds/1000D;
                double bps = _bitCount / elapsedSeconds;

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > _maxBitsPerSecond)
                {
                    // Calculate the time to delay.
                    double desiredTotalMilliseconds = ((double)_bitCount / _maxBitsPerSecond) * 1000;
                    double millisecondsToSleep = desiredTotalMilliseconds - elapsedMilliseconds;
                    millisecondsToSleep = millisecondsToSleep < MaxSleepMilliseconds
                        ? millisecondsToSleep
                        : MaxSleepMilliseconds;

                    if (millisecondsToSleep > 1)
                    {
                        try
                        {
                            // The time to sleep is more then a millisecond, so sleep.
                            await Task.Delay((int)millisecondsToSleep);
                        }
                        catch (ThreadAbortException)
                        {
                            // Eatup ThreadAbortException.
                        }
                    }
                }
            }
        }
    }
}