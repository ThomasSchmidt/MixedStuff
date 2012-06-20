using System.Collections.Generic;
using Demos.Orm.DomainModel;
using Demos.Orm.ViewModel;

namespace Demos.Orm.ServiceLayer
{
	public interface IBlogService
	{
		BlogViewModel GetLatestBlog();
		BlogViewModel GetBlog(int id);
		BlogListViewModel GetTenLatestBlogs();
		BlogPostListViewModel GetTenLatestBlogPosts(int id);
		BlogPostViewModel GetBlogPost(int id);
		Blog Save(Blog blog);
	}
}
