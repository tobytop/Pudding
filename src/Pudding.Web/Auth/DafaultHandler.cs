using Pudding.Core.Tool;
using Pudding.Web.Api;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Pudding.Web.Auth
{
    public class DafaultHandler : AuthorizationHandler<DefaultRequirement>
    {

        /// <summary>
        /// 验证方案提供对象
        /// </summary>
        private readonly IAuthenticationSchemeProvider _schemes;

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, DefaultRequirement requirement)
        {
            var authorizationFilterContext = context.Resource as AuthorizationFilterContext;
            var httpContext = authorizationFilterContext.HttpContext;
            var handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (var scheme in await _schemes.GetRequestHandlerSchemesAsync())
            {
                var handler = await handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    context.Fail();
                    return;
                }
            }
            var defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                var result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
                if (result?.Principal != null)
                {
                    if (long.Parse(result.Principal.Claims.SingleOrDefault(s => s.Type == "exp").Value) < DateTime.Now.ToIntS())
                    {
                        authorizationFilterContext.Result = new JsonResult(new MessageResult
                        {
                            Msg = ConifgMessage.TIMEOUT,
                            Status = false
                        })
                        { StatusCode = 401 };
                    }
                    else
                    {
                        httpContext.User = result.Principal;
                        if (requirement.Validation != null)
                        {
                            var validMsg = requirement.Validation(httpContext);
                            if (!validMsg.IsValid)
                            {
                                authorizationFilterContext.Result = new JsonResult(new MessageResult
                                {
                                    Msg = validMsg.Msg,
                                    Status = false
                                })
                                { StatusCode = 401 };
                            }
                        }
                    }
                }
                else
                {
                    authorizationFilterContext.Result = new JsonResult(new MessageResult
                    {
                        Msg = ConifgMessage.NOTRIGHT,
                        Status = false
                    })
                    { StatusCode = 401 };
                }
            }
            else
            {
                context.Fail();
                return;
            }
            context.Succeed(requirement);
        }
        public DafaultHandler(IAuthenticationSchemeProvider schemes)
        {
            _schemes = schemes;
        }
    }
}
