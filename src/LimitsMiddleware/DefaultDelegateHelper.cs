namespace LimitsMiddleware
{
    using System;

    internal static class DefaultDelegateHelper
    {
        public static readonly Func<int, string> DefaultReasonPhrase = _ => string.Empty;
    }
}