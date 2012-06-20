using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RavenDbDemo.Repository;
using RavenDdDemo.Model;

namespace RavenDbDemo.WebSite.Controllers
{
	public class FillDataController : Controller
	{
		private readonly IRepository<Category> _categoryRepository;
		private readonly IRepository<Product> _productRepository;

		public FillDataController(IRepository<Category> categoryRepository, IRepository<Product> productRepository)
		{
			_categoryRepository = categoryRepository;
			_productRepository = productRepository;
		}

		//
		// GET: /FillData/
		public ActionResult Index()
		{
			//delete everything
			List<Category> existingCategories = _categoryRepository.Query(c => c.Id != null).ToList();
			List<Product> existingProducts = _productRepository.Query(c => c.Id != null).ToList();

			existingCategories.ForEach(c => _categoryRepository.Delete(c));
			existingProducts.ForEach(p => _productRepository.Delete(p));

			//add new categories and products
			List<Category> newCategories = new List<Category>();
			for (int i = 0; i < 5; i++)
			{
				Category category = new Category
				{
					Description = "Description for " + i
				};
				newCategories.Add(category);
			}
			newCategories.ForEach(c => _categoryRepository.Save(c));

			Random rnd = new Random(DateTime.Now.Millisecond);

			List<Product> newProducts = new List<Product>();
			for (int i = 0; i < 20; i++)
			{
				Product product = new Product
				{
					Description = "Product description for " + i,
					Price = rnd.Next(1, 10000),
					CategoryId = newCategories[rnd.Next(0, newCategories.Count - 1)].Id
				};
				newProducts.Add(product);
			}
			newProducts.ForEach(p => _productRepository.Save(p));

			return View();
		}
	}
}
