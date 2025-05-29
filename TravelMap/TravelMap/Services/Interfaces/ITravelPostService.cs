using TravelMap.DTO;

namespace TravelMap.Services.Interfaces
{
    public interface ITravelPostService
    {
        Task<TravelPostResponseDto> CreateTravelPost(TravelPostRequest request, int userId);
    }
}
