using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace WebFrontend.Controllers
{
    [Route("[controller]")]
    public class DummyController : Controller
    {
        [HttpGet]
        public IActionResult Index()
        {
            return Ok();
        }
        [HttpPost]
        public IActionResult Post()
        {
            return Ok();
        }
    }
}
