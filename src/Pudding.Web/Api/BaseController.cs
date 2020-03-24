using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace Pudding.Web.Api
{
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>
        /// 格式化器
        /// </summary>
        public IMapper _mapper { protected get; set; }

        public IMediator _mediator { protected get; set; }

        protected long GetUserID(string key = Config.DEFAULT_CLAIM_USERID)
        {
            return Convert.ToInt64(HttpContext.User.Claims.First(o => o.Type == key).Value.First());
        }
    }
}