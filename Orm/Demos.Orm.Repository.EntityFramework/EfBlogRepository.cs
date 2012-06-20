using System;
using System.Linq;
using Demos.Orm.DomainModel;
using System.Data.Entity;

namespace Demos.Orm.Repository.EntityFramework
{
	public class EfBlogRepository : Demos.Orm.Repository.IBlogRepository
	{
		private readonly BlogDbContext _db;

		public EfBlogRepository()
		{
			_db = new BlogDbContext();
		}
		public Blog Get(int id)
		{
			//return _db.Blog.Include(b => b.BlogPosts).FirstOrDefault(b => b.BlogId == id);
			return _db.Blog.FirstOrDefault(b => b.Id == id);
		}

		public Blog Save(Blog blog)
		{
			blog = _db.Blog.Add(blog);
			_db.SaveChanges();
			return blog;
		}

		public void Delete(Blog blog)
		{
			_db.Blog.Remove(blog);
			_db.SaveChanges();
		}

		public IQueryable<Blog> FindAll()
		{
			return _db.Blog;//.Include(b => b.BlogPosts).Include("BlogPosts.Comments");
		}

		public BlogPost GetBlogPost(int blogPostId)
		{
			return _db.BlogPosts.Include(bp => bp.Comments).FirstOrDefault(bp => bp.Id == blogPostId);
		}
	}
}
