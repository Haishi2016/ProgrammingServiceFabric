using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Text;
using Microsoft.Net.Http.Headers;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace SudokuWeb.Controllers
{
    [Produces("application/json")]
    [Route("api/[controller]")]
    public class CartsController : Controller
    {
        private static Regex ipRex = new Regex(@"\b\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}\b");
        private readonly HttpClient httpClient;
        public CartsController(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }


        // GET: api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            HttpResponseMessage response = await this.httpClient.GetAsync(await ResolveAddress() + "/api/ShoppingCarts");
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return this.StatusCode((int)response.StatusCode);
            var cart = JsonConvert.DeserializeObject<ShoppingCart>(await response.Content.ReadAsStringAsync());
            return Json(cart);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]ShoppingCartItem item)
        {
            StringContent postContent = new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json");
            postContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
            HttpResponseMessage response = await this.httpClient.PostAsync(await ResolveAddress() + "/api/ShoppingCarts", postContent);
            return new OkResult();
        }


        // DELETE api/values/5
        [HttpDelete("{productName}")]
        public async Task<IActionResult> Delete(string productName)
        {
            HttpResponseMessage response = await this.httpClient.DeleteAsync(await ResolveAddress() + "/api/ShoppingCarts/" + productName);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                return this.StatusCode((int)response.StatusCode);
            return new OkResult();
        }
        private async Task<string> ResolveAddress()
        {
            var partitionResolver = ServicePartitionResolver.GetDefault();
            var resolveResults = await partitionResolver.ResolveAsync(new Uri("fabric:/SudokuApplication/SudokuService"),
                    new ServicePartitionKey(1), new System.Threading.CancellationToken());

            var endpoint = resolveResults.GetEndpoint();
            var endpointObject = JsonConvert.DeserializeObject<JObject>(endpoint.Address);
            var addressString = ((JObject)endpointObject.Property("Endpoints").Value)[""].Value<string>();
            return addressString;
        }
    }
}
