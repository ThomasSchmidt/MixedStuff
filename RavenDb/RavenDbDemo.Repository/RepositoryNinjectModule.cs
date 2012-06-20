using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Ninject;
using Ninject.Modules;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Indexes;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository
{
	public class RepositoryNinjectModule : NinjectModule
	{
		public override void Load()
		{
			Bind<IDocumentStore>().ToMethod(c =>
			{
				IDocumentStore store = new DocumentStore{Url = "http://localhost:8080", DefaultDatabase = "RavenDbDemo"};
				store.Initialize();
				store.Conventions.IdentityPartsSeparator = "-";
				IndexCreation.CreateIndexes(typeof(ProductPriceIndex).Assembly, store);
				return store;
			}).InSingletonScope();
			Bind<IDocumentSession>().ToMethod(c => c.Kernel.Get<IDocumentStore>().OpenSession()).InRequestScope();
			Bind<IRepository<Product>>().To<ProductRepository>().InRequestScope();
			Bind<IRepository<Category>>().To<CategoryRepository>().InRequestScope();
		}
	}
}
