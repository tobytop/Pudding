using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Pudding.Core.Tool;
using Pudding.Web.Api;
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
            AuthorizationFilterContext authorizationFilterContext = context.Resource as AuthorizationFilterContext;
            HttpContext httpContext = authorizationFilterContext.HttpContext;
            IAuthenticationHandlerProvider handlers = httpContext.RequestServices.GetRequiredService<IAuthenticationHandlerProvider>();
            foreach (AuthenticationScheme scheme in await _schemes.GetRequestHandlerSchemesAsync())
            {
                IAuthenticationRequestHandler handler = await handlers.GetHandlerAsync(httpContext, scheme.Name) as IAuthenticationRequestHandler;
                if (handler != null && await handler.HandleRequestAsync())
                {
                    context.Fail();
                    return;
                }
            }
            AuthenticationScheme defaultAuthenticate = await _schemes.GetDefaultAuthenticateSchemeAsync();
            if (defaultAuthenticate != null)
            {
                AuthenticateResult result = await httpContext.AuthenticateAsync(defaultAuthenticate.Name);
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
                        AuthResult validMsg = requirement.Validation?.Invoke(httpContext);
                        if (validMsg != null && !validMsg.IsValid)
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
