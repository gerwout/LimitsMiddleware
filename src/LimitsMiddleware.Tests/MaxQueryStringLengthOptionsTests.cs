namespace LimitsMiddleware
{
    using FluentAssertions;
    using Xunit;

    public class MaxQueryStringLengthOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_max_concurrent_requests()
        {
            new MaxQueryStringLengthOptions(1).MaxQueryStringLength.Should().Be(1);
        }
    }
}