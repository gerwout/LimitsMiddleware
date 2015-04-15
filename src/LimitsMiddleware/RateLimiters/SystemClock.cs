// https://github.com/robertmircea/RateLimiters

namespace LimitsMiddleware.RateLimiters
{
    using System;

    public static class SystemClock
    {
        public static readonly GetUtcNow GetUtcNow = () => DateTimeOffset.UtcNow;

        public static readonly GetEnvironmentTickCount GetEnvironmentTickCount = () => Environment.TickCount;
    }
}