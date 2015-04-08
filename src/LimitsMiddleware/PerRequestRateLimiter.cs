namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using System.Threading.Tasks;

    internal class PerRequestRateLimiter : RateLimiterBase
    {
        private readonly int _maxBytesPerSecond;
        private Stopwatch _stopWatch;
        private long _byteCount;
        private const int MaxSleepMilliseconds = 10000;

        internal PerRequestRateLimiter(int maxBytesPerSecond)
        {
            _maxBytesPerSecond = maxBytesPerSecond;
        }

        public override async Task Throttle(int bytesToWrite)
        {
            if (_maxBytesPerSecond <= 0)
            {
                // no throttling
                return;
            }
            _byteCount += bytesToWrite;

            if (_stopWatch == null)
            {
                // don't start timer until first write and only throttle on second and subsequent writes
                _stopWatch = Stopwatch.StartNew();
                return;
            }
            double elapsedMilliseconds = _stopWatch.Elapsed.TotalMilliseconds;
            elapsedMilliseconds = (Math.Abs(elapsedMilliseconds) < 0.001) ? 1 : elapsedMilliseconds;

            // Calculate the current bps.
            double elapsedSeconds = elapsedMilliseconds/1000D;
            double bytesPerSecond = _byteCount / elapsedSeconds;

            // If the bps are more then the maximum bps, try to throttle.
            if (bytesPerSecond > _maxBytesPerSecond)
            {
                // Calculate the time to delay.
                double desiredTotalMilliseconds = ((double)_byteCount / _maxBytesPerSecond) * 1000;
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