namespace LimitsMiddleware
{
    using System;
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
        private readonly FixedTokenBucket _tokenBucket;

        public ThrottledStream(
            Stream innerStream,
            FixedTokenBucket fixedTokenBucket)
        {
            innerStream.MustNotNull("innerStream");

            _innerStream = innerStream;
            _tokenBucket = fixedTokenBucket;
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
            var rateKiloBytesPerSecond = _tokenBucket.Capacity;
            if (rateKiloBytesPerSecond <= 0)
            {
                _innerStream.Write(buffer, offset, count);
                return;
            }

            var tempCount = count;
            var rateBytesPerSecond = rateKiloBytesPerSecond * 1024;

            // In the unlikely event that the count is greater than the rate (i.e. buffer
            // is 16KB (typically this is the max sixe) and the rate is < 16Kbs), we'll need
            // to split it into multiple.

            TimeSpan wait;
            while (tempCount > rateBytesPerSecond)
            {
                if (_tokenBucket.ShouldThrottle(tempCount / 1024, out wait))
                {
                    Console.WriteLine(wait);
                    if (wait > TimeSpan.Zero)
                    {
                        await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                    }
                }
                await _innerStream.WriteAsync(buffer, offset, rateBytesPerSecond, cancellationToken)
                    .ConfigureAwait(false);
                offset += rateBytesPerSecond;
                tempCount -= rateBytesPerSecond;
            }
            Console.WriteLine("Here {0}", _tokenBucket.CurrentTokenCount);

            while (_tokenBucket.ShouldThrottle(tempCount / 1024, out wait))
            {
                Console.WriteLine("{0} {1} {2}", tempCount / 1024, wait, _tokenBucket.CurrentTokenCount);
                if (wait > TimeSpan.Zero)
                {
                    await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                }
            }
            await _innerStream.WriteAsync(buffer, offset, tempCount, cancellationToken)
                .ConfigureAwait(false);
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
            WriteAsync(buffer, offset, count).Wait();
        }

        public override string ToString()
        {
            return _innerStream.ToString();
        }
    }
}