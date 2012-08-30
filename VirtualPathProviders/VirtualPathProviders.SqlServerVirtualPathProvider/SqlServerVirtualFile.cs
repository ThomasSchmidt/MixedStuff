using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;
using Dapper;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
	public class SqlServerVirtualFile : VirtualFile
	{
		private SqlServerVirtualPathProvider _provider;

		public SqlServerVirtualFile(string virtualPath, SqlServerVirtualPathProvider provider) 
			: base(virtualPath)
		{
			_provider = provider;
		}

		public override Stream Open()
		{
			using(IDbConnection conn = _provider.ConnectionFactory.CreateConnection())
			{
				return null;
			}
		}

		public override bool IsDirectory
		{
			get { return false; }
		}
	}
}
