using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using NHibernate;
using NHibernate.Linq;

namespace Demos.Orm.Repository.NHibernate
{
	public class NhBlogRepository : IBlogRepository
	{
		private readonly ISession _session;

		public NhBlogRepository(ISession session)
		{
			_session = session;
		}

		public Blog Get(int id)
		{
			return _session.Get<Blog>(id);
		}

		public IQueryable<Blog> FindAll()
		{
			return _session.Query<Blog>();
		}

		public BlogPost GetBlogPost(int blogPostId)
		{
			return _session.Get<BlogPost>(blogPostId);
		}

		public void Delete(Blog blog)
		{
			using (ITransaction transaction = _session.BeginTransaction())
			{
				_session.Delete(blog);
				transaction.Commit();
			}
		}

		public Blog Save(Blog blog)
		{
			using (ITransaction transaction = _session.BeginTransaction())
			{
				//session.SaveOrUpdate(blog);
				_session.Save(blog);
				transaction.Commit();
			}
			return blog;
		}
	}
}
