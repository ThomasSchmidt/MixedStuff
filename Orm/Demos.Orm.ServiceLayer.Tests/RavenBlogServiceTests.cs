using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using Demos.Orm.Repository;
using Demos.Orm.Repository.RavenDb;
using Demos.Orm.ViewModel;
using NUnit.Framework;
using Ninject;

namespace Demos.Orm.ServiceLayer.Tests
{
	[TestFixture]
	public class RavenBlogServiceTests
	{
		private static IKernel _kernel;

		public RavenBlogServiceTests()
		{
			_kernel = new StandardKernel();
			_kernel.Load(new NinjectModules());
			_kernel.Rebind<IBlogRepository>().To<RavenBlogRepository>();
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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
