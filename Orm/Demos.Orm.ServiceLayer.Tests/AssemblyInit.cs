using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Demos.Orm.ServiceLayer.Tests
{
	[TestClass]
	public class AssemblyInit
	{
		[AssemblyInitialize]
		public static void AssemblyInitialize(TestContext context)
		{
			Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

			log4net.Config.XmlConfigurator.Configure();
		}
	}
}
