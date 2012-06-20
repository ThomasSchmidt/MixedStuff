using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Ninject;
using Raven.Abstractions.Data;
using Raven.Client;
using Raven.Client.Linq;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository.Tests
{
	[TestFixture]
	public class CategoryRepositoryTests
	{
		private static IKernel _kernel;

		[SetUp]
		public void SetUp()
		{
			_kernel = new StandardKernel();
			
			_kernel.Bind<IDocumentStore>().ToMethod(c =>
			{
				//IDocumentStore store = new EmbeddableDocumentStore { RunInMemory = true, DataDirectory = "Data", UseEmbeddedHttpServer = false };
				IDocumentStore store = new DocumentStore {Url = "http://localhost:8080"};
				store.Initialize();
				store.Conventions.IdentityPartsSeparator = "-";
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
		public void CanAddCategoryToRavenDbThroughRepository()
		{
			//arrange
			IRepository<Category> repo = _kernel.Get<IRepository<Category>>();
			Category cat = new Category{Description = "unit-test-category"};

			//act
			repo.Save(cat);

			//assert
			Assert.That(cat.Description, Is.Not.Empty);
			Assert.That(cat.Id, Is.Not.Empty);
		}

		[Test]
		public void CanAddCategoryToRavenDbDirectly()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			Category cat = new Category{Description = "unit-test-category"};

			//act
			using( IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}

			//assert
			Assert.That(cat.Description, Is.Not.Empty);
			Assert.That(cat.Id, Is.Not.Empty);
		}

		[Test]
		public void CanAddCategoryWithSpecificIdThroughRepository()
		{
			//arrange
			IRepository<Category> repo = _kernel.Get<IRepository<Category>>();
			Category cat = new Category { Description = "unit-test-category", Id = "1"};

			//act
			repo.Save(cat);

			//assert
			Assert.That(cat.Description, Is.Not.Empty);
			Assert.That(cat.Id, Is.EqualTo("1"));
		}

		[Test]
		public void CanAddCategoryWithSpecificIdThroughRavenDbDirectly()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			Category cat = new Category { Description = "unit-test-category", Id = "1" };

			//act
			using (IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
			}

			//assert
			Assert.That(cat.Description, Is.Not.Empty);
			Assert.That(cat.Id, Is.EqualTo("1"));
		}

		
		[Test]
		public void CanGetCategoryByIdThroughRepository()
		{
			//arrange
			IRepository<Category> repo = _kernel.Get<IRepository<Category>>();
			Category cat = new Category { Description = "unit-test-category" };
			repo.Save(cat);
			string id = cat.Id;

			//act
			Category actual = repo.Get(id);

			//assert
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(id));
		}

		[Test]
		public void CanGetCategoryByIdThroughRavenDbDirectly()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			Category cat = new Category { Description = "unit-test-category" };
			using (IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}
			string id = cat.Id;

			//act
			Category actual;
			using (IDocumentSession session = store.OpenSession())
			{
				actual = session.Query<Category>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.FirstOrDefault(c => c.Id == id);
			}

			//assert
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(id));
		}

		[Test]
		public void CanGetCategoryBySpecificIdThroughRepository()
		{
			//arrange
			IRepository<Category> repo = _kernel.Get<IRepository<Category>>();
			string id = "1";
			Category cat = new Category { Description = "unit-test-category", Id = id};
			repo.Save(cat);

			//act
			Category actual = repo.Get(id);

			//assert
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(id));
		}

		[Test]
		public void CanGetCategoryBySpecificIdThroughRavenDbDirectly()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			string id = "1";
			Category cat = new Category { Description = "unit-test-category", Id = id};
			using (IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}

			//act
			Category actual;
			using (IDocumentSession session = store.OpenSession())
			{
				actual = session.Query<Category>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.FirstOrDefault(c => c.Id == id);
			}

			//assert
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Id, Is.EqualTo(id));
		}

		[Test]
		public void CanDeleteCategoryBySpecificIdThroughRepository()
		{
			//arrange
			IRepository<Category> repo = _kernel.Get<IRepository<Category>>();
			string id = "1";
			Category cat = new Category { Description = "unit-test-category", Id = id };
			repo.Save(cat);

			//act
			Category delete = repo.Get(id);
			repo.Delete(delete);
			Category actual = repo.Get(id);

			//assert
			Assert.That(actual, Is.Null);
		}

		[Test]
		public void CanDeleteCategoryBySpecificIdThroughRavenDbDirectly()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			string id = "1";
			Category cat = new Category { Description = "unit-test-category", Id = id };
			using (IDocumentSession session = store.OpenSession())
			{
				session.Store(cat);
				session.SaveChanges();
			}

			//act
			using(IDocumentSession session = store.OpenSession())
			{
				Category delete = session.Load<Category>(id);
				session.Delete(delete);
				session.SaveChanges();
			}

			Category actual;
			using (IDocumentSession session = store.OpenSession())
			{
				actual = session.Query<Category>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.FirstOrDefault(c => c.Id == id);
			}

			//assert
			Assert.That(actual, Is.Null);
		}

		[Test]
		public void CanSummarizeCategoryPrices()
		{
			//arrange
			IDocumentStore store = _kernel.Get<IDocumentStore>();
			using (IDocumentSession session = store.OpenSession())
			{
				Category cat1 = new Category {Id = "categories-1", Description = "cat1"};
				session.Store(cat1);

				Category cat2 = new Category {Id = "categories-2", Description = "cat2"};
				session.Store(cat2);

				Product prod1 = new Product {Id = "products-1", CategoryId = cat1.Id, Price = 2, Description = "prod1"};
				session.Store(prod1);

				Product prod2 = new Product {Id = "products-2", CategoryId = cat1.Id, Price = 3, Description = "prod2"};
				session.Store(prod2);

				Product prod3 = new Product {Id = "products-3", CategoryId = cat2.Id, Price = 4, Description = "prod3"};
				session.Store(prod3);

				session.SaveChanges();
			}

			using (IDocumentSession session = store.OpenSession())
			{
				var categoryTotals = session.Query<ProductTotalPricePerCategoryIndex.ReduceResult, ProductTotalPricePerCategoryIndex>()
					.Customize(c => c.WaitForNonStaleResultsAsOfNow())
					.Where(r => r.CategoryId == "categories-1").ToList();
			}
		}
	}
}
