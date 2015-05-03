namespace LimitsMiddleware
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using LimitsMiddleware.RateLimiters;

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
            var rateBytesPerSecond = Convert.ToInt32(_tokenBucket.Rate);
            if (rateBytesPerSecond <= 0)
            {
                _innerStream.Write(buffer, offset, count);
                return;
            }

            var tempCountBytes = count;

            // In the unlikely event that the count is greater than the rate (i.e. buffer
            // is 16KB (typically this is the max size) and the rate is < 16Kbs), we'll need
            // to split it into multiple.

            TimeSpan wait;
            while (tempCountBytes > rateBytesPerSecond)
            {
                if (_tokenBucket.ShouldThrottle(tempCountBytes, out wait))
                {
                    if (wait > TimeSpan.Zero)
                    {
                        await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                    }
                }
                await _innerStream.WriteAsync(buffer, offset, rateBytesPerSecond, cancellationToken)
                    .ConfigureAwait(false);
                offset += rateBytesPerSecond;
                tempCountBytes -= rateBytesPerSecond;
            }
            while (_tokenBucket.ShouldThrottle(tempCountBytes, out wait))
            {
                if (wait > TimeSpan.Zero)
                {
                    await Task.Delay(wait, cancellationToken).ConfigureAwait(false);
                }
            }
            await _innerStream.WriteAsync(buffer, offset, tempCountBytes, cancellationToken)
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