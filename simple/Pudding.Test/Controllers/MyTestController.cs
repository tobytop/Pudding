using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CacheManager.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Pudding.Test.Controllers
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
            mydata = "";
            return _cache.Get("lala");
        }
    }
}