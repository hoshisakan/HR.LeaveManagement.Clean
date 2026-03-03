using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HR.LeaveManagement.Domain;
using HR.LeaveManagement.Persistence.Configurations;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using HR.LeaveManagement.Domain.Common;


namespace HR.LeaveManagement.Persistence.DatabaseContext
{
    public class HrDatabaseContext : DbContext
    {
        public HrDatabaseContext(DbContextOptions<HrDatabaseContext> options) : base(options)
        {
        }

        public DbSet<LeaveType> LeaveTypes { get; set; }
        public DbSet<LeaveRequest> LeaveRequests { get; set; }
        public DbSet<LeaveAllocation> LeaveAllocations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply all the configurations in the assembly
            // Seed the LeaveType entity
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDatabaseContext).Assembly);
            
            // Apply the configuration for the LeaveType entity
            base.OnModelCreating(modelBuilder);
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            foreach (var entry in ChangeTracker.Entries<BaseEntity>()
            .Where(q => q.State == EntityState.Added || q.State == EntityState.Modified))
            {
                // Set the DateModified property to the current date and time
                entry.Entity.DateModified = DateTime.Now;

                // Set the DateCreated property if the entity is being added
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.DateCreated = DateTime.Now;
                }

                // Prevent the DateCreated property from being modified
                if (entry.State == EntityState.Modified)
                {
                    entry.Property(x => x.DateCreated).IsModified = false;
                }
            }
            return base.SaveChangesAsync(cancellationToken);
        }
    }
}