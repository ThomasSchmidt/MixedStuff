using System;
using System.Linq;
using Raven.Client.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository.Indexes
{
	public class ProductTotalPricePerCategoryIndex : AbstractIndexCreationTask<Product, ProductTotalPricePerCategoryIndex.ReduceResult>
	{
		public class ReduceResult
		{
			public string CategoryId { get; set; }
			public int Total { get; set; }
		}

		public ProductTotalPricePerCategoryIndex()
		{
			Map = products => products
				.Select(p => new { CategoryId = p.CategoryId, Total = p.Price });

			Reduce = results => results
				.GroupBy(p => p.CategoryId)
				.Select(g => new { CategoryId = g.Key, Total = g.Sum(p => p.Total) });

			//Index(p => p.CategoryId, FieldIndexing.Analyzed);
		}
	}
}
