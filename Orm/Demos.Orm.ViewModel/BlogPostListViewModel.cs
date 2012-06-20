using System.Collections.Generic;
using Demos.Orm.DomainModel;

namespace Demos.Orm.ViewModel
{
	public class BlogPostListViewModel
	{
		public IList<BlogPost> BlogPosts { get; set; }
	}
}