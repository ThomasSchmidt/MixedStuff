using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository
{
	public class CategoryRepository : RepositoryBase<Category>
	{
		public CategoryRepository(IDocumentSession session) 
			: base(session)
		{
		}
	}
}
