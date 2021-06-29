using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NetCasbin;
using Pudding.Web.Api;

namespace Pudding.Base.Controllers
{
    [ApiController]
    public class MyTestController : BaseController
    {

        private readonly ICacheManager<string> _cache;
        private readonly Enforcer _enforcer;
        public MyTestController(ICacheManager<string> cache, Enforcer enforcer)
        {
            _cache = cache;
            _enforcer = enforcer;
        }

        [HttpGet]
        public string MyLog3()
        {
            var result = _enforcer.Enforce("alice", "data1", "read");
            return _cache.Get("lala");
        }
    }
}