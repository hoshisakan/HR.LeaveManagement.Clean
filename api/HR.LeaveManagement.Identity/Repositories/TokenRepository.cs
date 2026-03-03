using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Models.Identity;
using HR.LeaveManagement.Identity.DatabaseContext;
using HR.LeaveManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace HR.LeaveManagement.Identity.Repositories
{
    public class TokenRepository : ITokenRepository
    {
        private readonly HrLeaveManagementIdentityDbContext _context;
        private readonly ILogger<TokenRepository> _logger;
        private readonly JwtSettings _jwtSettings;
        private readonly UserManager<ApplicationUser> _userManager;

        public TokenRepository(
            HrLeaveManagementIdentityDbContext context,
            ILogger<TokenRepository> logger,
            IOptions<JwtSettings> jwtSettings,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _logger = logger;
            _jwtSettings = jwtSettings.Value;
            _userManager = userManager;
        }

        public async Task<string> CreateJwtToken(string userId, string jwtId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException($"User with id {userId} not found.");

            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = roles.Select(role => new Claim(ClaimTypes.Role, role)).ToList();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName ?? string.Empty),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("uid", user.Id),
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key));
            var credentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtSettings.Issuer,
                audience: _jwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_jwtSettings.DurationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> CreateRefreshTokenAsync(string userId, string jwtId)
        {
            var existingRefreshTokens = await _context.RefreshTokens
                .Where(x => x.UserId == userId && !x.IsUsed && !x.IsRevoked)
                .ToListAsync();
            foreach (var token in existingRefreshTokens)
                token.IsRevoked = true;

            var refreshToken = new RefreshToken
            {
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                JwtId = jwtId,
                UserId = userId,
                AddedDate = DateTime.UtcNow,
                ExpiryDate = DateTime.UtcNow.AddDays(7),
                IsUsed = false,
                IsRevoked = false
            };
            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken.Token;
        }

        public async Task<RefreshTokenDto?> GetRefreshTokenAsync(string refreshToken)
        {
            var entity = await _context.RefreshTokens
                .FirstOrDefaultAsync(x => x.Token == refreshToken && !x.IsUsed && !x.IsRevoked);
            if (entity == null)
                return null;
            return new RefreshTokenDto
            {
                UserId = entity.UserId,
                IsUsed = entity.IsUsed,
                IsRevoked = entity.IsRevoked,
                ExpiryDate = entity.ExpiryDate
            };
        }

        public async Task MarkRefreshTokenAsUsedAsync(string refreshToken)
        {
            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (existing != null)
            {
                existing.IsUsed = true;
                existing.IsRevoked = true;
                _logger.LogInformation("Refresh token {Token} marked as used and revoked", refreshToken);
            }
            await _context.SaveChangesAsync();
        }

        public async Task RevokeRefreshTokenAsync(string refreshToken)
        {
            var existing = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken);
            if (existing != null)
            {
                existing.IsRevoked = true;
                _logger.LogInformation("Refresh token {Token} revoked", refreshToken);
            }
            await _context.SaveChangesAsync();
        }
    }
}
