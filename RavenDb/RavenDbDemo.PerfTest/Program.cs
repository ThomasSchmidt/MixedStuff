using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client;
using Raven.Client.Document;
using Raven.Client.Embedded;
using Raven.Client.Indexes;
using RavenDbDemo.Repository.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.PerfTest
{
	class Program
	{
		private static List<string> _simpleCreatedIds = new List<string>();
		private static List<string> _complesCreatedIds = new List<string>();

		static void Main(string[] args)
		{
			IDocumentStore store = GetStore();
			Log("PerfTestSimpleStart", ConsoleColor.Blue);
			PerfTestSimple(store, 100);
			Log("PerfTestSimpleEnd", ConsoleColor.Blue);
			Log("PerfTestComplexStart", ConsoleColor.Blue);
			PerfTestComplex(store, 100);
			Log("PerfTestComplexEnd", ConsoleColor.Blue);
			Log("AllDone", ConsoleColor.Red);
			Console.ReadLine();
		}

		private static IDocumentStore GetStore()
		{
			IDocumentStore store = new DocumentStore(){Url = "http://localhost:8080"};
			//IDocumentStore store = new EmbeddableDocumentStore();
			store.Initialize();
			//store.JsonRequestFactory.EnableBasicAuthenticationOverUnsecuredHttpEvenThoughPasswordsWouldBeSentOverTheWireInClearTextToBeStolenByHackers = true;
			//store.JsonRequestFactory.ConfigureRequest += (sender, args) => ((HttpWebRequest)args.Request).UnsafeAuthenticatedConnectionSharing = true;
			//store.Conventions.IdentityPartsSeparator = "-";
			IndexCreation.CreateIndexes(typeof(ProductPriceIndex).Assembly, store);
			IndexCreation.CreateIndexes(typeof(ComplexCategoryIndex).Assembly, store);
			return store;	
		}

		private static void Log(string message, ConsoleColor color)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message);
		}

		private static void PerfTestSimple(IDocumentStore store, int numberOfDocs)
		{
			//clean up
			store.DatabaseCommands.DeleteByIndex("CategoryIndex", new IndexQuery { Query = "" }, true);
			//wait for indexes
			int countDelete = store.OpenSession().Query<Category, CategoryIndex>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.Count();

			Stopwatch w = Stopwatch.StartNew();
			using (IDocumentSession session = store.OpenSession())
			{
				for (int i = 0; i < numberOfDocs; i++)
				{
					//session.Store(new Category { Description = "unit-test-description-" + i, Id = "categories-" + i });
					session.Store(new Category { Description = "unit-test-description-" + i });
					if (i % 1024 == 0)
						session.SaveChanges();
					if (i == numberOfDocs - 1)
						session.SaveChanges();
				}
			}
			
			long elapsedInsertRaven = w.ElapsedMilliseconds;
			Log("elapsedInsertRaven: " + elapsedInsertRaven, ConsoleColor.DarkYellow);
			w.Restart();

			//wait for indexes
			int count = store.OpenSession().Query<Category, CategoryIndex>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.Count();

			//get ids
			_simpleCreatedIds = store.OpenSession().Query<Category>().Take(numberOfDocs).Select(c => c.Id).ToList();

			w.Restart();

			using (IDocumentSession session = store.OpenSession())
			{
				//string id = "categories-1";
				//Category result = session.Query<Category, CategoryIndex>().FirstOrDefault(cat => cat.Id == id);
				Category result = session.Load<Category>(_simpleCreatedIds[0]);
				Debug.Assert(result != null);
			}

			long elapsedSingleSelectRaven = w.ElapsedMilliseconds;
			Log("elapsedSingleSelectRaven: " + elapsedSingleSelectRaven, ConsoleColor.DarkYellow);
			w.Restart();

			Parallel.ForEach(Enumerable.Range(0, _simpleCreatedIds.Count), i =>
			{
				using (IDocumentSession session = store.OpenSession())
				{
					//string id = "categories-" + i;
					//Category result = session.Query<Category, CategoryIndex>().FirstOrDefault(cat => cat.Id == id);
					Category result = session.Load<Category>(_simpleCreatedIds[i]);
					Debug.Assert(result != null);
				}
			});

			long elapsedSelectRaven = w.ElapsedMilliseconds;
			Log("elapsedSelectRaven: " + elapsedSelectRaven, ConsoleColor.DarkYellow);
			w.Restart();

			using (IDocumentSession session = store.OpenSession())
			{
				Category result = session.Load<Category>(_simpleCreatedIds[0]);
			}

			long elapsedSingleLoadRaven = w.ElapsedMilliseconds;
			Log("elapsedSingleLoadRaven: " + elapsedSingleLoadRaven, ConsoleColor.DarkYellow);
			w.Restart();

			Parallel.ForEach(Enumerable.Range(0, numberOfDocs), i =>
			{
				using (IDocumentSession session = store.OpenSession())
				{
					Category result = session.Load<Category>(_simpleCreatedIds[i]);
				}
			});

			long elapsedLoadRaven = w.ElapsedMilliseconds;
			Log("elapsedLoadRaven: " + elapsedLoadRaven, ConsoleColor.DarkYellow);
			w.Restart();
		}

		private static void PerfTestComplex(IDocumentStore store, int numberOfDocs)
		{
			//clean up
			store.DatabaseCommands.DeleteByIndex("ComplexCategoryIndex", new IndexQuery { Query = "" }, true);
			//wait for indexes
			int countDelete = store.OpenSession().Query<ComplexCategory, ComplexCategoryIndex>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.Count();

			Stopwatch w = Stopwatch.StartNew();

			using (IDocumentSession session = store.OpenSession())
			{
				for (int ix = 0; ix < numberOfDocs; ix++)
				{
					ComplexCategory cat = ComplexCategory.CreateRandom();
					session.Store(cat);
					if ( ix % 1024 == 0)
					{
						session.SaveChanges();
					}
					if ( ix == numberOfDocs - 1)
					{
						session.SaveChanges();
					}
				}
			}
			long elapsedComplexInsertRaven = w.ElapsedMilliseconds;
			Log("elapsedComplexInsertRaven: " + elapsedComplexInsertRaven, ConsoleColor.DarkYellow);

			//wait for indexes
			int count = store.OpenSession().Query<ComplexCategory, ComplexCategoryIndex>()
				.Customize(c => c.WaitForNonStaleResultsAsOfNow())
				.Count();

			//get ids
			_complesCreatedIds = store.OpenSession().Query<ComplexCategory>().Take(numberOfDocs).Select(c => c.Id).ToList();


			w.Restart();
			Parallel.ForEach(Enumerable.Range(0, numberOfDocs), i =>
			{
				using (IDocumentSession session = store.OpenSession())
				{
					ComplexCategory result = session.Load<ComplexCategory>(_complesCreatedIds[i]);
				}
			});
			long elapsedComplexSelectRaven = w.ElapsedMilliseconds;
			Log("elapsedComplexSelectRaven: " + elapsedComplexSelectRaven, ConsoleColor.DarkYellow);

			w.Restart();
			using (IDocumentSession session = store.OpenSession())
			{
				ComplexCategory result = session.Load<ComplexCategory>(_complesCreatedIds[0]);
			}
			long elapsedComplexSelectSingleRaven = w.ElapsedMilliseconds;
			Log("elapsedComplexSelectSingleRaven: " + elapsedComplexSelectSingleRaven, ConsoleColor.DarkYellow);

			w.Restart();
			Parallel.ForEach(Enumerable.Range(1, numberOfDocs), i =>
			{
				using (IDocumentSession session = store.OpenSession())
				{
					//string id = "complexcategories-" + i;
					//ComplexCategory result = session.Load<ComplexCategory>(id);
					ComplexCategory result = session.Load<ComplexCategory>(_complesCreatedIds[i - 1]);
				}
			});
			long elapsedComplexLoadRaven = w.ElapsedMilliseconds;
			Log("elapsedComplexLoadRaven: " + elapsedComplexLoadRaven, ConsoleColor.DarkYellow);

			w.Restart();
			using (IDocumentSession session = store.OpenSession())
			{
				//string id = "complexcategories-" + 1;
				//ComplexCategory result = session.Load<ComplexCategory>(id);
				ComplexCategory result = session.Load<ComplexCategory>(_complesCreatedIds[0]);
			}
			long elapsedComplexLoadSingleRaven = w.ElapsedMilliseconds;
			Log("elapsedComplexLoadSingleRaven: " + elapsedComplexSelectSingleRaven, ConsoleColor.DarkYellow);
		}

		public static class RandomGenerator
		{
			private static readonly Random _rnd = new Random(DateTime.Now.Millisecond);

			public static string GetRandomString()
			{
				int length = _rnd.Next(1, 100);
				StringBuilder sb = new StringBuilder(length);

				for (int ix = 0; ix < length; ix++)
				{
					int charNo = _rnd.Next(65, 90);
					sb.Append(Convert.ToChar(charNo));
				}
				return sb.ToString();
			}

			public static int GetRandomInt()
			{
				return _rnd.Next(int.MinValue, int.MaxValue);
			}

			public static DateTime GetRandomDateTime()
			{
				//long ticks = (long)((_rnd.NextDouble() * 2.0 - 1.0) * DateTime.MaxValue.Ticks);
				//return new DateTime(ticks);

				DateTime start = DateTime.MinValue;

				int range = ((TimeSpan)(DateTime.Today - start)).Days;
				return start.AddDays(_rnd.Next(range));
			}
		}

		public class ComplexCategoryIndex : AbstractIndexCreationTask<ComplexCategory>
		{
			public ComplexCategoryIndex()
			{
				Map = categories => categories.Select(cat => new 
				{ 
					cat.Id, 
					cat.Field1,
					cat.Field2,
					cat.Field3,
					cat.Field4,
					cat.Field5,
					cat.Field6,
					cat.Field7,
					cat.Field8,
					cat.Field9,
					cat.Field10,
					cat.Field11,
					cat.Field12,
					cat.Field13,
					cat.Field14,
					cat.Field15,
					cat.Field16,
					cat.Field17,
					cat.Field18,
					cat.Field19,
					cat.Field20,
					cat.Field21,
				});
				Index(cat => cat.Id, FieldIndexing.NotAnalyzed);
				Index(cat => cat.Field1, FieldIndexing.Analyzed);
				Index(cat => cat.Field2, FieldIndexing.Analyzed);
				Index(cat => cat.Field3, FieldIndexing.Analyzed);
				Index(cat => cat.Field4, FieldIndexing.Analyzed);
				Index(cat => cat.Field5, FieldIndexing.Analyzed);
				Index(cat => cat.Field6, FieldIndexing.Analyzed);
				Index(cat => cat.Field7, FieldIndexing.Analyzed);
				Index(cat => cat.Field8, FieldIndexing.Analyzed);
				Index(cat => cat.Field9, FieldIndexing.Analyzed);
				Index(cat => cat.Field10, FieldIndexing.Analyzed);
				Index(cat => cat.Field11, FieldIndexing.Analyzed);
				Index(cat => cat.Field12, FieldIndexing.Analyzed);
				Index(cat => cat.Field13, FieldIndexing.Analyzed);
				Index(cat => cat.Field14, FieldIndexing.Analyzed);
				Index(cat => cat.Field15, FieldIndexing.Analyzed);
				Index(cat => cat.Field16, FieldIndexing.Analyzed);
				Index(cat => cat.Field17, FieldIndexing.Analyzed);
				Index(cat => cat.Field18, FieldIndexing.Analyzed);
				Index(cat => cat.Field19, FieldIndexing.Analyzed);
				Index(cat => cat.Field20, FieldIndexing.Analyzed);
				Index(cat => cat.Field21, FieldIndexing.Analyzed);
			}
		}

		public class ComplexCategory
		{
			public static ComplexCategory CreateRandom()
			{
				return new ComplexCategory
				{
					Field1 = RandomGenerator.GetRandomString(),
					Field2 = RandomGenerator.GetRandomString(),
					Field3 = RandomGenerator.GetRandomString(),
					Field4 = RandomGenerator.GetRandomString(),
					Field5 = RandomGenerator.GetRandomString(),
					Field6 = RandomGenerator.GetRandomInt(),
					Field7 = RandomGenerator.GetRandomInt(),
					Field8 = RandomGenerator.GetRandomInt(),
					Field9 = RandomGenerator.GetRandomInt(),
					Field10 = RandomGenerator.GetRandomInt(),
					Field11 = RandomGenerator.GetRandomDateTime(),
					Field12 = RandomGenerator.GetRandomDateTime(),
					Field13 = RandomGenerator.GetRandomDateTime(),
					Field14 = RandomGenerator.GetRandomDateTime(),
					Field15 = RandomGenerator.GetRandomDateTime(),
					Field16 = ComplexOne.CreateRandom(),
					Field17 = ComplexOne.CreateRandom(),
					Field18 = ComplexOne.CreateRandom(),
					Field19 = ComplexTwo.CreateRandom(),
					Field20 = ComplexTwo.CreateRandom(),
					Field21 = ComplexTwo.CreateRandom()
				};
			}

			public string Id { get; set; }
			public string Field1 { get; set; }
			public string Field2 { get; set; }
			public string Field3 { get; set; }
			public string Field4 { get; set; }
			public string Field5 { get; set; }
			public int Field6 { get; set; }
			public int Field7 { get; set; }
			public int Field8 { get; set; }
			public int Field9 { get; set; }
			public int Field10 { get; set; }
			public DateTime Field11 { get; set; }
			public DateTime Field12 { get; set; }
			public DateTime Field13 { get; set; }
			public DateTime Field14 { get; set; }
			public DateTime Field15 { get; set; }
			public ComplexOne Field16 { get; set; }
			public ComplexOne Field17 { get; set; }
			public ComplexOne Field18 { get; set; }
			public ComplexTwo Field19 { get; set; }
			public ComplexTwo Field20 { get; set; }
			public ComplexTwo Field21 { get; set; }
		}

		public class ComplexOne
		{
			public static ComplexOne CreateRandom()
			{
				return new ComplexOne
				{
					ComplexOneField1 = RandomGenerator.GetRandomString(),
					ComplexOneField2 = RandomGenerator.GetRandomInt(),
					ComplexOneField3 = RandomGenerator.GetRandomDateTime()
				};
			}

			public string ComplexOneField1 { get; set; }
			public int ComplexOneField2 { get; set; }
			public DateTime ComplexOneField3 { get; set; }
		}

		public class ComplexTwo
		{
			public static ComplexTwo CreateRandom()
			{
				return new ComplexTwo
				{
					ComplexTwoField1 = RandomGenerator.GetRandomString(),
					ComplexTwoField2 = RandomGenerator.GetRandomInt(),
					ComplexTwoField3 = RandomGenerator.GetRandomDateTime()
				};
			}

			public string ComplexTwoField1 { get; set; }
			public int ComplexTwoField2 { get; set; }
			public DateTime ComplexTwoField3 { get; set; }
		}
	}
}
