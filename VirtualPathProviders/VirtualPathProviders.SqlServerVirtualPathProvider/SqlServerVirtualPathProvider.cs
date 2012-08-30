using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using Dapper;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
    public class SqlServerVirtualPathProvider : VirtualPathProvider
    {
	    private static ISqlConnectionFactory _factory;

		public SqlServerVirtualPathProvider(ISqlConnectionFactory factory)
		{
			_factory = factory;
		}

		public ISqlConnectionFactory ConnectionFactory 
		{
			get { return _factory; } 
		}

		public static void AppInitialize()
		{
			ConnectionStringSettings connectionString = ConfigurationManager.ConnectionStrings["SqlServerVirtualPathProvider"];
			ISqlConnectionFactory factory = new SqlConnectionFactory(connectionString);
			if (HostingEnvironment.IsHosted)
			{
				HostingEnvironment.RegisterVirtualPathProvider(new SqlServerVirtualPathProvider(factory));
			}
			
			//test for proper connection
			try
			{
				using (IDbConnection conn = factory.CreateConnection())
				{
					conn.Close();
				}
			}
			catch (SqlException)
			{
				throw new ArgumentException("connection string missing or invalid");
			}

			EnsureDatabaseSchemaIsValid();

		}

		private static void EnsureDatabaseSchemaIsValid()
		{
			const string sql = @"
				IF NOT EXISTS (SELECT * FROM sys.tables WHERE name='SqlServerVirtualPathProvider' and [type]='U')
				BEGIN
					CREATE TABLE [dbo].[SqlServerVirtualPathProvider]
					(
						[Id] [uniqueidentifier] NOT NULL,
						[VirtualPath] [nvarchar](1024) NOT NULL,
						[RawContent] [varbinary](max) NOT NULL,
						[MimeType] [nvarchar](50) NOT NULL,
						[IsDirectory] [bit] NOT NULL,
						CONSTRAINT [PK_SqlServerVirtualPathProvider] PRIMARY KEY CLUSTERED 
						(
							[Id] ASC
						)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
					) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
				END
				GO
				IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE [name] = (N'DF_SqlServerVirtualPathProvider_Id') AND type = 'D')
				BEGIN
				ALTER TABLE [dbo].[SqlServerVirtualPathProvider] ADD  CONSTRAINT [DF_SqlServerVirtualPathProvider_Id]  DEFAULT (newid()) FOR [Id]
				END
				GO
				IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE [name] = (N'DF_SqlServerVirtualPathProvider_MimeType') AND type = 'D')
				BEGIN
				ALTER TABLE [dbo].[SqlServerVirtualPathProvider] ADD  CONSTRAINT [DF_SqlServerVirtualPathProvider_MimeType]  DEFAULT ('') FOR [MimeType]
				END
				GO
				IF NOT EXISTS (SELECT * FROM dbo.sysobjects WHERE [name] = (N'DF_SqlServerVirtualPathProvider_IsDirectory') AND type = 'D')
				BEGIN
				ALTER TABLE [dbo].[SqlServerVirtualPathProvider] ADD  CONSTRAINT [DF_SqlServerVirtualPathProvider_IsDirectory]  DEFAULT (0) FOR [IsDirectory]
				END
				GO";

			using(IDbConnection conn = _factory.CreateConnection())
			{
				conn.Execute(sql);
			}
		}

		private bool IsPathVirtual(string virtualPath)
		{
			String checkPath = VirtualPathUtility.ToAppRelative(virtualPath);
			return checkPath.StartsWith("~/test", StringComparison.InvariantCultureIgnoreCase);
		}

		public override bool FileExists(string virtualPath)
		{
			if (!IsPathVirtual(virtualPath))
				return Previous.FileExists(virtualPath);

			using(IDbConnection conn = _factory.CreateConnection())
			{
				var entry = conn.Query<SqlServerVirtualPathEntry>("SELECT p.Id FROM dbo.SqlServerVirtualPathProvider p WHERE p.IsDirectory = 0 AND p.VirtualPath = @VirtualPath", new { VirtualPath = virtualPath }).FirstOrDefault();
				if (entry == null)
					return false;
				return true;
			}
		}

		public override VirtualFile GetFile(string virtualPath)
		{
			if (IsPathVirtual(virtualPath))
				return new SqlServerVirtualFile(virtualPath, this);
			else
				return Previous.GetFile(virtualPath);
		}

		public override bool DirectoryExists(string virtualDir)
		{
			if (!IsPathVirtual(virtualDir))
				return Previous.DirectoryExists(virtualDir);

			using(IDbConnection conn = _factory.CreateConnection())
			{
				var entry = conn.Query<SqlServerVirtualPathEntry>("SELECT p.Id FROM dbo.SqlServerVirtualPathProvider p WHERE p.IsDirectory = 1 AND p.VirtualPath = @VirtualPath", new {VirtualPath = virtualDir}).FirstOrDefault();
				return entry != null;
			}
		}

		public override VirtualDirectory GetDirectory(string virtualDir)
		{
			if (!IsPathVirtual(virtualDir))
				return Previous.GetDirectory(virtualDir);

			using(IDbConnection conn = _factory.CreateConnection())
			{
				var entry = conn.Query<SqlServerVirtualPathEntry>("SELECT p.Id FROM dbo.SqlServerVirtualPathProvider p WHERE p.IsDirectory = 1 AND p.VirtualPath = @VirtualPath", new { VirtualPath = virtualDir }).FirstOrDefault();
				if (entry == null)
					return null;
				return new SqlServerVirtualDirectory(virtualDir);
			}
		}

		public override System.Web.Caching.CacheDependency GetCacheDependency(string virtualPath, System.Collections.IEnumerable virtualPathDependencies, DateTime utcStart)
		{
			if (IsPathVirtual(virtualPath))
				return null;
			return Previous.GetCacheDependency(virtualPath, virtualPathDependencies, utcStart);
		}

		protected override void Initialize()
		{
			AppInitialize();
			base.Initialize();
		}
    }
}
