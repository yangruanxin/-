using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using TravelMap.Models;
using TravelMap.Data;
using TravelMap.DTO;
using Microsoft.EntityFrameworkCore;


namespace TravelMap.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // 需要登录，携带 JWT Token
    public class TravelPostsController : ControllerBase
    {
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public TravelPostsController(IWebHostEnvironment env, IConfiguration config, AppDbContext context)
        {
            _env = env;
            _config = config;
            _context = context;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateTravelPost([FromForm] TravelPostCreateDto dto)
        {
            // 获取当前用户 ID（假设你用的是用户名）
            var username = User.Identity?.Name;
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user == null)
                return Unauthorized();

            // 保存图片
            List<TravelPostImage> images = new List<TravelPostImage>();
            if (dto.Images != null && dto.Images.Count > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsFolder);

                for (int i = 0; i < dto.Images.Count; i++)
                {
                    var file = dto.Images[i];
                    var uniqueFileName = $"{Guid.NewGuid()}_{file.FileName}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    images.Add(new TravelPostImage
                    {
                        ImageUrl = $"/uploads/{uniqueFileName}",
                        SortOrder = i + 1  // 或者其他定义的顺序
                    });
                }

                // 根据 orders 重新排序
                if (dto.Orders != null && dto.Orders.Count == images.Count)
                {
                    images = dto.Orders
                        .Select(order => int.TryParse(order, out int index) && index - 1 >= 0 && index - 1 < images.Count
                            ? images[index - 1]
                            : null)
                        .Where(url => url != null)
                        .ToList();
                }
            }

            // 保存到数据库
            var post = new TravelPost
            {
                Title = dto.Title,
                Content = dto.Content,
                LocationName = dto.LocationName,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                BeginTime = DateTime.ParseExact(dto.BeginTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                EndTime = DateTime.ParseExact(dto.EndTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture),
                Images = images
            };

            _context.TravelPosts.Add(post);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                code = 200,
                message = "发布成功",
                data = new
                {
                    postId = post.Id,
                    userId = post.UserId,
                    post.LocationName,
                    post.Latitude,
                    post.Longitude,
                    imageUrls = post.Images,
                    beginTime = post.BeginTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    endTime = post.EndTime.ToString("yyyy-MM-dd HH:mm:ss")
                }
            });
        }
    }

}
