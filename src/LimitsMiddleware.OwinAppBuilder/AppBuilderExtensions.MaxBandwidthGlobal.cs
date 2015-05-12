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
        /// Limits the bandwith used globally by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxBandwidthGlobal(this IAppBuilder app, int maxBytesPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthGlobal(app, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used globally by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MaxBandwidthGlobal(this IAppBuilder app, Func<int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");

            app.Use(Limits.MaxBandwidthGlobal(getMaxBytesPerSecond));
            return app;
        }
    }
}
