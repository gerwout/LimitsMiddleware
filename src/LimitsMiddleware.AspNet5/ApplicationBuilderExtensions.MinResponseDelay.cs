namespace Microsoft.AspNet.Builder
{
    using System;
    using LimitsMiddleware;
    using Owin;

    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="minDelay">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, int minDelay)
        {
            app.MustNotNull("app");

            return MinResponseDelay(app, () => minDelay);
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<int> getMinDelay)
        {
            app.MustNotNull("app");

            app.Use(Limits.MinResponseDelay(getMinDelay));
            return app;
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<TimeSpan> getMinDelay)
        {
            app.MustNotNull("app");

            app.Use(Limits.MinResponseDelay(getMinDelay));
            return app;
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMinDelay">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IApplicationBuilder MinResponseDelay(this IApplicationBuilder app, Func<RequestContext, TimeSpan> getMinDelay)
        {
            app.MustNotNull("app");

            app.Use(Limits.MinResponseDelay(getMinDelay));
            return app;
        }
    }
}
