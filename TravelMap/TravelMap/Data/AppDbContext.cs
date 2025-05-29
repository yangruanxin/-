using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using TravelMap.Models;

namespace TravelMap.Data
{
    public class AppDbContext : DbContext
    {
        //构造函数
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        // 数据库表映射到类
        public DbSet<User> Users { get; set; }
        public DbSet<TravelPost> TravelPosts { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //指定实体类 User 映射到数据库中的表名为 "Users"
            modelBuilder.Entity<User>().ToTable("Users");

            // 配置TravelPost实体将ImageUrls存储为JSON
            modelBuilder.Entity<TravelPost>()
                .Property(e => e.ImageUrls)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
        }
    }
}

