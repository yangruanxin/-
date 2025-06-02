using TravelMap.DTO;
using TravelMap.Models;

namespace TravelMap.Services.Interfaces
{
    public interface ITravelPostService
    {
        Task<TravelPostResponseDto> CreateTravelPost(TravelPostRequest request, int userId);
        Task<List<TravelPost>> GetTravelPostsByUserIdAsync(int userId);
        Task<TravelPost?> GetPostByIdAsync(int postId);
        Task<bool> DeletePostByIdAsync(int postId);

    }
}
