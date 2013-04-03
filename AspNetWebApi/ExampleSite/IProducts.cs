using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using ExampleSite.Controllers;

namespace ExampleSite
{
	// NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IProducts" in both code and config file together.
	[ServiceContract]
	public interface IProducts
	{
		[OperationContract]
		IEnumerable<Product> Get();
	}
}
