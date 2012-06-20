using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using Demos.Orm.Repository;
using Demos.Orm.Repository.RavenDb;
using Demos.Orm.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace Demos.Orm.ServiceLayer.Tests
{
	[TestClass]
	public class RavenBlogServiceTests
	{
		private static IKernel _kernel;

		[ClassInitialize]
		public static void ClassInit(TestContext testContext)
		{
			_kernel = new StandardKernel();
			_kernel.Load(new NinjectModules());
			_kernel.Rebind<IBlogRepository>().To<RavenBlogRepository>();
		}

		[TestMethod]
		public void GetLatestBlogTest()
		{
			//arrange
			IBlogService service = _kernel.Get<IBlogService>();

			//act
			BlogViewModel result = service.GetLatestBlog();

			//assert
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Blog);
		}

		[TestMethod]
		public void GetLatestBlogWithBlogPostsTest()
		{
			//arrange
			IBlogService service = _kernel.Get<IBlogService>();

			//act
			BlogViewModel result = service.GetLatestBlog();

			//assert
			Assert.IsNotNull(result);
			Assert.IsNotNull(result.Blog);
			Assert.IsNotNull(result.Blog.BlogPosts);
		}

		[TestMethod]
		public void CanSaveBlogTest()
		{
			//arrange
			IBlogService service = _kernel.Get<IBlogService>();

			//act
			Blog expected = new Blog
			{
				Id = 1,
				Name = "unit-test-blog",
				BlogPosts = new List<BlogPost>
				{
					new BlogPost
					{
						Id = 1,
						BlogSubject	= "blog subject 1-1",
						BlogContent = "blog content 1-1",
						Comments = new List<Comment>
						{
							new Comment
							{
								Id = 1,
								CommentContent = "comment 1-1",
								Username = "username 1-1"
							}
						}
					},
					new BlogPost
					{
						Id = 2,
						BlogSubject = "blog subject 1-2",
						BlogContent = "blog content 1-2",
					}
				}
			};

			Blog actual = service.Save(expected);

			Assert.IsNotNull(actual);
		}

		[TestMethod]
		public void CanSaveBlogWithSpecificIdAndGetItBackByIdTest()
		{
			//arrange
			int blogId = 5;
			IBlogService service = _kernel.Get<IBlogService>();
			Blog expected = new Blog
			{
				Id = blogId,
			    Name = "raven-db-unit-test-blog-no-" + blogId
			};

			//act
			Blog saved = service.Save(expected);

			BlogViewModel actual = service.GetBlog(blogId);

			Assert.IsNotNull(actual);
			Assert.IsNotNull(actual.Blog);
		}

	}
}
