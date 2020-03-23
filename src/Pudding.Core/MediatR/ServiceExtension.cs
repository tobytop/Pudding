using Autofac;
using MediatR.Extensions.Autofac.DependencyInjection;
using System.Reflection;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        public static ContainerBuilder BuildMediatR(this ContainerBuilder builder, params Assembly[] assemblys)
        {
            builder.AddMediatR(assemblys);
            return builder;
        }
    }
}