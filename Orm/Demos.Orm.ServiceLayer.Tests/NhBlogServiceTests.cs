using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using Demos.Orm.Repository;
using Demos.Orm.Repository.NHibernate;
using Demos.Orm.ViewModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Ninject;

namespace Demos.Orm.ServiceLayer.Tests
{
	[TestClass]
	public class NhBlogServiceTests
	{
		private static IKernel _kernel;

		[ClassInitialize]
		public static void ClassInit(TestContext testContext)
		{
			_kernel = new StandardKernel();
			_kernel.Load(new NinjectModules());
			_kernel.Rebind<IBlogRepository>().To<NhBlogRepository>();
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
		public void CanSaveOnlyBlogTest()
		{
			//arrange
			IBlogService service = _kernel.Get<IBlogService>();
			Blog blog = new Blog {Name = "unit-test-simple-blog"};

			//act
			Blog actual = service.Save(blog);

			//assert
			Assert.IsNotNull(actual);
			Assert.IsTrue(actual.Id > 0);
		}

		[TestMethod]
		public void CanSaveBlogTest()
		{
			//arrange
			IBlogService service = _kernel.Get<IBlogService>();

			//act
			Blog expected = new Blog
			{
				Name = "unit-test-blog",
				BlogPosts = new List<BlogPost>
				{
					new BlogPost
					{
						BlogSubject	= "blog subject 1-1",
						BlogContent = "blog content 1-1",
						Comments = new List<Comment>
						{
							new Comment
							{
								CommentContent = "comment 1-1",
								Username = "username 1-1"
							}
						}
					},
					new BlogPost
					{
						BlogSubject = "blog subject 1-2",
						BlogContent = "blog content 1-2",
					}
				}
			};

			Blog actual = service.Save(expected);

			Assert.IsNotNull(actual);
		}
	}
}
