using System.Data.Entity;
using System.Data.Entity.Infrastructure;

[assembly: WebActivator.PreApplicationStartMethod(typeof(Demos.Orm.WebSite.App_Start.EntityFrameworkSqlServerCompact), "Start")]

namespace Demos.Orm.WebSite.App_Start {
    public static class EntityFrameworkSqlServerCompact {
        public static void Start() {
            Database.DefaultConnectionFactory = new SqlCeConnectionFactory("System.Data.SqlServerCe.4.0");
        }
    }
}
