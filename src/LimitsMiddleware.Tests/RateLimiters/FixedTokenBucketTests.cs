namespace LimitsMiddleware.RateLimiters
{
    using System;
    using System.Threading;
    using FluentAssertions;
    using Xunit;

    public class FixedTokenBucketTests
    {
        private readonly FixedTokenBucket _bucket;
        private const long MaxTokens = 10;
        private const long RefillInterval = 10;
        private const long NLessThanMax = 2;
        private const long NGreaterThanMax = 12;
        private const int Cumulative = 2;
        private GetUtcNow _getUtcNow = () => SystemClock.GetUtcNow();

        public FixedTokenBucketTests()
        {
            _bucket = new FixedTokenBucket(MaxTokens, RefillInterval, 1000, _getUtcNow);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNTokensLessThanMax_ReturnsFalse()
        {
            int waitTime;
            var shouldThrottle = _bucket.ShouldThrottle(NLessThanMax, out waitTime);

            shouldThrottle.Should().BeFalse();
            _bucket.CurrentTokenCount.Should().Be(MaxTokens - NLessThanMax);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNTokensGreaterThanMax_ReturnsTrue()
        {
            int waitTime;
            var shouldThrottle = _bucket.ShouldThrottle(NGreaterThanMax, out waitTime);

            shouldThrottle.Should().BeTrue();
            _bucket.CurrentTokenCount.Should().Be(MaxTokens);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsLessThanMaxTokens_ReturnsFalse()
        {
            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax).Should().BeFalse();
            }

            var tokens = _bucket.CurrentTokenCount;

            tokens.Should().Be(MaxTokens - (Cumulative*NLessThanMax));
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsGreaterThanMaxTokens_ReturnsTrue()
        {
            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NGreaterThanMax).Should().BeTrue();
            }

            var tokens = _bucket.CurrentTokenCount;

            tokens.Should().Be(MaxTokens);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepNLessThanMax_ReturnsFalse()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            var before = _bucket.ShouldThrottle(NLessThanMax);
            var tokensBefore = _bucket.CurrentTokenCount;
            before.Should().BeFalse();
            tokensBefore.Should().Be(MaxTokens - NLessThanMax);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            var after = _bucket.ShouldThrottle(NLessThanMax);
            var tokensAfter = _bucket.CurrentTokenCount;

            after.Should().BeFalse();
            tokensAfter.Should().Be(MaxTokens - NLessThanMax);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNGreaterThanMaxSleepNGreaterThanMax_ReturnsTrue()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            var before = _bucket.ShouldThrottle(NGreaterThanMax);
            var tokensBefore = _bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().Be(MaxTokens);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            var after = _bucket.ShouldThrottle(NGreaterThanMax);
            var tokensAfter = _bucket.CurrentTokenCount;
            after.Should().BeTrue();
            tokensAfter.Should().Be(MaxTokens);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepCumulativeNLessThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            long sum = 0;
            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax).Should().BeFalse();
                sum += NLessThanMax;
            }
            var tokensBefore = _bucket.CurrentTokenCount;
            tokensBefore.Should().Be(MaxTokens - sum);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax).Should().BeFalse();
            }
            var tokensAfter = _bucket.CurrentTokenCount;
            tokensAfter.Should().Be(MaxTokens - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNLessThanMaxSleepCumulativeNGreaterThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            long sum = 0;
            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax).Should().BeFalse();
                sum += NLessThanMax;
            }
            var tokensBefore = _bucket.CurrentTokenCount;
            tokensBefore.Should().Be(MaxTokens - sum);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            for (int i = 0; i < 3*Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax);
            }

            var after = _bucket.ShouldThrottle(NLessThanMax);
            var tokensAfter = _bucket.CurrentTokenCount;

            after.Should().BeTrue();
            tokensAfter.Should().BeLessThan(NLessThanMax);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNLessThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            for (var i = 0; i < 3*Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax);
            }

            var before = _bucket.ShouldThrottle(NLessThanMax);
            var tokensBefore = _bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().BeLessThan(NLessThanMax);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            long sum = 0;
            for (int i = 0; i < Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax).Should().BeFalse();
                sum += NLessThanMax;
            }

            var tokensAfter = _bucket.CurrentTokenCount;
            tokensAfter.Should().Be(MaxTokens - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNGreaterThanMax()
        {
            _getUtcNow = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = _getUtcNow();

            for (int i = 0; i < 3*Cumulative; i++)
                _bucket.ShouldThrottle(NLessThanMax);

            var before = _bucket.ShouldThrottle(NLessThanMax);
            var tokensBefore = _bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().BeLessThan(NLessThanMax);

            _getUtcNow = () => virtualNow.AddSeconds(RefillInterval);

            for (int i = 0; i < 3*Cumulative; i++)
            {
                _bucket.ShouldThrottle(NLessThanMax);
            }
            var after = _bucket.ShouldThrottle(NLessThanMax);
            var tokensAfter = _bucket.CurrentTokenCount;

            after.Should().BeTrue();
            tokensAfter.Should().BeLessThan(NLessThanMax);
        }

        [Fact]
        public void ShouldThrottle_WhenThread1NLessThanMaxAndThread2NLessThanMax()
        {
            var t1 = new Thread(p =>
            {
                var throttle = _bucket.ShouldThrottle(NLessThanMax);
                throttle.Should().BeFalse();
            });            
            
            var t2 = new Thread(p =>
            {
                var throttle = _bucket.ShouldThrottle(NLessThanMax);
                throttle.Should().BeFalse();
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            _bucket.CurrentTokenCount.Should().Be(MaxTokens - 2*NLessThanMax);
        }
        
        [Fact]
        public void ShouldThrottle_Thread1NGreaterThanMaxAndThread2NGreaterThanMax()
        {
            var shouldThrottle = _bucket.ShouldThrottle(NGreaterThanMax);
            shouldThrottle.Should().BeTrue();

            var t1 = new Thread(p =>
            {
                var throttle = _bucket.ShouldThrottle(NGreaterThanMax);
                throttle.Should().BeTrue();
            });            
            
            var t2 = new Thread(p =>
            {
                var throttle = _bucket.ShouldThrottle(NGreaterThanMax);
                throttle.Should().BeTrue();
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            _bucket.CurrentTokenCount.Should().Be(MaxTokens);
        }
    }
}