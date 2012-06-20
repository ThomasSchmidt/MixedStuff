using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Demos.Orm.ServiceLayer;
using Demos.Orm.ViewModel;

namespace Demos.Orm.WebSite.Controllers
{
	public class BlogPostController : Controller
	{
		private readonly IBlogService _blogService;

		public BlogPostController(IBlogService blogService)
		{
			_blogService = blogService;
		}

		//
		// GET: /BlogPost/5
		public ActionResult Index([Bind(Prefix = "id")] int blogPostId)
		{
			BlogPostViewModel viewModel = _blogService.GetBlogPost(blogPostId);
			return View(viewModel);
		}
	}
}
