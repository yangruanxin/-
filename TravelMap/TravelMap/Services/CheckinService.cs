using TravelMap.Models;
using TravelMap.Services.Interfaces;

namespace TravelMap.Services
{
    public class CheckinService : ICheckinService
    {
        private readonly TravelMapDbContext _context;
        private readonly IImageUploader _imageUploader;
        private readonly ILogger<CheckinService> _logger;

        public CheckinService(
            TravelMapDbContext context,
            IImageUploader imageUploader,
            ILogger<CheckinService> logger)
        {
            _context = context;
            _imageUploader = imageUploader;
            _logger = logger;
        }

        public async Task<CheckinData> CheckinAsync(string userId, CheckinRequest request)
        {
            // 验证地点是否存在
            var place = await _context.Places.FindAsync(request.PlaceId);
            if (place == null)
            {
                throw new KeyNotFoundException("地点不存在");
            }

            // 创建打卡记录
            var checkin = new Checkin
            {
                Id = Guid.NewGuid().ToString(),
                UserId = userId,
                PlaceId = request.PlaceId,
                CheckinTime = DateTime.UtcNow,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Comment = request.Comment
            };

            _context.Checkins.Add(checkin);

            // 处理上传的照片
            var photoUrls = new List<string>();
            if (request.Photos != null && request.Photos.Any())
            {
                foreach (var photo in request.Photos)
                {
                    try
                    {
                        var photoUrl = await _imageUploader.UploadImageAsync(photo);
                        var checkinPhoto = new CheckinPhoto
                        {
                            Id = Guid.NewGuid().ToString(),
                            CheckinId = checkin.Id,
                            PhotoUrl = photoUrl,
                            UploadTime = DateTime.UtcNow
                        };
                        _context.CheckinPhotos.Add(checkinPhoto);
                        photoUrls.Add(photoUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "上传照片失败");
                        // 可以继续处理其他照片而不是直接失败
                    }
                }
            }

            // 计算获得的积分
            var pointsEarned = CalculatePoints(checkin, photoUrls.Count);

            await _context.SaveChangesAsync();

            return new CheckinData
            {
                CheckinId = checkin.Id,
                PlaceName = place.Name,
                CheckinTime = checkin.CheckinTime,
                PointsEarned = pointsEarned,
                PhotoUrls = photoUrls
            };
        }

        private int CalculatePoints(Checkin checkin, int photoCount)
        {
            // 简单积分规则：基础分+照片加分
            int basePoints = 10;
            int photoPoints = Math.Min(photoCount, 3) * 2; // 最多3张照片加分
            return basePoints + photoPoints;
        }
    }
}
