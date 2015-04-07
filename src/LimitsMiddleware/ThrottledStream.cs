namespace LimitsMiddleware
{
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    /* From http://www.codeproject.com/Articles/18243/Bandwidth-throttling
     * Release under CPOL licence http://www.codeproject.com/info/cpol10.aspx
     */

    internal class ThrottledStream : Stream
    {
        private readonly Stream _innerStream;
        private readonly RateLimiterBase _rateLimiter;

        public ThrottledStream(Stream innerStream, RateLimiterBase rateLimiter)
        {
            innerStream.MustNotNull("innerStream");
            rateLimiter.MustNotNull("rateLimiter");

            _innerStream = innerStream;
            _rateLimiter = rateLimiter;
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
            _rateLimiter.Throttle(count * 8).Wait();

            return _innerStream.Read(buffer, offset, count);
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _innerStream.Seek(offset, origin);
        }

        public override async Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _rateLimiter.Throttle(count * 8);
            await _innerStream.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            await _rateLimiter.Throttle(count);
            return await _innerStream.ReadAsync(buffer, offset, count, cancellationToken);
        }

        public override void SetLength(long value)
        {
            _innerStream.SetLength(value);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _rateLimiter.Throttle(count).Wait();
            _innerStream.Write(buffer, offset, count);
        }

        public override string ToString()
        {
            return _innerStream.ToString();
        }
    }
}