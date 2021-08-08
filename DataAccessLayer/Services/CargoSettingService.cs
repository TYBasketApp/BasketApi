using DataAccessLayer.Entities;
using MongoDB.Driver;

namespace DataAccessLayer.Services
{
    public class CargoSettingService
    {
        private IMongoCollection<CargoSettings> _cargoSettingCollection;
        
        public CargoSettingService()
        {
            var client = new MongoClient("mongodb://localhost:27017");
            var database = client.GetDatabase("BasketDb");
            
            _cargoSettingCollection = database.GetCollection<CargoSettings>("cargoSetting");
        }

        public CargoSettings Add(CargoSettings entity)
        {
            var previouslyAddedEntity = _cargoSettingCollection.Find(x => true).FirstOrDefault();

            if (previouslyAddedEntity == null)
            {
                _cargoSettingCollection.InsertOne(entity);

                return entity;
            }

            return previouslyAddedEntity;
        }

        public void Update(CargoSettings entity)
        {
            var previouslyAddedEntity = _cargoSettingCollection.Find(x => true).FirstOrDefault();

            entity.Id = previouslyAddedEntity.Id;
            
            _cargoSettingCollection.ReplaceOne(x => x.Id == previouslyAddedEntity.Id, entity);
        }

        public CargoSettings Get()
        {
            return _cargoSettingCollection.Find(x => true).FirstOrDefault();
        }
    }
}