namespace Microsoft.AspNet.Builder
{
    using MidFunc = System.Func<
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>,
       System.Func<System.Collections.Generic.IDictionary<string, object>, System.Threading.Tasks.Task>
       >;

    public static partial class ApplicationBuilderExtensions
    {
        public static IApplicationBuilder Use(
            this IApplicationBuilder applicationBuilder, 
            MidFunc middleware)
        {
            var buildFunc = applicationBuilder.UseOwin();
            buildFunc(middleware);
            return applicationBuilder;
        }
    }
}