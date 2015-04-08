namespace LimitsMiddleware
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class MovingAverageTests
    {
        [Fact]
        public void Should_calculate_average()
        {
            var movingAverage = new GlobalRateLimiter.MovingAverage(TimeSpan.FromSeconds(5));

            movingAverage.AddInterval(100, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(200, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(300, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(200, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(100, TimeSpan.FromSeconds(1));

            movingAverage.Average.Should().Be(180d);
        }

        [Fact]
        public void Should_calculate_moving_average_within_window()
        {
            var movingAverage = new GlobalRateLimiter.MovingAverage(TimeSpan.FromSeconds(5));

            movingAverage.AddInterval(100, TimeSpan.FromSeconds(1)); //should be discarded
            movingAverage.AddInterval(200, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(300, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(200, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(100, TimeSpan.FromSeconds(1));
            movingAverage.AddInterval(150, TimeSpan.FromSeconds(1));

            movingAverage.Average.Should().Be(190d);
        }
    }
}