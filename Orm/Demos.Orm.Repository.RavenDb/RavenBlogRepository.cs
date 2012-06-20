using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Linq;

namespace Demos.Orm.Repository.RavenDb
{
	public class RavenBlogRepository : IBlogRepository
	{
		/*
		 * var fooModel = Session.Advanced.Lazily.Load<Foo>(fooId);
 
	var barModel = Session.Advanced.Lazily.Load<Bar>(barId);
 
	var blahModel = Session.Advanced.Lazily.Load<Blah>(blahId);
 
	var baseModel = Session.Advanced.Lazily.Load<Base>(baseId);
 
	Session.Advanced.Eagerly.ExecuteAllPendingLazyOperations();
 
	return View(new FBBBViewModel { foo = fooModel.Value, bar = barModel.Value, blah = blahModel.Value, base = baseModel.Value });
		 */
		private static DocumentStore _documentStore;

		public RavenBlogRepository()
		{
			_documentStore = BlogDbContext.DocumentStore();
		}

		public Blog Get(int id)
		{
			IDocumentSession session = _documentStore.OpenSession();
			return session.Load<Blog>(id);
		}

		public Blog Save(Blog blog)
		{
			IDocumentSession session = _documentStore.OpenSession();
			//code below is for demo purposes only, shows how child collections could get "assigned" an id
			if (blog.BlogPosts != null)
			{
				foreach (BlogPost blogPost in blog.BlogPosts.Where(bp => bp.Id <= 0))
				{
					session.Advanced.Conventions.GenerateDocumentKey(blogPost);
					if (blogPost.Comments != null)
					{
						foreach (Comment comment in blogPost.Comments.Where(c => c.Id <= 0))
						{
							session.Advanced.Conventions.GenerateDocumentKey(comment);
						}
					}
				}
			}
			session.Store(blog);
			session.SaveChanges();
			return blog;
		}

		public void Delete(Blog blog)
		{
			IDocumentSession session = _documentStore.OpenSession();
			session.Delete(blog);
			session.SaveChanges();
		}

		public IQueryable<Blog> FindAll()
		{
			IDocumentSession session = _documentStore.OpenSession();
			return session.Query<Blog>();
		}

		public BlogPost GetBlogPost(int blogPostId)
		{
			IDocumentSession session = _documentStore.OpenSession();
			//return session.Query<BlogPost>().FirstOrDefault(bp => bp.Id == blogPostId);
			var q = session.Query<Blog>()
				.Where(b => b.BlogPosts.Any(bp => bp.Id == blogPostId));
			return q.FirstOrDefault().BlogPosts.Where(bp => bp.Id == blogPostId) .FirstOrDefault();
			//.Where(b => b.BlogPosts.Any(bp => bp.Id == blogPostId))
			//.Select(b => b.BlogPosts.FirstOrDefault(bp => bp.Id == blogPostId)).FirstOrDefault();
		}
	}
}
