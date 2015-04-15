namespace LimitsMiddleware
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LimitsMiddleware.RateLimiters;

    /* From http://www.codeproject.com/Articles/18243/Bandwidth-throttling
     * Release under CPOL licence http://www.codeproject.com/info/cpol10.aspx
     */

    internal class ThrottledStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly int _rateKiloBytesPerSecond;
        private readonly FixedTokenBucket _throttler;

        public ThrottledStream(Stream innerStream, int rateKiloBytesPerSecond = 0)
        {
            innerStream.MustNotNull("innerStream");

            _innerStream = innerStream;
            _rateKiloBytesPerSecond = rateKiloBytesPerSecond;
            if (rateKiloBytesPerSecond > 0)
            {
                //_throttler = new RollingWindowThrottler(rateKiloBytesPerSecond, TimeSpan.FromSeconds(1)); // TODO use null throttler
                _throttler = new FixedTokenBucket(rateKiloBytesPerSecond, TimeSpan.FromSeconds(1));
            }
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

        public override void Close()
        {
            _innerStream.Close();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            if (_throttler != null)
            {
                TimeSpan wait;
                if (_throttler.ShouldThrottle(count / 1024, out wait))
                {

                    Console.WriteLine(wait);
                    if (wait > TimeSpan.Zero)
                    {
                        await Task.Delay(wait, cancellationToken);
                    }
                }
            }
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

            if (_throttler != null)
            {
                var x = count;
                var rateBytesPerSecond = _rateKiloBytesPerSecond*1024;
                TimeSpan wait;
                while (x > rateBytesPerSecond)
                {
                    if (_throttler.ShouldThrottle(x / 1024, out wait))
                    {
                        Console.WriteLine(wait);
                        if (wait > TimeSpan.Zero)
                        {
                            Task.Delay(wait).Wait();
                        }
                    }
                    _innerStream.Write(buffer, offset, rateBytesPerSecond);
                    offset += rateBytesPerSecond;
                    x -= rateBytesPerSecond;
                }
                Console.WriteLine("Here {0}", _throttler.CurrentTokenCount);

                while (_throttler.ShouldThrottle(x/1024, out wait))
                {
                    Console.WriteLine("{0} {1} {2}", x/1024, wait, _throttler.CurrentTokenCount);
                    if (wait > TimeSpan.Zero)
                    {
                        Task.Delay(wait).Wait();
                    }
                }
                _innerStream.Write(buffer, offset, x);
            }
            else
            {
                _innerStream.Write(buffer, offset, count);
            }
        }

        public override string ToString()
        {
            return _innerStream.ToString();
        }
    }
}