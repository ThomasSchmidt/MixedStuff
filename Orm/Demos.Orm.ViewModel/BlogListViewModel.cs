using System.Collections.Generic;
using Demos.Orm.DomainModel;

namespace Demos.Orm.ViewModel
{
	public class BlogListViewModel
	{
		public IList<Blog> Blogs { get; set; }
	}
}