using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Demos.Orm.ServiceLayer;
using Demos.Orm.ViewModel;

namespace Demos.Orm.WebSite.Controllers
{
	public class BlogController : Controller
	{
		private readonly IBlogService _blogService;

		public BlogController(IBlogService blogService)
		{
			_blogService = blogService;
		}

		//
		// GET: /Blog/5
		public ActionResult Index([Bind(Prefix = "id")] int blogId)
		{
			BlogPostListViewModel viewModel = _blogService.GetTenLatestBlogPosts(blogId);
			return View(viewModel);
		}
	}
}
