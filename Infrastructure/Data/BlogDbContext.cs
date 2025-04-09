using Microsoft.EntityFrameworkCore;
using BlogProject.Core.Entities;

namespace BlogProject.Infrastructure.Data
{
    public class BlogDbContext : DbContext
    {
        public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

        public DbSet<Blog> Blogs { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           

            // ✅ Blog Entity yapılandırması (zorunlu değil ama dilersen ekleyebilirsin)
            modelBuilder.Entity<Blog>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
                entity.Property(e => e.Summary).HasMaxLength(500);
                entity.Property(e => e.Content).IsRequired();
                entity.Property(e => e.ImageUrl).HasMaxLength(500);
                entity.Property(e => e.Tags).HasMaxLength(300);
                entity.Property(e => e.Category).HasMaxLength(100);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("NOW()");
            });

            // ✅ Comment -> Blog ilişkisi
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Blog)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BlogId)
                .HasConstraintName("FK_Comments_Blogs_BlogId")
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
