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

        internal class MovingAverage
        {
            private readonly TimeSpan _samplingInterval;
            private TimeSpan _currentInterval = TimeSpan.Zero;
            private readonly LinkedList<Interval> _intervals = new LinkedList<Interval>();
            private int _totalBytesWritten;

            public MovingAverage(TimeSpan samplingInterval)
            {
                _samplingInterval = samplingInterval;
            }

            public double Average { get; private set; }

            public void AddInterval(int bytesWritten, TimeSpan timespan)
            {
                _intervals.AddLast(new Interval(bytesWritten, timespan));
                _totalBytesWritten += bytesWritten;
                _currentInterval += timespan;

                while (_currentInterval - _intervals.First.Value.TimeSpan >= _samplingInterval)
                {
                    _currentInterval -= _intervals.First.Value.TimeSpan;
                    _totalBytesWritten -= _intervals.First.Value.BytesWritten;
                    _intervals.RemoveFirst();
                }

                Average = _totalBytesWritten / _currentInterval.TotalSeconds;
            }

            private class Interval
            {
                internal readonly int BytesWritten;
                internal readonly TimeSpan TimeSpan;

                public Interval(int bytesWritten, TimeSpan timeSpan)
                {
                    BytesWritten = bytesWritten;
                    TimeSpan = timeSpan;
                }
            }
        }
    }
}