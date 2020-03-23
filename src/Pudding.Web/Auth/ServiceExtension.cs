using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Pudding.Web.Auth;
using System;
using System.Collections.Generic;

namespace Pudding.Core
{
    public static partial class ServiceExtension
    {
        /// <summary>
        /// 验证客户端拓展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="hasAuthServer">验证服务器是否分离</param>
        /// <returns></returns>
        public static IServiceCollection AddAuthHandler(this IServiceCollection services, bool hasAuthServer = false)
        {
            services.AddSingleton<IAuthorizationHandler, DafaultHandler>();
            if (!hasAuthServer)
            {
                services.AddSingleton<IToken, JwtToken>();
            }
            return services;
        }

        /// <summary>
        /// 验证客户端拓展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="requirement"></param>
        /// <param name="hasAuthServer">验证服务器是否分离</param>
        /// <returns></returns>
        public static AuthenticationBuilder AddAuthJwtBearer(this IServiceCollection services, DefaultRequirement requirement, bool hasAuthServer = false)
        {
            if (!hasAuthServer)
            {
                services.AddSingleton(requirement);
            }
            return services.AddAuthorization(options =>
            {
                options.AddPolicy(requirement.AuthName, policy => policy.Requirements.Add(requirement));
            })
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = requirement.AuthScheme;
                })
                .AddJwtBearer(requirement.AuthScheme, o =>
                {
                    o.RequireHttpsMetadata = false;
                    o.TokenValidationParameters = requirement.TokenParameters;
                });
        }

        /// <summary>
        /// 验证服务端拓展
        /// </summary>
        /// <param name="services"></param>
        /// <param name="requirementFunc"></param>
        /// <returns></returns>
        public static IServiceCollection AddAuthServer(this IServiceCollection services, Func<IEnumerable<DefaultRequirement>> requirementFunc)
        {
            services.AddSingleton(requirementFunc());
            services.AddSingleton<IToken, JwtToken>();
            return services;
        }
    }
}