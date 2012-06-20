using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository.Indexes
{
	public class ProductPriceIndex : AbstractIndexCreationTask<Product>
	{
		public ProductPriceIndex()
		{
			Map = products => products.Select(p => new { p.Price });
			Index(p => p.Price, FieldIndexing.Analyzed);
		}
	}
}
