namespace LimitsMiddleware
{
    using System.Threading.Tasks;

    internal abstract class RateLimiterBase
    {
        public abstract Task Throttle(int bytesToWrite);
    }
}