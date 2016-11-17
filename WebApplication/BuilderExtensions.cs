using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Owin.Builder;
using Owin;

namespace WebApplication
{
    using AppFunc = Func<IDictionary<string, object>, Task>;

    public static class BuilderExtensions
    {
        #region Static Methods

        public static IApplicationBuilder UseAppBuilder(
            this IApplicationBuilder app,
            Action<IAppBuilder> configure)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties["builder.DefaultApp"] = next;

                    configure(appBuilder);

                    return appBuilder.Build<AppFunc>();
                });
            });

            return app;
        }

        public static void UseSignalR2(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseAppBuilder(appBuilder => appBuilder.MapSignalR());
        }

        #endregion
    }
}