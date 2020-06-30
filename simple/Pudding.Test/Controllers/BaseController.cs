using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Pudding.Test.Controllers
{
    [Route("api/v{version:apiVersion}/[controller]/[action]")]
    public abstract partial class BaseController : ControllerBase
    {
        protected string mydata { get; set; }

    }
}