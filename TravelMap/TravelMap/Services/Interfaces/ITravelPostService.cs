using TravelMap.DTO;
using TravelMap.Models;

namespace TravelMap.Services.Interfaces
{
    public interface ITravelPostService
    {
        Task<TravelPostResponseDto> CreateTravelPost(TravelPostRequest request, int userId);
        Task<List<TravelPost>> GetTravelPostsByUserIdAsync(int userId);
        Task<TravelPost?> GetTravelPostByTravelPostIdAsync(int TravelPostId);
        Task<bool> DeleteTravelPostByTravelPostIdAsync(int TravelPostId);

    }
}
