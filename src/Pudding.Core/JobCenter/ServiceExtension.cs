using Autofac;
using Pudding.Core.JobCenter;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using System.Collections.Specialized;
using System.Reflection;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 添加任务模块
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="properties">配置</param>
        /// <param name="assemblyCurrent">当前程序集</param>
        /// <returns></returns>
        public static ContainerBuilder BuildQuartz(this ContainerBuilder builder, NameValueCollection properties, Assembly assemblyCurrent)
        {
            builder.Register(x => new StdSchedulerFactory(properties).GetScheduler().Result).As<IScheduler>().SingleInstance().PropertiesAutowired();
            builder.RegisterType<AutofacJobFactory>().As<IJobFactory>().SingleInstance();
            builder.RegisterAssemblyTypes(assemblyCurrent).Where(x => typeof(IJob).IsAssignableFrom(x));
            return builder;
        }
    }
}