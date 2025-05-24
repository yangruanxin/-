using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelMap.Models;
using TravelMap.Services.Interfaces;

namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要认证
    public class CheckinController : ControllerBase
    {
        private readonly ICheckinService _checkinService;
        private readonly ILogger<CheckinController> _logger;

        public CheckinController(ICheckinService checkinService, ILogger<CheckinController> logger)
        {
            _checkinService = checkinService;
            _logger = logger;
        }

        [HttpPost("checkin")]
        public async Task<IActionResult> Checkin([FromBody] CheckinRequest request)
        {
            try
            {
                // 从JWT token中获取用户ID
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { success = false, message = "无效的用户凭证" });
                }

                // 调用打卡服务
                var result = await _checkinService.CheckinAsync(userId, request);

                return Ok(new CheckinResponse
                {
                    Success = true,
                    Message = "打卡成功",
                    Data = result
                });
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, "参数验证失败");
                return BadRequest(new { success = false, message = ex.Message });
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex, "地点未找到");
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打卡过程中发生错误");
                return StatusCode(500, new { success = false, message = "服务器内部错误" });
            }
        }
    }
}
