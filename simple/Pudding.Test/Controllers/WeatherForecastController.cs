using CacheManager.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pudding.Web.Api;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Pudding.Test.Controllers
{
    [ApiVersion("1.0")]
    [ApiController]
    public class WeatherForecastController : BaseController
    {

        //private readonly IHomeRepository _homeRepository;
        private readonly ILogger _logger;
        private readonly ICacheManager<string> _cache;
        //private readonly IMediator _mediator;
        public WeatherForecastController(ILogger logger, ICacheManager<string> cache)
        {
            //_homeRepository = homeRepository;
            _logger = logger;
            _cache = cache;
            //_mediator = mediator;
        }

        [HttpGet]
        public string MyLog2()
        {
            return _cache.Get("lala");
        }

        /// <summary>
        /// 测试函数
        /// </summary>
        /// <returns>整的</returns>
        [HttpGet]
        [ApiVersion("2.0")]
        public string MyLog1()
        {
            Random rng = new Random();
            _cache.Put("lala", rng.Next(-20, 55).ToString());
            
            //_cache.OnGet += new EventHandler<CacheActionEventArgs> (MyHandler);
            //_cache["lala"] = rng.Next (-20, 55).ToString ();
            //_cache.Expire ("lala", ExpirationMode.Absolute, TimeSpan.FromSeconds (1));
            _logger.Information("{classname}测试", this);
            return "wahaha";

        }

        //private void MyHandler (object sender, CacheActionEventArgs args) {
        //    var main = (ICacheManager<string>) sender;
        //    //_logger.Information (main.Get (args.Key));
        //}
    }
}