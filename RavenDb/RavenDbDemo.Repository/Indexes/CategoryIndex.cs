using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;
using RavenDdDemo.Model;

namespace RavenDbDemo.Repository.Indexes
{
	public class CategoryIndex : AbstractIndexCreationTask<Category>
	{
		public CategoryIndex()
		{
			Map = categories => categories.Select(cat => new {cat.Id, cat.Description});
			Index(cat => cat.Id, FieldIndexing.NotAnalyzed);
			Index(cat => cat.Description, FieldIndexing.Analyzed);
		}
	}
}
