namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LimitsMiddleware.Logging;

    internal class TimeoutStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly TimeSpan _timeout;
        private static readonly ILog Logger = LogProvider.For<TimeoutStream>();
        private readonly Timer _timer;

        public TimeoutStream(Stream innerStream, TimeSpan timeout)
        {
            _innerStream = innerStream;
            _timeout = timeout;

            _timer = new Timer(_timeout, () =>
            {
                Logger.Info("Timeout of {0} reached.".FormatWith(_timeout));
                base.Dispose();
            });
        }

        public override bool CanRead
        {
            get { return _innerStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return _innerStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return _innerStream.CanWrite; }
        }

        public override long Length
        {
            get { return _innerStream.Length; }
        }

        public override long Position
        {
            get { return _innerStream.Position; }
            set { _innerStream.Position = value; }
        }

        public override void Flush()
        {
            _innerStream.Flush();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _innerStream.Write(buffer, offset, count);
        }

        protected override void Dispose(bool disposing)
        {
            if(disposing)
            {
                _timer.Dispose();
                _innerStream.Dispose();
            }
            base.Dispose(disposing);
        }

        public override Task CopyToAsync(Stream destination, int bufferSize, CancellationToken cancellationToken)
        {
            return _innerStream.CopyToAsync(destination, bufferSize, cancellationToken);
        }

        public override Task FlushAsync(CancellationToken cancellationToken)
        {
            return _innerStream.FlushAsync(cancellationToken);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            int read = await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
            Reset();
            return read;
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
            Reset();
        }

        private void Reset()
        {
            _timer.Reset();
        }
    }
}