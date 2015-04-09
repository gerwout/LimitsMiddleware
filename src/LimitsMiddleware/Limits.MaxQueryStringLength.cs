namespace LimitsMiddleware
{
    using System;
    using LimitsMiddleware.LibOwin;
    using LimitsMiddleware.Logging;
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

    public static partial class Limits
    {
        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="maxQueryStringLength">Maximum length of the query string.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxQueryStringLength(int maxQueryStringLength)
        {
            return MaxQueryStringLength(_ => maxQueryStringLength);
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        public static MidFunc MaxQueryStringLength(Func<int> getMaxQueryStringLength)
        {
            return MaxQueryStringLength(_ => getMaxQueryStringLength());
        }

        /// <summary>
        /// Limits the length of the query string.
        /// </summary>
        /// <param name="getMaxQueryStringLength">A delegate to get the maximum query string length.</param>
        /// <returns>An OWIN middleware delegate.</returns>
        /// <exception cref="System.ArgumentNullException">getMaxQueryStringLength</exception>
        public static MidFunc MaxQueryStringLength(Func<RequestContext, int> getMaxQueryStringLength)
        {
            getMaxQueryStringLength.MustNotNull("getMaxQueryStringLength");

            var logger = LogProvider.GetLogger("LimitsMiddleware.MaxQueryStringLength");

            return
                next =>
                async env =>
                {
                    var context = new OwinContext(env);
                    var requestContext = new RequestContext(context.Request);

                    QueryString queryString = context.Request.QueryString;
                    if (queryString.HasValue)
                    {
                        int maxQueryStringLength = getMaxQueryStringLength(requestContext);
                        string unescapedQueryString = Uri.UnescapeDataString(queryString.Value);
                        logger.Debug("Querystring of request with an unescaped length of {0}".FormatWith(unescapedQueryString.Length));
                        if (unescapedQueryString.Length > maxQueryStringLength)
                        {
                            logger.Info("Querystring (Length {0}) too long (allowed {1}). Request rejected.".FormatWith(
                                unescapedQueryString.Length,
                                maxQueryStringLength));
                            context.Response.StatusCode = 414;
                            context.Response.ReasonPhrase = "Request-URI Too Large";
                            return;
                        }
                        logger.Debug("Querystring length check passed.");
                    }
                    else
                    {
                        logger.Debug("No querystring.");
                    }
                    await next(env);
                };
        }
    }
}
