using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.Application.Identity;
using HR.LeaveManagement.Identity.Models;
using Microsoft.AspNetCore.Identity;
using HR.LeaveManagement.Application.Exceptions;
using System.Text;
using HR.LeaveManagement.Application.Contracts.Persistence;
using HR.LeaveManagement.Application.Models.Identity;


namespace HR.LeaveManagement.Identity.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ITokenRepository _tokenRepository;

        public AuthService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ITokenRepository tokenRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenRepository = tokenRepository;
        }

        public async Task<AuthResponse> LoginAsync(AuthRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                throw new BadRequestException($"User with email {request.Email} not found");
        
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);

            if (!result.Succeeded)
                throw new BadRequestException("Credentials for '{request.Email}' are not valid");

            var jwtTokenId = Guid.NewGuid().ToString();
            var jwtToken = await _tokenRepository.CreateJwtToken(user.Id, jwtTokenId);
            var refreshToken = await _tokenRepository.CreateRefreshTokenAsync(user.Id, jwtTokenId);

            var response = new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = jwtToken,
                RefreshToken = refreshToken
            };

            return response;
        }

        public async Task<RegistrationResponse> RegisterAsync(RegistrationRequest request)
        {
            var user = new ApplicationUser
            {
                UserName = request.UserName,
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailConfirmed = true
            };
            
            var result = await _userManager.CreateAsync(user, request.Password);

            if (result.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, "Employee");
                return new RegistrationResponse { UserId = user.Id };
            }
            else
            {
                StringBuilder errors = new StringBuilder();
                foreach (var error in result.Errors)
                {
                    errors.AppendLine(error.Description);
                }
                throw new BadRequestException(errors.ToString());
            }
        }

        public async Task<AuthResponse> RefreshTokenAsync(TokenRequest request)
        {
            var storedToken = await _tokenRepository.GetRefreshTokenAsync(request.RefreshToken);
            if (storedToken == null || storedToken.IsRevoked || storedToken.IsUsed || storedToken.ExpiryDate < DateTime.UtcNow)
                throw new BadRequestException("Invalid refresh token!");

            await _tokenRepository.MarkRefreshTokenAsUsedAsync(request.RefreshToken);

            var user = await _userManager.FindByIdAsync(storedToken.UserId);
            if (user == null)
                throw new BadRequestException("Invalid refresh token!");

            var jwtTokenId = Guid.NewGuid().ToString();
            var jwtToken = await _tokenRepository.CreateJwtToken(user.Id, jwtTokenId);
            var refreshToken = await _tokenRepository.CreateRefreshTokenAsync(user.Id, jwtTokenId);
        
            return new AuthResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Token = jwtToken,
                RefreshToken = refreshToken
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            await _tokenRepository.RevokeRefreshTokenAsync(refreshToken);
        }
    }
}