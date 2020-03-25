using Autofac;
using AutofacSerilogIntegration;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Elasticsearch;
using System;
using System.IO;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 日志输出到自定义模式
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="logger"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder, Func<ILogger> logger)
        {
            Log.Logger = logger();
            builder.RegisterLogger(autowireProperties: true);
            return builder;
        }

        /// <summary>
        /// 日志输出到文本
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="loggerName"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder,string loggerName) =>
            builder.BuildSerilog(() =>
                 new LoggerConfiguration()
                         .MinimumLevel.Information()
                         .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                         .Enrich.FromLogContext()
                         .WriteTo.Console()
                         .WriteTo.File(Path.Combine("logs", loggerName), rollingInterval: RollingInterval.Day)
                         .CreateLogger());
        /// <summary>
        /// 日志输出到控制台
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildSerilog(this ContainerBuilder builder) => 
            builder.BuildSerilog(() =>
                 new LoggerConfiguration()
                         .Enrich.FromLogContext()
                         .WriteTo
                         .Console(LogEventLevel.Verbose, "[{Timestamp:yyyy-mm-dd HH:mm:ss.FFF} {Level}] {Message}{NewLine}{Exception}")
                         .CreateLogger());

        /// <summary>
        /// 日志输出到Elasticsearch
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
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
                        FailureCallback = e => Console.WriteLine("Unable to submit event " + e.MessageTemplate),
                        IndexFormat = configuration["Serilog:ProjectName"] + "-{0:yyyy.MM.dd}"
                    }).CreateLogger()
            );
    }
}