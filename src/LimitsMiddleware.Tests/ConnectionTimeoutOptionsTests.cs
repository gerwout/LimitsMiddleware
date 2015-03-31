namespace LimitsMiddleware
{
    using System;
    using FluentAssertions;
    using Xunit;

    public class ConnectionTimeoutOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_a_timeout()
        {
            new ConnectionTimeoutOptions(TimeSpan.FromSeconds(1)).GetTimeout(null).Should().Be(TimeSpan.FromSeconds(1));
        }
    }
}