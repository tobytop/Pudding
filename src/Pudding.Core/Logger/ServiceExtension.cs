using Autofac;
using AutofacSerilogIntegration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
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

        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder, IConfiguration configuration) =>
                builder.BuildSerilog(() =>
                    new LoggerConfiguration()
                    .MinimumLevel.Verbose()
                    .MinimumLevel.Override("Microsoft",LogEventLevel.Warning)
                    .MinimumLevel.Override("System", LogEventLevel.Warning)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Elasticsearch(new ElasticsearchSinkOptions(new Uri(configuration["Serilog:EsUrl"]))
                    {
                        MinimumLogEventLevel = LogEventLevel.Information,
                        AutoRegisterTemplate = true,
                        IndexFormat = configuration["Serilog:ProjectName"] + "-{0:yyyy.MM.dd}"
                    }).CreateLogger()
            );
    }
}