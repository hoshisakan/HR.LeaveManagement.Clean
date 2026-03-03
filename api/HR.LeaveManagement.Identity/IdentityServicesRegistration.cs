using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Identity;
using HR.LeaveManagement.Identity.Models;
using HR.LeaveManagement.Identity.DatabaseContext;
using Microsoft.EntityFrameworkCore;
using HR.LeaveManagement.Identity.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using HR.LeaveManagement.Application.Models.Identity;
using System.Text;
using HR.LeaveManagement.Application.Identity;


namespace HR.LeaveManagement.Identity
{
    public static class IdentityServicesRegistration
    {
        public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Configure the JwtSettings
            services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
            
            // Add the DbContext
            services.AddDbContext<HrLeaveManagementIdentityDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("HrDatabaseConnectionString")));

            // Add the Identity services
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<HrLeaveManagementIdentityDbContext>()
                .AddDefaultTokenProviders();

            // Add the AuthService
            services.AddTransient<IAuthService, AuthService>();
            // Add the UserService
            services.AddTransient<IUserService, UserService>();

            // Add JwtAuthentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = configuration["JwtSettings:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["JwtSettings:Audience"],
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.Zero,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:Key"]))
                };
            });

            return services;
        }
    }
}