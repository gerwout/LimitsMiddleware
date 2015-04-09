namespace LimitsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;

    internal class GlobalRateLimiter : RateLimiterBase
    {
        private readonly Func<int> _getMaxBytesPerSecond;
        private long _byteCount;
        private readonly long _start;
        private readonly InterlockedBoolean _resetting;

        private long CurrentMilliseconds
        {
            get { return Environment.TickCount; }
        }

        public GlobalRateLimiter(Func<int> getMaxBytesPerSecond)
        {
            _getMaxBytesPerSecond = getMaxBytesPerSecond;

            _start = CurrentMilliseconds;
            _byteCount = 0;
            _resetting = new InterlockedBoolean();
        }

        public override async Task Throttle(int bytesToWrite)
        {
            // Make sure the buffer isn't empty.
            int maximumBytesPerSecond = _getMaxBytesPerSecond();

            if (maximumBytesPerSecond <= 0 || bytesToWrite <= 0)
            {
                return;
            }
            Interlocked.Add(ref _byteCount, bytesToWrite);
            long elapsedMilliseconds = CurrentMilliseconds - _start;

            if (elapsedMilliseconds >= 0)
            {
                // Calculate the current bps.
                long bps = elapsedMilliseconds == 0 
                    ? long.MaxValue : 
                    _byteCount / (elapsedMilliseconds * 1000L);

                // If the bps are more then the maximum bps, try to throttle.
                if (bps > maximumBytesPerSecond)
                {
                    // Calculate the time to sleep.
                    long wakeElapsed = _byteCount / maximumBytesPerSecond;
                    var toSleep = (wakeElapsed*1000L) - elapsedMilliseconds;

                    if (toSleep > 1)
                    {
                        try
                        {
                            // The time to sleep is more then a millisecond, so sleep.
                            await Task.Delay((int)toSleep);
                        }
                        catch (ThreadAbortException)
                        {
                            // Eatup ThreadAbortException.
                        }

                        // A sleep has been done, reset.
                        Reset();
                    }
                }
            }
        }

        private void Reset()
        {
            if (!_resetting.EnsureCalledOnce())
            {
                return;
            }
            
            long difference = CurrentMilliseconds - _start;

            // Only reset counters when a known history is available of more then 1 second.
            if (difference > 1000)
            {
                Interlocked.Exchange(ref _byteCount, 0);
            }

            _resetting.Set(false);
        }
    }
}