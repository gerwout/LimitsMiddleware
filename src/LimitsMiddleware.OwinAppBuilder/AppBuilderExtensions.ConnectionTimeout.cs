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

#pragma warning disable 1591
    public static partial class AppBuilderExtensions
#pragma warning restore 1591
    {
        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="timeout">The timeout.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, TimeSpan timeout)
        {
            app.MustNotNull("app");

            app.Use(Limits.ConnectionTimeout(timeout));

            return app;
        }

        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, Func<TimeSpan> getTimeout)
        {
            app.MustNotNull("app");
            getTimeout.MustNotNull("getTimeout");

            app.Use(Limits.ConnectionTimeout(getTimeout));

            return app;
        }


        /// <summary>
        /// Timeouts the connection if there hasn't been an read activity on the request body stream or any
        /// write activity on the response body stream.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getTimeout">A delegate to retrieve the timeout timespan. Allows you
        /// to supply different values at runtime.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getTimeout</exception>
        public static IAppBuilder ConnectionTimeout(this IAppBuilder app, Func<RequestContext, TimeSpan> getTimeout)
        {
            app.MustNotNull("app");
            getTimeout.MustNotNull("getTimeout");

            app.Use(Limits.ConnectionTimeout(getTimeout));

            return app;
        }
    }
}
