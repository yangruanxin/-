using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TravelMap.Models;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace TravelMap.Data
{
    public class AppDbContext : DbContext
    {
        //构造函数
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        // 数据库表映射到类
        public DbSet<User> Users { get; set; }
        public DbSet<TravelPost> TravelPosts { get; set; }
        public DbSet<TravelImage> TravelImages { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            //指定实体类 User 映射到数据库中的表名
            modelBuilder.Entity<User>().ToTable("Users");

            modelBuilder.Entity<TravelPost>().ToTable("travelposts");

            modelBuilder.Entity<TravelImage>()
        .HasOne(img => img.TravelPost)
        .WithMany(post => post.ImageUrls)
        .HasForeignKey(img => img.TravelPostId)
        .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            

            
        }
    }
}

