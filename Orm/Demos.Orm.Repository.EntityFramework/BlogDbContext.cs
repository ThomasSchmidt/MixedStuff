using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Common;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Demos.Orm.DomainModel;

namespace Demos.Orm.Repository.EntityFramework
{
	internal class BlogDbContext : DbContext
	{
		public DbSet<Blog> Blog { get; set; }
		public DbSet<BlogPost> BlogPosts { get; set; }
		public DbSet<Comment> Comments { get; set; }

		public BlogDbContext()
			: base(ConnectionString)
		{
		}

		private static string ConnectionString
		{
			get
			{
				string connectionString = ConfigurationManager.ConnectionStrings["BlogDbContext"].ConnectionString;
				if (connectionString.Contains("|DataDirectory|"))
					return connectionString;
				//get datasource and fix path so it works with resharper unit test runner
				string[] connectionStringElements = connectionString.Split(new char[] { '=' }, StringSplitOptions.RemoveEmptyEntries);
				string dataSourcePath = connectionStringElements[1];
				string fullSdfPath = Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dataSourcePath));
				return connectionString.Replace(dataSourcePath, fullSdfPath);
			}
		}

		protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			//blog
			modelBuilder.Configurations.Add(new BlogMap());

			//blogpost
			modelBuilder.Configurations.Add(new BlogPostMap());
			
			//comments
			modelBuilder.Configurations.Add(new CommentMap());

			base.OnModelCreating(modelBuilder);
		}
	}
}
