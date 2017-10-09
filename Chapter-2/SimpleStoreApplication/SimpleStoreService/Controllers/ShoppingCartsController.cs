using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;

namespace SimpleStoreService.Controllers
{
    [Route("api/[controller]")]
    public class ShoppingCartsController : Controller
    {
        private readonly IReliableStateManager mStateManager;
        const string Shoppingcart = "shoppingCart";
        public ShoppingCartsController(IReliableStateManager stateManager)
        {
            this.mStateManager = stateManager;
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(Shoppingcart);
            using (var tx = mStateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, getUserIdentity());
                if (result.HasValue)
                    return Json(new {
                        Total = result.Value.Total,
                        Items = result.Value.GetItems()
                });
                else
                    return Json(new ShoppingCart());
            }            
        }
        
        // POST api/values
        [HttpPost]
        public async void Post([FromBody]ShoppingCartItem item)
        {
            await addToCart(item);
        }

        // PUT api/values/5
        [HttpPut]
        public async void Put([FromBody]ShoppingCartItem item)
        {
            await addToCart(item);  
        }
        // DELETE api/values/5
        [HttpDelete("{name}")]
        public async Task<IActionResult> Delete(string name)
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(Shoppingcart);
            using (var tx = mStateManager.CreateTransaction())
            {
                var result = await myDictionary.TryGetValueAsync(tx, getUserIdentity());
                if (result.HasValue)
                {
                    await myDictionary.SetAsync(tx, name, result.Value.RemoveItem(name));
                    await tx.CommitAsync();
                    return new OkResult();
                }
                else
                    return new NotFoundResult();
            }
        }
        private async Task addToCart(ShoppingCartItem item)
        {
            var myDictionary = await mStateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCart>>(Shoppingcart);            
            using (var tx = mStateManager.CreateTransaction())
            {
                await myDictionary.AddOrUpdateAsync(tx, getUserIdentity(), new ShoppingCart(item), (k, v) => v.AddItem(item));
                await tx.CommitAsync();
            }
        }
        private string getUserIdentity()
        {             
            if (HttpContext.User != null
                && HttpContext.User.Identity != null
                && !string.IsNullOrEmpty(HttpContext.User.Identity.Name))
                return HttpContext.User.Identity.Name;
            else
                return "anonymouse";
        }
    }
}
