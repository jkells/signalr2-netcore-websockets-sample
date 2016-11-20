using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Logging;
using Microsoft.Owin.Builder;
using Owin;

namespace WebApplication
{
    using AppFunc = Func<IDictionary<string, object>, Task>;
    using CreateDataProtectionProvider = Func<string[], Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>>;

    public static class BuilderExtensions
    {
        private const string OwinHostOnAppDisposingKey = "host.OnAppDisposing";
        private const string OwinHostAppNameKey = "host.AppName";
        private const string OwinDataProtectionProviderKey = "security.DataProtectionProvider";
        private const string OwinDefaultAppKey = "builder.DefaultApp";
        private const string HostTraceOutputKey = "host.TraceOutput";

        #region Static Methods

        private static IApplicationBuilder UseAppBuilder(this IApplicationBuilder app,
            Func<IAppBuilder, IAppBuilder> configure,
            string appName,
            CancellationToken shuttingDownToken,
            IDataProtectionProvider dataProtectionProvider,
            ILogger logger)
        {
            app.UseOwin(addToPipeline =>
            {
                addToPipeline(next =>
                {
                    var appBuilder = new AppBuilder();
                    appBuilder.Properties[OwinDefaultAppKey] = next;
                    appBuilder.Properties[OwinHostAppNameKey] = appName;
                    appBuilder.Properties[OwinHostOnAppDisposingKey] = shuttingDownToken;
                    appBuilder.Properties[OwinDataProtectionProviderKey] =
                        (CreateDataProtectionProvider) (args => CreateProvider(args, dataProtectionProvider));
                    appBuilder.Properties[HostTraceOutputKey] = TextWriter.Synchronized(new LogWriter(logger));
                    configure(appBuilder);

                    return appBuilder.Build<AppFunc>();
                });
            });

            return app;
        }

        private static Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>> CreateProvider(string[] purposes,
            IDataProtectionProvider dataProtectionProvider)
        {
            var protector = dataProtectionProvider.CreateProtector(purposes);
            return new Tuple<Func<byte[], byte[]>, Func<byte[], byte[]>>(protector.Protect, protector.Unprotect);
        }

        public static void UseSignalR2(this IApplicationBuilder app,
            string appName,
            CancellationToken shuttingDownToken,
            IDataProtectionProvider dataProtectionProvider,
            ILogger logger)
        {
            app.UseAppBuilder(appBuilder => appBuilder.MapSignalR(), appName, shuttingDownToken, dataProtectionProvider, logger);            
        }

        #endregion
        private class LogWriter : TextWriter
        {
            private readonly ILogger _logger;

            public LogWriter(ILogger logger)
            {
                _logger = logger;
            }
            public override Encoding Encoding => Encoding.UTF8;

            private StringBuilder _stringBuilder = new StringBuilder();            

            public override void Write(char c)
            {
                if(!_logger.IsEnabled(LogLevel.Debug))
                    return;

                _stringBuilder.Append(c);
                if (c != '\n')
                    return;

                _logger.LogDebug(_stringBuilder.ToString());
                _stringBuilder = new StringBuilder();
            }
        }
    }

}