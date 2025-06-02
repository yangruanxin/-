using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using TravelMap.Models;
using TravelMap.DTO;

namespace TravelMap.Services.Interfaces
{
    public interface IAuthService
    {
        Task<(bool success, string message)> Register(string username, string password);
        Task<(bool success, string token)> Login(string username, string password);
        int GetUserIdFromToken(ClaimsPrincipal user);
        Task<User> GetUserByIdAsync(int Id);
        Task<bool> DeleteUserAsync(int Id);
        
    }
}