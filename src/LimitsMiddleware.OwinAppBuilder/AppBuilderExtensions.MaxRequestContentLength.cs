﻿#if ASPNET5
namespace Microsoft.AspNet.Builder
#else
namespace Owin
#endif
{
    using System;
    using LimitsMiddleware;

#if ASPNET5
    using IAppBuilder = Microsoft.AspNet.Builder.IApplicationBuilder;
#endif

#pragma warning disable 1591
    public static partial class AppBuilderExtensions
#pragma warning restore 1591
    {
        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxContentLength">Maximum length of the content.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, int maxContentLength)
        {
            app.MustNotNull("app");

            return MaxRequestContentLength(app, () => maxContentLength);
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, Func<int> getMaxContentLength)
        {
            app.MustNotNull("app");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength));

            return app;
        }

        /// <summary>
        /// Limits the length of the request content.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxContentLength">A delegate to get the maximum content length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxRequestContentLength(this IAppBuilder app, Func<RequestContext, int> getMaxContentLength)
        {
            app.MustNotNull("app");
            getMaxContentLength.MustNotNull("getMaxContentLength");

            app.Use(Limits.MaxRequestContentLength(getMaxContentLength));

            return app;
        }
    }
}
