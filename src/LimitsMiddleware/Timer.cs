namespace LimitsMiddleware
{
    using System;
    using System.Threading;

    internal sealed class Timer : IDisposable
    {
        private readonly TimeSpan _delay;
        private readonly Action _onTimeout;
        private CancellationTokenRegistration _cancellationTokenRegistration;

        public Timer(TimeSpan delay, Action onTimeout)
        {
            _delay = delay;
            _onTimeout = onTimeout;
            Reset();
        }

        public void Reset()
        {
            var cts = new CancellationTokenSource(_delay);
            _cancellationTokenRegistration = cts.Token.Register(_onTimeout);
        }

        public void Dispose()
        {
            _cancellationTokenRegistration.Dispose();
        }
    }
}