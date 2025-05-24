using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using TravelMap.Data;
using TravelMap.Models;
using BCrypt.Net;


namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public UserController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // 注册接口
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] User userDto)
        {
            if (await _context.Users.AnyAsync(u => u.Username == userDto.Username))
                return BadRequest("用户名已存在");

            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
            var user = new User
            {
                Username = userDto.Username,
                Password = hashedPassword
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return Ok("注册成功");
        }

        // 登录接口
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] User loginDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == loginDto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(loginDto.Password, user.Password))
                return Unauthorized("用户名或密码错误");

            var token = GenerateJwtToken(user);
            return Ok(new { token, username = user.Username });
        }

        // 生成 JWT 方法
        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(int.Parse(_config["Jwt:ExpiresInMinutes"])),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
