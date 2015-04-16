namespace LimitsMiddleware
{
    using System;
    using System.Threading.Tasks;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

    public static partial class Limits
    {
        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="maxUrlLength">Maximum length of the URL.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxUrlLength(int maxUrlLength)
        {
            return MaxUrlLength(() => maxUrlLength);
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="getMaxUrlLength">Maximum length of the URL.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxUrlLength(Func<int> getMaxUrlLength)
        {
            return MaxUrlLength(_ => getMaxUrlLength());
        }

        /// <summary>
        /// Limits the length of the URL.
        /// </summary>
        /// <param name="getMaxUrlLength">A delegate to get the maximum URL length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxUrlLength</exception>
        public static MidFunc MaxUrlLength(Func<RequestContext, int> getMaxUrlLength)
        {
            getMaxUrlLength.MustNotNull("getMaxUrlLength");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxUrlLength");

            return
                next =>
                env =>
                {
                    var context = new OwinContext(env);
                    int maxUrlLength = getMaxUrlLength(new RequestContext(context.Request));
                    string unescapedUri = Uri.UnescapeDataString(context.Request.Uri.AbsoluteUri);

                    logger.Debug("Checking request url length.");
                    if (unescapedUri.Length > maxUrlLength)
                    {
                        logger.Info(
                            "Url \"{0}\"(Length: {2}) exceeds allowed length of {1}. Request rejected.".FormatWith(
                            unescapedUri,
                            maxUrlLength,
                            unescapedUri.Length));
                        context.Response.StatusCode = 414;
                        context.Response.ReasonPhrase = "Request-URI Too Large";
                        context.Response.Write(context.Response.ReasonPhrase);
                        return Task.FromResult(0);
                    }
                    logger.Debug("Check passed. Request forwarded.");
                    return next(env);
                };
        }

    }
}
