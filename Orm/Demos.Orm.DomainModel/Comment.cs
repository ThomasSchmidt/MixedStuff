using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Demos.Orm.DomainModel
{
	public class Comment
	{
		public virtual int Id { get; set; }
		public virtual string Username { get; set; }
		public virtual string CommentContent { get; set; }
		public virtual DateTime CreateDate { get; set; }
		public virtual DateTime ModifyDate { get; set; }
	}
}
