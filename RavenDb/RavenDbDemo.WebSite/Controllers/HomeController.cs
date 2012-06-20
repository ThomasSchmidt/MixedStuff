using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Raven.Client;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.WebSite.Controllers
{
	public class HomeController : Controller
	{
		private readonly IDocumentSession _session;

		public HomeController(IDocumentSession session)
		{
			_session = session;
		}

		//
		// GET: /Home/
		public ActionResult Index()
		{
			IList<Category> categories = _session.Query<Category>().Where(c => c.Description != null).ToList();
			IList<Product> simpleProducts = _session.Query<Product,ProductPriceIndex>().Where(p => p.Price > 3000).ToList();
			IList<Product> rangeProducts = _session.Query<Product, ProductPriceIndex>().Where(p => p.Price > 3000 && p.Price < 8000).ToList();
			var categoryTotals = _session.Query<ProductTotalPricePerCategoryIndex.ReduceResult, ProductTotalPricePerCategoryIndex>().ToList();
			Product specificProduct = _session.Query<Product,ProductPriceIndex>().SingleOrDefault(p => p.Price == 640);

			return View(categories);
		}
	}
}
