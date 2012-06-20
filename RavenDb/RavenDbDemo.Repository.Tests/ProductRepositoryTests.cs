using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Ninject;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using Raven.Client.Linq;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository.Tests
{
	[TestFixture]
	public class ProductRepositoryTests
	{
		private static IKernel _kernel;

		[SetUp]
		public void SetUp()
		{
			_kernel = new StandardKernel();

			_kernel.Bind<IDocumentStore>().ToMethod(c =>
			{
				//IDocumentStore store = new EmbeddableDocumentStore { RunInMemory = true, DataDirectory = "Data", UseEmbeddedHttpServer = false };
				IDocumentStore store = new DocumentStore { Url = "http://localhost:8080" };
				store.Initialize();
				store.Conventions.IdentityPartsSeparator = "-";
				//store.Conventions
				IndexCreation.CreateIndexes(typeof(ProductPriceIndex).Assembly, store);
				return store;
			}).InTransientScope();
			_kernel.Bind<IDocumentSession>().ToMethod(c => c.Kernel.Get<IDocumentStore>().OpenSession()).InTransientScope();
			_kernel.Bind<IRepository<Product>>().To<ProductRepository>().InTransientScope();
			_kernel.Bind<IRepository<Category>>().To<CategoryRepository>().InTransientScope();
		}

		[TearDown]
		public void TearDown()
		{
		}

		[Test]
		public void CanLoadProductAndCategoryInOnlyOneHttpRequest()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			
			Category cat = new Category {Description = "unit-test-category"};
			using(IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}

			Product prod = new Product {CategoryId = cat.Id, Price = 1234, Description = "unit-test-product"};
			using(IDocumentSession session = store.OpenSession())
			{
				session.Store(prod);
				session.SaveChanges();
			}
			string pId = prod.Id;

			//act
			RavenQueryStatistics stats;
			Product actualProduct;
			Category actualCategory;
			using(IDocumentSession session = store.OpenSession())
			{
				actualProduct = session.Query<Product>()
					.Customize(c => c.Include<Product>(p => p.CategoryId).WaitForNonStaleResultsAsOfNow())
					.Statistics(out stats)
					.FirstOrDefault(p => p.Id == pId);
				actualCategory = session.Load<Category>(actualProduct.CategoryId);
			}

			//assert
			Assert.That(cat.Id, Is.Not.Empty);
			Assert.That(prod.Id, Is.Not.Empty);
			Assert.That(stats, Is.Not.Null);
			Assert.That(stats.TotalResults, Is.EqualTo(1));
			Assert.That(actualProduct, Is.Not.Null);
			Assert.That(actualProduct, Is.Not.Null);
			//cleanup
			using(IDocumentSession session = store.OpenSession())
			{
				Category deleteCategory = session.Load<Category>(actualCategory.Id);
				session.Delete(deleteCategory);
				Product deleteProduct = session.Load<Product>(actualProduct.Id);
				session.Delete(deleteProduct);
			}
		}

		[Test]
		public void CanLazilyLoadSeveralProducts()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();

			Category cat = new Category { Id = "categories-1", Description = "unit-test-category" };
			using (IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}

			using (IDocumentSession session = store.OpenSession())
			{
				for (int i = 1; i <= 10; i++)
				{
					Product prod = new Product { Id = "products-" + i, CategoryId = cat.Id, Price = 1234, Description = "unit-test-product" };
					session.Store(prod);
					session.SaveChanges();
				}
			}

			using(IDocumentSession session = store.OpenSession())
			{
				var cat1 = session.Advanced.Lazily.Load<Category>("categories-1");
				var prod1 = session.Advanced.Lazily.Load<Product>("products-1");
				var prod2 = session.Advanced.Lazily.Load<Product>("products-2");
				var prod3 = session.Advanced.Lazily.Load<Product>("products-3");
				var prod4 = session.Advanced.Lazily.Load<Product>("products-4");
				var prod5 = session.Advanced.Lazily.Load<Product>("products-5");
				var prod6 = session.Advanced.Lazily.Load<Product>("products-6");
				var prod7 = session.Advanced.Lazily.Load<Product>("products-7");
				var prod8 = session.Advanced.Lazily.Load<Product>("products-8");
				var prod9 = session.Advanced.Lazily.Load<Product>("products-9");
				var prod10 = session.Advanced.Lazily.Load<Product>("products-10");

				session.Advanced.Eagerly.ExecuteAllPendingLazyOperations();

				Assert.That(cat1.Value.Description, Is.EqualTo("unit-test-category"));
				Assert.That(prod1.Value.Description, Is.EqualTo("unit-test-product"));
				Assert.That(prod2.Value.Description, Is.EqualTo("unit-test-product"));
			}

			//clean up
			using (IDocumentSession session = store.OpenSession())
			{
				Category deleteCategory = session.Load<Category>("categories-1");
				session.Delete(deleteCategory);
				session.SaveChanges();
				for (int i = 1; i <= 10; i++)
				{
					Product deleteProd = session.Load<Product>("products-" + i);
					session.Delete(deleteProd);
					session.SaveChanges();
				}
			}

		}
	}
}
