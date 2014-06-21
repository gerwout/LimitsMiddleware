namespace LimitsMiddleware
{
    using FluentAssertions;
    using Xunit;

    public class MaxRequestContentLengthOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_max_request_content_length()
        {
            new MaxRequestContentLengthOptions(1).MaxContentLength.Should().Be(1);
        }
    }
}