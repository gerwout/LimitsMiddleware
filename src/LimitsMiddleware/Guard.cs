// ReSharper disable once CheckNamespace
namespace LimitsMiddleware
{
    using System;

    internal static class Guard
    {
        internal static void MustNotNull(this object argument, string name)
        {
            if (argument == null)
            {
                throw new ArgumentNullException("name");
            }
        }
    }
}