using System.Collections.Generic;
using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.Services
{
    public class UserIdsBelongsProductService
    {
        private readonly IMongoCollection<UserIdsBelongsProduct> _userIdsBelongProductCollection;

        public UserIdsBelongsProductService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("BasketDb");

            _userIdsBelongProductCollection = database.GetCollection<UserIdsBelongsProduct>("userIdsBelongsProduct");
        }

        public UserIdsBelongsProduct Add(UserIdsBelongsProduct entity)
        {
            _userIdsBelongProductCollection.InsertOne(entity);

            return entity;
        }

        public void Update(UserIdsBelongsProduct entity)
        {
            _userIdsBelongProductCollection.ReplaceOne(x => x.ProductId == entity.ProductId, entity);
        }
        
        public UserIdsBelongsProduct GetByProductId(int productId)
        {
            return _userIdsBelongProductCollection.Find<UserIdsBelongsProduct>(x => x.ProductId == productId).FirstOrDefault();
        }
        
        public void AddUserToProduct(int userId, int productId)
        {
            var entity = GetByProductId(productId);

            if (entity == null)
            {
                entity = new UserIdsBelongsProduct
                {
                    ProductId = productId,
                    UserIds = new List<int> {userId}
                };

                Add(entity);

                return;
            }
            
            entity.UserIds.Add(userId);
            
            Update(entity);
        }

        public void RemoveUserFromProduct(int userId, int productId)
        {
            var entity = GetByProductId(productId);

            entity.UserIds.Remove(userId);
            
            Update(entity);
        }
    }
}