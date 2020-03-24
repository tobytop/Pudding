using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Pudding.Test.Controllers
{
    public abstract partial class BaseController : ControllerBase
    {
        protected string mydata { get; set; }
    }
}