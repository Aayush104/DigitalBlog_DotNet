using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DigitalBlog.Models;

public partial class DigitalBlogContext : DbContext
{
    public DigitalBlogContext()
    {
    }

    public DigitalBlogContext(DbContextOptions<DigitalBlogContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Blog> Blogs { get; set; }

    public virtual DbSet<BlogSubscription> BlogSubscriptions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("name=Conn");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Blog>(entity =>
        {
            entity.HasKey(e => e.Bid).HasName("PK__Blog__C6D111C9F464847C");

            entity.ToTable("Blog");

            entity.Property(e => e.Bid).ValueGeneratedNever();
            entity.Property(e => e.Amount).HasColumnType("decimal(8, 2)");
            entity.Property(e => e.Bdescription).HasColumnName("BDescription");
            entity.Property(e => e.Bstatus)
                .HasMaxLength(50)
                .HasColumnName("BStatus");

            entity.HasOne(d => d.User).WithMany(p => p.Blogs)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Blog__UserId__3C69FB99");
        });

        modelBuilder.Entity<BlogSubscription>(entity =>
        {
            entity.HasKey(e => e.Subid).HasName("PK__BlogSubs__4D98A47200333893");

            entity.ToTable("BlogSubscription");

            entity.Property(e => e.Subid).ValueGeneratedNever();
            entity.Property(e => e.SubAmount).HasColumnType("decimal(8, 2)");

            entity.HasOne(d => d.BidNavigation).WithMany(p => p.BlogSubscriptions)
                .HasForeignKey(d => d.Bid)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogSubscri__Bid__4222D4EF");

            entity.HasOne(d => d.User).WithMany(p => p.BlogSubscriptions)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__BlogSubsc__UserI__412EB0B6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CC4CFB7333D2");

            entity.HasIndex(e => e.Phone, "UQ__Users__5C7E359ED749C436").IsUnique();

            entity.Property(e => e.UserId).ValueGeneratedNever();
            entity.Property(e => e.EmailAddress).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(40);
            entity.Property(e => e.LoginName).HasMaxLength(30);
            entity.Property(e => e.LoginPassword).HasMaxLength(50);
            entity.Property(e => e.LoginStatus).HasDefaultValue(true);
            entity.Property(e => e.Phone).HasMaxLength(15);
            entity.Property(e => e.UserRole).HasMaxLength(20);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
