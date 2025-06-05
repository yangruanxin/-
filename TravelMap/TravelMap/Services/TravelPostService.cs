using MySqlConnector;
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
        private readonly IConfiguration _configuration;

        public TravelPostService(IWebHostEnvironment hostingEnvironment,AppDbContext dbContext, IConfiguration configuration,IHttpContextAccessor httpContextAccessor)
        {
            _hostingEnvironment = hostingEnvironment;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
        }

        //创建旅游记录
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
                var images = new List<TravelImage>();
                if (request.ImageUrls != null && request.ImageUrls.Count > 0)
                {
                    images = await UploadImages(request.ImageUrls, request.Orders);
                }

                // 创建旅行记录
                var travelPost = new TravelPost
                {
                    Content = request.Content,
                    LocationName = request.LocationName,
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    BeginTime = beginTime,
                    EndTime = endTime,
                    CreatedAt = DateTime.UtcNow,
                    UserId = userId,
                    ImageUrls = images
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
                        LocationName = travelPost.LocationName,
                        Latitude = travelPost.Latitude,
                        Longitude = travelPost.Longitude,
                        BeginTime = travelPost.BeginTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        EndTime = travelPost.EndTime.ToString("yyyy-MM-ddTHH:mm:ss"),
                        UserId = userId,
                        ImageUrls = travelPost.ImageUrls
                    .OrderBy(img => img.Order)
                    .Select(img => img.Url)
                    .ToList()
                    }
                };
            }
            catch (Exception ex)
            {
                var message = ex.InnerException?.Message ?? ex.Message;

                return new TravelPostResponseDto
                {
                    Code = 500,
                    Message = $"An error occurred: {message}"
                };
            }

        }

        //上传图片
        private async Task<List<TravelImage>> UploadImages(List<IFormFile> images, List<string> orders)
        {
            var travelImages = new List<TravelImage>();
            var uploadsFolder = Path.Combine(_hostingEnvironment.WebRootPath, "uploads");

            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            if (images == null || images.Count == 0)
            {
                // 空图片列表，直接返回空列表
                return travelImages;
            }

            for (int i = 0; i < images.Count; i++)
            {
                var image = images[i];
                var order = i;
                if (orders != null && orders.Count > i && int.TryParse(orders[i], out int ord))
                {
                    order = ord;
                }

                if (image.Length > 0)
                {
                    var uniqueName = Guid.NewGuid() + Path.GetExtension(image.FileName);
                    var filePath = Path.Combine(uploadsFolder, uniqueName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await image.CopyToAsync(stream);
                    }

                    var request = _httpContextAccessor.HttpContext.Request;
                    var imageUrl = $"{request.Scheme}://{request.Host}/uploads/{uniqueName}";

                    travelImages.Add(new TravelImage
                    {
                        Url = imageUrl,
                        Order = order
                    });
                }
            }
            return travelImages;
        }

        //从用户Id获得travelposts
        public async Task<List<TravelPost>> GetTravelPostsByUserIdAsync(int userId)
        {
            var posts = new List<TravelPost>();
            var postDict = new Dictionary<int, TravelPost>();

            using (var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await connection.OpenAsync();

                // 获取基础URL（协议+主机）
                var request = _httpContextAccessor.HttpContext.Request;
                var baseUrl = $"{request.Scheme}://{request.Host}";

                //查询 travelposts
                string postQuery = @"SELECT Id, Content, LocationName, Latitude, Longitude, 
                                    BeginTime, EndTime, UserId, CreatedAt, UpdatedAt 
                             FROM travelposts 
                             WHERE UserId = @UserId";

                using (var command = new MySqlCommand(postQuery, connection))
                {
                    command.Parameters.AddWithValue("@UserId", userId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var post = new TravelPost
                            {
                                Id = reader.GetInt32("Id"),
                                Content = reader.GetString("Content"),
                                LocationName = reader.GetString("LocationName"),
                                Latitude = reader.GetDouble("Latitude"),
                                Longitude = reader.GetDouble("Longitude"),
                                BeginTime = reader.GetDateTime("BeginTime"),
                                EndTime = reader.GetDateTime("EndTime"),
                                CreatedAt = reader.GetDateTime("CreatedAt"),
                                UpdatedAt = reader.GetDateTime("UpdatedAt"),
                                UserId = reader.GetInt32("UserId"),
                                ImageUrls = new List<TravelImage>()
                            };
                            posts.Add(post);
                            postDict[post.Id] = post;
                        }
                    }
                }

                if (posts.Count == 0) return posts;

                //查询 travelimages
                string imageQuery = @"SELECT Id, Url, `Order`, TravelPostId
                      FROM travelimages 
                      WHERE TravelPostId IN (" + string.Join(",", postDict.Keys) + ")";

                using (var imageCommand = new MySqlCommand(imageQuery, connection))
                using (var imageReader = await imageCommand.ExecuteReaderAsync())
                {
                    while (await imageReader.ReadAsync())
                    {
                        var relativeUrl = imageReader.GetString("Url");
                        // 不拼接 baseUrl，直接返回相对路径
                        var relativePath = relativeUrl.StartsWith("/") ? relativeUrl : "/" + relativeUrl;
                        var image = new TravelImage
                        {
                            Id = imageReader.GetInt32("Id"),
                            Url = relativePath, // 只传相对路径
                            Order = imageReader.GetInt32("Order"),
                            TravelPostId = imageReader.GetInt32("TravelPostId")
                            
                        };

                        if (postDict.TryGetValue(image.TravelPostId, out var post))
                        {
                            post.ImageUrls.Add(image);
                        }
                    }
                }
            }

            return posts;
        }

        //从travelpostId获得post
        public async Task<TravelPost?> GetTravelPostByTravelPostIdAsync(int TravelPostId)
        {
            TravelPost? post = null;

            using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            // 查询 travelposts
            string postQuery = @"SELECT Id, Content, LocationName, Latitude, Longitude, 
                                BeginTime, EndTime, CreatedAt, UpdatedAt, UserId
                         FROM travelposts 
                         WHERE Id = @Id";

            using (var postCommand = new MySqlCommand(postQuery, connection))
            {
                postCommand.Parameters.AddWithValue("@Id", TravelPostId);

                using var reader = await postCommand.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    post = new TravelPost
                    {
                        Id = reader.GetInt32("Id"),
                        Content = reader.GetString("Content"),
                        LocationName = reader.GetString("LocationName"),
                        Latitude = reader.GetDouble("Latitude"),
                        Longitude = reader.GetDouble("Longitude"),
                        BeginTime = reader.GetDateTime("BeginTime"),
                        EndTime = reader.GetDateTime("EndTime"),
                        CreatedAt = reader.GetDateTime("CreatedAt"),
                        UpdatedAt = reader.GetDateTime("UpdatedAt"),
                        UserId = reader.GetInt32("UserId"),
                        ImageUrls = new List<TravelImage>()
                    };
                }
            }

            if (post == null)
                return null;

            // 查询 travelimages
            string imageQuery = @"SELECT Id, `Order`, Url, TravelPostId
                      FROM travelimages 
                      WHERE TravelPostId = @TravelPostId
                      ORDER BY `Order` ASC";
     
            using (var imageCommand = new MySqlCommand(imageQuery, connection))
            {
                imageCommand.Parameters.AddWithValue("@TravelPostId", TravelPostId);

                using var imgReader = await imageCommand.ExecuteReaderAsync();
                while (await imgReader.ReadAsync())
                {
                    var image = new TravelImage
                    {
                        Id = imgReader.GetInt32("Id"),
                        Url = imgReader.GetString("Url"),
                        Order = imgReader.GetInt32("Order"),
                        TravelPostId = imgReader.GetInt32("TravelPostId"),
                    };
                    post.ImageUrls.Add(image); 
                }
            }
            return post;
        }

        //通过TravelPostId删除post
        public async Task<bool> DeleteTravelPostByTravelPostIdAsync(int TravelPostId)
        {
            using var connection = new MySqlConnection(_configuration.GetConnectionString("DefaultConnection"));
            await connection.OpenAsync();

            string sql = "DELETE FROM travelposts WHERE Id = @Id";
            using var cmd = new MySqlCommand(sql, connection);
            cmd.Parameters.AddWithValue("@Id", TravelPostId);

            var result = await cmd.ExecuteNonQueryAsync();
            return result > 0;
        }
    }
}
