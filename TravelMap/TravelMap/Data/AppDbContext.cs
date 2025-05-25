using Microsoft.EntityFrameworkCore;
using TravelMap.Models;

namespace TravelMap.Data
{
    public class AppDbContext : DbContext
    {
        // 数据库表映射到类
        public DbSet<User> Users { get; set; } 
        public DbSet<CheckIn> CheckIns { get; set; }
        //构造函数
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options){}
    }
}

