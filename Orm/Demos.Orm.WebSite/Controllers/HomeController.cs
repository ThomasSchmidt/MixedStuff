using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Demos.Orm.ServiceLayer;
using Demos.Orm.ViewModel;

namespace Demos.Orm.WebSite.Controllers
{
	public class HomeController : Controller
	{
		private readonly IBlogService _blogService;

		public HomeController(IBlogService blogService)
		{
			_blogService = blogService;
		}

		public ActionResult Index()
		{
			BlogListViewModel viewModel = _blogService.GetTenLatestBlogs();
			return View(viewModel);
		}
	}
}
