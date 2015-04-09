namespace LimitsMiddleware
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using FluentAssertions;
    using LimitsMiddleware.Helpers;
    using Xunit;

    public class MovingAverageTests : IDisposable
    {
        private readonly MovingAverageCalculator _sut;

        public MovingAverageTests()
        {
            _sut = new MovingAverageCalculator();
        }

        [Fact]
        public async Task Should_calculate_moving_average()
        {
            Task<double> bytesPerSecondChanged = TaskExt
                .FromEvent<double>()
                .Start(
                    h => _sut.BytesPerSecondChanged += h,
                    h => _sut.BytesPerSecondChanged -= h);
            var bytesToWrite = new[] { 100, 200, 300, 200, 100 };
            var delay = MovingAverageCalculator.DefaultSamplingIntevalMilliseconds / bytesToWrite.Length;
            var bytesWrittenTotal = bytesToWrite.Sum();
            Func<int, Task> bytesWritten = async count =>
            {
                _sut.BytesWritten(count);
                await Task.Delay(delay);
            };
            
            foreach (var i in bytesToWrite)
            {
                await bytesWritten(i);
            }

            (await bytesPerSecondChanged)
                .Should()
                // Should be within +/- 5% of the max bytes written
                .BeInRange(bytesWrittenTotal * 0.95, bytesWrittenTotal * 1.05); 
        }

        [Fact]
        public async Task Should_calculate_moving_average_per_client()
        {
            var calculator = new MovingAverageCalculator();
            Task<double> bytesPerSecondPerRequestChanged = TaskExt
                .FromEvent<double>()
                .Start(
                    h => calculator.BytesPerSecondPerRequestChanged += h,
                    h => calculator.BytesPerSecondPerRequestChanged -= h);
            var bytesToWrite = new[] { 100, 200, 300, 200, 100 };
            var delay = MovingAverageCalculator.DefaultSamplingIntevalMilliseconds / bytesToWrite.Length;
            var bytesWrittenTotal = bytesToWrite.Sum();
            Func<int, Task> bytesWritten = async count =>
            {
                calculator.BytesWritten(count);
                await Task.Delay(delay);
            };

            // 3 concurrent requests
            using (calculator.AddRequest())
            using (calculator.AddRequest())
            using (calculator.AddRequest())
            {
                Parallel.ForEach(bytesToWrite, i => bytesWritten(i));

                var i1 = (bytesWrittenTotal/3);
                var bytesPerSecondPerRequest = await bytesPerSecondPerRequestChanged;

                bytesPerSecondPerRequest.Should()
                    // Should be within +/- 5% of the (max bytes written/number of requests)
                    .BeInRange(i1*0.95, i1*1.05);
            }
        }

        public void Dispose()
        {
            _sut.Dispose();
        }
    }
}