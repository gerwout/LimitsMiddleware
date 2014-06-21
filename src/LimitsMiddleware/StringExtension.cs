// ReSharper disable once CheckNamespace
namespace LimitsMiddleware
{
    internal static class StringExtension
    {
        internal static string FormatWith(this string source, params object[] args)
        {
            return string.Format(source, args);
        }
    }
}