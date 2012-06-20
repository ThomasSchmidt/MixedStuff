using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using FluentNHibernate.Mapping;

namespace Demos.Orm.Repository.NHibernate
{
	public class BlogMap : ClassMap<Blog>
	{
		public BlogMap()
		{
			Table("Blog");
			Id(b => b.Id).Column("Id")
				.Not.Nullable()
				.GeneratedBy.Identity();
			Map(b => b.Name).Column("Name").Length(255);
			Map(b => b.CreateDate).Column("CreateDate").Generated.Insert();
			Map(b => b.ModifyDate).Column("ModifyDate").Generated.Always();

			HasMany(b => b.BlogPosts)
				.KeyColumn("BlogId")
				//.AsBag()
				.Not.Inverse()
				.Not.KeyNullable()
				.Cascade.All();
		 }
	}

	public class BlogPostMap : ClassMap<BlogPost>
	{
		public BlogPostMap()
		{
			Table("BlogPost");
			Id(bp => bp.Id).Column("Id")
				.Not.Nullable()
				.GeneratedBy.Identity();
			Map(bp => bp.BlogSubject).Column("Subject");
			Map(bp => bp.BlogContent).Column("Content");
			Map(bp => bp.CreateDate).Column("CreateDate").Generated.Insert();
			Map(bp => bp.ModifyDate).Column("ModifyDate").Generated.Always();

			HasMany(bp => bp.Comments)
				.KeyColumn("BlogPostId")
				.Not.Inverse()
				.Not.KeyNullable()
				.Cascade.All();
		}
	}

	public class CommentMap : ClassMap<Comment>
	{
		public CommentMap()
		{
			Table("Comment");
			Id(c => c.Id).Column("Id")
				.Not.Nullable()
				.GeneratedBy.Identity();
			Map(c => c.CommentContent).Column("CommentContent");
			Map(c => c.Username).Column("Username");
			Map(c => c.CreateDate).Column("CreateDate").Generated.Insert();
			Map(c => c.ModifyDate).Column("ModifyDate").Generated.Always();
		}
	}
}
