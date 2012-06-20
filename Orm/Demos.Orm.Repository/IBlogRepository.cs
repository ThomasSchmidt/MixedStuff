using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;

namespace Demos.Orm.Repository
{
	public interface IBlogRepository
	{
		Blog Get(int id);
		Blog Save(Blog blog);
		void Delete(Blog blog);
		IQueryable<Blog> FindAll();
		BlogPost GetBlogPost(int blogPostId);
	}
}
