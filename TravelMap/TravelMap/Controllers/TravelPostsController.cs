using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TravelMap.Models;
using TravelMap.Data;
using TravelMap.DTO;
using Microsoft.EntityFrameworkCore;
using TravelMap.Services.Interfaces;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using System.Text.Json;



namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/travel-posts")]
    [Authorize]
    public class TravelPostsController : ControllerBase
    {
        //私有字段声明
        private readonly AppDbContext _context;//数据库上下文
        private readonly ITravelPostService _travelPostService;//旅游记录服务接口
        private readonly IAuthService _authService;//用户服务接口
        private readonly ILogger<UserController> _logger;//日志记录

        //构造函数
        public TravelPostsController(AppDbContext context, ITravelPostService travelPostService, IAuthService authService, ILogger<UserController> logger)
        {
            _context = context;
            _travelPostService = travelPostService;
            _authService = authService;
            _logger = logger;
        }

        //发表一条旅游记录
        [HttpPost]
        public async Task<IActionResult> CreateTravelPost([FromForm] TravelPostRequest request)
        {
            // 从token中获取用户ID
            var Id = _authService.GetUserIdFromToken(User);

            if (Id == 0)
            {
                return Unauthorized(ApiResponseDto<string>.Error("认证失败，无法获取用户ID。", 401));
            }

            // 遍历图片和顺序
            for (int i = 0; i < request.ImageUrls.Count; i++)
            {
                var image = request.ImageUrls[i];
                var order = request.Orders[i];

                Console.WriteLine($"图片 {i}: {image.FileName}, 顺序: {order}");
            }

            var response = await _travelPostService.CreateTravelPost(request, Id );

            if (response.Code != 200)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        //查询个人旅游记录
        [HttpGet("me")]
        public async Task<IActionResult> GetMyTravelPosts()
        {
            try
            {
                // 从token中获取用户ID
                var Id = _authService.GetUserIdFromToken(User);

                if (Id == 0)
                {
                    return Unauthorized(ApiResponseDto<string>.Error("认证失败，无法获取用户ID。", 401));
                }
                
                // 获取用户的旅游记录
                var posts = await _travelPostService.GetTravelPostsByUserIdAsync(Id);

                // 按开始时间降序排序
                var sortedPosts = posts.OrderByDescending(p => p.BeginTime).ToList();

                // 确保每个post的图片URL按sort_order升序排序
                foreach (var post in sortedPosts)
                {
                    post.ImageUrls = post.ImageUrls.OrderBy(img => img.Order).ToList();
                }

                return Ok(new ApiResponseDto<List<TravelPost>>
                {
                    Code = 200,
                    Message = "Success",
                    Data = sortedPosts
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting travel posts");
                return StatusCode(500, new ApiResponseDto<object>
                {
                    Code = -1,
                    Message = "An error occurred while retrieving travel posts"
                });
            }
        }

        //删除旅游记录
        [HttpDelete("{postId}")]
        [Authorize]
        public async Task<IActionResult> DeleteTravelPost(int postId)
        {
            try
            {
                // 从token中获取用户ID
                var Id = _authService.GetUserIdFromToken(User);

                if (Id == 0)
                {
                    return Unauthorized(ApiResponseDto<string>.Error("认证失败，无法获取用户ID。", 401));
                }

                // 检查该记录是否属于当前用户
                var post = await _travelPostService.GetTravelPostByTravelPostIdAsync(postId);
                if (post == null || post.UserId != Id)
                {
                    return Forbid("您无权删除此记录。");
                }

                // 执行删除
                var success = await _travelPostService.DeleteTravelPostByTravelPostIdAsync(postId);
                if (!success)
                {
                    return StatusCode(500, ApiResponseDto<string>.Error("删除失败", 500));
                }

                return Ok(ApiResponseDto<object>.Success(new { }, "删除成功"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除旅游记录失败");
                return StatusCode(500, ApiResponseDto<string>.Error("服务器错误", 500));
            }
        }

        //修改旅游记录
        //修改记录文本部分
        [HttpPatch("{postId}/text")]
        public async Task<IActionResult> UpdateTravelPostText(int postId, [FromBody] TravelPostTextUpdateDto dto)
        {
            var post = await _context.TravelPosts.FindAsync(postId);
            if (post == null) return NotFound();

            post.Content = dto.Content;
            post.LocationName = dto.LocationName;
            post.BeginTime = dto.BeginTime;
            post.EndTime = dto.EndTime;
            post.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(post);
        }


        //修改图片
        //上传图片
        [HttpPost("{postId}/images")]
        public async Task<IActionResult> UploadImage(int postId, [FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("未选择任何文件");

            // 处理单文件保存逻辑（和你之前写的类似）

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var savePath = Path.Combine("wwwroot/uploads", fileName);
            var relativeUrl = $"/uploads/{fileName}";

            using (var stream = new FileStream(savePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var image = new TravelImage
            {
                Url = relativeUrl,
                TravelPostId = postId,
                Order = 0
            };

            _context.TravelImages.Add(image);
            await _context.SaveChangesAsync();

            return Ok(new { id = image.Id, url = image.Url });
        }



        //删除图片
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteImage(int imageId)
        {
            var image = await _context.TravelImages.FindAsync(imageId);
            if (image == null) return NotFound();

            // 可选：删除服务器上的文件
            var filePath = Path.Combine("wwwroot", image.Url.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
            }

            _context.TravelImages.Remove(image);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //更新排序
        [HttpPatch("{postId}/images/order")]
        public async Task<IActionResult> UpdateImageOrder(int postId, [FromBody] List<ImageOrderDto> images)
        {
            foreach (var img in images)
            {
                var image = await _context.TravelImages.FindAsync(img.Id);
                if (image != null && image.TravelPostId == postId)
                {
                    image.Order = img.Order;
                }
            }

            await _context.SaveChangesAsync();
            return NoContent();
        }

        


    }
}
