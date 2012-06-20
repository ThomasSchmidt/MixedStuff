using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using NHibernate;

namespace Demos.Orm.Repository.NHibernate
{
	public class BlogDbContext
	{
		internal static ISessionFactory CreateSessionFactory()
		{
			try
			{
				string connectionString = ConnectionString;

				var cfg = Fluently.Configure()
					.Database(
						MsSqlCeConfiguration.Standard
						.ShowSql()
						.ConnectionString(connectionString)
						)
					//.ExposeConfiguration(e => e.SetInterceptor(new SqlStatementInterceptor()))
					.Mappings(m =>
						{
							m.FluentMappings.AddFromAssemblyOf<BlogMap>();
						});
				return cfg.BuildSessionFactory();
			}
			catch
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
