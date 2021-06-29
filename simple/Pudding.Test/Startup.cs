using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Pudding.Core;
using Pudding.Core.Policy;
using Pudding.Test.Adapter;
using Pudding.Web.Swagger;
using System;
using System.IO;
using System.Reflection;

namespace Pudding.Test
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            IConfigurationBuilder builder = new ConfigurationBuilder()
                //env.ContentRootPath：获取当前项目的跟路径
                .SetBasePath(env.ContentRootPath)
                //使用AddJsonFile方法把项目中的appsettings.json配置文件加载进来，后面的reloadOnChange顾名思义就是文件如果改动就重新加载
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            Configuration = builder.Build();
        }
        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddJsonAndVersion().AddSwagger(new SwaggerDoc {
                Title = "微服务接口 v",
                Description = "切换版本请点右上角版本切换",
                AuthDescription = "获取Token",
                DocPath= Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml")
            });
            //services.AddJsonAndVersion().AddSwaggerGen(c=> {
            //    var provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();

            //    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "System Management", Version = "v1" });
            //    foreach (var description in provider.ApiVersionDescriptions)
            //    {
            //        c.SwaggerDoc(description.GroupName,
            //             new OpenApiInfo()
            //             {
            //                 Title = $"体检微服务接口 v{description.ApiVersion}",
            //                 Version = description.ApiVersion.ToString(),
            //                 Description = "切换版本请点右上角版本切换",
            //             }
            //        );
            //    }
            //    //c.SwaggerDoc("v1", new OpenApiInfo { Title = "System Management", Version = "v1" });

            //    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            //    {
            //        Name = "Authorization",
            //        Type = SecuritySchemeType.ApiKey,
            //        Scheme = "Bearer",
            //        BearerFormat = "JWT",
            //        In = ParameterLocation.Header,
            //        Description = "JWT Authorization header using the Bearer scheme."
            //    });
            //    c.AddSecurityRequirement(new OpenApiSecurityRequirement
            //    {
            //        {
            //            new OpenApiSecurityScheme
            //            {
            //                Reference = new OpenApiReference
            //                {
            //                    Type = ReferenceType.SecurityScheme,
            //                    Id = "Bearer"
            //                }
            //            },
            //            new string[] {}
            //        }
            //    });

            //    c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"));
            //    //c.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SystemManagement.Dto.xml"));
            //});
            
            /*ContainerBuilder builder = new ContainerBuilder()
                .BuildWeb()
                .BuildSerilog()
                .BuildPolicy<TestAdapter,int>("examples/rbac_model.conf")
                .BuildCacheManager();
            builder.Populate(services);
            IContainer container = builder.Build();

            return new AutofacServiceProvider(container);*/
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.BuildWeb()
                .BuildSerilog()
                .BuildPolicy<TestAdapter, int>("examples/rbac_model.conf")
                .BuildCacheManager();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            /*app.UseCors(builder => builder
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials());*/

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseApiSwagger();

            app.UseAuthorization();

            //app.UseSwaggerUI(c =>
            //{
            //    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "System Management V1");
            //    var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
            //    foreach (var description in provider.ApiVersionDescriptions)
            //    {
            //        c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
            //    }
            //    //c.SwaggerEndpoint("/swagger/v1/swagger.json", "System Management V1");
            //    c.RoutePrefix = string.Empty;
            //});
        }
    }
}
