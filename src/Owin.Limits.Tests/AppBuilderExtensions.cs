namespace Owin.Limits
{
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
        internal static BuildFunc ToBuildFunc(this IAppBuilder builder)
        {
            return createMiddleware => builder.Use(createMiddleware(builder.Properties));
        }

        internal static IAppBuilder ToAppBuilder(this BuildFunc buildFunc, IAppBuilder builder)
        {
            return builder;
        }
    }
}