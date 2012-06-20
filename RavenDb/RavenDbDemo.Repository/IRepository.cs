using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Client.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository
{
	public interface IRepository<T>
	{
		void Save(T item);
		void Delete(T item);
		T Get(string id);
		IQueryable<T> Query(Expression<Func<T, bool>> query);
	}
}
