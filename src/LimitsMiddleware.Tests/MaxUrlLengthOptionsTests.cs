namespace LimitsMiddleware
{
    using FluentAssertions;
    using Xunit;

    public class MaxUrlLengthOptionsTests
    {
        [Fact]
        public void When_create_then_can_get_max_url_length()
        {
            new MaxUrlLengthOptions(1).MaxUrlLength.Should().Be(1);
        }
    }
}