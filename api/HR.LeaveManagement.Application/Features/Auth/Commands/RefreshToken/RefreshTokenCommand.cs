using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HR.LeaveManagement.Application.Models.Identity;
using MediatR;


namespace HR.LeaveManagement.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommand : IRequest<AuthResponse>
    {
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }
    }
}