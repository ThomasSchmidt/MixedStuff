using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Hosting;
using NUnit.Framework;

namespace VirtualPathProviders.SqlServerVirtualPathProvider.Tests
{
	[TestFixture]
    public class SqlServerVirtualPathProviderTests
    {
		private SqlServerVirtualPathProvider GetProvider()
		{
			ISqlConnectionFactory factory = GetConnectionFactory();
			return new SqlServerVirtualPathProvider(factory);
		}

		private ISqlConnectionFactory GetConnectionFactory()
		{
			return new SqlConnectionFactory(ConfigurationManager.ConnectionStrings["SqlServerVirtualPathProvider"]);
		}

		[Test]
		public void CanInitializeSqlServerVirtualPathProvider()
		{
			SqlServerVirtualPathProvider.AppInitialize();
		}

		[Test]
		public void CanGetVirtualFile()
		{
			SqlServerVirtualPathProvider provider = GetProvider();

			VirtualFile actual = provider.GetFile("/test/file.jpg");

			Assert.IsNotNull(actual);
		}

		[Test]
		public void CanCheckIfVirtualFileExistsForFileThatDoesNotExist()
		{
			SqlServerVirtualPathProvider provider = GetProvider();

			bool actual = provider.FileExists("/test/i/do/not/exist.jpg");

			Assert.IsFalse(actual);
		}

		[Test]
		public void CanCheckIfVirtualDirectoryExistsForDirectoryThatDoesNotExist()
		{
			SqlServerVirtualPathProvider provider = GetProvider();

			bool actual = provider.DirectoryExists("/test/i/do/not/exist/");

			Assert.IsFalse(actual);
		}
    }
}
