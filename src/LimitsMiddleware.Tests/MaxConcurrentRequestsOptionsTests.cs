namespace LimitsMiddleware
{
    using FluentAssertions;
    using Xunit;

    public class MaxConcurrentRequestsOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_max_concurrent_requests()
        {
            new MaxConcurrentRequestOptions(1).GetMaxConcurrentRequests(null).Should().Be(1);
        }
    }
}