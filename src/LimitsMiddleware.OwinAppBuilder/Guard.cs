#if ASPNET5
namespace Microsoft.AspNet.Builder
#else
namespace Owin
#endif
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