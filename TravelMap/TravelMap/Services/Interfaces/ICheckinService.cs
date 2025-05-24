using TravelMap.Models;

namespace TravelMap.Services.Interfaces { 

public interface ICheckinService
{
    Task<CheckinResult> CheckinAsync(CheckinRequest request);
}
}