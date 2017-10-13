using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SimpleStoreWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CartsController : Controller
    {
        // GET: api/values
        [HttpGet]
        public ShoppingCart Get()
        {
            var cart = new ShoppingCart();
            cart.Items.Add(new ShoppingCartItem { ProductName = "XBOX", Quantity = 1, UnitPrice = 499.99, LineTotal = 499.99});
            cart.Total = 499.99;
            return cart;
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
