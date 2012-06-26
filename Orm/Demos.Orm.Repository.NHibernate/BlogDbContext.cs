using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NHibernate;
using NHibernate.Cfg;
using NHibernate.Cfg.MappingSchema;
using NHibernate.Dialect;
using NHibernate.Mapping.ByCode;
using Configuration = NHibernate.Cfg.Configuration;

namespace Demos.Orm.Repository.NHibernate
{
	public class BlogDbContext
	{
		internal static ISessionFactory CreateSessionFactory()
		{
			string connectionString = ConnectionString;

			ModelMapper mapper = new ModelMapper();
			mapper.AddMappings(Assembly.GetExecutingAssembly().GetExportedTypes());
			//mapper.BeforeMapSet += (mi, t, map) =>
			//{
			//    map.BatchSize(20);
			//    map.Cascade(Cascade.All | Cascade.DeleteOrphans);
			//};
			HbmMapping domainMapping = mapper.CompileMappingForAllExplicitlyAddedEntities();

			var configuration = new Configuration();
			configuration.DataBaseIntegration(c =>
			{
				c.Dialect<MsSqlCe40Dialect>();
				c.ConnectionString = connectionString;
				c.KeywordsAutoImport = Hbm2DDLKeyWords.AutoQuote;
				c.BatchSize = 25;
				//c.SchemaAction = SchemaAutoAction.Update;
			});
			configuration.Cache(c =>
			{
				//c.Provider<NHibernate.Caches.SysCache.SysCacheProvider>();
				c.UseQueryCache = true;
				c.DefaultExpiration = 10000;
			});
			configuration.AddMapping(domainMapping);

			try
			{
				return configuration.BuildSessionFactory();
			}
			catch (Exception)
			{
				throw;
			}
		}

		public static string ConnectionString
		{
			get
			{
				string connectionString = ConfigurationManager.ConnectionStrings["BlogDbContext"].ConnectionString;
				if (connectionString.Contains("|DataDirectory|"))
					return connectionString;
				//get datasource and fix path so it works with resharper unit test runner
				string[] connectionStringElements = connectionString.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				string dataSourcePath = connectionStringElements[1];
				string fullSdfPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataSourcePath));
				return connectionString.Replace(dataSourcePath, fullSdfPath);
				
			}
		}
	}

	public class SqlStatementInterceptor : EmptyInterceptor
	{
		public override global::NHibernate.SqlCommand.SqlString OnPrepareStatement(global::NHibernate.SqlCommand.SqlString sql)
		{
			Debug.WriteLine(sql.ToString());
			return base.OnPrepareStatement(sql);
		}
	}
}
