namespace LimitsMiddleware
{
    using FluentAssertions;
    using Xunit;

    public class MaxBandwidthOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_max_bytes_per_second()
        {
            new MaxBandwidthPerRequestOptions(1).GetMaxBytesPerSecond(null).Should().Be(1);
        }
    }
}