using System;
using Raven.Client;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository
{
	public class ProductRepository : RepositoryBase<Product>
	{
		public ProductRepository(IDocumentSession session) 
			: base(session)
		{
		}
	}
}
