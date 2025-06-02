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
using Microsoft.AspNetCore.Authorization;
using System.Data.Entity;
using Microsoft.AspNetCore.Identity;


namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        //私有字段声明
        private readonly AppDbContext _context;//数据库上下文
        private readonly IConfiguration _config;//配置接口
        private readonly IAuthService _authService;//用户服务接口
        private readonly ILogger<UserController> _logger;//日志记录
        private readonly IPasswordHasher<User> _passwordHasher;//密码加密

        //构造函数
        public UserController(AppDbContext context, IConfiguration config,  IAuthService authService, ILogger<UserController> logger, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _config = config;
            _authService = authService;
            _logger = logger;
            _passwordHasher = passwordHasher;
        }

        //注册
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

        //登录
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

        //注销用户
        [HttpDelete]
        [Authorize]//需要认证
        public async Task<IActionResult> DeleteUser()
        {
            // 从token中获取用户ID
            var Id = _authService.GetUserIdFromToken(User);

            if (Id == 0)
            {
                return Unauthorized(ApiResponseDto<string>.Error("认证失败，无法获取用户ID。", 401));
            }

            var result = await _authService.DeleteUserAsync(Id);

            if (result)
            {
                // 删除成功
                return Ok(ApiResponseDto<string>.Success("用户账号已成功注销。", "账号已删除"));
            }
            else
            {
                // 删除失败，可能用户不存在（虽然经过认证，但用户可能已被手动删除）
                return NotFound(ApiResponseDto<string>.Error("用户不存在或已被删除。", 404));
            }
        }

        //查询个人信息
        [HttpGet("me")]
        [Authorize] // 需要认证
        public async Task<IActionResult> GetMyUser()
        {
            try
            {
                // 从token中获取用户ID
                var Id = _authService.GetUserIdFromToken(User);

                if (Id == 0)
                {
                    return Unauthorized(ApiResponseDto<string>.Error("认证失败，无法获取用户ID。", 401));
                }
                
                // 获取用户的个人信息
                var user = await _authService.GetUserByIdAsync(Id);

                if (user == null)
                {
                    return NotFound(new
                    {
                        code = 404,
                        message = "User not found",
                        data = (object)null
                    });
                }

                return Ok(new ApiResponseDto<User>
                {
                    Code = 0,
                    Message = "Success",
                    Data = user
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Code = -1,
                    Message = "An error occurred while retrieving users"
                });
            }
        }

        //修改用户信息
        [HttpPut("info")]
        [Authorize]
        public async Task<IActionResult> UpdateUserInfo([FromBody] UpdateUserInfoRequestDto request)
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound(new { code = 404, message = "用户不存在" });

            bool hasPasswordUpdated = false;
            string? newUsername = null;

            // 如果修改密码，必须校验原密码
            if (!string.IsNullOrEmpty(request.Password))
            {
                if (string.IsNullOrEmpty(request.OriginPassword) ||
                    _passwordHasher.VerifyHashedPassword(user, user.Password, request.OriginPassword) == PasswordVerificationResult.Failed)
                {
                    return BadRequest(new { code = 400, message = "原始密码错误" });
                }

                user.Password = _passwordHasher.HashPassword(user, request.Password);
                hasPasswordUpdated = true;
            }

            // 修改用户名
            if (!string.IsNullOrEmpty(request.Username) && request.Username != user.Username)
            {
                if (_context.Users.Any(u => u.Username == request.Username))
                {
                    return BadRequest(new { code = 400, message = "用户名已存在" });
                }

                user.Username = request.Username;
                newUsername = request.Username;
            }

            user.UpdatedTime = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = 200,
                message = "success",
                data = new
                {
                    username = newUsername,
                    hasPasswordUpdated,
                    updatedTime = user.UpdatedTime
                }
            });
        }

        //退出登录
        [HttpPost("logout")]
        [Authorize] // 需要认证
        public IActionResult Logout()
        {
            return Ok(new
            {
                Message = "退出登录成功"
            });
        }


    }
}
