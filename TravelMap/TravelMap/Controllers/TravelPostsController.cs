using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TravelMap.Models;
using TravelMap.Data;
using TravelMap.DTO;
using Microsoft.EntityFrameworkCore;
using TravelMap.Services.Interfaces;


namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TravelPostsController : ControllerBase
    {
        private readonly ITravelPostService _travelPostService;
        private readonly IAuthService _authService;

        public TravelPostsController(ITravelPostService travelPostService, IAuthService authService)
        {
            _travelPostService = travelPostService;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateTravelPost([FromForm] TravelPostRequest request)
        {
            // 从token中获取用户ID
            var userId = _authService.GetUserIdFromToken(User);

            if (userId == null)
            {
                return Unauthorized(new { code = 401, message = "Invalid token" });
            }
            // 遍历图片和顺序
            for (int i = 0; i < request.Images.Count; i++)
            {
                var image = request.Images[i];
                var order = request.Orders[i];

                Console.WriteLine($"图片 {i}: {image.FileName}, 顺序: {order}");
            }

            var response = await _travelPostService.CreateTravelPost(request, userId.Value);

            if (response.Code != 200)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }
    }
}
