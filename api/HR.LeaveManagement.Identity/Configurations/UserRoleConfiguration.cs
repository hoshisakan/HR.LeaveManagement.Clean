using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.AspNetCore.Identity;
using HR.LeaveManagement.Identity.Models;


namespace HR.LeaveManagement.Identity.Configurations
{
    public class UserRoleConfiguration : IEntityTypeConfiguration<IdentityUserRole<string>>
    {
        public void Configure(EntityTypeBuilder<IdentityUserRole<string>> builder)
        {
            builder.HasData(
                new IdentityUserRole<string>
                {
                    UserId = "f0493071-d633-4ec6-8215-051097bc8bef1",
                    RoleId = "de2dbc34-f0d2-480f-ad56-718be1a036c5",
                },
                new IdentityUserRole<string>
                {
                    UserId = "d04eb003-db0e-4eff-943d-aba0f84aa153",
                    RoleId = "01815a0a-a125-4a6d-92f5-5b7f75e23d36"
                }
            );
        }
    }
}