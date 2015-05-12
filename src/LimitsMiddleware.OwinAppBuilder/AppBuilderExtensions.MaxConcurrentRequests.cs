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
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxConcurrentRequests">The maximum number of concurrent requests. Use 0 or a negative
        /// number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, int maxConcurrentRequests)
        {
            app.MustNotNull("app");

            return MaxConcurrentRequests(app, () => maxConcurrentRequests);
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, Func<int> getMaxConcurrentRequests)
        {
            app.MustNotNull("app");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests));

            return app;
        }

        /// <summary>
        /// Limits the number of concurrent requests that can be handled used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxConcurrentRequests">A delegate to retrieve the maximum number of concurrent requests. Allows you
        /// to supply different values at runtime. Use 0 or a negative number to specify unlimited number of concurrent requests.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxConcurrentRequests(this IAppBuilder app, Func<RequestContext, int> getMaxConcurrentRequests)
        {
            app.MustNotNull("app");
            getMaxConcurrentRequests.MustNotNull("getMaxConcurrentRequests");

            app.Use(Limits.MaxConcurrentRequests(getMaxConcurrentRequests));

            return app;
        }
    }
}
