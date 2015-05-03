namespace LimitsMiddleware
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using FluentAssertions;
    using Xunit;

    public class TaskTimerTests
    {
        [Fact]
        public async Task Should_timeout()
        {
            var timedout = false;
            using(new Timer(TimeSpan.FromMilliseconds(1), () => timedout = true))
            {
                await Task.Delay(TimeSpan.FromSeconds(1));
                timedout.Should().BeTrue();
            }
        }

        [Fact]
        public async Task When_disposed_should_not_timeout()
        {
            var timedout = false;
            using(new Timer(TimeSpan.FromMilliseconds(10), () => timedout = true))
            {}
            await Task.Delay(TimeSpan.FromSeconds(1));
            timedout.Should().BeFalse();
        }

        [Fact]
        public async Task When_reset_should_timeout_once()
        {
            var timeoutCount = 0;
            using(var timer = new Timer(TimeSpan.FromMilliseconds(100), () => Interlocked.Increment(ref timeoutCount)))
            {
                await Task.Delay(25);
                timer.Reset();
            }
            await Task.Delay(TimeSpan.FromSeconds(1));
            timeoutCount.Should().Be(1);
        }
    }
}