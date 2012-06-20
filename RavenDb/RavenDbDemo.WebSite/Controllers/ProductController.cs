using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using RavenDbDemo.Repository;
using RavenDdDemo.Model;

namespace RavenDbDemo.WebSite.Controllers
{
	public class ProductController : Controller
	{
		private readonly IRepository<Product> _productRepository;

		public ProductController(IRepository<Product> productRepository)
		{
			_productRepository = productRepository;
		}

		//
		// GET: /Product/Id
		public ActionResult Index(string id)
		{
			return View(_productRepository.Query(p => p.CategoryId == id));
		}

		// GET: /Product/Get/Id
		public ActionResult Get(string id)
		{
			return View(_productRepository.Get(id));
		}
	}
}
