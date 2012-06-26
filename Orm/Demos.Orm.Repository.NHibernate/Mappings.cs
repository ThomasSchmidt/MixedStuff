using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;
using NHibernate.Mapping.ByCode;
using NHibernate.Mapping.ByCode.Conformist;

namespace Demos.Orm.Repository.NHibernate
{
	public class BlogMap : ClassMapping<Blog>
	{
		public BlogMap()
		{
			Table("Blog");
			Id(b => b.Id, m =>
			{
				m.Column("Id");
				m.Generator(Generators.Identity);
			});
			Property(b => b.Name, m =>
			{
				m.Column("Name"); 
				m.Length(255);
			});
			Property(b => b.CreateDate, m =>
			{
				m.Column("CreateDate");
				m.Generated(PropertyGeneration.Insert);
			});
			Property(b => b.ModifyDate, m =>
			{
				m.Column("ModifyDate");
				m.Generated(PropertyGeneration.Always);
			});

			Set(b => b.BlogPosts,
			    m =>
			    {
					m.Table("BlogPosts");
					m.Cascade(Cascade.All);
					m.Inverse(false);
					m.Key(k => k.Column("BlogId"));
			    },
			    rel =>
			    {
					rel.OneToMany(otm =>
					{
						otm.NotFound(NotFoundMode.Ignore);
					});
			    });
		 }
	}

	public class BlogPostMap : ClassMapping<BlogPost>
	{
		public BlogPostMap()
		{
			Table("BlogPost");
			Id(bp => bp.Id, m =>
			{
				m.Column("Id");
				m.Generator(Generators.Identity);
			});
			Property(bp => bp.BlogSubject, m => m.Column("Subject"));
			Property(bp => bp.BlogContent, m => m.Column("Content"));
			Property(bp => bp.CreateDate, m =>
			{
				m.Column("CreateDate");
				m.Generated(PropertyGeneration.Insert);
			});
			Property(bp => bp.ModifyDate, m =>
			{
				m.Column("ModifyDate");
				m.Generated(PropertyGeneration.Always);
			});

			Set(b => b.Comments,
				m =>
				{
					m.Table("Comments");
					m.Cascade(Cascade.All);
					m.Inverse(true);
					m.Key(k => k.Column("BlogPostId"));
				},
				rel =>
				{
					rel.OneToMany(otm =>
					{
						otm.NotFound(NotFoundMode.Ignore);
					});
				});
		}
	}

	public class CommentMap : ClassMapping<Comment>
	{
		public CommentMap()
		{
			Table("Comment");
			Id(c => c.Id, m =>
			{
				m.Column("Id");
				m.Generator(Generators.Identity);
			});
			Property(c => c.CommentContent, m => m.Column("CommentContent"));
			Property(c => c.Username, m => m.Column("Username"));
			Property(c => c.CreateDate, m =>
			{
				m.Column("CreateDate");
				m.Generated(PropertyGeneration.Insert);
			});
			Property(c => c.ModifyDate, m =>
			{
				m.Column("ModifyDate");
				m.Generated(PropertyGeneration.Always);
			});
		}
	}
}
