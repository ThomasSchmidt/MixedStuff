using System;
using System.Collections.Generic;

namespace Demos.Orm.DomainModel
{
	public class BlogPost
	{
		public BlogPost()
		{
			this.Comments = new List<Comment>();
		}
		public virtual int Id { get; set; }
		public virtual string BlogSubject { get; set; }
		public virtual string BlogContent { get; set; }
		public virtual DateTime CreateDate { get; set; }
		public virtual DateTime ModifyDate { get; set; }
		public virtual ICollection<Comment> Comments { get; set; }
	}
}
