using System;
using System.Collections.Generic;
using System.Linq;
using Demos.Orm.DomainModel;
using Demos.Orm.Repository;
using Demos.Orm.ViewModel;

namespace Demos.Orm.ServiceLayer
{
	internal class BlogService : IBlogService
	{
		private readonly IBlogRepository _blogRepository;

		public BlogService(IBlogRepository blogRepository)
		{
			_blogRepository = blogRepository;
		}

		public BlogViewModel GetBlog(int blogId)
		{
			return new BlogViewModel
			{
				Blog = _blogRepository.Get(blogId)
			};
		}
		public BlogViewModel GetLatestBlog()
		{
			return new BlogViewModel
			{
				Blog = _blogRepository.FindAll().OrderBy(b => b.CreateDate).FirstOrDefault()
			};
		}

		public BlogListViewModel GetTenLatestBlogs()
		{
			return new BlogListViewModel
			{
				Blogs = _blogRepository.FindAll()
					.OrderBy(b => b.CreateDate)
					.Take(10).ToList()
			};
		}

		public BlogPostListViewModel GetTenLatestBlogPosts(int id)
		{
			return new BlogPostListViewModel
			{
				BlogPosts = _blogRepository.FindAll()
					.Where(b => b.Id == id)
					.FirstOrDefault()
					.BlogPosts.OrderBy(bp => bp.CreateDate)
					.Take(10).ToList()
			};
		}

		public BlogPostViewModel GetBlogPost(int blogPostId)
		{
			return new BlogPostViewModel
			{
				BlogPost = _blogRepository.GetBlogPost(blogPostId)
			};
		}

		public Blog Save(Blog blog)
		{
			return _blogRepository.Save(blog);
		}
	}
}
