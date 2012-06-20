using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Transactions;
using Raven.Client;
using Raven.Client.Indexes;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository
{
	public abstract class RepositoryBase<T> : IRepository<T> where T : EntityBase
	{
		private readonly IDocumentSession _session;

		protected RepositoryBase(IDocumentSession session)
		{
			_session = session;
		}

		public virtual void Save(T item)
		{
			_session.Store(item);
			_session.SaveChanges();
		}

		public virtual void Delete(T item)
		{
			T itemToDelete = _session.Load<T>(item.Id);
			if (itemToDelete != null)
			{
				_session.Delete(itemToDelete);
				_session.SaveChanges();
			}
		}

		public virtual T Get(string id)
		{
			return _session.Load<T>(id);
		}

		public IQueryable<T> Query(Expression<Func<T,bool>> query)
		{
			return _session.Query<T>().Where(query);
		}
	}
}
