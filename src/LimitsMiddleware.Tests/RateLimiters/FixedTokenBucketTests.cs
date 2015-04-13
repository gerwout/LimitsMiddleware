using System;
using System.Threading;

namespace Bert.RateLimiters.Tests
{
    using FluentAssertions;
    using Xunit;

    public class FixedTokenBucketTests
    {
        private FixedTokenBucket bucket;
        public const long MAX_TOKENS = 10;
        public const long REFILL_INTERVAL = 10;
        public const long N_LESS_THAN_MAX = 2;
        public const long N_GREATER_THAN_MAX = 12;
        private const int CUMULATIVE = 2;

        public FixedTokenBucketTests()
        {
            bucket = new FixedTokenBucket(MAX_TOKENS, REFILL_INTERVAL, 1000);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNTokensLessThanMax_ReturnsFalse()
        {
            int waitTime;
            var shouldThrottle = bucket.ShouldThrottle(N_LESS_THAN_MAX, out waitTime);

            shouldThrottle.Should().BeFalse();
            bucket.CurrentTokenCount.Should().Be(MAX_TOKENS - N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNTokensGreaterThanMax_ReturnsTrue()
        {
            int waitTime;
            var shouldThrottle = bucket.ShouldThrottle(N_GREATER_THAN_MAX, out waitTime);

            shouldThrottle.Should().BeTrue();
            bucket.CurrentTokenCount.Should().Be(MAX_TOKENS);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsLessThanMaxTokens_ReturnsFalse()
        {
            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX).Should().BeFalse();
            }

            var tokens = bucket.CurrentTokenCount;

            tokens.Should().Be(MAX_TOKENS - (CUMULATIVE*N_LESS_THAN_MAX));
        }

        [Fact]
        public void ShouldThrottle_WhenCalledCumulativeNTimesIsGreaterThanMaxTokens_ReturnsTrue()
        {
            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_GREATER_THAN_MAX).Should().BeTrue();
            }

            var tokens = bucket.CurrentTokenCount;

            tokens.Should().Be(MAX_TOKENS);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepNLessThanMax_ReturnsFalse()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            var before = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensBefore = bucket.CurrentTokenCount;
            before.Should().BeFalse();
            tokensBefore.Should().Be(MAX_TOKENS - N_LESS_THAN_MAX);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            var after = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensAfter = bucket.CurrentTokenCount;

            after.Should().BeFalse();
            tokensAfter.Should().Be(MAX_TOKENS - N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNGreaterThanMaxSleepNGreaterThanMax_ReturnsTrue()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            var before = bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            var tokensBefore = bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().Be(MAX_TOKENS);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            var after = bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            var tokensAfter = bucket.CurrentTokenCount;
            after.Should().BeTrue();
            tokensAfter.Should().Be(MAX_TOKENS);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithNLessThanMaxSleepCumulativeNLessThanMax()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX).Should().BeFalse();
                sum += N_LESS_THAN_MAX;
            }
            var tokensBefore = bucket.CurrentTokenCount;
            tokensBefore.Should().Be(MAX_TOKENS - sum);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX).Should().BeFalse();
            }
            var tokensAfter = bucket.CurrentTokenCount;
            tokensAfter.Should().Be(MAX_TOKENS - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNLessThanMaxSleepCumulativeNGreaterThanMax()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX).Should().BeFalse();
                sum += N_LESS_THAN_MAX;
            }
            var tokensBefore = bucket.CurrentTokenCount;
            tokensBefore.Should().Be(MAX_TOKENS - sum);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }

            var after = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensAfter = bucket.CurrentTokenCount;

            after.Should().BeTrue();
            tokensAfter.Should().BeLessThan(N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNLessThanMax()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            for (int i = 0; i < 3*CUMULATIVE; i++)
                bucket.ShouldThrottle(N_LESS_THAN_MAX);

            var before = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensBefore = bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().BeLessThan(N_LESS_THAN_MAX);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            long sum = 0;
            for (int i = 0; i < CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX).Should().BeFalse();
                sum += N_LESS_THAN_MAX;
            }

            var tokensAfter = bucket.CurrentTokenCount;
            tokensAfter.Should().Be(MAX_TOKENS - sum);
        }

        [Fact]
        public void ShouldThrottle_WhenCalledWithCumulativeNGreaterThanMaxSleepCumulativeNGreaterThanMax()
        {
            SystemTime.SetCurrentTimeUtc = () => new DateTime(2014, 2, 27, 0, 0, 0, DateTimeKind.Utc);
            var virtualNow = SystemTime.UtcNow;

            for (int i = 0; i < 3*CUMULATIVE; i++)
                bucket.ShouldThrottle(N_LESS_THAN_MAX);

            var before = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensBefore = bucket.CurrentTokenCount;

            before.Should().BeTrue();
            tokensBefore.Should().BeLessThan(N_LESS_THAN_MAX);

            SystemTime.SetCurrentTimeUtc = () => virtualNow.AddSeconds(REFILL_INTERVAL);

            for (int i = 0; i < 3*CUMULATIVE; i++)
            {
                bucket.ShouldThrottle(N_LESS_THAN_MAX);
            }
            var after = bucket.ShouldThrottle(N_LESS_THAN_MAX);
            var tokensAfter = bucket.CurrentTokenCount;

            after.Should().BeTrue();
            tokensAfter.Should().BeLessThan(N_LESS_THAN_MAX);
        }

        [Fact]
        public void ShouldThrottle_WhenThread1NLessThanMaxAndThread2NLessThanMax()
        {
            var t1 = new Thread(p =>
            {
                var throttle = bucket.ShouldThrottle(N_LESS_THAN_MAX);
                throttle.Should().BeFalse();
            });            
            
            var t2 = new Thread(p =>
            {
                var throttle = bucket.ShouldThrottle(N_LESS_THAN_MAX);
                throttle.Should().BeFalse();
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            bucket.CurrentTokenCount.Should().Be(MAX_TOKENS - 2*N_LESS_THAN_MAX);
        }
        
        [Fact]
        public void ShouldThrottle_Thread1NGreaterThanMaxAndThread2NGreaterThanMax()
        {
            var shouldThrottle = bucket.ShouldThrottle(N_GREATER_THAN_MAX);
            shouldThrottle.Should().BeTrue();

            var t1 = new Thread(p =>
            {
                var throttle = bucket.ShouldThrottle(N_GREATER_THAN_MAX);
                throttle.Should().BeTrue();
            });            
            
            var t2 = new Thread(p =>
            {
                var throttle = bucket.ShouldThrottle(N_GREATER_THAN_MAX);
                throttle.Should().BeTrue();
            });

            t1.Start();
            t2.Start();

            t1.Join();
            t2.Join();

            bucket.CurrentTokenCount.Should().Be(MAX_TOKENS);
        }
    }
}