using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 设置json的规则和版本
        /// </summary>
        /// <param name="services"></param>
        /// <param name="resolver">序列化版本 默认为小驼峰格式</param>
        /// <returns></returns>
        public static IServiceCollection AddJsonAndVersion(this IServiceCollection services, DefaultContractResolver resolver = null)
        {
            services.Replace(ServiceDescriptor.Transient<IControllerActivator, ServiceBasedControllerActivator>());
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson(options => {
                options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                options.SerializerSettings.ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy
                    {
                        ProcessDictionaryKeys = true,
                        OverrideSpecifiedNames = true
                    }
                };
                //设置时间格式
                options.SerializerSettings.DateFormatString = "yyyy-MM-dd";
                options.SerializerSettings.Formatting = Formatting.Indented;
            });

            services.AddRouting(options => options.LowercaseUrls = true).AddApiVersioning(option =>
            {
                option.ReportApiVersions = true;
                option.AssumeDefaultVersionWhenUnspecified = true;
                option.DefaultApiVersion = new ApiVersion(1, 0);
            }).AddVersionedApiExplorer(option =>
            {
                option.GroupNameFormat = "'v'VVV";
                option.AssumeDefaultVersionWhenUnspecified = true;
            });
            return services;
        }
    }
}