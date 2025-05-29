using TravelMap.Data;
using TravelMap.DTO;
using TravelMap.Models;
using TravelMap.Services.Interfaces;

namespace TravelMap.Services
{
    public class TravelPostService : ITravelPostService
    {
        private readonly IWebHostEnvironment _hostingEnvironment;
        private readonly AppDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public TravelPostService(
            IWebHostEnvironment hostingEnvironment,
            AppDbContext dbContext,
            IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<TravelPostResponseDto> CreateTravelPost(TravelPostRequest request, int userId)
        {
            try
            {
                // 验证时间格式
                if (!DateTime.TryParse(request.BeginTime, out DateTime beginTime) ||
                    !DateTime.TryParse(request.EndTime, out DateTime endTime))
                {
                    return new TravelPostResponseDto
                    {
                        Code = 400,
                        Message = "Invalid date format. Please use yyyy-MM-dd HH:mm:ss"
                    };
                }

                // 处理图片上传
                var imageUrls = new List<string>();
                if (request.Images != null && request.Images.Count > 0)
                {
                    imageUrls = await UploadImages(request.Images);
                }

                // 创建旅行记录
                var travelPost = new TravelPost
                {
                    //Title = request.Title,
                    Content = request.Content,
                    LocationName = request.LocationName,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    BeginTime = beginTime,
                    EndTime = endTime,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                    ImageUrls = imageUrls
                };

                _dbContext.TravelPosts.Add(travelPost);
                await _dbContext.SaveChangesAsync();

                return new TravelPostResponseDto
                {
                    Code = 200,
                    Message = "post creation success",
                    Data = new TravelPostData
                    {
                        PostId = travelPost.Id,
                        UserId = userId,
                        LocationName = travelPost.LocationName,
                        Latitude = travelPost.Latitude,
                        Longitude = travelPost.Longitude,
                        ImageUrls = travelPost.ImageUrls,
                        BeginTime = travelPost.BeginTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        EndTime = travelPost.EndTime.ToString("yyyy-MM-ddTHH:mm:ss")
                    }
                };
            }
            catch (Exception ex)
            {
                return new TravelPostResponseDto
                {
                    Code = 500,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }

        private async Task<List<string>> UploadImages(List<IFormFile> images)
        {
            var imageUrls = new List<string>();
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            foreach (var image in images)
            {
                if (image.Length > 0)
                {
                    var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(fileStream);
                    }

                    var request = _httpContextAccessor.HttpContext.Request;
                    var imageUrl = $"{request.Scheme}://{request.Host}/uploads/{uniqueFileName}";
                    imageUrls.Add(imageUrl);
                }
            }

            return imageUrls;
        }
    }
}
