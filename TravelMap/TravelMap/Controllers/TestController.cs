using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelMap.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        // 需要登录才能访问
        [HttpGet("protected")]
        [Authorize]
        public IActionResult GetProtected()
        {
            var username = User.Identity?.Name;
            return Ok(new { message = $"你已通过身份验证，欢迎 {username}！" });
        }

        // 不需要登录也能访问
        [HttpGet("public")]
        public IActionResult GetPublic()
        {
            return Ok(new { message = "这是公开接口，任何人都能访问。" });
        }
    }
}
