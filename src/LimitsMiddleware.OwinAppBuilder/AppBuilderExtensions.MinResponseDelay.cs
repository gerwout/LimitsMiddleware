namespace Owin
{
    using System;
    using LimitsMiddleware;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;

    public static partial class AppBuilderExtensions
    {
        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="minDelay">The maximum number of bytes per second to be transferred. Use 0 or a negative
        /// number to specify infinite bandwidth.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MinResponseDelay(this IAppBuilder app, int minDelay)
        {
            app.MustNotNull("app");

            return MinResponseDelay(app, () => minDelay);
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMinDelay">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MinResponseDelay(this IAppBuilder app, Func<int> getMinDelay)
        {
            app.MustNotNull("app");

            return MinResponseDelay(app, new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMinDelay">A delegate to retrieve the maximum number of bytes per second to be transferred.
        /// Allows you to supply different values at runtime. Use 0 or a negative number to specify infinite bandwidth.</param>
        /// <returns>The app instance.</returns>
        public static IAppBuilder MinResponseDelay(this IAppBuilder app, Func<TimeSpan> getMinDelay)
        {
            app.MustNotNull("app");

            return MinResponseDelay(app, new MinResponseDelayOptions(getMinDelay));
        }

        /// <summary>
        /// Sets a minimum delay before sending the response.
        /// </summary>
        /// <param name="options">The min response delay options.</param>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="perRequestOptions">The max bandwith options.</param>
        /// <returns>The IAppBuilder instance.</returns>
        /// <exception cref="System.ArgumentNullException">app</exception>
        public static IAppBuilder MinResponseDelay(this IAppBuilder app, MinResponseDelayOptions perRequestOptions)
        {
            app.MustNotNull("app");
            perRequestOptions.MustNotNull("options");

            app.Use(Limits.MinResponseDelay(perRequestOptions));
            return app;
        }
    }
}
