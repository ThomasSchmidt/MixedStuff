using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using NHibernate;
using Ninject;
using Ninject.Modules;

namespace Demos.Orm.Repository.NHibernate
{
	public class NhNinjectModule : NinjectModule
	{
		public override void Load()
		{
			Bind<ISessionFactory>().ToMethod(ctx => BlogDbContext.CreateSessionFactory()).InSingletonScope();
			Bind<ISession>().ToMethod(ctx => ctx.Kernel.Get<ISessionFactory>().OpenSession()).InScope(context => HttpContext.Current);
		}
	}
}
