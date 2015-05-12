namespace Microsoft.AspNet.Builder
{
    using System;
    using LimitsMiddleware;
    using Owin;

    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app, int maxQueryStringLength)
        {
            app.MustNotNull("app");

            return MaxQueryStringLength(app, () => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app, Func<int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }


        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxQueryStringLength(this IApplicationBuilder app, Func<RequestContext, int> getMaxQueryStringLength)
        {
            app.MustNotNull("app");
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            app.Use(Limits.MaxQueryStringLength(getMaxQueryStringLength));

            return app;
        }
    }
}
