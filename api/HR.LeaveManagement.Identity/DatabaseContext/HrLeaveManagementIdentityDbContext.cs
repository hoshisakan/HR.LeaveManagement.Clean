using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using HR.LeaveManagement.Identity.Models;
using Microsoft.EntityFrameworkCore;


namespace HR.LeaveManagement.Identity.DatabaseContext
{
    public class HrLeaveManagementIdentityDbContext : IdentityDbContext<ApplicationUser>
    {
        public HrLeaveManagementIdentityDbContext(DbContextOptions<HrLeaveManagementIdentityDbContext> options) : base(options)
        {
        }

        public DbSet<RefreshToken> RefreshTokens { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply the base configurations
            base.OnModelCreating(modelBuilder);
            // Apply all the configurations in the assembly
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrLeaveManagementIdentityDbContext).Assembly);
        }
    }
}