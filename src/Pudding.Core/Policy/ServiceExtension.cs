using Autofac;
using NetCasbin.Persist;

namespace Pudding.Core.Policy
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildPolicy<TAdapter,Tkey>(this ContainerBuilder builder) where TAdapter: PolicyAdapter<Tkey>
        {
            builder.RegisterType<IAdapter>().As<TAdapter>().SingleInstance();
            return builder;
        }
    }
}
