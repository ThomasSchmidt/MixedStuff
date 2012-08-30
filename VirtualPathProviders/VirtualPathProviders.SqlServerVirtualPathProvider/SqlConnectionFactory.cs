using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
	public class SqlConnectionFactory : ISqlConnectionFactory
	{
		private static ConnectionStringSettings _connectionString;

		public SqlConnectionFactory(ConnectionStringSettings connectionString)
		{
			if ( connectionString == null )
				throw new ArgumentException("connectionString");

			_connectionString = connectionString;
		}

		public IDbConnection CreateConnection()
		{
			return CreateConnection(true);
		}

		public IDbConnection CreateConnection(bool openConnection)
		{
			SqlConnection conn = new SqlConnection(_connectionString.ConnectionString);
			if (openConnection)
				conn.Open();
			return conn;
		}
	}
}
