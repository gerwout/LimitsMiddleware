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
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, int maxBytesPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthPerRequest(app, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBytesPerSecond));
            return app;
        }

        /// <summary>
        /// Limits the bandwith used by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <exception cref="System.ArgumentNullException">app</exception>
        /// <exception cref="System.ArgumentNullException">getMaxBytesPerSecond</exception>
        public static IAppBuilder MaxBandwidthPerRequest(this IAppBuilder app, Func<RequestContext, int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");
            getMaxBytesPerSecond.MustNotNull("getMaxBytesPerSecond");

            app.Use(Limits.MaxBandwidthPerRequest(getMaxBytesPerSecond));
            return app;
        }
    }
}
