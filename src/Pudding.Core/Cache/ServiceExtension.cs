using Autofac;
using CacheManager.Core;
using System;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 使用redis缓存
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="config"></param>
        /// <param name="ismultiply"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildCacheManager(this ContainerBuilder builder, RedisConfig config, bool ismultiply = false)
            =>builder.BuildCacheManager(() => ConfigurationBuilder.BuildConfiguration(settings =>
                {
                    if (ismultiply)
                    {
                        settings.WithDictionaryHandle(Config.MAINCACHE).And
                            .WithRedisConfiguration(Config.MAINREDIS, configuration =>
                            {
                                configuration.WithAllowAdmin()
                                    .EnableKeyspaceEvents()
                                    .WithDatabase(config.Database)
                                    .WithEndpoint(config.Endpoints, config.Prot);
                                if (!string.IsNullOrEmpty(config.Password))
                                {
                                    configuration.WithPassword(config.Password);
                                }
                            })
                            .WithJsonSerializer()
                            .WithMaxRetries(100)
                            .WithRetryTimeout(50)
                            .WithRedisBackplane(Config.MAINREDIS)
                            .WithRedisCacheHandle(Config.MAINREDIS);
                    }
                    else
                    {
                        settings
                            .WithRedisConfiguration(Config.MAINREDIS, configuration =>
                            {
                                configuration.WithAllowAdmin()
                                    .EnableKeyspaceEvents()
                                    .WithDatabase(config.Database)
                                    .WithEndpoint(config.Endpoints, config.Prot);
                                if (!string.IsNullOrEmpty(config.Password))
                                {
                                    configuration.WithPassword(config.Password);
                                }
                            })
                            .WithJsonSerializer()
                            .WithMaxRetries(100)
                            .WithRetryTimeout(50)
                            .WithRedisBackplane(Config.MAINREDIS)
                            .WithRedisCacheHandle(Config.MAINREDIS, true);
                    }

                }));

        /// <summary>
        /// 使用本机缓存
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ContainerBuilder BuildCacheManager(this ContainerBuilder builder)
            =>builder.BuildCacheManager(() =>
                    ConfigurationBuilder.BuildConfiguration(settings => settings.WithDictionaryHandle(true)));


        public static ContainerBuilder BuildCacheManager(this ContainerBuilder builder, Func<ICacheManagerConfiguration> config)
        {
            builder.RegisterInstance(config()).SingleInstance();
            builder.RegisterGeneric(typeof(BaseCacheManager<>)).As(typeof(ICacheManager<>)).SingleInstance();
            return builder;
        }
    }
    public class RedisConfig
    {
        public int Database { get; set; } = 0;
        public string Endpoints { get; set; }
        public int Prot { get; set; } = 6379;
        public string Password { get; set; }
    }
}