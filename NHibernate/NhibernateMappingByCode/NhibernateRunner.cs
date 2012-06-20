using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Linq;
using NHibernate.Mapping;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace NhibernateMappingByCode
{
	public class NhibernateRunner
	{
		public void Run()
		{
			ModelMapper mapper = new ModelMapper();
			mapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());
			mapper.BeforeMapSet += (mi, t, map) =>
			{
				map.BatchSize(20);
				map.Cascade(Cascade.All | Cascade.DeleteOrphans);
			};
			HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

			var configuration = new Configuration();
			configuration.DataBaseIntegration(c =>
			{
				c.Dialect<MsSql2008Dialect>();
				c.ConnectionString = @"Data Source=localhost;Initial Catalog=Test;Integrated Security=True;Application Name=NHibernate";
				c.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
				c.BatchSize = 5;
				//c.SchemaAction = SchemaAutoAction.Update;
			});
			configuration.AddMapping(domainMapping);

			ISessionFactory factory = configuration.BuildSessionFactory();

			//adding new order and orderlines
			using(ISession session = factory.OpenSession())
			{
				using(ITransaction transaction = session.BeginTransaction())
				{
					Order o = new Order {Id = 1, OrderDate = DateTime.Now};
					o.OrderLines = new Collection<OrderLine>();
					o.OrderLines.Add(new OrderLine {Id = 1, Note = "note1", Price = 123, ProductId = 1, Order = o});
					o.OrderLines.Add(new OrderLine {Id = 2, Note = "note2", Price = 234, ProductId = 2, Order = o});
					session.Save(o);
					transaction.Commit();
				}
			}

			//querying for orders
			using(ISession session = factory.OpenSession())
			{
				//Order order = session.Query<Order>().Fetch(o => o.OrderLines).FirstOrDefault(o => o.Id == 1);
				Order order = session.QueryOver<Order>().Fetch(o => o.OrderLines).Eager.Where(o => o.Id == 1).SingleOrDefault<Order>();
				IList<Order> orders = session.Query<Order>().Take(5).ToList();

				foreach (Order currentOrder in orders)
				{
					foreach (OrderLine orderLine in currentOrder.OrderLines)
					{
						Console.WriteLine(orderLine.Id);
					}
				}
			}

			//adding new orderline to existing order
			using(ISession session = factory.OpenSession())
			{
				using(ITransaction transaction = session.BeginTransaction())
				{
					Order o = session.Get<Order>(1);
					o.OrderLines.Add(new OrderLine{Id = 3, Note = "note3", Price = 345, ProductId = 1, Order = o});
					session.SaveOrUpdate(o);
					transaction.Commit();
				}
			}

			//remove orderline from order
			using (ISession session = factory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					Order o = session.Get<Order>(1);
					OrderLine ol = o.OrderLines.Last();
					o.OrderLines.Remove(ol);
					session.Delete(ol);
					transaction.Commit();
				}
			}

			//deleting order and cascade deleting orderlines
			using(ISession session = factory.OpenSession())
			{
				using(ITransaction transaction = session.BeginTransaction())
				{
					Order o = session.Get<Order>(1);
					session.Delete(o);
					transaction.Commit();
				}
			}

			//add massive amount of orderlines to order
			using (ISession session = factory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					Order o = new Order{Id = 2, OrderDate = DateTime.Now};
					session.SaveOrUpdate(o);

					for (int i = 10; i <= 100; i++)
					{
						OrderLine ol = new OrderLine{Id = i, Note = "note" + i, Order = o, Price = i, ProductId = 1};
						o.OrderLines.Add(ol);
					}
					session.SaveOrUpdate(o);

					transaction.Commit();
				}
			}

			//enumerate over orderlines on order with massive amounts of orderlines
			using (ISession session = factory.OpenSession())
			{
				Order o = session.Get<Order>(2);
				foreach (OrderLine orderLine in o.OrderLines)
				{
					Console.WriteLine(orderLine.Id);
				}
			}

			//delete order with massive amounts of orderlines
			using (ISession session = factory.OpenSession())
			{
				using (ITransaction transaction = session.BeginTransaction())
				{
					Order o = session.Get<Order>(2);
					session.Delete(o);
					transaction.Commit();
				}
			}

		}
	}

	public class OrderMapping : ClassMapping<Order>
	{
		public OrderMapping()
		{
			Table("order");
			Schema("dbo");
			Id(o => o.Id, m =>
			{
				m.Column("Id");
				//m.Generator(Generators.HighLow, gm => gm.Params(new { max_low = 100 }));
				m.Generator(Generators.Assigned);
			});
			Property(o => o.OrderDate, m =>
			{
				m.Column("OrderDate");
				m.NotNullable(true);
			});
			Set(o => o.OrderLines,
				map =>
				{
					map.Table("orderline");
					map.Key(k => k.Column("OrderId"));
					map.Cascade(Cascade.All);
					map.Inverse(true);
					map.Lazy(CollectionLazy.Extra);
				},
				rel =>
				{
					rel.OneToMany(oneToMany =>
					{
						oneToMany.NotFound(NotFoundMode.Ignore);
					});
				}
			);
		}
	}

	public class OrderLineMapping : ClassMapping<OrderLine>
	{
		public OrderLineMapping()
		{
			Table("orderline");
			Schema("dbo");
			Id(ol => ol.Id, m =>
			{
				m.Column("Id");
				//m.Generator(Generators.HighLow, gm => gm.Params(new { max_low = 100 }));
				m.Generator(Generators.Assigned);
			});
			Property(ol => ol.Price, m =>
			{
				m.Column("Price");
				m.NotNullable(true);
			});
			Property(ol => ol.ProductId, m =>
			{
				m.Column("ProductId");
				m.NotNullable(true);
			});
			Property(ol => ol.Note, m =>
			{
				m.Column("Note");
				m.Length(50);
			});
			ManyToOne(ol => ol.Order, m =>
			{
				m.Column("OrderId");
			});
		}
	}
}
