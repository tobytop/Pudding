using Autofac;
using Autofac.Features.AttributeFilters;
using AutoMapper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using Pudding.Web.Valid;
using System.Linq;
using System.Reflection;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 添加autotfac的中间件
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="configuration">配置</param>
        /// <returns></returns>
        public static ContainerBuilder BuildWeb(this ContainerBuilder builder)
        {
            Assembly assemblyCurrent = Assembly.GetCallingAssembly();
            builder.RegisterAssemblyTypes(assemblyCurrent).Where(p => p.Name.EndsWith("Validator"))
                .AsImplementedInterfaces().InstancePerLifetimeScope();
            builder.RegisterType<ValidatorFactory>().As<IValidatorFactory>().SingleInstance();

            MapperConfiguration mappingConfig = new MapperConfiguration(mc =>
            {
                mc.AddMaps(assemblyCurrent);
            });
            IMapper mapper = mappingConfig.CreateMapper();
            builder.Register(c => mapper).AsSelf().SingleInstance();

            //十分重要
            ApplicationPartManager manager = new ApplicationPartManager();
            manager.ApplicationParts.Add(new AssemblyPart(assemblyCurrent));
            manager.FeatureProviders.Add(new ControllerFeatureProvider());
            ControllerFeature feature = new ControllerFeature();
            manager.PopulateFeature(feature);
            builder.RegisterTypes(feature.Controllers.Select(ti => ti.AsType()).ToArray()).PropertiesAutowired();

            //拦截器注入
            builder.RegisterType<ValidateResponseAttribute>().WithAttributeFiltering();
            return builder;
        }
    }
}