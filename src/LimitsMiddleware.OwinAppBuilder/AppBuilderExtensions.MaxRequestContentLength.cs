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
