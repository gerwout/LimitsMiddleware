namespace Microsoft.AspNet.Builder
{
    using System;
    using LimitsMiddleware;
    using Owin;

    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Limits the bandwith used globally by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxBytesPerSecond">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxBandwidthGlobal(this IApplicationBuilder app, int maxBytesPerSecond)
        {
            app.MustNotNull("app");

            return MaxBandwidthGlobal(app, () => maxBytesPerSecond);
        }

        /// <summary>
        /// Limits the bandwith used globally by the subsequent stages in the owin pipeline.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxBytesPerSecond">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MaxBandwidthGlobal(this IApplicationBuilder app, Func<int> getMaxBytesPerSecond)
        {
            app.MustNotNull("app");

            app.Use(Limits.MaxBandwidthGlobal(getMaxBytesPerSecond));
            return app;
        }
    }
}
