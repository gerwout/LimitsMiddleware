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
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, int maxUrlLength)
        {
            app.MustNotNull("app");

            return MaxUrlLength(app, () => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, Func<int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="app">The IAppBuilder instance.</param>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>The IAppBuilder instance.</returns>
        public static IAppBuilder MaxUrlLength(this IAppBuilder app, Func<RequestContext, int> getMaxUrlLength)
        {
            app.MustNotNull("app");
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            app.Use(Limits.MaxUrlLength(getMaxUrlLength));
            return app;
        }
    }
}