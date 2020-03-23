using Autofac;
using Autofac.Features.Indexed;
using Microsoft.Extensions.Configuration;
using Pudding.Core.Data;
using System.Collections.Concurrent;
using System.Reflection;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildData(this ContainerBuilder builder, IConfiguration configuration, string assemblyName)
        {
            Assembly assembly = Assembly.Load(assemblyName);
            builder.RegisterAssemblyTypes(assembly).Where(p => p.Name.EndsWith("Repository") && !p.Name.Equals("BaseRepository"))
                .AsImplementedInterfaces().InstancePerLifetimeScope().PropertiesAutowired().OnActivated(e =>
                {
                    BaseRepository baseRepository = e.Instance as BaseRepository;
                    if (baseRepository.DbType != DbType.None)
                    {
                        IIndex<DbType, IDbConnectionFactory> dbManager = e.Context.Resolve<IIndex<DbType, IDbConnectionFactory>>();
                        baseRepository.Conn = dbManager[baseRepository.DbType].GetDbInstance(baseRepository.ConnectionName);
                    }
                });
            ConcurrentDictionary<string, string> connections = new ConcurrentDictionary<string, string>();
            foreach (IConfigurationSection child in configuration.GetSection("ConnectionStrings").GetChildren())
            {
                connections.TryAdd(child.Key, child.Value);
            }
            builder.RegisterInstance(connections).SingleInstance();
            builder.RegisterType<MsSqlDbConnectionFactory>().Keyed<IDbConnectionFactory>(DbType.MsSql).SingleInstance();
            builder.RegisterType<MySqlDbConnectionFactory>().Keyed<IDbConnectionFactory>(DbType.MySql).SingleInstance();
            builder.RegisterInstance(configuration).SingleInstance();
            return builder;
        }
    }
}