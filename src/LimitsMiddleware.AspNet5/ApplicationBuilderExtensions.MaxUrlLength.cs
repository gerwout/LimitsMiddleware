namespace Microsoft.AspNet.Builder
{
    using System;
    using LimitsMiddleware;
    using Owin;

    public static partial class ApplicationBuilderExtensions
    {
        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, int maxUrlLength)
        {
            app.MustNotNull("app");

            return MaxUrlLength(app, () => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, Func<int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IApplicationBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IApplicationBuilder instance.</returns>
        public static IApplicationBuilder MaxUrlLength(this IApplicationBuilder app, Func<RequestContext, int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }
    }
}