namespace LimitsMiddleware
{
    using global::Owin;
    using Owin;
    using AppFunc = System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>;
    using MidFunc = System.Func<
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
        System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >;
    using BuildFunc = System.Action<
        System.Func<
            System.Collections.Generic.IDictionary<string, object>,
            System.Func<
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
                System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
        >>>;

    internal static class AppBuilderExtensions
    {
        internal static BuildFunc UseOwin(this IAppBuilder builder)
        {
            return middleware => builder.Use(middleware(builder.Properties));
        }

        internal static IAppBuilder UseAppBuilder(this BuildFunc buildFunc, IAppBuilder builder)
        {
            return builder;
        }
    }
}