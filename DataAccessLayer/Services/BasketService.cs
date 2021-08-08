using System.Collections.Generic;
using System.Linq;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.Services
{
    public class BasketService
    {
        private readonly IMongoCollection<Basket> _baskets;
        private readonly UserIdsBelongsProductService _userIdsBelongsProductService;
        
        public BasketService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("BasketDb");
            
            _baskets = database.GetCollection<Basket>("basket");

            _userIdsBelongsProductService = new UserIdsBelongsProductService();
        }

        public Basket Create(Basket basket)
        {
            _baskets.InsertOne(basket);
            return basket;
        }

        public void Update(Basket basket)
        {
            _baskets.ReplaceOne(x => x.Id == basket.Id, basket);
        }

        public Basket GetBasketOfUser(int userId)
        {
            return _baskets.Find<Basket>(x => x.UserId == userId).FirstOrDefault();
        }

        public void AddProductToBasket(int userId, int productId)
        {
            var basket = GetBasketOfUser(userId);

            if (basket == null)
            {
                Create(new Basket
                {
                    UserId = userId,
                    ProductIdAndQuantityPairs = new List<ProductIdQuantityPair>
                    {
                        new ProductIdQuantityPair
                        {
                            ProductId = productId,
                            Quantity = 1
                        }
                    }
                });

                _userIdsBelongsProductService.AddUserToProduct(userId, productId);
                
                return;
            }

            if (basket.ProductIdAndQuantityPairs.Any())
            {
                if (basket.ProductIdAndQuantityPairs.Any(x => x.ProductId == productId))
                {
                    var indexOfProduct = basket.ProductIdAndQuantityPairs.FindIndex(x => x.ProductId == productId);

                    basket.ProductIdAndQuantityPairs[indexOfProduct].Quantity++;

                    // ürün tamamen silinmediği için tekrar eklendiğinde user ekleniyor
                    if (basket.ProductIdAndQuantityPairs[indexOfProduct].Quantity == 1)
                    {
                        _userIdsBelongsProductService.AddUserToProduct(userId, productId);
                    }
                
                    Update(basket);

                    return;
                }
            }
            
            basket.ProductIdAndQuantityPairs.Add(new ProductIdQuantityPair
            {
                ProductId = productId,
                Quantity = 1
            });
            
            Update(basket);
            
            _userIdsBelongsProductService.AddUserToProduct(userId, productId);
        }

        public void RemoveProductFromBasket(int userId, int productId)
        {
            var basket = GetBasketOfUser(userId);

            if (basket.ProductIdAndQuantityPairs.Any(x => x.ProductId == productId))
            {
                var indexOfProduct = basket.ProductIdAndQuantityPairs.FindIndex(x => x.ProductId == productId);

                basket.ProductIdAndQuantityPairs[indexOfProduct].Quantity = 0;
                
                Update(basket);
                
                _userIdsBelongsProductService.RemoveUserFromProduct(userId,productId);
            }
        }

        public void RemoveOneProductFromBasket(int userId, int productId)
        {
            var basket = GetBasketOfUser(userId);

            if (basket.ProductIdAndQuantityPairs.Any(x => x.ProductId == productId))
            {
                var indexOfProduct = basket.ProductIdAndQuantityPairs.FindIndex(x => x.ProductId == productId);

                if (basket.ProductIdAndQuantityPairs[indexOfProduct].Quantity == 1)
                {
                    _userIdsBelongsProductService.RemoveUserFromProduct(userId, productId);
                }

                basket.ProductIdAndQuantityPairs[indexOfProduct].Quantity--;
                
                Update(basket);
            }
        }
    }
}