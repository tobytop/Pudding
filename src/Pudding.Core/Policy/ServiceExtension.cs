using Autofac;
using NetCasbin;
using NetCasbin.Persist;

namespace Pudding.Core.Policy
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildPolicy<TAdapter,Tkey>(this ContainerBuilder builder, string policyPath) where TAdapter: PolicyAdapter<Tkey>
        {
            builder.RegisterType<TAdapter>().As<IAdapter>().SingleInstance();
            builder.Register(o =>
            {
                var adapter = o.Resolve<IAdapter>();
                var e = new Enforcer(policyPath, adapter);
                e.LoadPolicy();
                return e;
            }).AsSelf().SingleInstance();
            return builder;
        }
    }
}
