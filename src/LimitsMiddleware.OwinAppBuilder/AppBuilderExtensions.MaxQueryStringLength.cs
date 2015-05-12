#if ASPNET5
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

    public static partial class AppBuilderExtensions
    {
        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, int maxQueryStringLength)
        {
            app.MustNotNull("app");

            return MaxQueryStringLength(app, () => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, Func<int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }


        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxQueryStringLength(this IAppBuilder app, Func<RequestContext, int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }
    }
}
