using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace VirtualPathProviders.SqlServerVirtualPathProvider
{
	public interface ISqlConnectionFactory
	{
		IDbConnection CreateConnection();
		IDbConnection CreateConnection(bool openConnection);
	}
}
