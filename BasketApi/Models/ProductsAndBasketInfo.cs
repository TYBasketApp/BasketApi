using System.Collections.Generic;
using DataAccessLayer.Entities;

namespace BasketApi.Models
{
    public class ProductsAndBasketInfo
    {
        public List<Product> Products { get; set; }
        public BasketInfo BasketInfo { get; set; }
    }
}