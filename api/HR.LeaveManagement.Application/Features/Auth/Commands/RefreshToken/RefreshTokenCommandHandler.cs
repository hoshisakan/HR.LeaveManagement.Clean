using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Exceptions;
using HR.LeaveManagement.Application.Models.Identity;
using MediatR;

namespace HR.LeaveManagement.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
    {
        private readonly ITokenRepository _tokenRepository;

        public RefreshTokenCommandHandler(ITokenRepository tokenRepository)
        {
            _tokenRepository = tokenRepository;
        }

        public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var storedToken = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
                throw new BadRequestException("Invalid refresh token!");

            await _tokenRepository.MarkRefreshTokenAsUsedAsync(request.RefreshToken);

            var newJwtId = Guid.NewGuid().ToString();
            var newJwtToken = await _tokenRepository.CreateJwtToken(storedToken.UserId, newJwtId);
            var newRefreshToken = await _tokenRepository.CreateRefreshTokenAsync(storedToken.UserId, newJwtId);

            return new AuthResponse { Token = newJwtToken, RefreshToken = newRefreshToken };
        }
    }
}