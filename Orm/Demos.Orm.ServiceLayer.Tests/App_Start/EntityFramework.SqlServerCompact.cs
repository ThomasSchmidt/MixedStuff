using System.Data.Entity;
using System.Data.Entity.Infrastructure;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Demos.Orm.ServiceLayer.Tests.App_Start.EntityFramework_SqlServerCompact), "Start")]

namespace Demos.Orm.ServiceLayer.Tests.App_Start {
    public static class EntityFramework_SqlServerCompact {
        public static void Start() {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }
    }
}
