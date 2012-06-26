using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Demos.Orm.ServiceLayer.Tests
{
	[SetUpFixture]
	public class AssemblyInit
	{
		[SetUp]
		public void AssemblyInitialize()
		{
			Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");

			log4net.Config.XmlConfigurator.Configure();
		}
	}
}
