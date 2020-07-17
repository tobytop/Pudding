using Autofac;
using Pudding.RuleEngine.Container;
using Pudding.RuleEngine.Repository;

namespace Pudding.RuleEngine.Extension
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildRule(this ContainerBuilder builder)
        {
            builder.RegisterType<ExecuterContainer>().As<IExecuterContainer>().InstancePerDependency().PropertiesAutowired();
            builder.RegisterType<ExecuterRepository>().As<IExecuterRepository>().InstancePerDependency().PropertiesAutowired();
            return builder;
        }
    }
}
