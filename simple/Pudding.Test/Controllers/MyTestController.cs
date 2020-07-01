using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Pudding.Web.Api;

namespace Pudding.Base.Controllers
{
    [ApiController]
    public class MyTestController : BaseController
    {

        private readonly ICacheManager<string> _cache;
        public MyTestController(ICacheManager<string> cache)
        {
            _cache = cache;
        }

        [HttpGet]
        public string MyLog3()
        {
            return _cache.Get("lala");
        }
    }
}