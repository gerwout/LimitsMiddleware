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
