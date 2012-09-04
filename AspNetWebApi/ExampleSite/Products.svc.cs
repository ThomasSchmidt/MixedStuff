using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using ExampleSite.Controllers;

namespace ExampleSite
{
	public class Products : IProducts
	{
		[WebGet(UriTemplate = "Get", BodyStyle = WebMessageBodyStyle.Bare, ResponseFormat = WebMessageFormat.Json)]
		public IEnumerable<Product> Get()
		{
			return ProductController.Products;
		}
	}
}
