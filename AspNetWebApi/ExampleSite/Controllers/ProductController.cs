using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;

namespace ExampleSite.Controllers
{
	public class Product
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public decimal Price { get; set; }
	}

	public class ProductController : ApiController
	{
		public static readonly Product[] Products = new Product[] 
        { 
            new Product { Id = 1, Name = "Tomato Soup", Category = "Groceries", Price = 1 }, 
            new Product { Id = 2, Name = "Yo-yo", Category = "Toys", Price = 3.75M }, 
            new Product { Id = 3, Name = "Hammer", Category = "Hardware", Price = 16.99M } 
        };

        public IEnumerable<Product> Get()
        {
            return Products;
        }

        public Product Get(int id)
        {
            var product = Products.FirstOrDefault((p) => p.Id == id);
            if (product == null)
            {
                throw new HttpResponseException(HttpStatusCode.NotFound);
            }
            return product;
        }

        public IEnumerable<Product> GetProductsByCategory(string category)
        {
            return Products.Where(
                (p) => string.Equals(p.Category, category,
                    StringComparison.OrdinalIgnoreCase));
        }
	}
}