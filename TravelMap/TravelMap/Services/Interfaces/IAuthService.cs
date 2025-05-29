using System.Security.Claims;
using TravelMap.Models;

namespace TravelMap.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string message)> Register(string username, string password);
        Task<(bool success, string token)> Login(string username, string password);

        int? GetUserIdFromToken(ClaimsPrincipal user);
    }
}