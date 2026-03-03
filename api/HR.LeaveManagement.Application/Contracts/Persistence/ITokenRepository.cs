using HR.LeaveManagement.Application.Models.Identity;

namespace HR.LeaveManagement.Application.Contracts.Persistence
{
    public interface ITokenRepository
    {
        Task<string> CreateJwtToken(string userId, string jwtId);
        Task<string> CreateRefreshTokenAsync(string userId, string jwtId);
        Task<RefreshTokenDto?> GetRefreshTokenAsync(string refreshToken);
        Task MarkRefreshTokenAsUsedAsync(string refreshToken);
        Task RevokeRefreshTokenAsync(string refreshToken);
    }
}