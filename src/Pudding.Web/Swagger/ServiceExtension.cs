using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace Pudding.Web.Swagger
{
    public static partial class ServiceExtension
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services, SwaggerDoc doc)
        {
            services.AddSwaggerGen(c =>
            {
                IApiVersionDescriptionProvider provider = services.BuildServiceProvider().GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerDoc(description.GroupName,
                         new OpenApiInfo()
                         {
                             Title = doc.Title + description.ApiVersion,
                             Version = description.ApiVersion.ToString(),
                             Description = doc.Description,
                         }
                    );
                }

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = doc.AuthName,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = doc.AuthScheme,
                    BearerFormat = doc.AuthFormat,
                    In = ParameterLocation.Header,
                    Description = doc.AuthDescription
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[] {}
                    }
                });

                c.IncludeXmlComments(doc.DocPath);
            });
            return services;
        }

        public static IApplicationBuilder UseApiSwagger(this IApplicationBuilder app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                IApiVersionDescriptionProvider provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                foreach (ApiVersionDescription description in provider.ApiVersionDescriptions)
                {
                    c.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json", description.GroupName.ToUpperInvariant());
                }
                c.RoutePrefix = string.Empty;
            });
            return app;
        }
    }

    public class SwaggerDoc
    {
        /// <summary>
        /// 文档位置
        /// </summary>
        public string DocPath { get; set; }
        /// <summary>
        /// 标题前缀
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 验证描述
        /// </summary>
        public string AuthDescription { get; set; }

        /// <summary>
        /// 验证名称
        /// </summary>
        public string AuthName { get; set; }

        /// <summary>
        /// 验证样式
        /// </summary>
        public string AuthScheme { get; set; } = "Bearer";

        /// <summary>
        /// 验证格式
        /// </summary>
        public string AuthFormat { get; set; } = "JWT";
    }
}
