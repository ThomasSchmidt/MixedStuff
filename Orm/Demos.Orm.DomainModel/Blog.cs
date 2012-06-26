using System;
using System.Collections.Generic;

namespace Demos.Orm.DomainModel
{
	public class Blog
	{
		public virtual int Id { get; set; }
		public virtual string Name { get; set; }
		public virtual DateTime CreateDate { get; set; }
		public virtual DateTime ModifyDate { get; set; }
		public virtual ICollection<BlogPost> BlogPosts { get; set; }
	}
}
