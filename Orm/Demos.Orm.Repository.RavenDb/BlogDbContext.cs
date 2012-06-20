using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Client.Document;

namespace Demos.Orm.Repository.RavenDb
{
	public class BlogDbContext
	{
		public static DocumentStore DocumentStore()
		{
			DocumentStore store = new DocumentStore { Url = "http://localhost:8080" };
			store.Initialize();
			return store;
		}
	}
}
