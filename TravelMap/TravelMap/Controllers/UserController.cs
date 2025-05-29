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
using TravelMap.DTO;
using TravelMap.Services.Interfaces;
using TravelMap.Services;


namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<UserController> _logger;
        private readonly IAuthService _authService; 

        public UserController(AppDbContext context, IConfiguration config, ILogger<UserController> logger, IAuthService authService)
        {
            _context = context;
            _config = config;
            _logger = logger;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ApiResponseDto<string>> Register([FromBody] RegisterDto request)
        {
            // 调用 AuthService 注册用户
            var (success, message) = await _authService.Register(request.Username, request.Password);
            // 返回统一响应格式
            return new ApiResponseDto<string>
            {
                Code = success ? 200 : 400,
                Message = message,
                Data = success ? null : message
            };
        }

        [HttpPost("login")]
        public async Task<ApiResponseDto<LoginResponseDto>> Login([FromBody] LoginDto request)
        {
            // 调用 AuthService 验证用户
            var (success, token) = await _authService.Login(request.Username, request.Password);
            // 返回统一响应格式
            return new ApiResponseDto<LoginResponseDto>
            {
                Code = success ? 200 : 400,
                Message = success ? "登录成功" : "用户名或密码错误",
                Data = success ? new LoginResponseDto { Token = token } : null
            };
        }

        
        


       
    }
}
