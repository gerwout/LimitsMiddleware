namespace LimitsMiddleware
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using LimitsMiddleware.Logging.LogProviders;
    using Timer = System.Timers.Timer;

    /// <summary>
    /// Calculates the moving average within an interval.
    /// 
    /// Even though an exaxt sampling interval is specified, the actual interval at any given sampling will be at least
    /// equal to the sampling interval but usually longer. Thus we use a stopwat to get the actual interval time to 
    /// calculate an more accurate average.
    /// </summary>
    internal sealed class MovingAverageCalculator : IDisposable
    {
        private int _totalBytesWritten;
        private readonly Timer _timer;
        private readonly Stopwatch _stopwatch;
        private double _bytesPerSecond;
        private double _bytesPerSecondPerRequest;
        private long _concurrentRequestCount;
        internal const int DefaultSamplingIntevalMilliseconds = 1000;

        public MovingAverageCalculator(int samplingIntervalMilliseconds = DefaultSamplingIntevalMilliseconds)
        {
            _timer = new Timer(samplingIntervalMilliseconds)
            {
                AutoReset = false,
            };
            _timer.Elapsed += (sender, args) =>
            {
                var bytesWritten = Interlocked.Exchange(ref _totalBytesWritten, 0);
                BytesPerSecond = bytesWritten / _stopwatch.Elapsed.TotalSeconds;

                _stopwatch.Restart();
                _timer.Start();
            };
            _stopwatch = Stopwatch.StartNew();
            _timer.Start();
        }

        public event EventHandler<double> BytesPerSecondChanged;
        public event EventHandler<double> BytesPerSecondPerRequestChanged;

        public IDisposable AddRequest()
        {
            Interlocked.Increment(ref _concurrentRequestCount);
            return new DisposableAction(() => Interlocked.Decrement(ref _concurrentRequestCount));
        }

        public void BytesWritten(int count)
        {
            Interlocked.Add(ref _totalBytesWritten, count);
        }

        public double BytesPerSecond
        {
            get { return _bytesPerSecond; }
            set
            {
                _bytesPerSecond = value;
                var clientCount = Interlocked.Read(ref _concurrentRequestCount);
                BytesPerSecondPerRequest = clientCount == 0 
                    ? double.MaxValue 
                    : value/clientCount;
                var temp = BytesPerSecondChanged;
                if (temp != null)
                {
                    temp(this, value);
                }
            }
        }

        public double BytesPerSecondPerRequest
        {
            get { return _bytesPerSecondPerRequest; }
            set
            {
                _bytesPerSecondPerRequest = value;
                var temp = BytesPerSecondPerRequestChanged;
                if (temp != null)
                {
                    temp(this, value);
                }
            }
        }

        public void Dispose()
        {
            _timer.Dispose();
        }
    }
}