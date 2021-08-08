using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BasketApi.Models;
using DataAccessLayer.Entities;
using DataAccessLayer.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson.IO;

namespace BasketApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BasketController : ControllerBase
    {
        private BasketService _basketService;
        private CargoSettingService _cargoSettingService;
        
        public BasketController()
        {
            _basketService = new BasketService();
            _cargoSettingService = new CargoSettingService();
        }
        
        [HttpPost]
        public IActionResult AddProduct([FromBody] UserIdProductIdPair userIdProductIdPair)
        {
            _basketService.AddProductToBasket(userIdProductIdPair.UserId, userIdProductIdPair.ProductId);

            return Ok();
        }
        
        [HttpPost]
        public IActionResult RemoveProduct([FromBody] UserIdProductIdPair userIdProductIdPair)
        {
            _basketService.RemoveProductFromBasket(userIdProductIdPair.UserId, userIdProductIdPair.ProductId);

            return Ok();
        }
        
        [HttpPost]
        public IActionResult RemoveOneProduct([FromBody] UserIdProductIdPair userIdProductIdPair)
        {
            _basketService.RemoveOneProductFromBasket(userIdProductIdPair.UserId, userIdProductIdPair.ProductId);

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> GetBasketInfo([FromBody] int userId)
        {
            var basket = _basketService.GetBasketOfUser(userId);

            var productIds = basket.ProductIdAndQuantityPairs.Where(x => x.Quantity > 0).Select(x => x.ProductId)
                .ToList();

            var content = JsonContent.Create(productIds);
            HttpResponseMessage result;
            
            using (var httpClientHandler = new HttpClientHandler())
            {
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
                using (var client = new HttpClient(httpClientHandler))
                {
                    result = await client.PostAsync("http://localhost:5000/api/Product/GetProducts", content);
                }
            }

            var products = JsonSerializer.Deserialize<List<Product>>(await result.Content.ReadAsStringAsync(), new JsonSerializerOptions(new JsonSerializerOptions{PropertyNameCaseInsensitive = true}));

            var productsAndBasketInfo = new ProductsAndBasketInfo();

            productsAndBasketInfo.Products = new List<Product>();
            
            double total = 0;
            
            foreach (var productIdAndQuantityPair in basket.ProductIdAndQuantityPairs.Where(x => x.Quantity > 0))
            {
                var product = products.FirstOrDefault(x => x.Id == productIdAndQuantityPair.ProductId);
                
                productsAndBasketInfo.Products.Add(new Product
                {
                    Id = product.Id,
                    ImageUrl = product.ImageUrl,
                    Title = product.Title,
                    Price = product.Price,
                    Quantity = productIdAndQuantityPair.Quantity
                });

                total += product.Price * productIdAndQuantityPair.Quantity;
            }

            productsAndBasketInfo.BasketInfo = new BasketInfo();
            productsAndBasketInfo.BasketInfo.Total = total.ToString() + " TL";
            productsAndBasketInfo.BasketInfo.CargoPrice = "0 TL";
            productsAndBasketInfo.BasketInfo.TotalWithCargo = total.ToString() + " TL";

            var cargoSettings = _cargoSettingService.Get();

            if (cargoSettings.CargoPriceLimit > total)
            {
                productsAndBasketInfo.BasketInfo.CargoPrice = cargoSettings.CargoPrice.ToString() + " TL";
                productsAndBasketInfo.BasketInfo.TotalWithCargo = (total + cargoSettings.CargoPrice).ToString() + " TL";
            }
            
            return Ok(productsAndBasketInfo);
        }
    }
}