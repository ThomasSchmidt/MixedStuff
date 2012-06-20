using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.Repository;
//using Demos.Orm.Repository.EntityFramework;
//using Demos.Orm.Repository.EntityFramework;
using Demos.Orm.Repository.EntityFramework;
using Demos.Orm.Repository.NHibernate;
using Demos.Orm.Repository.RavenDb;
using NHibernate;
using Ninject.Modules;

namespace Demos.Orm.ServiceLayer
{
	public class NinjectModules : NinjectModule
	{
		public override void Load()
		{
			Bind<IBlogService>().To<BlogService>().InRequestScope();
			
			//to use Entity Framework
			//Bind<IBlogRepository>().To<EfBlogRepository>().InRequestScope();
			//to use NHibernate
			//Bind<IBlogRepository>().To<NhBlogRepository>().InRequestScope();

			//to use RavenDB
			Bind<IBlogRepository>().To<RavenBlogRepository>().InRequestScope();
		}
	}
}
