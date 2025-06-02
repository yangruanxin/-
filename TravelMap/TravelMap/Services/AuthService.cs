//using System.Data.Entity;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MySqlConnector;
using TravelMap.Data;
using TravelMap.Models;
using TravelMap.Services.Interfaces;
using TravelMap.DTO;

namespace TravelMap.Services
{
    public class AuthService : IAuthService
    {
        //私有字段声明
        private readonly AppDbContext _context;//数据库上下文
        private readonly IConfiguration _config;//配置接口
        

        //构造函数
        public AuthService(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
            
        }

        //注册用户
        public async Task<(bool success, string message)> Register(string username, string password)
        {

            if (await _context.Users.AnyAsync(u => u.Username == username))
                return (false, "用户名已存在");

            _context.Users.Add(new User { Username = username, Password = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12) });
            await _context.SaveChangesAsync();
            return (true, "注册成功");
        }

        //登录用户
        public async Task<(bool success, string token)> Login(string username, string password)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);

            if (user == null)
            {
                return (false, null);
            }

            // 使用 BCrypt 验证密码
            if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
            {
                return (false, null);
            }

            var token = GenerateJwtToken(user);
            return (true, token);
        }

        //生成token
        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier,user.Id.ToString() )
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        //从token中获得id
        public int GetUserIdFromToken(ClaimsPrincipal user)
        {
            var Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(Id, out int userId))
            {
                return userId;
            }
            return 0;
        }

        //从id获得用户信息
        public async Task<User> GetUserByIdAsync(int Id)
        {
            using var connection = new MySqlConnection(_config.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string userQuery = @"SELECT Id, Username FROM Users WHERE Id = @Id";

            using var command = new MySqlCommand(userQuery, connection);
            command.Parameters.AddWithValue("@Id", Id);

            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("Id"),
                    Username = reader.GetString("Username"),
                };
            }

            return null;
        }

        //注销用户
        public async Task<bool> DeleteUserAsync(int Id)
        {
            // 查找用户
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == Id);

            // 如果用户不存在，返回 false
            if (user == null)
            {
                return false;
            }

            // 从数据库中移除用户
            _context.Users.Remove(user);

            // 保存更改到数据库
            var changes = await _context.SaveChangesAsync();

            // 如果有更改，表示删除成功
            return changes > 0;
        }
        
    }
}
