using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.ModelConfiguration;
using System.Data.Entity;
using System.Linq;
using System.Text;
using Demos.Orm.DomainModel;

namespace Demos.Orm.Repository.EntityFramework
{
	internal partial class BlogMap : EntityTypeConfiguration<Blog>
	{
		public BlogMap()
		{
			this.HasKey(b => b.Id);
			this.Property(b => b.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(b => b.Name).HasColumnName("Name").HasColumnType("nvarchar").HasMaxLength(255);
			this.Property(b => b.CreateDate).HasColumnName("CreateDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(b => b.ModifyDate).HasColumnName("ModifyDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
			this.ToTable("Blog", "dbo");

			this.HasMany(b => b.BlogPosts).WithRequired().Map(fk => fk.MapKey("BlogId")).WillCascadeOnDelete(true);
		}
	}

	internal partial class BlogPostMap : EntityTypeConfiguration<BlogPost>
	{
		public BlogPostMap()
		{
			this.HasKey(bp => bp.Id);
			this.Property(bp => bp.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(bp => bp.BlogSubject).HasColumnName("Subject");
			this.Property(bp => bp.BlogContent).HasColumnName("Content").HasColumnType("nvarchar").IsMaxLength();
			this.Property(bp => bp.CreateDate).HasColumnName("CreateDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(bp => bp.ModifyDate).HasColumnName("ModifyDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
			this.ToTable("BlogPost", "dbo");

			this.HasMany(bp => bp.Comments).WithRequired().Map(fk => fk.MapKey("BlogPostId")).WillCascadeOnDelete(true);
		}
	}

	internal partial  class CommentMap : EntityTypeConfiguration<Comment>
	{
		public CommentMap()
		{
			this.HasKey(c => c.Id);
			this.Property(c => c.Id).HasColumnName("Id").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(c => c.Username).HasColumnName("Username").HasMaxLength(255);
			this.Property(c => c.CommentContent).HasColumnName("CommentContent").HasMaxLength(255);
			this.Property(c => c.CreateDate).HasColumnName("CreateDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
			this.Property(c => c.ModifyDate).HasColumnName("ModifyDate").HasColumnType("datetime").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
			this.ToTable("Comment", "dbo");
		}
	}
}
