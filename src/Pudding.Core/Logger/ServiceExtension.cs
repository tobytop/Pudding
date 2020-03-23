using Autofac;
using AutofacSerilogIntegration;
using Serilog;
using Serilog.Events;
using System;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder, Func<ILogger> logger)
        {
            Log.Logger = logger();
            builder.RegisterLogger(autowireProperties: true);
            return builder;
        }

        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder) => 
            builder.BuildSerilog(() =>
                 new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .WriteTo
                         .Console(LogEventLevel.Verbose, "[{Timestamp:yyyy-mm-dd HH:mm:ss.FFF} {Level}] {Message}{NewLine}{Exception}")
                         .CreateLogger());
        
    }
}