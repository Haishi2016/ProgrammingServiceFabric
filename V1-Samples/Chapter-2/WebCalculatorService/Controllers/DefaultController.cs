using System.Collections.Generic;
using System.Web.Http;

namespace WebCalculatorService.Controllers
{
    public class DefaultController: ApiController
    {
        [HttpGet]
        public int Add(int a, int b)
        {
            return a + b;
        }
        [HttpGet]
        public int Subtract(int a, int b)
        {
            return a - b;
        }
    }
}
